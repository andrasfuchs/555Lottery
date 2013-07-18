using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Models
{
	public class TicketList : List<Ticket>
	{
		public string SessionId { get; private set; }

		public decimal TotalPrice 
		{
			get
			{
				return this.Sum(t => t.Price) * this.Draws;
			}
		}

		public int TotalGames 
		{
			get
			{
				return this.Sum(t => t.NumberOfGames);
			}			
		}

		public int Draws { get; set; }

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

					Array.Copy(this.ToArray(), this.ScrollPosition, result, 0, Math.Min(this.Count - this.ScrollPosition, 5));

					return result;
				}
		}

		public TicketList(string sessionId)
		{
			this.SessionId = sessionId;
			this.Draws = 2;
			this.Add(new Ticket(TicketMode.Empty, 0, new int[5], new int[1]));

			this[0].Index = -1;
		}

		public void GenerateRandomTickets(TicketMode mode, int type, int[] numbers, int[] jokers)
		{
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

				for (int j = 0; j<5; j++)
				{
					if (generatedNumbers[j] == 0)
					{
						int rndNumber = 0;
						do {
							rndNumber = rnd.Next(55) + 1;
						} while (generatedNumbers.Contains(rndNumber));

						generatedNumbers[j] = rndNumber;
					}
				}
				Array.Sort(generatedNumbers);

				AppendTicket(new Ticket(mode, 0, generatedNumbers, jokers));
			}
		}

		public void AppendTicket(Ticket newTicket)
		{
			newTicket.Index = this.Max(t => t.Index) + 1;
			if (newTicket.Index <= 0) newTicket.Index = 1;

			this.Insert(this.Count - 1, newTicket);
		}

		public void ReplaceTicket(int ticketIndex, Ticket newTicket)
		{
			int oldIndex = this.FindIndex(t => t.Index == ticketIndex);

			if (oldIndex == -1) this.AppendTicket(newTicket);

			//newTicket.CreatedUtc = this[oldIndex].CreatedUtc;
			newTicket.Index = this[oldIndex].Index;

			this.RemoveAt(oldIndex);
			this.Insert(oldIndex, newTicket);
		}

		public int DeleteTicket(int ticketIndex)
		{
			int oldIndex = this.FindIndex(t => t.Index == ticketIndex);

			if (oldIndex == -1) return -1;

			this.RemoveAt(oldIndex);

			return this[oldIndex].Index;
		}
	}
}