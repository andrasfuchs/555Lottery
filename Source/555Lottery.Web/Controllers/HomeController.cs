using _555Lottery.DataModel;
using _555Lottery.Service;
using _555Lottery.Web.Models;
using _555Lottery.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace _555Lottery.Web.Controllers
{
	public class HomeController : Controller
	{
		private new System.Web.SessionState.HttpSessionState Session 
		{
			get
			{
				return System.Web.HttpContext.Current.Session;
			}
		}

		private static TimeSpan[] delaySteps = new TimeSpan[] { 
			new TimeSpan(2, 0, 0), new TimeSpan(1, 0, 0), new TimeSpan(0, 45, 0), new TimeSpan(0, 30, 0), new TimeSpan(0, 20, 0), new TimeSpan(0, 15, 0), 
			new TimeSpan(0, 10, 0), new TimeSpan(0, 5, 0), new TimeSpan(0, 3, 0)
		};

		private static string[] delayStepNamesEng = new string[] {
			"2 hours", "1 hour", "45 minutes", "30 minutes", "20 minutes", "15 minutes", "10 minutes", "5 minutes", "3 minutes"
		};

		private TicketLotViewModel tickets
		{
			get
			{
				if (Session["Tickets"] == null)
				{
					Session["Tickets"] = new TicketLotViewModel(this.Session.SessionID, AutoMapper.Mapper.Map<DrawViewModel>(LotteryService.Instance.CurrentDraw));
				}

				return (TicketLotViewModel)Session["Tickets"];
			}

			set
			{
				Session["Tickets"] = value;
			}
		}

		public void Initialize()
		{
			//HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

			LotteryService.Instance.Initialize(this.HttpContext, true);
		}

		[HttpPost]
		public ActionResult Jackpot(string currency)
		{
			Initialize();

			string currencySign = "";

			switch (currency)
			{
				case "USD":
					currencySign += "$";
					break;
				
				case "EUR":
					currencySign += "€";
					break;

				case "BTC":
					currencySign += "฿";
					break;
			}

			decimal jackpotToDisplay = Math.Min(19999999, LotteryService.Instance.CurrentDraw.JackpotBTC * LotteryService.Instance.GetExchangeRate("BTC", currency).Rate);

			char[] jackpot = String.Format(currencySign + "{0:n0}", jackpotToDisplay).ToCharArray();
			// we need to clean up the jackpot amount to look nice
			int oneCounter = 0;
			for (int i = 0; i < jackpot.Length; i++)
			{
				if (jackpot[i] == '1')
				{
					oneCounter++;

					if (oneCounter > 2)
					{
						jackpot[i] = '0';
					}
				}
			}

			return PartialView("_Jackpot", new String(jackpot));
		}

		public ActionResult Index()
		{
			Initialize();

			tickets = null;
			var initializeTickets = tickets; // this is needed because of the HTML rendering

			string lastDrawText = LotteryService.Instance.LastDraw.WinningTicketSequence;

			if (String.IsNullOrEmpty(lastDrawText))
			{
				int delayIndex = 0;
				TimeSpan delayTimeSpan = delaySteps[0];

				while ((delayIndex + 1 < delaySteps.Length) && (LotteryService.Instance.LastDraw.DeadlineUtc.Add(delayTimeSpan) - DateTime.UtcNow < delaySteps[delayIndex + 1]))
				{
					delayIndex++;
				}

				
				lastDrawText = "Please wait until we get the new winners in approximately " + delayStepNamesEng[delayIndex] + "...";
			}

			return View(new string[] { null, lastDrawText, LotteryService.Instance.LastDraw.DrawCode });
		}

		[HttpPost]
		public ActionResult TicketBottom(string text)
		{
			if (String.IsNullOrEmpty(text))
			{
				text = ",,,,|";
			}

			string[] parts = text.Split('|');
			string[] numbers = parts[0].Split(',');
			string[] jokers = parts[1].Split(',');

			StringBuilder sb = new StringBuilder();

			for (int j = 0; j < numbers.Length; j++)
			{
				int i;

				if (Int32.TryParse(numbers[j], out i))
				{
					if (i < 10)
					{
						sb.Append(' ');
						sb.Append(i);
					}
					else
					{
						sb.Append(i);
					}
				}
				else
				{
					sb.Append("  ");
				}

				if (j != numbers.Length-1)
				{
					sb.Append(',');
				}
			}

			sb.Append('|');

			for (int j = 0; j < jokers.Length; j++)
			{
				int i;

				if (Int32.TryParse(jokers[j], out i))
				{		
					sb.Append(i);
					sb.Append(',');
				}
			}

			while (sb[sb.Length - 1] == ',')
			{
				sb.Remove(sb.Length - 1, 1);
			}

			sb.Replace("|", " | ");

			return PartialView("_TicketBottom", sb.ToString());
		}

		[HttpPost]
		public ActionResult TicketPrice(int ticketLotId, int ticketIndex, string ticketType, string ticketSequence)
		{
			TicketViewModel ticket = null;

			if (ticketLotId == 0)
			{
				ticket = new TicketViewModel(LotteryService.Instance.CurrentDraw.OneGameBTC, ticketType, ticketSequence);
				
				return PartialView("_TicketPrice", ticket.Price.ToString("0.0000"));
			}
			else
			{
				if (Session["Tickets#" + ticketLotId] == null)
				{
					Session["Tickets#" + ticketLotId] = AutoMapper.Mapper.Map<TicketLotViewModel>(LotteryService.Instance.GetTicketLot(ticketLotId));
				}

				TicketLotViewModel tl = (TicketLotViewModel)Session["Tickets#" + ticketLotId];
				ticket = tl.Tickets.First(t => t.Index == ticketIndex);

				string winnings = "-.--";
				if (ticket.WinningsBTC.HasValue)
				{
					if (ticket.WinningsBTC.Value < 1.0M)
					{
						winnings = ticket.WinningsBTC.Value.ToString("0.0000");
					} else {
						winnings = ticket.WinningsBTC.Value.ToString("0.00");
					} 
				}

				return PartialView("_TicketPrice", winnings);
			}
		}

		[HttpPost]
		public ActionResult TotalGames()
		{
			return PartialView("_InfoBoxNumber", tickets.TotalGames.ToString("0"));
		}

		[HttpPost]
		public ActionResult Draws(int valueChange)
		{
			tickets.DrawNumber = Math.Min(26, Math.Min(LotteryService.Instance.DrawsRemaining, Math.Max(1, tickets.DrawNumber + valueChange)));

			return PartialView("_InfoBoxNumber", tickets.DrawNumber.ToString("0"));
		}

		[HttpPost]
		public ActionResult TotalPrice()
		{
			tickets.TotalBTC = tickets.Tickets.Sum(t => t.Price) * tickets.DrawNumber;

			return PartialView("_TotalPrice", tickets.TotalBTC.ToString("0.0000"));
		}

		[HttpPost]
		public JsonResult AcceptTicket(string ticketType, string ticketSequence, int overwriteTicketIndex)
		{
			TicketViewModel newTicket = new TicketViewModel(tickets.Draw.OneGameBTC, ticketType, ticketSequence);
			
			if ((newTicket.Mode == TicketMode.Random) && (newTicket.Type != 0))
			{
				GenerateRandomTickets(newTicket.Mode, newTicket.Type, newTicket.Numbers, newTicket.Jokers);
			}
			else
			{
				if (overwriteTicketIndex == -1)
				{
					tickets.AppendTicket(newTicket);
				}
				else
				{
					tickets.ReplaceTicket(overwriteTicketIndex, newTicket);
				}
			}

			LotteryService.Instance.Log(LogLevel.Information, "CLICKACCEPT", "{0}: user clicked accept button", new SessionInfo(null, Session.SessionID));

			return Json(new int[2] { overwriteTicketIndex == -1 ? -1 : newTicket.Index, tickets.TotalGames });
		}

		[HttpPost]
		public JsonResult DeleteTicket(int ticketIndex)
		{
			int nextTicketIndex = tickets.DeleteTicket(ticketIndex);

			LotteryService.Instance.Log(LogLevel.Information, "CLICKCLEAR", "{0}: user clicked clear button", new SessionInfo(null, Session.SessionID));

			return Json(new int[2] { nextTicketIndex, tickets.TotalGames });
		}


		[HttpPost]
		public int GetTimeLeftToNextDraw()
		{
			if (LotteryService.Instance.CurrentDraw == null)
			{
				return 0;
			}

			return (int)((LotteryService.Instance.CurrentDraw.DeadlineUtc - DateTime.UtcNow).TotalSeconds);
		}

		[HttpPost]
		public ActionResult TimeLeft(int secondsToNextDraw)
		{
			return PartialView("_TimeLeft", secondsToNextDraw);
		}

		[HttpPost]
		public ActionResult TicketSidebar(int ticketLotId, bool showSidebarSelectors)
		{
			TicketLotViewModel tl = null;
			if (ticketLotId == 0)
			{
				tl = tickets;
				tl.ShowSelectors = false;
			}
			else
			{
				tl = (TicketLotViewModel)Session["Tickets#" + ticketLotId];
				tl.ShowSelectors = true;
			}

			return PartialView("_TicketSidebar", tl);
		}

		[HttpPost]
		public JsonResult MoveSidebar(int ticketLotId, int scrollPositionChange, int selectedTicketIndex)
		{
			TicketLotViewModel tl = null;
			if (ticketLotId == 0)
			{
				tl = tickets;
				tl.ShowSelectors = false;
			}
			else
			{
				if (Session["Tickets#" + ticketLotId] == null)
				{
					Session["Tickets#" + ticketLotId] = AutoMapper.Mapper.Map<TicketLotViewModel>(LotteryService.Instance.GetTicketLot(ticketLotId));
				}

				tl = (TicketLotViewModel)Session["Tickets#" + ticketLotId];
				tl.ShowSelectors = true;
			}

			if (selectedTicketIndex != 0)
			{
				tl.SelectedIndex = tl.Tickets.IndexOf(tl.Tickets.First(t => t.Index == selectedTicketIndex));
			}

			tl.ScrollPosition += scrollPositionChange;

			if (scrollPositionChange == 0)
			{
				if (tl.ScrollPosition + 4 < tl.SelectedIndex) tl.ScrollPosition = tl.SelectedIndex - 4;
				if (tl.ScrollPosition > tl.SelectedIndex) tl.ScrollPosition = tl.SelectedIndex;
			}

			if (tl.ScrollPosition + 5 > tl.Tickets.Count) tl.ScrollPosition = tl.Tickets.Count - 5;
			if (tl.ScrollPosition < 0) tl.ScrollPosition = 0;

			return Json(tl.SelectedTicket, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Stats()
		{
			DrawViewModel[] draws = AutoMapper.Mapper.Map<DrawViewModel[]>(LotteryService.Instance.Draws);

			decimal btcusd = LotteryService.Instance.GetExchangeRate("BTC", "USD").Rate;
			decimal btceur = LotteryService.Instance.GetExchangeRate("BTC", "EUR").Rate;

			foreach (DrawViewModel d in draws)
			{
				d.JackpotUSD = d.JackpotBTC * btcusd;
				d.JackpotEUR = d.JackpotBTC * btceur;
			}

			return View(draws);
		}

		public ActionResult Draw(string id)
		{
			Initialize();

			//DrawViewModel draw = AutoMapper.Mapper.Map<DrawViewModel>(LotteryService.Instance.GetDraw(id));
			DrawViewModel draw = null;
			List<DrawViewModel> draws = new List<DrawViewModel>(AutoMapper.Mapper.Map<DrawViewModel[]>(LotteryService.Instance.Draws));
			List<DrawViewModel> drawsToRemove = new List<DrawViewModel>();

			foreach (DrawViewModel d in draws)
			{
				if (d.DrawCode == id)
				{
					draw = d;
					continue;
				}

				if (d.WinningTicketSequence == null)
				{
					drawsToRemove.Add(d);
				}
			}

			foreach (DrawViewModel d in drawsToRemove)
			{
				draws.Remove(d);
			}

			if (draw != null)
			{
				draws.Remove(draw);
				draws.Insert(0, draw);

				decimal btcusd = LotteryService.Instance.GetExchangeRate("BTC", "USD").Rate;
				decimal btceur = LotteryService.Instance.GetExchangeRate("BTC", "EUR").Rate;

				draw.JackpotUSDAtDeadline = btcusd;
				draw.JackpotEURAtDeadline = btceur;

				draw.JackpotUSD = draw.JackpotBTC * btcusd;
				draw.JackpotEUR = draw.JackpotBTC * btceur;
			}

			return View(draws.ToArray());
		}

		public ActionResult Check(string id)
		{
			Initialize();

			TicketLotViewModel[] tlVM = AutoMapper.Mapper.Map<TicketLotViewModel[]>(LotteryService.Instance.GetTicketLot(id));
			tlVM.All(tl => tl.ShowSelectors = true);

			return View(tlVM);
		}


		private void GenerateRandomTickets(TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			if (mode != TicketMode.Random) throw new LotteryException("Ticket mode must be Random to use this function!");

			int iterationNumber = 1;
			switch (type)
			{
				case 1:
					iterationNumber = 3;
					break;
				case 2:
					iterationNumber = 5;
					break;
				case 3:
					iterationNumber = 7;
					break;
				case 4:
					iterationNumber = 10;
					break;
				case 5:
					iterationNumber = 15;
					break;
			}

			Random rnd = new Random();

			for (int i = 0; i < iterationNumber; i++)
			{
				int[] generatedNumbers = new int[5];

				Array.Copy(numbers, generatedNumbers, numbers.Length);

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

				int[] generatedJokers = null;
				if (jokers.Length > 0)
				{
					generatedJokers = jokers;
				}
				else
				{
					generatedJokers = new int[1] { rnd.Next(5) + 1 };
				}

				TicketViewModel newTicket = new TicketViewModel(tickets.Draw.OneGameBTC, mode, 0, generatedNumbers, generatedJokers);
				tickets.AppendTicket(newTicket);
			}
		}

		[HttpPost]
		public JsonResult LetsPlay()
		{
			tickets.TotalBTC = tickets.Tickets.Sum(t => t.Price) * tickets.DrawNumber;

			TicketLot tlToSave = AutoMapper.Mapper.Map<TicketLot>(tickets);
			tlToSave.Draw = null;
			tlToSave.Owner = LotteryService.Instance.GetUser(Session.SessionID);

			Ticket[] ticketsToDiscard = tlToSave.Tickets.Where(t => t.Mode == TicketMode.Empty).ToArray();
			foreach (Ticket t in ticketsToDiscard)
			{
				tlToSave.Tickets.Remove(t);
			}

			// duplicate ticketlot for every draw
			Draw[] draws = LotteryService.Instance.Draws;
			int currentIndex = Array.FindIndex<Draw>(draws, d => d.DrawId == LotteryService.Instance.CurrentDraw.DrawId);
			
			for (int i = 0; i < tickets.DrawNumber; i++)
			{
				TicketLot tlToSaveClone = LotteryService.Instance.CloneTicketLot(tlToSave);
				tlToSaveClone.Draw = draws[currentIndex - i];
				if (i == 0)
				{
					tlToSaveClone.TotalBTC = tlToSave.TotalBTC;
				}

				LotteryService.Instance.SaveTicketLot(tlToSaveClone);
				
				if (i == 0)
				{
					tlToSave.Code = tlToSaveClone.Code;
					tlToSave.TotalDiscountBTC = tlToSaveClone.TotalDiscountBTC;
				}
			}
			

			tickets.Code = tlToSave.Code;
			tickets.TotalBTC = tickets.Tickets.Sum(t => t.Price) * tickets.DrawNumber;
			tickets.TotalDiscountBTC = tlToSave.TotalDiscountBTC;

			TicketLotViewModel newTL = new TicketLotViewModel(this.Session.SessionID, AutoMapper.Mapper.Map<DrawViewModel>(LotteryService.Instance.CurrentDraw));
			foreach (TicketViewModel t in tickets.Tickets)
			{
				if (t.Mode == TicketMode.Empty) continue;

				TicketViewModel newTicket = new TicketViewModel(newTL.Draw.OneGameBTC, t.Mode, t.Type, t.Numbers, t.Jokers);
				newTL.AppendTicket(newTicket);	
			}
			newTL.DrawNumber = tickets.DrawNumber;
			Session["TicketLot"] = tickets;
			tickets = newTL;

			LotteryService.Instance.Log(LogLevel.Information, "CLICKLETSPLAY", "{0}: user clicked let's play button ({1})", new SessionInfo(null, Session.SessionID), tlToSave.Code);

			return Json((TicketLotViewModel)Session["TicketLot"], JsonRequestBehavior.AllowGet);
		}


		public ActionResult EmailTemplateTest()
		{
			_555Lottery.Service.TemplateModels.EmailTemplateModelTEST model = LotteryService.Instance.DoEmailTemplateTest("TEST") as _555Lottery.Service.TemplateModels.EmailTemplateModelTEST;

			return View(model);
		}


		[HttpPost]
		public void PageOpened(string url)
		{
			LotteryService.Instance.Log(LogLevel.Information, "PAGEOPENED", "{0}: page '{1}' was opened", new SessionInfo(null, Session.SessionID), url);
		}

		[HttpPost]
		public void PageLeft(string url)
		{
			LotteryService.Instance.Log(LogLevel.Information, "PAGELEFT", "{0}: page '{1}' was left", new SessionInfo(null, Session.SessionID), url);
		}

		[HttpPost]
		public void TabChanged(string changedTo)
		{
			string tabHeader = changedTo;

			if (changedTo == "blue")
			{
				tabHeader = "normal ticket";
			}
			else if (changedTo == "orange")
			{
				tabHeader = "system ticket";
			}
			else if (changedTo == "green")
			{
				tabHeader = "random ticket";
			}

			LotteryService.Instance.Log(LogLevel.Information, "TABCHANGED", "{0}: user changed to tab '{1}'", new SessionInfo(null, Session.SessionID), tabHeader);
		}

		[HttpPost]
		public void RandomClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKRANDOM", "{0}: user clicked RANDOM button", new SessionInfo(null, Session.SessionID));
		}
		
		[HttpPost]
		public void PayClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKPAY", "{0}: user clicked PAY button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TryoutClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKTRYOUT", "{0}: user clicked TRY OUT button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void ClientYesClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKCLIENTYES", "{0}: user clicked CLIENT YES button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void ClientNoClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKCLIENTNO", "{0}: user clicked CLIENT NO button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void ClientSkipClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKCLIENTSKIP", "{0}: user clicked CLIENT SKIP button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void DoneClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKDONE", "{0}: user clicked DONE button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void CloseClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKCLOSE", "{0}: user clicked CLOSE button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void NextClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CLICKNEXT", "{0}: user clicked NEXT button", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void CheckoutStep1Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CHECKOUT1CLOSED", "{0}: user closed the checkout step 1 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void CheckoutStep2Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CHECKOUT2CLOSED", "{0}: user closed the checkout step 2 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void CheckoutStep3Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CHECKOUT3CLOSED", "{0}: user closed the checkout step 3 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void CheckoutQuestionClosed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "CHECKOUTQUESTIONCLOSED", "{0}: user closed the checkout question modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void EmailEntered(string email)
		{
			string emailAddress = email.ToLower();

			// check if the e-mail is valid
			bool isValidEmail = IsValidEmail(emailAddress);
			if (isValidEmail)
			{
				LotteryService.Instance.SetUserEmail(Session.SessionID, emailAddress);

				Response.Cookies.Add(new HttpCookie("#notificationemail", emailAddress));
			}

			LotteryService.Instance.Log(LogLevel.Information, "EMAILENTERED", "{0}: user entered their e-mail address '{1}'", new SessionInfo(null, Session.SessionID), emailAddress, isValidEmail);
		}

		[HttpPost]
		public void NameEntered(string name)
		{
			LotteryService.Instance.SetUserName(Session.SessionID, name);

			Response.Cookies.Add(new HttpCookie("#username", name));

			LotteryService.Instance.Log(LogLevel.Information, "USERNAMEENTERED", "{0}: user entered their name '{1}'", new SessionInfo(null, Session.SessionID), name);
		}

		[HttpPost]
		public bool AddressEntered(string address)
		{
			bool result = false;

			bool isValidAddress = System.Text.RegularExpressions.Regex.IsMatch(address, @"^([0-9a-zA-Z]{27,34})$");
			if ((!String.IsNullOrEmpty(address)) && (isValidAddress))
			{
				LotteryService.Instance.SetUserReturnBitcoinAddress(Session.SessionID, address);

				TicketLotViewModel tl = (TicketLotViewModel)Session["TicketLot"];
				LotteryService.Instance.SetTicketLotRefundAddress(tl.Code, address);

				result = true;
			}

			LotteryService.Instance.Log(LogLevel.Information, "RETURNADDRESSENTERED", "{0}: user entered their return address '{1}'", new SessionInfo(null, Session.SessionID), address, isValidAddress);

			return result;
		}

		[HttpPost]
		public void BitcoinWalletSelected(string wallet)
		{
			LotteryService.Instance.Log(LogLevel.Information, "BITCOINWALLETSELECTED", "{0}: user selected '{1}' as their wallet", new SessionInfo(null, Session.SessionID), wallet);
		}

		private bool IsValidEmail(string emailaddress)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(emailaddress, @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
		}

		public JsonResult CheckHash(string sequence, string hash)
		{
			string computedHash = BitConverter.ToString(new System.Security.Cryptography.SHA256Managed().ComputeHash(ASCIIEncoding.ASCII.GetBytes(sequence)));

			if (computedHash == hash)
			{
				return Json("ok", JsonRequestBehavior.AllowGet);
			}
			else
			{
				return Json("invalid", JsonRequestBehavior.AllowGet);
			}
		}

		[HttpPost]
		public string GetUtcTimeStr()
		{
			return @DateTime.UtcNow.ToString("MMM.dd. HH:mm").ToUpper();
		}

		public ActionResult Terms() 
		{
			return View();
		}

		public ActionResult ExportClicked()
		{
			TicketLotViewModel tl = (TicketLotViewModel)Session["TicketLot"];

			MemoryStream documentStream = new MemoryStream();

			StreamWriter sw = new StreamWriter(documentStream);
			sw.WriteLine("555 Lottery");
			sw.WriteLine("===========");
			sw.WriteLine();
			sw.WriteLine("Generated at: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)");
			sw.WriteLine("Draw: " + tl.Draw.DrawCode);
			sw.WriteLine("Your Ticket ID: " + tl.Code);
			sw.WriteLine("Target address: " + tl.Draw.BitCoinAddress);
			sw.WriteLine("Amount to pay: " + (tl.TotalBTC - tl.TotalDiscountBTC).ToString("0.00000000") + " BTC");
			sw.WriteLine("Deadline: " + tl.Draw.DeadlineUtc.ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)");
			sw.WriteLine("");
			sw.WriteLine("Check your ticket any time at:");
			sw.WriteLine("http://www.555lottery.com/check/" + tl.Code);
			sw.Flush();


			LotteryService.Instance.Log(LogLevel.Information, "CLICKSAVETXT", "{0}: user clicked SAVE TXT button", new SessionInfo(null, Session.SessionID));


			documentStream.Position = 0;

			return new FileStreamResult(documentStream, "text/plain")
			{
				FileDownloadName = "555lottery_" + tl.Code + ".txt"
			};
		}

		[HttpPost]
		public void TutorialStep1NextClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL1NEXT", "{0}: user clicked the next button in tutorial step 1 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep2PrevClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL2PREV", "{0}: user clicked the previous button in tutorial step 2 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep2NextClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL2NEXT", "{0}: user clicked the next button in tutorial step 2 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep3PrevClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL3PREV", "{0}: user clicked the previous button in tutorial step 3 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep3OkClicked()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL3OK", "{0}: user clicked the ok button in tutorial step 3 modal window", new SessionInfo(null, Session.SessionID));
		}
				
		[HttpPost]
		public void TutorialStep1Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL1CLOSED", "{0}: user closed the tutorial step 1 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep2Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL2CLOSED", "{0}: user closed the tutorial step 2 modal window", new SessionInfo(null, Session.SessionID));
		}

		[HttpPost]
		public void TutorialStep3Closed()
		{
			LotteryService.Instance.Log(LogLevel.Information, "TUTORIAL3CLOSED", "{0}: user closed the tutorial step 3 modal window", new SessionInfo(null, Session.SessionID));
		}

		}
}
