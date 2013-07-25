using _555Lottery.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service
{
	internal class LogService
	{
		private LotteryDbContext context = null;

		private DateTime logMustBeWrittenAt = DateTime.MinValue;
		private int logItemsToWrite = 0;

		public LogService(LotteryDbContext context)
		{
			this.context = context;
		}

		private void LogDBAsync(Log log, bool forceDBWrite)
		{
			BackgroundWorker bgWorker = new BackgroundWorker();
			bgWorker.DoWork += (sender, e) =>
			{
				lock (context)
				{
					context.Configuration.AutoDetectChangesEnabled = false;
					context.Logs.Add((Log)e.Argument);
					context.Configuration.AutoDetectChangesEnabled = true;

					if (logItemsToWrite == 0)
					{
						logMustBeWrittenAt = DateTime.UtcNow.AddMinutes(5);
					}
					logItemsToWrite++;

					if (forceDBWrite || (logItemsToWrite > 20) || (logMustBeWrittenAt < DateTime.UtcNow))
					{
						context.SaveChanges();
						logItemsToWrite = 0;
					}
				}
			};
			bgWorker.RunWorkerAsync(log);
		}

		public void LogException(Exception ex)
		{
			Log("EXCEPTION", "An exception with the message '{1}' was thrown.", ex.Message);
		}

		public void LogDB(string action, string formatterText, object[] parameters)
		{
			Log log = new Log()
			{
				UtcTime = DateTime.UtcNow,
				//IPAddress = CurrentSessions[HttpContext.Session.SessionID].IPAddress,
				//SessionId = HttpContext.Session.SessionID,
				Action = action,
				Parameters = String.Join(",", parameters),
				FormattedMessage = String.Format(formatterText, parameters)
			};

			LogDBAsync(log, true);
		}

		public void Log(string action, string formatterText, params object[] param)
		{
			// TODO: implement logging
		}
	}
}
