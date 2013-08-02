using _555Lottery.DataModel;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.Service
{
	internal class LogService
	{
		private LotteryDbContext context = null;
		
		private log4net.ILog l4n_object = null;
		private log4net.ILog l4n
		{
			get
			{
				if ((l4n_object == null) || (!l4n_object.Logger.Repository.Configured))
				{
					string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
					FileInfo configFile = new FileInfo(path);
					XmlConfigurator.ConfigureAndWatch(configFile);
					l4n_object = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
				}
				
				return l4n_object;
			}
		}

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
			Log("EXCEPTION", "An exception with the message '{0}' was thrown.", ex.Message);
			l4n.Error("The application threw an exception.", ex);
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
			l4n.InfoFormat(action + "|" + formatterText, param);
		}
	}
}
