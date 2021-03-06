﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _555Lottery.DataModel
{
	public class Game
	{
		[Column(Order = 1)]
		public int GameId { get; set; }

		[InverseProperty("Games")]
		[Required]
		[Column(Order = 2)]
		public Ticket Ticket { get; set; }

		[Required]
		[Column(Order = 3)]
		public string Sequence { get; set; }

		[Required]
		[Column(Order = 4)]
		public byte[] SequenceHash { get; set; }

		[Column(Order = 5)]
		public string Hits { get; set; }

		[Column(Order = 6)]
		public decimal? WinningsBTC { get; set; }

		public override string ToString()
		{
			return this.Sequence + (this.Hits == null ? "" : "(" + this.Hits + ")");
		}
	}
}