using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace _555Lottery.Web.ViewModels
{
	public class TicketLotViewModel
	{
		public int TicketLotId { get; set; }

		public DateTime CreatedUtc { get; set; }

		public virtual DrawViewModel Draw { get; set; }

		public string Code { get; set; }

		public decimal TotalBTC { get; set; }

		public decimal TotalBTCDiscount { get; set; }

		public string RefundAddress { get; set; }

		public TicketLotState State { get; set; }

		public string SessionId { get; private set; }

		public string EmailAddress { get; private set; }

		public TransactionLog MostRecentTransactionLog { get; set; }

		public List<TicketViewModel> Tickets { get; set; }

		public int TotalGames
		{
			get
			{
				return this.Tickets.Sum(t => t.NumberOfGames);
			}
		}

		public int DrawNumber { get; set; }

		public int SelectedIndex { get; set; }

		public int ScrollPosition { get; set; }

		[ScriptIgnore]
		public TicketViewModel SelectedTicket
		{
			get
			{
				return this.Tickets[SelectedIndex];
			}
		}

		[ScriptIgnore]
		public TicketViewModel[] VisibleTickets
		{
			get
			{
				TicketViewModel[] result = new TicketViewModel[5];

				Array.Copy(this.Tickets.ToArray(), this.ScrollPosition, result, 0, Math.Min(this.Tickets.Count - this.ScrollPosition, 5));

				return result;
			}
		}

		public TicketLotViewModel() { }

		public TicketLotViewModel(string sessionId, DrawViewModel draw)
		{
			this.CreatedUtc = DateTime.UtcNow;
			this.SessionId = sessionId;
			this.Draw = draw;
			this.DrawNumber = 2;
			this.Tickets = new List<TicketViewModel>();

			TicketViewModel emptyTicket = new TicketViewModel(0.00M, TicketMode.Empty, 0, new int[5], new int[1]);
			this.Tickets.Add(emptyTicket);
		}

		public void AppendTicket(TicketViewModel newTicket)
		{
			newTicket.Index = this.Tickets.Max(t => t.Index) + 1;
			if (newTicket.Index <= 0) newTicket.Index = 1;

			this.Tickets.Insert(this.Tickets.Count - 1, newTicket);
		}

		public void ReplaceTicket(int ticketIndex, TicketViewModel newTicket)
		{
			int oldIndex = this.Tickets.FindIndex(t => t.Index == ticketIndex);

			if (oldIndex == -1) this.AppendTicket(newTicket);

			//newTicket.CreatedUtc = this[oldIndex].CreatedUtc;
			newTicket.Index = this.Tickets[oldIndex].Index;

			this.Tickets.RemoveAt(oldIndex);
			this.Tickets.Insert(oldIndex, newTicket);
		}

		public int DeleteTicket(int ticketIndex)
		{
			int oldIndex = this.Tickets.FindIndex(t => t.Index == ticketIndex);

			if (oldIndex == -1) return -1;

			this.Tickets.RemoveAt(oldIndex);

			return this.Tickets[oldIndex].Index;
		}
	}
}