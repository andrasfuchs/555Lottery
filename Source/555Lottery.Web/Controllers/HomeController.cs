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

		public ActionResult Index()
		{
			Draw lastDraw = context.Draws.Where(d => (d.DeadlineUtc < DateTime.UtcNow) && (d.WinningTicketSequence != null)).OrderByDescending(d => d.DeadlineUtc).First();
			Draw currentDraw = context.Draws.Where(d => d.DeadlineUtc > DateTime.UtcNow).OrderBy(d => d.DeadlineUtc).First();

			Session["Tickets"] = new TicketList(Session.SessionID);

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
			return PartialView("_TicketPrice", CreateTicketFromSequence(ticketType, ticketSequence).Price.ToString("0.00"));
		}

		[HttpPost]
		public ActionResult TotalGames()
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			return PartialView("_InfoBoxNumber", tickets.TotalGames.ToString("0"));
		}

		[HttpPost]
		public ActionResult Draws(int value)
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			tickets.Draws = Math.Max(1, value);

			return PartialView("_InfoBoxNumber", tickets.Draws.ToString("0"));
		}

		[HttpPost]
		public ActionResult TotalPrice()
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			return PartialView("_TotalPrice", tickets.TotalPrice.ToString("0.00"));
		}

		[HttpPost]
		public int AcceptTicket(string ticketType, string ticketSequence, int overwriteTicketIndex)
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			Ticket newTicket = CreateTicketFromSequence(ticketType, ticketSequence);

			if ((newTicket.Mode == TicketMode.Random) && (newTicket.Type != 0))
			{
				tickets.GenerateRandomTickets(newTicket.Mode, newTicket.Type, newTicket.Numbers, newTicket.Jokers);
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


			Session["Tickets"] = tickets;

			return overwriteTicketIndex == -1 ? -1 : newTicket.Index;
		}

		[HttpPost]
		public int DeleteTicket(int ticketIndex)
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			int nextTicketIndex = tickets.DeleteTicket(ticketIndex);

			Session["Tickets"] = tickets;

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
			TicketList tickets = Session["Tickets"] as TicketList;

			return PartialView("_TicketSidebar", tickets);
		}

		[HttpPost]
		public JsonResult MoveSidebar(int scrollPositionChange, int selectedTicketIndex)
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			tickets.SelectedIndex = tickets.IndexOf(tickets.First(t => t.Index == selectedTicketIndex));

			tickets.ScrollPosition += scrollPositionChange;

			if (scrollPositionChange == 0)
			{
				if (tickets.ScrollPosition + 4 < tickets.SelectedIndex) tickets.ScrollPosition = tickets.SelectedIndex - 4;
				if (tickets.ScrollPosition > tickets.SelectedIndex) tickets.ScrollPosition = tickets.SelectedIndex;
			}

			if (tickets.ScrollPosition + 5 > tickets.Count) tickets.ScrollPosition = tickets.Count - 5;
			if (tickets.ScrollPosition < 0) tickets.ScrollPosition = 0;

			Session["Tickets"] = tickets;

			return Json(tickets.SelectedTicket, JsonRequestBehavior.AllowGet);
		}

		private Ticket CreateTicketFromSequence(string ticketType, string ticketSequence)
		{
			string[] segments = ticketSequence.Split('|');
			int[] numbers = segments[0].Replace(",","") == "" ? new int[0] : segments[0].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
			int[] jokers = segments[1] == "" ? new int[0] : segments[1].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();

			return new Ticket(ticketType[0] == 'N' ? TicketMode.Normal : ticketType[0] == 'S' ? TicketMode.System : ticketType[0] == 'R' ? TicketMode.Random : TicketMode.Empty, Int32.Parse(ticketType[1].ToString()), numbers, jokers);
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

	}
}
