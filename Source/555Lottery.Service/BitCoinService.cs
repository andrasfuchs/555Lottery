using _555Lottery.DataModel;
using _555Lottery.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service
{
	internal class BitCoinService
	{
		private LogService log;

		private LotteryDbContext context;
		private LotteryDbContext Context
		{
			get
			{
				if (context == null)
				{
					context = new LotteryDbContext();
				}

				return context;
			}
		}

		private BlockChainInfoLatestBlock latest;
		public static readonly decimal OneSatoshi = 0.00000001M;
			
		public BitCoinService(LogService log)
		{
			this.log = log;
			
			this.latest = GetLastestBlock();			
		}

		public void UpdateTransactionLog(string address)
		{
			this.latest = GetLastestBlock();			

			BlockChainInfoAddressInfo ai = GetAddressInfo(address);

			foreach (BlockChainInfoAddressInfoTx tx in ai.Transactions)
			{
				IGrouping<string, BlockChainInfoRawTxOutput>[] inputs = tx.Inputs.Select(i => i.PrevOut).GroupBy(i => i.Addr).ToArray();
				IGrouping<string, BlockChainInfoRawTxOutput>[] outputs = tx.Outputs.GroupBy(o => o.Addr).ToArray();

				if ((inputs.Length == 1) && (outputs.Length == 1))
				{
					TransactionLog lastTH = Context.TransactionLogs.Where(th => th.TransactionHash == tx.Hash).OrderByDescending(th => th.DownloadedUtc).FirstOrDefault();
					if ((lastTH == null) || (lastTH.Confirmations < 6))
					{
						TransactionLog th = Context.TransactionLogs.Create();
						th.DownloadedUtc = DateTime.UtcNow;
						th.Confirmations = (int)(latest.Height - tx.BlockHeight) + 1;
						th.TransactionHash = tx.Hash;
						th.BlockHeight = tx.BlockHeight;
						th.BlockTimeStampUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(tx.Time);

						th.InputAddress = inputs[0].Key;
						th.TotalInput = inputs[0].Sum(gi => gi.Value) * BitCoinService.OneSatoshi;

						th.OutputAddress = outputs[0].Key;
						th.TotalOutput = outputs[0].Sum(gi => gi.Value) * BitCoinService.OneSatoshi;

						Context.TransactionLogs.Add(th);
					}
				}
				else
				{
					log.Log(LogLevel.Warning, "BITCOININVALIDTX", "Only one-to-one transactions are supported at the moment. Tx '{0}' is not valid, so it is discarded for now.", tx.Hash);
				}
			}

			Context.SaveChanges();
		}

		public void MatchUpTransactionsAndTicketLots(Draw draw)
		{
			foreach (TicketLot tl in draw.TicketLots)
			{
				TransactionLog[] logs = Context.TransactionLogs.Where(log => (log.OutputAddress == draw.BitCoinAddress) && (log.TotalOutput == tl.TotalBTC - tl.TotalBTCDiscount)).OrderByDescending(log => log.DownloadedUtc).ToArray();

				if (logs.Length == 0)
				{
					tl.State = TicketLotState.WaitingForPayment;
				}
				else
				{
					tl.MostRecentTransactionLog = logs[0];

					if (logs[logs.Length - 1].DownloadedUtc >= draw.DeadlineUtc)
					{
						tl.State = TicketLotState.TooLateFirstConfirmation;
					}
					else
					{
						if (logs[0].Confirmations < 6)
						{
							tl.State = TicketLotState.NotEnoughConfirmations;
						}
						else
						{
							tl.State = TicketLotState.PaymentConfirmed;
						}
					}
				}
			}

			Context.SaveChanges();
		}

		public void EvaluateDrawTicketLots(Draw draw)
		{
			if (String.IsNullOrEmpty(draw.WinningTicketSequence))
			{
				throw new LotteryException("The draw '" + draw.DrawCode + "' doesn't have the winning sequence yet.");
			}

			foreach (TicketLot tl in draw.TicketLots)
			{
				if (tl.State != TicketLotState.PaymentConfirmed) continue;

				foreach (Ticket t in tl.Tickets)
				{
					
				}
			}

			Context.SaveChanges();
		}

		private BlockChainInfoLatestBlock GetLastestBlock()
		{
			BlockChainInfoLatestBlock result = null;
			string url = "http://blockchain.info/latestblock";

			HttpWebRequest request = HttpWebRequest.CreateHttp(url);
			request.Timeout = 2000;
			WebResponse response = null;

			try
			{
				response = request.GetResponse();
				log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE:'{1}'", url, response);

				DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(BlockChainInfoLatestBlock));
				result = (BlockChainInfoLatestBlock)js.ReadObject(response.GetResponseStream());
				log.Log(LogLevel.Information, "BITCOINLATESTBLOCK", "The latest block (index {0}) was downloaded succesfully.", result.BlockIndex);
			}
			catch (Exception ex)
			{
				log.LogException(ex);
			}
			finally
			{
				if (response != null) response.Close();
			}

			return result;
		}

		private BlockChainInfoAddressInfo GetAddressInfo(string bitcoinAddress)
		{
			BlockChainInfoAddressInfo result = null;
			string url = "http://blockchain.info/address/" + bitcoinAddress + "?format=json";

			HttpWebRequest request = HttpWebRequest.CreateHttp(url);
			request.Timeout = 2000;
			WebResponse response = null;

			try
			{
				response = request.GetResponse();
				log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE:'{1}'", url, response);

				DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(BlockChainInfoAddressInfo));
				result = (BlockChainInfoAddressInfo)js.ReadObject(response.GetResponseStream());
				log.Log(LogLevel.Information, "BITCOINADDRESSINFO", "The address information for '{0}' was downloaded succesfully.", result.Address);
			}
			catch (Exception ex)
			{
				log.LogException(ex);
			}
			finally
			{
				if (response != null) response.Close();
			}

			return result;
		}
	}
}
