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
					log = new LogService(context);
					email = new EmailService();

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
				Draw draw = Context.Draws.Where(d => (d.DeadlineUtc < DateTime.UtcNow)).OrderByDescending(d => d.DeadlineUtc).First();
				draw.JackpotUSD = draw.JackpotBTC * GetExchangeRate("BTC", "USD");
				draw.JackpotEUR = draw.JackpotBTC * GetExchangeRate("BTC", "EUR");

				return draw;
			}
		}

		public Draw CurrentDraw
		{
			get
			{
				Draw draw = Context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();
				draw.JackpotUSD = draw.JackpotBTC * GetExchangeRate("BTC", "USD");
				draw.JackpotEUR = draw.JackpotBTC * GetExchangeRate("BTC", "EUR");

				return draw;
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
			// check if we need to randomize the numbers for the next draw, and fill in the USD and EUR jackpot values
			DateTime generationDeadline = DateTime.UtcNow.AddHours(-2);
			Draw lastDraw = Context.Draws.Where(d => (d.DeadlineUtc < generationDeadline)).OrderByDescending(d => d.DeadlineUtc).First();
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

				// TODO: write to log
				log.Log("WINNINGTICKETGENERATION", "{0} {1} {2}", lastDraw.WinningTicketSequence, lastDraw.WinningTicketHash, lastDraw.WinningTicketGeneratedAt);

				// TODO: send e-mail
				//email.Send("TEST", "en-US", null, null, null);
			}

			// TODO: update BitCoin confirmations every 15 minutes if the confirmation number is low
			// TODO: check if we need to evaluate some tickets
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
						HttpWebRequest request = HttpWebRequest.CreateHttp("http://data.mtgox.com/api/1/" + currencyISO1 + currencyISO2 + "/ticker");
						request.Timeout = 2000;
						WebResponse response = null;

						try
						{
							response = request.GetResponse();

							DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(MtGoxTicker));
							MtGoxTicker ticker = (MtGoxTicker)js.ReadObject(response.GetResponseStream());

							exrate.TimeUtc = DateTime.UtcNow;
							exrate.CurrencyISO1 = currencyISO1;
							exrate.CurrencyISO2 = currencyISO2;
							exrate.Rate = ticker.Return.Avg.Value;

							log.Log("NEWEXCHANGERATE", "A new rate of {0}/{1} was succesfully downloaded.", currencyISO1, currencyISO2);
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
			Draw[] draws = context.Draws.OrderByDescending(d => d.DeadlineUtc).ToArray();

			decimal btcusd = LotteryService.Instance.GetExchangeRate("BTC", "USD");
			decimal btceur = LotteryService.Instance.GetExchangeRate("BTC", "EUR");

			foreach (Draw d in draws)
			{
				d.JackpotUSD = d.JackpotBTC * btcusd;
				d.JackpotEUR = d.JackpotBTC * btceur;
			}
			
			return draws;
		}

		public Draw GetDraw(string drawCode)
		{
			Draw result = context.Draws.FirstOrDefault(d => d.DrawCode == drawCode);

			if (result == null) return null;

			decimal btcusd = LotteryService.Instance.GetExchangeRate("BTC", "USD");
			decimal btceur = LotteryService.Instance.GetExchangeRate("BTC", "EUR");

			result.JackpotUSD = result.JackpotBTC * btcusd;
			result.JackpotEUR = result.JackpotBTC * btceur;

			return result;
		}

		public TicketLot GetTicketLot(string ticketLotCode)
		{
			return context.TicketLots.FirstOrDefault(tl => tl.Code == ticketLotCode);
		}

		public TicketLot CreateTicketLot(Draw draw, string sessionId)
		{
			TicketLot tl = context.TicketLots.Create();
			tl.Initialize(sessionId);
			tl.Draw = draw;

			return tl;
		}

		public void SaveTicketLot(TicketLot tl)
		{
			string codeMap = "ABCDEFGHIJKLMOPQRSTUVWXYZ0123456789";

			tl.TotalBTC = tl.TotalPrice;

			Random rnd = new Random();
			do
			{
				tl.Code = "TL" + (tl.Draw.DrawId % 100).ToString("00") + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)];
			} while (context.TicketLots.FirstOrDefault(l => (l.Code == tl.Code) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

			Context.TicketLots.Add(tl);

			foreach (Ticket ticket in tl.Tickets)
			{
				if ((ticket.Mode != TicketMode.Empty) && (ticket.TicketId == 0))
				{
					Context.Tickets.Add(ticket);
				}
			}
			Context.SaveChanges();

			log.Log("TICKETLOT", "A new ticket lot ({0}) was saved succesfully.", tl.TicketLotId);
		}

		public Ticket CreateTicket(TicketLot tl, string ticketType, string ticketSequence)
		{
			Ticket newTicket = Context.Tickets.Create();
			newTicket.Initialize(tl, ticketType, ticketSequence);

			return newTicket;
		}

		public Ticket CreateTicket(TicketLot tl, TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			Ticket newTicket = Context.Tickets.Create();
			newTicket.Initialize(tl, mode, type, numbers, jokers);

			return newTicket;
		}
	}
}
