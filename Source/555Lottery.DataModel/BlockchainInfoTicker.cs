using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace _555Lottery.DataModel
{
	//[DataContract]
	//public class BlockchainInfoTicker
	//{
	//	[DataMember(Name = "")]
	//	public BlockchainInfoCurrencyList Return { get; set; }
	//}

	[DataContract]
	public class BlockchainInfoTicker
	{
		[DataMember(Name = "USD")]
		public BlockchainInfoTickerPrices USD { get; set; }
		[DataMember(Name = "CNY")]
		public BlockchainInfoTickerPrices CNY { get; set; }
		[DataMember(Name = "JPY")]
		public BlockchainInfoTickerPrices JPY { get; set; }
		[DataMember(Name = "SGD")]
		public BlockchainInfoTickerPrices SGD { get; set; }
		[DataMember(Name = "HKD")]
		public BlockchainInfoTickerPrices HKD { get; set; }
		[DataMember(Name = "CAD")]
		public BlockchainInfoTickerPrices CAD { get; set; }
		[DataMember(Name = "NZD")]
		public BlockchainInfoTickerPrices NZD { get; set; }
		[DataMember(Name = "AUD")]
		public BlockchainInfoTickerPrices AUD { get; set; }
		[DataMember(Name = "CLP")]
		public BlockchainInfoTickerPrices CLP { get; set; }
		[DataMember(Name = "GBP")]
		public BlockchainInfoTickerPrices GBP { get; set; }
		[DataMember(Name = "DKK")]
		public BlockchainInfoTickerPrices DKK { get; set; }
		[DataMember(Name = "SEK")]
		public BlockchainInfoTickerPrices SEK { get; set; }
		[DataMember(Name = "ISK")]
		public BlockchainInfoTickerPrices ISK { get; set; }
		[DataMember(Name = "CHF")]
		public BlockchainInfoTickerPrices CHF { get; set; }
		[DataMember(Name = "BRL")]
		public BlockchainInfoTickerPrices BRL { get; set; }
		[DataMember(Name = "EUR")]
		public BlockchainInfoTickerPrices EUR { get; set; }
		[DataMember(Name = "RUB")]
		public BlockchainInfoTickerPrices RUB { get; set; }
		[DataMember(Name = "PLN")]
		public BlockchainInfoTickerPrices PLN { get; set; }
		[DataMember(Name = "THB")]
		public BlockchainInfoTickerPrices THB { get; set; }
		[DataMember(Name = "KRW")]
		public BlockchainInfoTickerPrices KRW { get; set; }
		[DataMember(Name = "TWD")]
		public BlockchainInfoTickerPrices TWD { get; set; }
	}

	[DataContract]
	public class BlockchainInfoTickerPrices
	{
		[DataMember(Name = "15m")]
		public decimal Delayed { get; set; }
		[DataMember(Name = "last")]
		public decimal Last { get; set; }
		[DataMember(Name = "buy")]
		public decimal Buy { get; set; }
		[DataMember(Name = "sell")]
		public decimal Sell { get; set; }
		[DataMember(Name = "24h")]
		public decimal Average { get; set; }
		[DataMember(Name = "symbol")]
		public string Symbol { get; set; }
	}
}