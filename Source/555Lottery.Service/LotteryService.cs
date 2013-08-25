using _555Lottery.DataModel;
using _555Lottery.Service.TemplateModels;
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

		private LotteryService() 
		{
			log = new LogService();
			email = new EmailService(log);
			bitcoin = new BitCoinService(log);
		}

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
			// 2013-08-21
			// RUB | CNY | MXN | PHP | COP | ARS | INR
			// ok  | ok  | --- | --- | --- | --- | ok

			try
			{
				LotteryService.Instance.GetExchangeRate("BTC", "USD");
				LotteryService.Instance.GetExchangeRate("BTC", "EUR");
				LotteryService.Instance.GetExchangeRate("BTC", "RUB");
				LotteryService.Instance.GetExchangeRate("BTC", "CNY");
				LotteryService.Instance.GetExchangeRate("BTC", "INR");
			}
			catch
			{
				log.Log(LogLevel.Warning, "GETEXCHANGERATE", "Failed to update at least one of the exchange rates.");
			}


			DateTime generationDeadline = DateTime.UtcNow.AddHours(-2);
			Draw lastDraw = Context.Draws.Where(d => (d.DeadlineUtc < generationDeadline)).OrderByDescending(d => d.DeadlineUtc).First();
			Draw currentDraw = Context.Draws.Where(d => (d.DeadlineUtc > generationDeadline)).OrderBy(d => d.DeadlineUtc).First();

			// update BitCoin confirmations every 20 or so minutes if the confirmation number is low
			if (transactionsWereUpdatedAt.AddMinutes(20) < DateTime.UtcNow)
			{
				transactionsWereUpdatedAt = DateTime.UtcNow;

				bitcoin.UpdateTransactionLog(currentDraw.BitCoinAddress);

				bitcoin.MatchUpTransactionsAndTicketLots(currentDraw);
			}

			// check if we need to randomize the numbers for the next draw, and fill in the USD and EUR jackpot values
			if (lastDraw.WinningTicketSequence == null)
			{
				bitcoin.UpdateTransactionLog(lastDraw.BitCoinAddress);
				bitcoin.MatchUpTransactionsAndTicketLots(lastDraw);

				lastDraw.ExchangeRateUSDAtDeadline = GetExchangeRate("BTC", "USD");
				lastDraw.ExchangeRateEURAtDeadline = GetExchangeRate("BTC", "EUR");

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
				//int numberOfTicketsSoldAndConfirmed = 0;
				//Ticket[] validTickets = new Ticket[0];

				//email.Send("TEST", "en-US", email.AdminEmails, null, new { LastDraw = lastDraw, ValidTickets = validTickets });
			}
		}

		public ExchangeRate GetExchangeRate(string currencyISO1, string currencyISO2)
		{
			ExchangeRate result = null;

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

						result = exrate;
					}
					catch (Exception ex)
					{
						log.LogException(ex);

						if (lastExrate != null)
						{
							result = lastExrate;
						}
					}
				}
			}
			else
			{
				result = lastExrate;
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

		public TicketLot[] GetTicketLot(string ticketLotCode)
		{
			return context.TicketLots.Where(tl => tl.Code == ticketLotCode).OrderByDescending(tl => tl.CreatedUtc).ToArray();
		}

		public void SaveTicketLot(TicketLot tl)
		{
			//string codeMap = "ABCDEFGHIJKLMOPQRSTUVWXYZ0123456789";
			string codeMap = "0123456789";

			Random rnd = new Random();
			do
			{
				tl.Code = "TL" + (tl.Draw.DrawId % 100).ToString("00") + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)];
			} while (context.TicketLots.FirstOrDefault(l => (l.Code == tl.Code) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

			do {
				tl.TotalDiscountBTC = BitCoinService.OneSatoshi * rnd.Next(10000);
			} while (context.TicketLots.FirstOrDefault(l => (l.TotalDiscountBTC == tl.TotalDiscountBTC) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

			tl.State = TicketLotState.WaitingForPayment;

			foreach (Ticket t in tl.Tickets)
			{
				t.SequenceHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(t.Sequence));
			}
			
			Context.TicketLots.Add(tl);
			Context.SaveChanges();

			log.Log(LogLevel.Information, "TICKETLOT", "A new ticket lot ({0}) was saved succesfully.", tl.TicketLotId);
		}

		public void LogException(Exception ex)
		{
			log.LogException(ex);
		}

		public void Log(LogLevel level, string action, string formatterText, params object[] param)
		{
			log.Log(level, action, formatterText, param);
		}

		public object DoEmailTemplateTest(string templateName)
		{
			//int numberOfTicketsSoldAndConfirmed = 0;
			Ticket[] validTickets = new Ticket[0];
			EmailTemplateModelTEST model = new EmailTemplateModelTEST { LastDraw = this.LastDraw, ValidTickets = validTickets };

			email.Send(templateName, "en-US", null, null, model);

			return model;
		}
	}
}
