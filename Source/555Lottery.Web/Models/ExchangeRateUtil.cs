using _555Lottery.Web.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace _555Lottery.Web.Models
{
	public static class ExchangeRateUtil
	{
		public static decimal GetRate(LotteryDbContext context, string currencyISO1, string currencyISO2)
		{
			ExchangeRate exrate = context.ExchangeRates.Where(er => (er.CurrencyISO1 == currencyISO1) && (er.CurrencyISO2 == currencyISO2)).OrderByDescending(er => er.TimeUtc).FirstOrDefault();
			if ((exrate == null) || (exrate.TimeUtc.AddMinutes(15) < DateTime.UtcNow))
			{
				exrate = context.ExchangeRates.Create();

				try
				{
					HttpWebRequest request = HttpWebRequest.CreateHttp("https://data.mtgox.com/api/1/" + currencyISO1 + currencyISO2 + "/ticker");
					request.Timeout = 2000;
					WebResponse response = null;

					try
					{
						response = request.GetResponse();

						DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(MtGoxTicker));
						MtGoxTicker ticker = (MtGoxTicker)js.ReadObject(response.GetResponseStream());

						exrate.TimeUtc = DateTime.UtcNow;
						exrate.CurrencyISO1 = currencyISO1;
						exrate.CurrencyISO2 = currencyISO2;
						exrate.Rate = ticker.Return.Avg.Value;
					}
					finally
					{
						if (response != null) response.Close();
					}

					context.ExchangeRates.Add(exrate);
					context.SaveChanges();
				}
				catch
				{
					// TODO: Log exception into the DB
				}
			}

			return exrate.Rate;
		}
	}
}