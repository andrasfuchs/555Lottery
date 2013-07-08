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
		public ActionResult TicketPrice(string ticketType, int numberOfGames)
		{
			decimal price = 0.01M * numberOfGames;

			return PartialView("_TicketPrice", price.ToString("0.00"));
		}

		[HttpPost]
		public ActionResult TotalPrice()
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			decimal totalPrice = 1.04M;

			return PartialView("_TicketPrice", totalPrice.ToString("0.00"));
		}

		[HttpPost]
		public ActionResult AcceptTicket(string ticketType, string ticketSequence)
		{
			TicketList tickets = Session["Tickets"] as TicketList;

			string[] segments = ticketSequence.Split('|');
			int[] numbers = segments[0].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();
			int[] jokers = segments[1].Split(',').Select<string, int>(n => Int32.Parse(n)).ToArray();

			tickets.Add(new Ticket(ticketType[0] == 'N' ? TicketType.Normal : ticketType[0] == 'S' ? TicketType.System : ticketType[0] == 'R' ? TicketType.Random : TicketType.Empty, Int32.Parse(ticketType[1].ToString()), numbers, jokers));

			Session["Tickets"] = tickets;

			return null;
		}	
	}
}
