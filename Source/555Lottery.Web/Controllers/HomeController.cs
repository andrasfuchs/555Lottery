using _555Lottery.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace _555Lottery.Web.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			Session["Tickets"] = new TicketList(Session.SessionID);

			return View();
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
			DateTime nextDraw = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 18, 00, 00);

			if ((nextDraw.DayOfWeek == DayOfWeek.Friday) && (DateTime.UtcNow.Hour >= 18))
			{
				nextDraw = nextDraw.AddDays(1.0);
			}

			while (nextDraw.DayOfWeek != DayOfWeek.Friday)
			{
				nextDraw = nextDraw.AddDays(1.0);
			}

			return (int)((nextDraw - DateTime.UtcNow).TotalSeconds);
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
			int[] numbers = segments[0].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
			int[] jokers = segments[1].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();

			return new Ticket(ticketType[0] == 'N' ? TicketMode.Normal : ticketType[0] == 'S' ? TicketMode.System : ticketType[0] == 'R' ? TicketMode.Random : TicketMode.Empty, Int32.Parse(ticketType[1].ToString()), numbers, jokers);
		}
	}
}
