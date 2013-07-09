using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class TicketList : List<Ticket>
	{
		public string SessionId { get; private set; }

		public string TotalBtc { get; private set; }

		public string TotalGames { get; private set; }

		public string Draws { get; set; }

		public int SelectedIndex { get; set; }

		public int ScrollPosition { get; set; }

		public Ticket SelectedTicket
		{
			get
			{
				return this[SelectedIndex];
			}
		}

		public Ticket[] VisibleTickets {
				get {
					Ticket[] result = new Ticket[5];

					result[0] = this[0];
					result[1] = this[1];
					result[2] = this[2];
					result[3] = this[3];

					return result;
				}
		}

		public TicketList(string sessionId)
		{
			this.SessionId = sessionId;
			this.Add(new Ticket(TicketType.Normal, 0, new int[5], new int[1]));
			this.Add(new Ticket(TicketType.System, 0, new int[5], new int[1]));
			this.Add(new Ticket(TicketType.Random, 0, new int[5], new int[1]));
			this.Add(new Ticket(TicketType.Empty, 0, new int[5], new int[1]));

			this[0].Index = 1;
			this[1].Index = 2;
			this[2].Index = 3;
			this[3].Index = 4;
		}
	}
}