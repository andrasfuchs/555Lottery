using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace _555Lottery.Service
{
	public class LotteryService
	{
		private static LotteryService instance;
		private static LogService log;
		private static EmailService email;
		private static BitCoinService bitcoin;
		private static DateTime transactionsWereUpdatedAt = DateTime.MinValue;
		private static object syncRoot = new Object();
		
		private Timer timer;

		private LotteryDbContext context;
		public LotteryDbContext Context
		{
			private get
			{
				if (context == null)
				{
					Context = new LotteryDbContext();
				}

				return context;
			}

			set
			{
				if (context != value)
				{
					if (timer != null)
					{
						timer.Stop();
					}

					context = value;
					log = new LogService();
					email = new EmailService(log);
					bitcoin = new BitCoinService(log);

					timer = new Timer(60 * 1000);
					timer.Elapsed += timer_Elapsed;
					timer.Start();
					timer_Elapsed(timer, null);
				}
			}
		}

		public Draw LastDraw
		{
			get
			{
				return Context.Draws.Where(d => (d.DeadlineUtc < DateTime.UtcNow)).OrderByDescending(d => d.DeadlineUtc).First();
			}
		}

		public Draw CurrentDraw
		{
			get
			{
				return Context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();
			}
		}

		public int DrawsRemaining
		{
			get
			{
				return Context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).Count();
			}
		}

		private LotteryService() { }

		public static LotteryService Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncRoot)
					{
						if (instance == null)
						{
							instance = new LotteryService();
						}
					}
				}

				return instance;
			}
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			DateTime generationDeadline = DateTime.UtcNow.AddHours(-2);
			Draw lastDraw = Context.Draws.Where(d => (d.DeadlineUtc < generationDeadline)).OrderByDescending(d => d.DeadlineUtc).First();

			// update BitCoin confirmations every 20 or so minutes if the confirmation number is low
			if (transactionsWereUpdatedAt.AddMinutes(20) < DateTime.UtcNow)
			{
				transactionsWereUpdatedAt = DateTime.UtcNow;

				bitcoin.UpdateTransactionLog(lastDraw.BitCoinAddress);

				bitcoin.MatchUpTransactionsAndTicketLots(lastDraw);
			}

			// check if we need to randomize the numbers for the next draw, and fill in the USD and EUR jackpot values
			if (lastDraw.WinningTicketSequence == null)
			{
				lastDraw.JackpotUSDAtDeadline = lastDraw.JackpotBTC * GetExchangeRate("BTC", "USD");
				lastDraw.JackpotEURAtDeadline = lastDraw.JackpotBTC * GetExchangeRate("BTC", "EUR");

				// generate winning sequence
				Random rnd = new Random();
				int[] generatedNumbers = new int[5];
				for (int j = 0; j < 5; j++)
				{
					if (generatedNumbers[j] == 0)
					{
						int rndNumber = 0;
						do
						{
							rndNumber = rnd.Next(55) + 1;
						} while (generatedNumbers.Contains(rndNumber));

						generatedNumbers[j] = rndNumber;
					}
				}
				Array.Sort(generatedNumbers);

				int generatedJoker = rnd.Next(5) + 1;

				lastDraw.WinningTicketSequence = String.Join(",", generatedNumbers) + "|" + generatedJoker;
				lastDraw.WinningTicketGeneratedAt = DateTime.UtcNow;

				// compute hash
				lastDraw.WinningTicketHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(lastDraw.WinningTicketSequence));
				context.SaveChanges();

				// write to log
				log.Log(LogLevel.Information, "WINNINGTICKETGENERATION", "{0} {1} {2}", lastDraw.WinningTicketSequence, lastDraw.WinningTicketHash, lastDraw.WinningTicketGeneratedAt);

				bitcoin.EvaluateDrawTicketLots(lastDraw);

				// send e-mail with the results
				//email.Send("TEST", "en-US", null, null, null);
			}
		}

		public decimal GetExchangeRate(string currencyISO1, string currencyISO2)
		{
			decimal result = 0;

			ExchangeRate lastExrate = Context.ExchangeRates.Where(er => (er.CurrencyISO1 == currencyISO1) && (er.CurrencyISO2 == currencyISO2)).OrderByDescending(er => er.TimeUtc).FirstOrDefault();
			if ((lastExrate == null) || (lastExrate.TimeUtc.AddMinutes(15) < DateTime.UtcNow))
			{
				ExchangeRate exrate = Context.ExchangeRates.Create();

				lock (syncRoot)
				{
					try
					{
						string url = "http://data.mtgox.com/api/1/" + currencyISO1 + currencyISO2 + "/ticker";
						HttpWebRequest request = HttpWebRequest.CreateHttp(url);
						request.Timeout = 2000;
						WebResponse response = null;

						try
						{
							response = request.GetResponse();
							log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE:'{1}'", url, response);

							DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(MtGoxTicker));
							MtGoxTicker ticker = (MtGoxTicker)js.ReadObject(response.GetResponseStream());

							exrate.TimeUtc = DateTime.UtcNow;
							exrate.CurrencyISO1 = currencyISO1;
							exrate.CurrencyISO2 = currencyISO2;
							exrate.Rate = ticker.Return.Avg.Value;

							log.Log(LogLevel.Information, "NEWEXCHANGERATE", "A new rate of {0}/{1} was succesfully downloaded.", currencyISO1, currencyISO2);
							email.Send("TEST", "en-US", null, null, null);
						}
						catch (Exception ex)
						{
							log.LogException(ex);
						}
						finally
						{
							if (response != null) response.Close();

							Context.ExchangeRates.Add(exrate);
							Context.SaveChanges();
						}

						result = exrate.Rate;
					}
					catch
					{
						// TODO: Log exception into the DB
						if (lastExrate != null)
						{
							result = lastExrate.Rate;
						}
					}
				}
			}
			else
			{
				result = lastExrate.Rate;
			}

			return result;
		}

		public Draw[] GetDraws()
		{
			return context.Draws.OrderByDescending(d => d.DeadlineUtc).ToArray();
		}

		public Draw GetDraw(string drawCode)
		{
			return context.Draws.FirstOrDefault(d => d.DrawCode == drawCode);
		}

		public TicketLot GetTicketLot(string ticketLotCode)
		{
			return context.TicketLots.OrderByDescending(tl => tl.CreatedUtc).FirstOrDefault(tl => tl.Code == ticketLotCode);
		}

		public void SaveTicketLot(TicketLot tl)
		{
			string codeMap = "ABCDEFGHIJKLMOPQRSTUVWXYZ0123456789";

			Random rnd = new Random();
			do
			{
				tl.Code = "TL" + (tl.Draw.DrawId % 100).ToString("00") + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)];
			} while (context.TicketLots.FirstOrDefault(l => (l.Code == tl.Code) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

			do {
				tl.TotalBTCDiscount = BitCoinService.OneSatoshi * rnd.Next(10000);
			} while (context.TicketLots.FirstOrDefault(l => (l.TotalBTCDiscount == tl.TotalBTCDiscount) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

			tl.State = TicketLotState.WaitingForPayment;

			foreach (Ticket ticket in tl.Tickets)
			{
				if ((ticket.Mode != TicketMode.Empty) && (ticket.TicketId == 0))
				{
					Context.Tickets.Add(ticket);

					// TODO: generate games					
				}
			}
			// TODO: calculate totalBTC

			Context.TicketLots.Add(tl);
			Context.SaveChanges();

			log.Log(LogLevel.Information, "TICKETLOT", "A new ticket lot ({0}) was saved succesfully.", tl.TicketLotId);
		}
	}
}
