using _555Lottery.Web.DataAccess;
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
		private static LotteryDbContext context = new LotteryDbContext();
		private static Draw lastDraw = null;
		private static Draw currentDraw = null;

		public ActionResult Index()
		{
			lastDraw = context.Draws.Where(d => (d.DeadlineUtc < DateTime.UtcNow) && (d.WinningTicketSequence != null)).OrderByDescending(d => d.DeadlineUtc).First();
			currentDraw = context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();

			TicketLot tl = context.TicketLots.Create();
			tl.Initialize(this.Session.SessionID);
			tl.Draw = currentDraw;
			Session["Tickets"] = tl;

			currentDraw.JackpotUSD = currentDraw.JackpotBTC * ExchangeRateUtil.GetRate(context, "BTC", "USD");

			decimal jackpotToDisplay = Math.Min(9999999, currentDraw.JackpotUSD);

			string jackpot = String.Format("${0:n0}", jackpotToDisplay);

			return View(new string[] { jackpot, lastDraw.WinningTicketSequence });
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

			return PartialView("_TicketPrice", (ticket.NumberOfGames * currentDraw.OneGamePrice).ToString("0.00"));
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

			tl.DrawNumber = Math.Min(9, Math.Max(1, value));

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

			Ticket newTicket = context.Tickets.Create();				
			newTicket.Initialize(tl, ticketType, ticketSequence);

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
			Draw currentDraw = context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();

			return (int)((currentDraw.DeadlineUtc - DateTime.UtcNow).TotalSeconds);
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
			Draw[] draws = context.Draws.OrderByDescending(d => d.DeadlineUtc).ToArray();

			decimal btcusd = ExchangeRateUtil.GetRate(context, "BTC", "USD");
			decimal btceur = ExchangeRateUtil.GetRate(context, "BTC", "EUR");

			foreach (Draw d in draws)
			{
				d.JackpotUSD = d.JackpotBTC * btcusd;
				d.JackpotEUR = d.JackpotBTC * btceur;
			}

			return View(draws);
		}

		public void GenerateRandomTickets(TicketMode mode, int type, int[] numbers, int[] jokers)
		{
			TicketLot tl = Session["Tickets"] as TicketLot;

			if (mode != TicketMode.Random) throw new _555LotteryException("Ticket mode must be Random to use this function!");

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

				Ticket newTicket = context.Tickets.Create();
				newTicket.Initialize(tl, mode, 0, generatedNumbers, generatedJokers);
				tl.AppendTicket(newTicket);
			}
		}

		[HttpPost]
		public ActionResult LetsPlay()
		{
			TicketLot tl = Session["Tickets"] as TicketLot;
			context.TicketLots.Add(tl);
			foreach (Ticket ticket in tl.Tickets)
			{
				if ((ticket.Mode != TicketMode.Empty) && (ticket.TicketId == 0))
				{
					context.Tickets.Add(ticket);
				}
			}
			context.SaveChanges();

			tl = context.TicketLots.Create();
			tl.Initialize(this.Session.SessionID);
			tl.Draw = currentDraw;
			Session["Tickets"] = tl;

			return null; // PartialView("_TicketSidebar", tl);
		}

	}
}
