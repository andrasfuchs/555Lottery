using _555Lottery.DataModel;
using _555Lottery.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
		public LotteryDbContext Context
		{
			get
			{
				if (context == null)
				{
					//context = new LotteryDbContext();
					throw new LotteryException("You must set the context of the BitCoinService first!");
				}

				return context;
			}

			set
			{
				context = value;
			}
		}

		private BlockChainInfoLatestBlock latest;
		public static readonly decimal OneSatoshi = 0.00000001M;
		public static readonly string BlockChainInfoId = "d19d745dee988c1779d25f3ab51e526591380136470626";

		private DateTime getLastestBlockLastTime = DateTime.MinValue;
		private BlockChainInfoLatestBlock getLastestBlockLastResult = null;

		private List<string> ignoreTxs = new List<string>();

		private Dictionary<string, BlockChainInfoAddressInfoCacheItem> addressInfoCache = new Dictionary<string, BlockChainInfoAddressInfoCacheItem>();

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

			lock (Context)
			{
				try
				{
					foreach (BlockChainInfoAddressInfoTx tx in ai.Transactions)
					{
						if (ignoreTxs.Contains(tx.Hash)) continue;

						IGrouping<string, BlockChainInfoRawTxOutput>[] inputs = tx.Inputs.Select(i => i.PrevOut).GroupBy(i => i.Addr).ToArray();
						IGrouping<string, BlockChainInfoRawTxOutput>[] outputs = tx.Outputs.GroupBy(o => o.Addr).ToArray();

						if ((inputs.Length >= 1) && (outputs.Length >= 1))
						{
							foreach (IGrouping<string, BlockChainInfoRawTxOutput> o in outputs)
							{
								TransactionLog lastTH = Context.TransactionLogs.Where(th => th.TransactionHash == tx.Hash).OrderByDescending(th => th.DownloadedUtc).FirstOrDefault();
								if ((lastTH == null) || (lastTH.Confirmations < 6))
								{
									TransactionLog th = Context.TransactionLogs.Create();
									th.DownloadedUtc = DateTime.UtcNow;
									if (tx.BlockHeight > 0)
									{
										th.Confirmations = (int)(latest.Height - tx.BlockHeight) + 1;
									}

									th.TransactionHash = tx.Hash;
									th.BlockHeight = tx.BlockHeight;
									th.BlockTimeStampUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(tx.Time);

									th.InputAddress = "";
									th.TotalInputBTC = 0;
									foreach (IGrouping<string, BlockChainInfoRawTxOutput> input in inputs)
									{
										th.InputAddress += input.Key + ",";
										th.TotalInputBTC += input.Sum(gi => gi.Value) * BitCoinService.OneSatoshi;
									}

									th.OutputAddress = o.Key;
									th.OutputBTC = o.Sum(gi => gi.Value) * BitCoinService.OneSatoshi;

									if ((lastTH == null) || (th.Confirmations != lastTH.Confirmations))
									{
										Context.TransactionLogs.Add(th);
									}
								}
								else
								{
									ignoreTxs.Add(tx.Hash);
								}
							}
						}
						else
						{
							//log.Log(LogLevel.Warning, "BITCOININVALIDTX", "Only one-to-one and one-to-many transactions are supported at the moment. Tx '{0}' is not valid, so it is discarded for now.", tx.Hash);
							log.Log(LogLevel.Error, "BITCOININVALIDTX", "The transaction has no input or output. Tx '{0}' is not valid, so it is discarded for now.", tx.Hash);
							ignoreTxs.Add(tx.Hash);
						}

					}
					Context.SaveChanges();
				}
				catch (Exception ex)
				{
					log.LogException(ex);
					return false;
				}
			}

			return true;
		}

		public void MatchUpTransactionsAndTicketLots(Draw draw)
		{
			lock (Context)
			{
				TransactionLog[] allTransactionLogs = Context.TransactionLogs.Where(log => (log.OutputAddress == draw.BitCoinAddress)).ToArray();

				foreach (TicketLot tl in draw.TicketLots)
				{
					if ((tl.State == TicketLotState.PaymentConfirmed) ||
						(tl.State == TicketLotState.EvaluatedNotWon) ||
						(tl.State == TicketLotState.EvaluatedPrizePaymentPending))
						continue;

					TransactionLog[] logs = allTransactionLogs.Where(log => (log.OutputBTC == tl.TotalBTC - tl.TotalDiscountBTC)).OrderByDescending(log => log.DownloadedUtc).ToArray();

					if (logs.Length == 0)
					{
						//ChangeTicketLotState(tl, TicketLotState.WaitingForPayment);
					}
					else
					{
						SetTicketLotTransactionLog(tl.Code, logs[0]);

						if (logs[logs.Length - 1].BlockTimeStampUtc >= draw.DeadlineUtc)
						{
							ChangeTicketLotState(draw, tl.Code, TicketLotState.InvalidConfirmedTooLate);
						}
						else
						{
							if (logs[0].Confirmations < 6)
							{
								ChangeTicketLotState(draw, tl.Code, TicketLotState.TooFewConfirmations);
							}
							else
							{
								ChangeTicketLotState(draw, tl.Code, TicketLotState.PaymentConfirmed);
							}
						}
					}
				}

				Context.SaveChanges();
			}
		}

		public void MatchUpReturnTransactionsAndTicketLots(Draw draw)
		{
			lock (Context)
			{
				TransactionLog[] allTransactionLogs = Context.TransactionLogs.ToArray();

				foreach (TicketLot tl in draw.TicketLots)
				{
					if ((tl.State != TicketLotState.RefundInitiated) && (tl.State != TicketLotState.PrizePaymentInitiated)) continue;

					TransactionLog[] logs = allTransactionLogs.Where(log => (log.OutputAddress == tl.RefundAddress) && (log.OutputBTC == tl.WinningsBTC)).OrderByDescending(log => log.DownloadedUtc).ToArray();

					if (logs.Length > 0)
					{
						tl.MostRecentPayoutTransactionLog = logs[0];

						if (logs[0].Confirmations >= 6)
						{
							if (tl.State == TicketLotState.PrizePaymentInitiated)
							{
								ChangeTicketLotState(tl, TicketLotState.PrizePaymentConfirmed);
							}

							if (tl.State == TicketLotState.RefundInitiated)
							{
								ChangeTicketLotState(tl, TicketLotState.RefundConfirmed);
							}
						}
					}
				}

				Context.SaveChanges();
			}
		}

		public bool SetTicketLotTransactionLog(string code, TransactionLog log)
		{
			bool result = false;

			lock (Context)
			{
				foreach (TicketLot tl in Context.TicketLots.Include("Draw").Include("MostRecentTransactionLog").Where(l => l.Code == code))
				{
					if ((tl.MostRecentTransactionLog == null) || (tl.MostRecentTransactionLog.TransactionLogId != log.TransactionLogId))
					{
						tl.MostRecentTransactionLog = log;

						if (String.IsNullOrEmpty(tl.RefundAddress))
						{
							tl.RefundAddress = log.InputAddress;
						}

						result |= true;
					}
				}

				return result;
			}
		}

		public bool ChangeTicketLotState(Draw draw, string code, TicketLotState newState)
		{
			bool result = false;

			List<TicketLot> tls = Context.TicketLots.Include("Draw").Where(l => l.Code == code && l.Draw.DrawId >= draw.DrawId).OrderBy(l => l.Draw.DrawId).ToList();

			int currentDrawId = tls[0].Draw.DrawId;
			foreach (TicketLot tl in tls)
			{
				if (tl.Draw.DrawId != currentDrawId)
				{
					break;
				}

				result |= ChangeTicketLotState(tl, newState);
				currentDrawId++;
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

			Cookie c = new Cookie("__cfduid", BlockChainInfoId, "/", ".blockchain.info");
			c.Expires = new DateTime(2019, 12, 23);
			request.CookieContainer = new CookieContainer();
			request.CookieContainer.Add(c);
			
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

			lock (addressInfoCache)
			{
				if (addressInfoCache.ContainsKey(bitcoinAddress) && addressInfoCache[bitcoinAddress].Expires > DateTime.UtcNow)
				{
					return addressInfoCache[bitcoinAddress].Item;
				}

				string url = "http://blockchain.info/address/" + bitcoinAddress + "?format=json";

				HttpWebRequest request = HttpWebRequest.CreateHttp(url);
				request.Timeout = 2000;

				Cookie c = new Cookie("__cfduid", BlockChainInfoId, "/", ".blockchain.info");
				c.Expires = new DateTime(2019, 12, 23);
				request.CookieContainer = new CookieContainer();
				request.CookieContainer.Add(c);

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

				if (result != null) 
				{
					BlockChainInfoAddressInfoCacheItem cacheItem = new BlockChainInfoAddressInfoCacheItem() { Item = result, Expires = DateTime.UtcNow.AddMinutes(10) };

					if (!addressInfoCache.ContainsKey(result.Address))
					{
						addressInfoCache.Add(result.Address, cacheItem);
					}
					else 
					{
						addressInfoCache[result.Address] = cacheItem;
					}					
				}
			}

			return result;
		}
	}
}
