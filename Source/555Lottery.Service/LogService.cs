﻿using _555Lottery.DataModel;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace _555Lottery.Service
{
	internal class LogService
	{
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

		public LogService() {}

		private void LogDBAsync(Log log, bool forceDBWrite)
		{
			//BackgroundWorker bgWorker = new BackgroundWorker();
			//bgWorker.DoWork += (sender, e) =>
			{
				lock (Context)
				{
					Context.Configuration.AutoDetectChangesEnabled = false;
					Context.Logs.Add(log);
					Context.Configuration.AutoDetectChangesEnabled = true;

					if (logItemsToWrite == 0)
					{
						logMustBeWrittenAt = DateTime.UtcNow.AddMinutes(5);
					}
					logItemsToWrite++;

					if (forceDBWrite || (logItemsToWrite > 20) || (logMustBeWrittenAt < DateTime.UtcNow))
					{
						Context.SaveChanges();
						logItemsToWrite = 0;
					}
				}
			};
			//bgWorker.RunWorkerAsync(log);
		}

		public void LogException(Exception ex)
		{
			Log(LogLevel.Error, "EXCEPTION", "An exception with the message '{1}' was thrown.", ex, ex.Message);
		}

		private void LogDB(LogLevel level, string action, string formatterText, object[] parameters)
		{
			SessionInfo session = null;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i] is SessionInfo)
				{
					session = (SessionInfo)parameters[i];
				}
			}

			Log log = new Log()
			{
				UtcTime = DateTime.UtcNow,
				IPAddress = session == null || String.IsNullOrEmpty(session.IPAddress) ? "unknown" : session.IPAddress,
				SessionId = session == null || String.IsNullOrEmpty(session.SessionId) ? "unknown" : session.SessionId,
				Level = level,
				Action = action,
				Parameters = String.Join(",", parameters),
				FormattedMessage = String.Format(formatterText, parameters)
			};

			LogDBAsync(log, true);
		}

		public void Log(LogLevel level, string action, string formatterText, params object[] param)
		{
			for (int i = 0; i < param.Length; i++)
			{
				if (param[i] is byte[])
				{
					param[i] = BitConverter.ToString((byte[])param[i]);
				}
			}

			if ((level == LogLevel.Error) && (param[0] is Exception))
			{
				l4n.Error("The application threw an exception.", (Exception)param[0]);
			}
			else
			{
				switch (level)
				{
					case LogLevel.Error:
						l4n.ErrorFormat(action + "|" + formatterText, param);
						break;
					case LogLevel.Warning:
						l4n.WarnFormat(action + "|" + formatterText, param);
						break;
					case LogLevel.Information:
						l4n.InfoFormat(action + "|" + formatterText, param);
						break;
					case LogLevel.Debug:
					default:
						l4n.DebugFormat(action + "|" + formatterText, param);
						break;
				}
			}

			LogDB(level, action, formatterText, param);
		}
	}
}
