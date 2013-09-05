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

		private DateTime getLastestBlockLastTime = DateTime.MinValue;
		private BlockChainInfoLatestBlock getLastestBlockLastResult = null;

		public BitCoinService(LogService log)
		{
			this.log = log;

			this.latest = GetLastestBlock();
		}

		public bool UpdateTransactionLog(string address)
		{
			this.latest = GetLastestBlock();

			BlockChainInfoAddressInfo ai = GetAddressInfo(address);

			if (ai == null)
			{
				return false;
			}

			try
			{
				foreach (BlockChainInfoAddressInfoTx tx in ai.Transactions)
				{
					IGrouping<string, BlockChainInfoRawTxOutput>[] inputs = tx.Inputs.Select(i => i.PrevOut).GroupBy(i => i.Addr).ToArray();
					IGrouping<string, BlockChainInfoRawTxOutput>[] outputs = tx.Outputs.GroupBy(o => o.Addr).ToArray();

					if ((inputs.Length == 1) && (outputs.Length >= 1))
					{
						foreach (IGrouping<string, BlockChainInfoRawTxOutput> o in outputs)
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
								th.TotalInputBTC = inputs[0].Sum(gi => gi.Value) * BitCoinService.OneSatoshi;

								th.OutputAddress = o.Key;
								th.OutputBTC = o.Sum(gi => gi.Value) * BitCoinService.OneSatoshi;

								if ((lastTH == null) || (th.Confirmations != lastTH.Confirmations))
								{
									Context.TransactionLogs.Add(th);
								}
							}
						}
					}
					else
					{
						log.Log(LogLevel.Warning, "BITCOININVALIDTX", "Only one-to-one and one-to-many transactions are supported at the moment. Tx '{0}' is not valid, so it is discarded for now.", tx.Hash);
					}

					Context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				// this is an ugly workaround to prevent further errors (sometimes we get a "The property 'TransactionLogId' is part of the object's key information and cannot be modified." exception)
				context = new LotteryDbContext();

				log.LogException(ex);
				return false;
			}

			return true;
		}

		public void MatchUpTransactionsAndTicketLots(Draw draw)
		{
			foreach (TicketLot tl in draw.TicketLots)
			{
				if (tl.State == TicketLotState.PaymentConfirmed) continue;

				TransactionLog[] logs = Context.TransactionLogs.Where(log => (log.OutputAddress == draw.BitCoinAddress) && (log.OutputBTC == tl.TotalBTC - tl.TotalDiscountBTC)).OrderByDescending(log => log.DownloadedUtc).ToArray();

				if (logs.Length == 0)
				{
					//ChangeTicketLotState(tl, TicketLotState.WaitingForPayment);
				}
				else
				{
					tl.MostRecentTransactionLog = logs[0];

					if (logs[logs.Length - 1].BlockTimeStampUtc >= draw.DeadlineUtc)
					{
						ChangeTicketLotState(tl.Code, TicketLotState.InvalidConfirmedTooLate);
					}
					else
					{
						if (logs[0].Confirmations < 6)
						{
							ChangeTicketLotState(tl.Code, TicketLotState.TooFewConfirmations);
						}
						else
						{
							ChangeTicketLotState(tl.Code, TicketLotState.PaymentConfirmed);
						}
					}
				}
			}

			Context.SaveChanges();
		}

		public bool ChangeTicketLotState(string code, TicketLotState newState)
		{
			bool result = false;

			foreach (TicketLot tl in Context.TicketLots.Where(l => l.Code == code))
			{
				result |= ChangeTicketLotState(tl, newState);
			}

			return result;
		}

		public bool ChangeTicketLotState(TicketLot tl, TicketLotState newState)
		{
			if (tl.State != newState)
			{
				log.Log(LogLevel.Information, "CHANGETICKETLOTSTATE", "The state of TicketLot '{0}' (id:{3}) was changed from '{1}' to '{2}'.", tl.Code, tl.State, newState, tl.TicketLotId);

				tl.State = newState;

				return true;
			}

			return false;
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
			if (getLastestBlockLastTime.AddMinutes(3) > DateTime.UtcNow)
			{
				return getLastestBlockLastResult;
			}

			BlockChainInfoLatestBlock result = null;
			string url = "http://blockchain.info/latestblock";

			HttpWebRequest request = HttpWebRequest.CreateHttp(url);
			request.Timeout = 2000;
			WebResponse response = null;

			try
			{
				log.Log(LogLevel.Debug, "REQUEST", "URL:'{0}'", url);
				response = request.GetResponse();
				log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE LENGTH:'{1}'", url, response.ContentLength);

				DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(BlockChainInfoLatestBlock));
				result = (BlockChainInfoLatestBlock)js.ReadObject(response.GetResponseStream());
				log.Log(LogLevel.Information, "BITCOINLATESTBLOCK", "The latest BitCoin block (index {0}) was downloaded succesfully.", result.BlockIndex);

				getLastestBlockLastTime = DateTime.UtcNow;
				getLastestBlockLastResult = result;
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
				log.Log(LogLevel.Debug, "REQUEST", "URL:'{0}'", url);
				response = request.GetResponse();
				log.Log(LogLevel.Debug, "RECEIVED", "URL:'{0}' RESPONSE LENGTH:'{1}'", url, response.ContentLength);

				DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(BlockChainInfoAddressInfo));
				result = (BlockChainInfoAddressInfo)js.ReadObject(response.GetResponseStream());
				log.Log(LogLevel.Information, "BITCOINADDRESSINFO", "Address transaction history for '{0}' was downloaded succesfully.", result.Address);
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
