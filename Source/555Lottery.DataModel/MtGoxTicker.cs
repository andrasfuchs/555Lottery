using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace _555Lottery.DataModel
{
	[DataContract]
	public class MtGoxTicker
	{
		[DataMember(Name = "result")]
		public string Result { get; set; }
		[DataMember(Name = "return")]
		public MtGoxTickerReturn Return { get; set; }
	}

	[DataContract]
	public class MtGoxTickerReturn
	{
		[DataMember(Name = "high")]
		public MtGoxTickerPrice High { get; set; }
		[DataMember(Name = "low")]
		public MtGoxTickerPrice Low { get; set; }
		[DataMember(Name = "avg")]
		public MtGoxTickerPrice Avg { get; set; }
		[DataMember(Name = "vwap")]
		public MtGoxTickerPrice Vwap { get; set; }
		[DataMember(Name = "vol")]
		public MtGoxTickerPrice Vol { get; set; }
		[DataMember(Name = "last_local")]
		public MtGoxTickerPrice LastLocal { get; set; }
		[DataMember(Name = "last_orig")]
		public MtGoxTickerPrice LastOrig { get; set; }
		[DataMember(Name = "last_all")]
		public MtGoxTickerPrice LastAll { get; set; }
		[DataMember(Name = "last")]
		public MtGoxTickerPrice Last { get; set; }
		[DataMember(Name = "buy")]
		public MtGoxTickerPrice Buy { get; set; }
		[DataMember(Name = "sell")]
		public MtGoxTickerPrice Sell { get; set; }
		[DataMember(Name = "item")]
		public string Item { get; set; }
		[DataMember(Name = "now")]
		public long Now { get; set; }
	}

	[DataContract]
	public class MtGoxTickerPrice
	{
		[DataMember(Name = "value")]
		public decimal Value { get; set; }
		[DataMember(Name = "value_int")]
		public long ValueInt { get; set; }
		[DataMember(Name = "display")]
		public string Display { get; set; }
		[DataMember(Name = "display_short")]
		public string DisplayShort { get; set; }
		[DataMember(Name = "currency")]
		public string Currency { get; set; }
	}
}