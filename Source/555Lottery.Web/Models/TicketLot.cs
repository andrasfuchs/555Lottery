using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class TicketLot
	{
		[Column(Order = 1)]
		public int TicketLotId { get; set; }

		[Required]
		[Column(Order = 2)]
		public DateTime CreatedUtc { get; set; }

		[Required]
		[Column(Order = 3)]
		public Draw Draw { get; set; }

		[Required]
		[Column(Order = 4)]
		public decimal TotalBTC { get; set; }

		[Column(Order = 5)]
		public string RefundAddress { get; set; }

		[Required]
		[Column(Order = 6)]
		public TicketLotState State { get; set; }

		[Column(Order = 7)]
		public User Owner { get; set; }

		[Column(Order = 8)]
		public string SessionId { get; private set; }

		[InverseProperty("TicketLot")]
		public ICollection<Ticket> TicketsData { get; set; }

		[NotMapped]
		public List<Ticket> Tickets { get; private set; }

		[NotMapped]
		public decimal TotalPrice 
		{
			get
			{
				return this.Tickets.Sum(t => t.Price) * this.DrawNumber;
			}
		}

		[NotMapped]
		public int TotalGames 
		{
			get
			{
				return this.Tickets.Sum(t => t.NumberOfGames);
			}			
		}

		[NotMapped]
		public int DrawNumber { get; set; }

		[NotMapped]
		public int SelectedIndex { get; set; }

		[NotMapped]
		public int ScrollPosition { get; set; }

		[NotMapped]
		public Ticket SelectedTicket
		{
			get
			{
				return this.Tickets[SelectedIndex];
			}
		}

		[NotMapped]
		public Ticket[] VisibleTickets {
				get {
					Ticket[] result = new Ticket[5];

					Array.Copy(this.Tickets.ToArray(), this.ScrollPosition, result, 0, Math.Min(this.Tickets.Count - this.ScrollPosition, 5));

					return result;
				}
		}

		public void Initialize(string sessionId)
		{
			this.CreatedUtc = DateTime.UtcNow;
			this.SessionId = sessionId;
			this.DrawNumber = 2;
			this.Tickets = new List<Ticket>();

			Ticket emptyTicket = new Ticket();
			emptyTicket.Initialize(this, TicketMode.Empty, 0, new int[5], new int[1]);
			this.Tickets.Add(emptyTicket);
		}

		public void AppendTicket(Ticket newTicket)
		{
			newTicket.TicketLot = this;
			newTicket.Index = this.Tickets.Max(t => t.Index) + 1;
			if (newTicket.Index <= 0) newTicket.Index = 1;

			this.Tickets.Insert(this.Tickets.Count - 1, newTicket);
		}

		public void ReplaceTicket(int ticketIndex, Ticket newTicket)
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