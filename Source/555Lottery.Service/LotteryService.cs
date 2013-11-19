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
		private static DateTime refreshDrawPropertiesUpdatedAt = DateTime.MinValue;
		private static object staticLockObject = new Object();

		private Timer timer;
		private object contextLockObject;
		private object drawLockObject;
		private LotteryDbContext context;
		public LotteryDbContext Context
		{
			get
			{
				lock (contextLockObject)
				{
					if (context == null)
					{
						throw new Exception("You must call LotteryService.Initialize() first!");
					}
				}

				return context;
			}
		}

		private Draw[] draws;
		public Draw[] Draws
		{
			get
			{
				//if (draws == null)
				//{
				//	RefreshDrawProperties(true);
				//}

				lock (drawLockObject)
				{
					return draws;
				}
			}
			private set
			{
				draws = value;
			}
		}

		private Draw lastDraw;
		public Draw LastDraw
		{
			get 
			{
				//if (lastDraw == null)
				//{
				//	RefreshDrawProperties(true);
				//}

				lock (drawLockObject)
				{
					return lastDraw;
				}
			}
			
			private set 
			{
				lastDraw = value;
			}
		}

		private Draw currentDraw;
		public Draw CurrentDraw
		{
			get
			{
				//if (currentDraw == null)
				//{
				//	RefreshDrawProperties(true);
				//}

				lock (drawLockObject)
				{
					return currentDraw;
				}
			}

			private set
			{
				currentDraw = value;
			}
		}

		private Draw DrawAcceptingNewTickets
		{
			get;
			set;
		}

		private Draw DrawNotAcceptingTicketsAnyMore
		{
			get;
			set;
		}

		public int DrawsRemaining
		{
			get
			{
				return Context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).Count();
			}
		}

		public static LotteryService Instance
		{
			get
			{
				if (instance == null)
				{
					lock (staticLockObject)
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

		private LotteryService()
		{
			contextLockObject = new object();
			drawLockObject = new object();

			log = new LogService();
			email = new EmailService(log);
			bitcoin = new BitCoinService(log);
		}

		void LotteryServiceTimerElapsed(object sender, ElapsedEventArgs e)
		{
			lock (Context)
			{
				RefreshDrawProperties(false);

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

				// update BitCoin confirmations every 20 or so minutes if the confirmation number is low
				if ((transactionsWereUpdatedAt.AddMinutes(20) < DateTime.UtcNow) || (this.DrawNotAcceptingTicketsAnyMore.WinningTicketSequence == null))
				{
					transactionsWereUpdatedAt = DateTime.UtcNow;

					bitcoin.UpdateTransactionLog(this.DrawAcceptingNewTickets.BitCoinAddress);

					// check the transaction of the payouts
					string[] refundAddresses = this.DrawNotAcceptingTicketsAnyMore.TicketLots.Where(tl => (tl.State == TicketLotState.RefundInitiated) || (tl.State == TicketLotState.PrizePaymentInitiated)).Select(tl => tl.RefundAddress).ToArray();
					foreach (string refundAddress in refundAddresses)
					{
						bitcoin.UpdateTransactionLog(refundAddress);
					}

					bitcoin.MatchUpTransactionsAndTicketLots(this.DrawAcceptingNewTickets);

					bitcoin.MatchUpReturnTransactionsAndTicketLots(this.DrawNotAcceptingTicketsAnyMore);
				}

				// check if we need to draw and draw it if it's necessary
				DrawDraw(this.DrawNotAcceptingTicketsAnyMore);
			}
		}

		private void RefreshDrawProperties(bool force)
		{
			lock (drawLockObject)
			{
				if ((refreshDrawPropertiesUpdatedAt.AddMinutes(90) < DateTime.UtcNow) || (force))
				{
					DateTime start = DateTime.UtcNow;

					Draw drawToCheck = null;

					// this is fast
					drawToCheck = Context.Draws.Where(d => (d.DeadlineUtc < DateTime.UtcNow)).OrderByDescending(d => d.DeadlineUtc).First();
					if (this.LastDraw != drawToCheck)
					{
						// this is slow
						this.LastDraw = Context.Draws.Include("TicketLots").Where(d => (d.DeadlineUtc < DateTime.UtcNow)).OrderByDescending(d => d.DeadlineUtc).First();
					}


					drawToCheck = Context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();
					if (this.CurrentDraw != drawToCheck)
					{
						this.CurrentDraw = Context.Draws.Include("TicketLots").Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();
					}

					this.Draws = Context.Draws.OrderByDescending(d => d.DeadlineUtc).ToArray();

					DateTime generationDeadline = DateTime.UtcNow.AddHours(-2);
					drawToCheck = Context.Draws.Where(d => (d.DeadlineUtc < generationDeadline)).OrderByDescending(d => d.DeadlineUtc).First();
					if (this.LastDraw == drawToCheck)
					{
						this.DrawNotAcceptingTicketsAnyMore = this.LastDraw;
					}
					else if (this.DrawNotAcceptingTicketsAnyMore != drawToCheck)
					{
						this.DrawNotAcceptingTicketsAnyMore = Context.Draws.Include("TicketLots").Where(d => (d.DeadlineUtc < generationDeadline)).OrderByDescending(d => d.DeadlineUtc).First();
					}

					drawToCheck = Context.Draws.Where(d => (d.DeadlineUtc > generationDeadline)).OrderBy(d => d.DeadlineUtc).First();
					if (this.CurrentDraw == drawToCheck)
					{
						this.DrawAcceptingNewTickets = this.CurrentDraw;
					}
					else if (this.DrawAcceptingNewTickets != drawToCheck)
					{
						this.DrawAcceptingNewTickets = Context.Draws.Include("TicketLots").Where(d => (d.DeadlineUtc > generationDeadline)).OrderBy(d => d.DeadlineUtc).First();
					}

					double duration = (DateTime.UtcNow - start).TotalSeconds;
					if (duration > 30.0)
					{
						log.Log(LogLevel.Warning, "REFRESHDRAWWARNING", "Running the refresh method for the draws cache took {0} seconds.", duration.ToString("0.00"));
					}

					refreshDrawPropertiesUpdatedAt = DateTime.UtcNow;
				}
			}
		}

		private void InitializePrizePayments(Draw draw)
		{
			foreach (TicketLot tl in draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending)))
			{
				// TODO: make the prize payment initialization automatic
				bitcoin.ChangeTicketLotState(tl, TicketLotState.PrizePaymentInitiated);
			}
		}

		private void GenerateWinningSequence(Draw draw)
		{
			// check if we need to randomize the numbers for the next draw
			draw.ValidGameCount = 0;

			bitcoin.MatchUpTransactionsAndTicketLots(draw);

			// generate the games for all valid tickets
			foreach (TicketLot tl in draw.TicketLots)
			{
				if (tl.State == TicketLotState.PaymentConfirmed)
				{
					foreach (Ticket t in tl.Tickets)
					{
						if (t.Games.Count > 0) continue;

						List<Game> gamesToAdd = new List<Game>();

						if ((t.Mode == TicketMode.Normal) || (t.Mode == TicketMode.Random))
						{
							Game newGame = Context.Games.Create();
							newGame.Ticket = t;
							newGame.Sequence = t.Sequence;
							newGame.SequenceHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(t.Sequence));
							gamesToAdd.Add(newGame);
						}
						else if (t.Mode == TicketMode.System)
						{
							Game[] newGames = GenerateCombinedTicketGames(t);
							gamesToAdd.AddRange(newGames);
						}

						foreach (Game g in gamesToAdd)
						{
							Game[] games = GenerateJokerGames(g);

							foreach (Game gameToAdd in games)
							{
								Context.Games.Add(gameToAdd);
							}
						}
					}

					bitcoin.ChangeTicketLotState(tl, TicketLotState.ConfirmedNotEvaluated);
				}
				else if (tl.State == TicketLotState.WaitingForPayment)
				{
					bitcoin.ChangeTicketLotState(draw, tl.Code, TicketLotState.InvalidTimeUp);
				}
				else if (tl.State == TicketLotState.TooFewConfirmations)
				{
					bitcoin.ChangeTicketLotState(draw, tl.Code, TicketLotState.InvalidConfirmedTooLate);
				}

				if (tl.State == TicketLotState.ConfirmedNotEvaluated)
				{
					foreach (Ticket t in tl.Tickets)
					{
						draw.ValidGameCount += t.Games.Count;
					}
				}
			}

			// store the exchange rates at the time of the winning number generation
			draw.ExchangeRateUSDAtDeadline = GetExchangeRate("BTC", "USD");
			draw.ExchangeRateEURAtDeadline = GetExchangeRate("BTC", "EUR");

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

			draw.WinningTicketSequence = String.Join(",", generatedNumbers) + "|" + generatedJoker;
			draw.WinningTicketGeneratedAt = DateTime.UtcNow;

			// compute hash
			draw.WinningTicketHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(draw.WinningTicketSequence));

			// write to log
			log.Log(LogLevel.Information, "WINNINGTICKETGENERATION", "{0} {1} {2}", draw.WinningTicketSequence, draw.WinningTicketHash, draw.WinningTicketGeneratedAt);

			// calculate pools
			decimal totalDiscountBTC = draw.TicketLots.Where(tl => tl.State == TicketLotState.ConfirmedNotEvaluated).Sum(tl => tl.TotalDiscountBTC);
			//draw.ValidGameCount = draw.TicketLots.Where(tl => tl.State == TicketLotState.ConfirmedNotEvaluated).SelectMany(tl => tl.Tickets).SelectMany(t => t.Games).Count();
			draw.TotalIncomeBTC = (draw.ValidGameCount.Value * draw.OneGameBTC) - totalDiscountBTC;

			try
			{
				decimal[] ratios = draw.PoolRatios.Split(';').Select(r => Decimal.Parse(r)).ToArray();
				decimal[] amountsInPools = new decimal[ratios.Length];

				for (int i = 0; i < ratios.Length; i++)
				{
					amountsInPools[i] = draw.TotalIncomeBTC.Value * ratios[i];
				}

				draw.AmountInPools = String.Join(";", amountsInPools);
			}
			catch
			{
				log.Log(LogLevel.Error, "SETPOOLS", "There was an error while setting the pool sizes for draw '{0}'", draw.DrawCode, draw.PoolRatios);
			}
		}

		private bool EvaluateGames(Draw draw)
		{
			// evaluate games
			var ticketLotsToEvaluate = draw.TicketLots.Where(tl => tl.State == TicketLotState.ConfirmedNotEvaluated);
			if ((draw.WinningTicketSequence != null) && (ticketLotsToEvaluate.Count() > 0))
			{
				string[] seqParts = draw.WinningTicketSequence.Split('|');
				string[] winningNumbers = seqParts[0].Split(',');
				string winningJoker = seqParts[1];

				decimal[] amountsInPools = draw.AmountInPools.Split(';').Select(r => Decimal.Parse(r)).ToArray();

				foreach (TicketLot lot in ticketLotsToEvaluate.ToArray())
				{
					Game[] games = lot.Tickets.SelectMany(t => t.Games).ToArray();

					foreach (Game g in games)
					{
						g.Hits = EvaluateHitCount(winningNumbers, winningJoker, g.Sequence);
					}

					if (
						((amountsInPools[0] > 0) && (games.Count(g => g.Hits == "0+0") > 0))
						|| ((amountsInPools[1] > 0) && (games.Count(g => g.Hits == "0+1") > 0))
						|| ((amountsInPools[2] > 0) && (games.Count(g => g.Hits == "1+0") > 0))
						|| ((amountsInPools[3] > 0) && (games.Count(g => g.Hits == "1+1") > 0))
						|| ((amountsInPools[4] > 0) && (games.Count(g => g.Hits == "2+0") > 0))
						|| ((amountsInPools[5] > 0) && (games.Count(g => g.Hits == "2+1") > 0))
						|| ((amountsInPools[6] > 0) && (games.Count(g => g.Hits == "3+0") > 0))
						|| ((amountsInPools[7] > 0) && (games.Count(g => g.Hits == "3+1") > 0))
						|| ((amountsInPools[8] > 0) && (games.Count(g => g.Hits == "4+0") > 0))
						|| ((amountsInPools[9] > 0) && (games.Count(g => g.Hits == "4+1") > 0))
						|| ((amountsInPools[10] > 0) && (games.Count(g => g.Hits == "5+0") > 0))
						|| ((amountsInPools[11] > 0) && (games.Count(g => g.Hits == "5+1") > 0))
						)
					{
						bitcoin.ChangeTicketLotState(lot, TicketLotState.EvaluatedPrizePaymentPending);
					}
					else
					{
						bitcoin.ChangeTicketLotState(lot, TicketLotState.EvaluatedNotWon);
					}
				}

				log.Log(LogLevel.Error, "GAMESEVAL", "All games were evaluated for draw '{0}'", draw.DrawCode);

				return true;
			}

			return false;
		}

		private bool CalculateHits(Draw draw)
		{
			// calculate hits
			if ((draw.WinningTicketSequence != null) && (String.IsNullOrEmpty(draw.Hits)))
			{
				int[] hits = new int[12];

				var ticketLotsToCount = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon));

				foreach (TicketLot lot in ticketLotsToCount.ToArray())
				{
					Game[] games = lot.Tickets.SelectMany(t => t.Games).ToArray();

					hits[0] += games.Count(g => g.Hits == "0+0");
					hits[1] += games.Count(g => g.Hits == "0+1");
					hits[2] += games.Count(g => g.Hits == "1+0");
					hits[3] += games.Count(g => g.Hits == "1+1");
					hits[4] += games.Count(g => g.Hits == "2+0");
					hits[5] += games.Count(g => g.Hits == "2+1");
					hits[6] += games.Count(g => g.Hits == "3+0");
					hits[7] += games.Count(g => g.Hits == "3+1");
					hits[8] += games.Count(g => g.Hits == "4+0");
					hits[9] += games.Count(g => g.Hits == "4+1");
					hits[10] += games.Count(g => g.Hits == "5+0");
					hits[11] += games.Count(g => g.Hits == "5+1");
				}

				draw.Hits = String.Join(";", hits);

				log.Log(LogLevel.Information, "HITSCOUNT", "All hits were counted for draw '{0}'", draw.DrawCode, draw.Hits);

				return true;
			}

			return false;
		}

		private bool CalculateWinnings(Draw draw)
		{
			// calculate hits
			if ((draw.WinningTicketSequence != null) && (!draw.WinningsBTC.HasValue))
			{
				int[] hits = draw.Hits.Split(';').Select(h => Int32.Parse(h)).ToArray();
				decimal[] amounts = draw.AmountInPools.Split(';').Select(a => Decimal.Parse(a)).ToArray();

				decimal[] amountsToWin = new decimal[12];
				for (int i = 0; i < amountsToWin.Length; i++)
				{
					if (hits[i] > 0)
					{
						amountsToWin[i] = Math.Floor((amounts[i] / hits[i]) * 1000) / 1000;
					}
				}

				var allGames = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).SelectMany(t => t.Games).ToArray();
				foreach (Game game in allGames)
				{
					game.WinningsBTC = amountsToWin[IndexOfHits(game.Hits)];
				}

				var allTickets = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).ToArray();
				foreach (Ticket ticket in allTickets)
				{
					ticket.WinningsBTC = ticket.Games.Sum(g => g.WinningsBTC);
				}

				var allTicketLots = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).ToArray();
				foreach (TicketLot ticketLot in allTicketLots)
				{
					ticketLot.WinningsBTC = ticketLot.Tickets.Sum(t => t.WinningsBTC);
				}

				draw.WinningsBTC = draw.TicketLots.Sum(tl => tl.WinningsBTC);

				log.Log(LogLevel.Information, "WINNINGS", "All winnings were calculated for draw '{0}' and its lots, tickets and games.", draw.DrawCode, draw.WinningsBTC);

				return true;
			}

			return false;
		}

		private void GenerateAndSendReport(Draw draw)
		{
			var allTicketLots = Context.TicketLots.Include("MostRecentTransactionLog").Where(tl => tl.Draw.DrawId == draw.DrawId).ToArray();
			var validTickets = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).ToArray();
			var validGames = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).SelectMany(t => t.Games).ToArray();

			// send e-mail with the results
			email.Send("TEST", "en-US", email.AdminEmails, null, new EmailTemplateModelTEST { Draw = draw, TicketLots = allTicketLots, ValidTickets = validTickets, ValidGames = validGames });
		}

		private int IndexOfHits(string hits)
		{
			switch (hits)
			{
				case "0+0":
					return 0;
				case "0+1":
					return 1;
				case "1+0":
					return 2;
				case "1+1":
					return 3;
				case "2+0":
					return 4;
				case "2+1":
					return 5;
				case "3+0":
					return 6;
				case "3+1":
					return 7;
				case "4+0":
					return 8;
				case "4+1":
					return 9;
				case "5+0":
					return 10;
				case "5+1":
					return 11;
				default:
					throw new Exception("Invalid hit string '" + hits + "' was passed to the IndexOfHits function.");
			}
		}

		private string EvaluateHitCount(string[] winningNumbers, string winningJoker, string sequence)
		{
			string[] seqParts = sequence.Split('|');
			string[] numbers = seqParts[0].Split(',');
			string joker = seqParts[1];

			return numbers.Count(n => winningNumbers.Contains(n)) + "+" + (joker == winningJoker ? "1" : "0");

		}

		private Game[] GenerateCombinedTicketGames(Ticket t)
		{
			List<Game> result = new List<Game>();
			List<string> binaryCodes = new List<string>();

			if (t.Mode != TicketMode.System) return null;

			string[] numbers = t.Sequence.Substring(0, t.Sequence.IndexOf('|')).Split(',');
			string luckyNumber = t.Sequence.Substring(t.Sequence.IndexOf('|'));
			string formatter = new String('0', numbers.Length);

			for (int i = 0; i < Math.Pow(2, numbers.Length); i++)
			{
				string binary = Int64.Parse(Convert.ToString(i, 2)).ToString(formatter);
				if (binary.Count(c => c == '1') == 5)
				{
					binaryCodes.Add(binary);
				}
			}

			foreach (string binaryCode in binaryCodes)
			{
				Game newGame = Context.Games.Create();
				newGame.Ticket = t;
				newGame.Sequence = "";

				for (int i = 0; i < numbers.Length; i++)
				{
					if (binaryCode[numbers.Length - i - 1] == '1')
					{
						newGame.Sequence += numbers[i] + ",";
					}
				}

				newGame.Sequence = newGame.Sequence.Substring(0, newGame.Sequence.Length - 1) + luckyNumber;
				newGame.SequenceHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(t.Sequence));

				result.Add(newGame);
			}

			return result.ToArray();
		}

		private Game[] GenerateJokerGames(Game g)
		{
			string numbers = g.Sequence.Substring(0, g.Sequence.IndexOf('|'));
			string[] luckyNumber = g.Sequence.Substring(g.Sequence.IndexOf('|')).Split(',');

			if (luckyNumber.Length == 1)
			{
				return new Game[] { g };
			}

			List<Game> result = new List<Game>();
			foreach (string ln in luckyNumber)
			{
				Game ng = Context.Games.Create();

				ng.Sequence = numbers + "|" + ln;
				ng.SequenceHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(ng.Sequence));
				ng.Ticket = g.Ticket;
				result.Add(ng);
			}

			return result.ToArray();
		}

		public ExchangeRate GetExchangeRate(string currencyISO1, string currencyISO2)
		{
			ExchangeRate result = null;

			if (currencyISO1 == currencyISO2)
			{
				result = new ExchangeRate();

				result.TimeUtc = DateTime.UtcNow;
				result.CurrencyISO1 = currencyISO1;
				result.CurrencyISO2 = currencyISO2;
				result.Rate = 1.0M;

				return result;
			}

			ExchangeRate lastExrate = Context.ExchangeRates.Where(er => (er.CurrencyISO1 == currencyISO1) && (er.CurrencyISO2 == currencyISO2)).OrderByDescending(er => er.TimeUtc).FirstOrDefault();
			if ((lastExrate == null) || (lastExrate.TimeUtc.AddMinutes(15) < DateTime.UtcNow))
			{
				ExchangeRate exrate = Context.ExchangeRates.Create();

				lock (Context)
				{
					try
					{
						string url = "http://data.mtgox.com/api/1/" + currencyISO1 + currencyISO2 + "/ticker";
						HttpWebRequest request = HttpWebRequest.CreateHttp(url);
						request.Timeout = 2000;
						WebResponse response = null;

						try
						{
							log.Log(LogLevel.Debug, "REQUEST", "URL:'{0}'", url);
							response = request.GetResponse();
							log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE LENGTH:'{1}'", url, response.ContentLength);

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

							if (exrate.TimeUtc != DateTime.MinValue)
							{
								Context.ExchangeRates.Add(exrate);
								Context.SaveChanges();
							}
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

		public Draw GetDraw(string drawCode)
		{
			return this.Draws.FirstOrDefault(d => d.DrawCode == drawCode);
		}

		public TicketLot[] GetTicketLot(string ticketLotCode)
		{
			return Context.TicketLots.Include("Draw").Where(tl => tl.Code == ticketLotCode).OrderBy(tl => tl.CreatedUtc).ToArray();
		}

		public TicketLot GetTicketLot(int ticketLotId)
		{
			return Context.TicketLots.Include("Draw").First(tl => tl.TicketLotId == ticketLotId);
		}

		public void SaveTicketLot(TicketLot tl)
		{
			lock (Context)
			{
				//string codeMap = "ABCDEFGHIJKLMOPQRSTUVWXYZ0123456789";
				string codeMap = "0123456789";

				if (String.IsNullOrEmpty(tl.Code))
				{
					Random rnd = new Random();
					do
					{
						tl.Code = "TL" + (tl.Draw.DrawId % 100).ToString("00") + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)] + codeMap[rnd.Next(codeMap.Length)];
					} while (Context.TicketLots.FirstOrDefault(l => (l.Code == tl.Code) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);

					do
					{
						tl.TotalDiscountBTC = BitCoinService.OneSatoshi * rnd.Next(10000);
					} while (Context.TicketLots.FirstOrDefault(l => (l.TotalDiscountBTC == tl.TotalDiscountBTC) && (l.Draw.DrawId == tl.Draw.DrawId)) != null);
				}

				if (tl.State == TicketLotState.NotSet)
				{
					tl.State = TicketLotState.WaitingForPayment;
				}

				if (tl.Tickets != null)
				{
					foreach (Ticket t in tl.Tickets)
					{
						t.SequenceHash = new SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(t.Sequence));
					}
				}

				Context.TicketLots.Add(tl);
				Context.SaveChanges();
			}

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
			Draw draw = Context.Draws.Include("ExchangeRateUSDAtDeadline").Include("ExchangeRateEURAtDeadline").Where(d => d.DrawCode == "DRW2013-010").First();
			TicketLot[] allTicketLots = Context.TicketLots.Include("MostRecentTransactionLog").Where(tl => tl.Draw.DrawId == draw.DrawId).ToArray();
			Ticket[] validTickets = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).ToArray();
			Game[] validGames = draw.TicketLots.Where(tl => (tl.State == TicketLotState.EvaluatedPrizePaymentPending) || (tl.State == TicketLotState.EvaluatedNotWon)).SelectMany(tl => tl.Tickets).SelectMany(t => t.Games).OrderBy(g => g.Ticket.TicketLot.Code).OrderBy(g => g.Sequence).OrderByDescending(g => g.Hits).ToArray();


			EmailTemplateModelTEST model = new EmailTemplateModelTEST { Draw = draw, TicketLots = allTicketLots, ValidTickets = validTickets, ValidGames = validGames };

			email.Send(templateName, "en-US", null, null, model);

			return model;
		}

		public User GetUser(string sessionId)
		{
			User result = Context.Users.FirstOrDefault(u => u.SessionId == sessionId);

			if (result == null)
			{
				result = Context.Users.Create();
				result.SessionId = sessionId;
			}

			return result;
		}

		public void SetUserEmail(string sessionId, string email)
		{
			User user = Context.Users.FirstOrDefault(u => u.SessionId == sessionId);
			user.Email = email;
			Context.SaveChanges();
		}

		public TicketLot CloneTicketLot(TicketLot tl)
		{
			TicketLot result = new TicketLot(); //Context.TicketLots.Create();
			result.CreatedUtc = DateTime.UtcNow;
			result.Code = tl.Code;
			result.Draw = tl.Draw;
			result.MostRecentTransactionLog = tl.MostRecentTransactionLog;
			result.Owner = tl.Owner;
			result.RefundAddress = tl.RefundAddress;
			result.SecondChanceParticipant = tl.SecondChanceParticipant;
			result.SecondChanceWinner = tl.SecondChanceWinner;
			result.State = tl.State;
			//result.TotalBTC = tl.TotalBTC;
			//result.TotalDiscountBTC = tl.TotalDiscountBTC;

			lock (Context)
			{
				foreach (Ticket t in tl.Tickets)
				{
					Ticket tClone = CloneTicket(t);
					tClone.TicketLot = result;
					//result.Tickets.Add(tClone);
					Context.Tickets.Add(tClone);
				}
			}

			return result;
		}

		private Ticket CloneTicket(Ticket t)
		{
			Ticket result = Context.Tickets.Create();

			result.CreatedUtc = DateTime.UtcNow;
			result.Index = t.Index;
			result.Mode = t.Mode;
			result.Type = t.Type;
			result.Sequence = t.Sequence;
			result.SequenceHash = t.SequenceHash;
			result.TicketLot = t.TicketLot;

			return result;
		}

		public Draw CloneDraw(string drawCode, string newDrawCode, DateTime newDeadlineUtc)
		{
			Draw d = Context.Draws.First(drw => drw.DrawCode == drawCode);

			Draw result = Context.Draws.Create();
			result.DeadlineUtc = d.DeadlineUtc;
			result.DrawCode = d.DrawCode;
			result.BitCoinAddress = d.BitCoinAddress;
			result.JackpotBTC = d.JackpotBTC;
			result.OneGameBTC = d.OneGameBTC;
			result.PoolRatios = d.PoolRatios;

			result.DrawCode = newDrawCode;
			result.DeadlineUtc = newDeadlineUtc;

			Context.Draws.Add(result);
			Context.SaveChanges();

			RefreshDrawProperties(true);

			return result;
		}

		public bool DrawDraw(Draw draw)
		{
			if ((draw.WinningTicketSequence == null) && bitcoin.UpdateTransactionLog(draw.BitCoinAddress))
			{
				draw = Context.Draws.Include("TicketLots").Include("TicketLots.Tickets").Include("TicketLots.Tickets.Games").Where(d => d.DrawCode == draw.DrawCode).First();

				Context.Configuration.AutoDetectChangesEnabled = false;


				GenerateWinningSequence(draw);

				EvaluateGames(draw);

				CalculateHits(draw);

				CalculateWinnings(draw);
				
				GenerateAndSendReport(draw);

				InitializePrizePayments(draw);

				Context.ChangeTracker.DetectChanges();
				Context.SaveChanges();
				Context.Configuration.AutoDetectChangesEnabled = true;

				RefreshDrawProperties(true);

				return true;
			}

			return false;
		}

		public void MoveAndConfirmAllTicketLotsForTesting(Draw d, string userSessionId)
		{
			User u = LotteryService.Instance.GetUser(userSessionId);
			TicketLot[] ticketLots = u.TicketLots.ToArray();
			foreach (TicketLot tl in ticketLots)
			{
				tl.Draw = d;

				tl.State = TicketLotState.PaymentConfirmed;
				tl.RefundAddress = "1PNGexJ4kbZpCwr8v8Cm1vS1JgeqdZW8co";
				//tl.MostRecentTransactionLog = 
			}

			Context.SaveChanges();
		}

		public void Initialize(System.Web.HttpContextBase httpContext, bool startTimer)
		{
			if (context == null)
			{
				email.HttpContext = httpContext;

				lock (contextLockObject)
				{
					if (timer != null)
					{
						timer.Stop();
					}

					context = new LotteryDbContext();

					if (startTimer)
					{
						timer = new Timer(60 * 1000);
						timer.Elapsed += LotteryServiceTimerElapsed;
						timer.Start();
						LotteryServiceTimerElapsed(timer, null);
					}
				}
			}
		}
	}
}
