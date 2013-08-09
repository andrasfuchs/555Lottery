using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace _555Lottery.DataModel
{
	[DataContract]
	public class BlockChainInfoLatestBlock
	{
		[DataMember(Name = "hash")]
		public string Hash { get; set; }
		[DataMember(Name = "time")]
		public long UnixTime { get; set; }
		[DataMember(Name = "block_index")]
		public long BlockIndex { get; set; }
		[DataMember(Name = "height")]
		public long Height { get; set; }
		[DataMember(Name = "txIndexes")]
		public long[] TransactionIndexes { get; set; }
	}

	[DataContract]
	public class BlockChainInfoAddressInfo
	{
		[DataMember(Name = "hash160")]
		public string Hash160 { get; set; }
		[DataMember(Name = "address")]
		public string Address { get; set; }
		[DataMember(Name = "n_tx")]
		public int NumberOfTransactions { get; set; }
		[DataMember(Name = "total_received")]
		public long TotalReceived { get; set; }
		[DataMember(Name = "total_sent")]
		public long TotalSent { get; set; }
		[DataMember(Name = "final_balance")]
		public long FinalBalance { get; set; }
		[DataMember(Name = "txs")]
		public BlockChainInfoAddressInfoTx[] Transactions { get; set; }
	}

	[DataContract]
	public class BlockChainInfoRawTx
	{
		[DataMember(Name = "block_height")]
		public int BlockHeight { get; set; }
		[DataMember(Name = "time")]
		public long Time { get; set; }
		[DataMember(Name = "inputs")]
		public BlockChainInfoRawTxInput[] Inputs { get; set; }
		[DataMember(Name = "vout_sz")]
		public long VoutSz { get; set; }
		[DataMember(Name = "relayed_by")]
		public string RelayedBy { get; set; }
		[DataMember(Name = "hash")]
		public string Hash { get; set; }
		[DataMember(Name = "vin_sz")]
		public long VinSz { get; set; }
		[DataMember(Name = "tx_index")]
		public long TxIndex { get; set; }
		[DataMember(Name = "ver")]
		public int Version { get; set; }
		[DataMember(Name = "out")]
		public BlockChainInfoRawTxOutput[] Outputs { get; set; }
		[DataMember(Name = "size")]
		public int Size { get; set; }
	}
	
	[DataContract]
	public class BlockChainInfoAddressInfoTx : BlockChainInfoRawTx
	{
		[DataMember(Name = "result")]
		public long Result { get; set; }
	}

	[DataContract]
	public class BlockChainInfoRawTxInput
	{
		[DataMember(Name = "prev_out")]
		public BlockChainInfoRawTxOutput PrevOut { get; set; }
	}

	[DataContract]
	public class BlockChainInfoRawTxOutput
	{
		[DataMember(Name = "n")]
		public long N { get; set; }
		[DataMember(Name = "value")]
		public long Value { get; set; }
		[DataMember(Name = "addr")]
		public string Addr { get; set; }
		[DataMember(Name = "tx_index")]
		public long TxIndex { get; set; }
		[DataMember(Name = "type")]
		public long Type { get; set; }
	}
}