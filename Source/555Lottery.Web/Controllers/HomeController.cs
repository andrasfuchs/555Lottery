using _555Lottery.DataModel;
using _555Lottery.Service;
using _555Lottery.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace _555Lottery.Web.Controllers
{
	public class HomeController : Controller
	{
		private static TimeSpan[] delaySteps = new TimeSpan[] { 
			new TimeSpan(2, 0, 0), new TimeSpan(1, 0, 0), new TimeSpan(0, 45, 0), new TimeSpan(0, 30, 0), new TimeSpan(0, 20, 0), new TimeSpan(0, 15, 0), 
			new TimeSpan(0, 10, 0), new TimeSpan(0, 5, 0), new TimeSpan(0, 3, 0), new TimeSpan(0, 0, 60), new TimeSpan(0, 0, 30), new TimeSpan(0, 0, 10) 
		};

		private static string[] delayStepNamesEng = new string[] {
			"2 hours", "1 hour", "45 minutes", "30 minutes", "20 minutes", "15 minutes", "10 minutes", "5 minutes", "3 minutes", "60 seconds", "30 seconds", "10 seconds"
		};

		public ActionResult Index()
		{
			TicketLot tl = LotteryService.Instance.CreateTicketLot(LotteryService.Instance.CurrentDraw, this.Session.SessionID);
			Session["Tickets"] = tl;

			decimal jackpotToDisplay = Math.Min(9999999, LotteryService.Instance.CurrentDraw.JackpotUSD);

			string jackpot = String.Format("${0:n0}", jackpotToDisplay);

			string lastDrawText = LotteryService.Instance.LastDraw.WinningTicketSequence;

			if (String.IsNullOrEmpty(LotteryService.Instance.LastDraw.WinningTicketSequence))
			{
				int delayIndex = 0;
				TimeSpan delayTimeSpan = delaySteps[0];

				while ((delayIndex + 1 < delaySteps.Length) && (LotteryService.Instance.LastDraw.DeadlineUtc.Add(delayTimeSpan) - DateTime.UtcNow < delaySteps[delayIndex + 1]))
				{
					delayIndex++;
				}

				
				lastDrawText = "Please wait until we get the new winners in approximately " + delayStepNamesEng[delayIndex] + "...";
			}

			return View(new string[] { jackpot, lastDrawText });
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
		public ActionResult TicketPrice(string ticketType, string ticketSequence)
		{
			Ticket ticket = new Ticket();
			ticket.Initialize(null, ticketType, ticketSequence);

			return PartialView("_TicketPrice", (ticket.NumberOfGames * LotteryService.Instance.CurrentDraw.OneGamePrice).ToString("0.00"));
		}

		[HttpPost]
		public ActionResult TotalGames()
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			return PartialView("_InfoBoxNumber", tl.TotalGames.ToString("0"));
		}

		[HttpPost]
		public ActionResult Draws(int value)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			tl.DrawNumber = Math.Min(26, Math.Min(LotteryService.Instance.DrawsRemaining, Math.Max(1, value)));

			return PartialView("_InfoBoxNumber", tl.DrawNumber.ToString("0"));
		}

		[HttpPost]
		public ActionResult TotalPrice()
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			return PartialView("_TotalPrice", tl.TotalPrice.ToString("0.00"));
		}

		[HttpPost]
		public int AcceptTicket(string ticketType, string ticketSequence, int overwriteTicketIndex)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			Ticket newTicket = LotteryService.Instance.CreateTicket(tl, ticketType, ticketSequence);

			if ((newTicket.Mode == TicketMode.Random) && (newTicket.Type != 0))
			{
				GenerateRandomTickets(newTicket.Mode, newTicket.Type, newTicket.Numbers, newTicket.Jokers);
			}
			else
			{
				if (overwriteTicketIndex == -1)
				{
					tl.AppendTicket(newTicket);
				}
				else
				{
					tl.ReplaceTicket(overwriteTicketIndex, newTicket);
				}
			}


			Session["Tickets"] = tl;

			return overwriteTicketIndex == -1 ? -1 : newTicket.Index;
		}

		[HttpPost]
		public int DeleteTicket(int ticketIndex)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			int nextTicketIndex = tl.DeleteTicket(ticketIndex);

			Session["Tickets"] = tl;

			return nextTicketIndex;
		}


		[HttpPost]
		public int GetTimeLeftToNextDraw()
		{
			return (int)((LotteryService.Instance.CurrentDraw.DeadlineUtc - DateTime.UtcNow).TotalSeconds);
		}

		[HttpPost]
		public ActionResult TimeLeft(int secondsToNextDraw)
		{
			return PartialView("_TimeLeft", secondsToNextDraw);
		}

		[HttpPost]
		public ActionResult TicketSidebar()
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			return PartialView("_TicketSidebar", tl);
		}

		[HttpPost]
		public JsonResult MoveSidebar(int scrollPositionChange, int selectedTicketIndex)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

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

			Session["Tickets"] = tl;

			return Json(tl.SelectedTicket, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult LetPlay()
		{
			TicketLot tickets = Session["Tickets"] as TicketLot;

			//context.SaveChanges();

			Session["Tickets"] = tickets;

			return Json(tickets.SelectedTicket, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Stats()
		{
			return View(LotteryService.Instance.GetDraws());
		}

		public void GenerateRandomTickets(TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

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

				Ticket newTicket = LotteryService.Instance.CreateTicket(tl, mode, 0, generatedNumbers, generatedJokers);
				tl.AppendTicket(newTicket);
			}
		}

		[HttpPost]
		public ActionResult LetsPlay()
		{
			TicketLot tl = Session["Tickets"] as TicketLot;
			LotteryService.Instance.SaveTicketLot(tl);

			tl = LotteryService.Instance.CreateTicketLot(LotteryService.Instance.CurrentDraw, this.Session.SessionID);
			Session["Tickets"] = tl;

			return null;
		}

	}
}
