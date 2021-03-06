﻿using _555Lottery.DataModel;
using _555Lottery.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace _555Lottery.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			AuthConfig.RegisterAuth();

			AutoMapperConfig.Configure();

            // for desktop apps this:
            string dataDir = Server.MapPath("~/App_Data/");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);


			LotteryService.Instance.Log(LogLevel.Information, "APPSTART", "555 Lottery application started");
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			// Code that runs when an unhandled error occurs

			// Get the exception object.
			Exception ex = Server.GetLastError();

			// Log the exception
			try
			{
				_555Lottery.Service.LotteryService.Instance.LogException(ex);
			}
			catch { }
		}

		protected void Application_End()
		{
			LotteryService.Instance.Log(LogLevel.Information, "APPEND", "555 Lottery application terminated");
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			// set the language variable
			HttpCookie cookie = this.Context.Request.Cookies["Language"];
			if (cookie == null)
			{
				cookie = new HttpCookie("Language");
				cookie.Value = "eng";
				this.Context.Response.Cookies.Add(cookie);
			}
			cookie.Expires = DateTime.Now.AddDays(14);


			if (this.Context.Request.Params["l"] != null)
			{
				switch (this.Context.Request.Params["l"])
				{
					case "e":
						cookie.Value = "eng";
						break;
					case "s":
						cookie.Value = "spa";
						break;
					case "g":
						cookie.Value = "deu";
						break;
					case "r":
						cookie.Value = "rus";
						break;
					case "c":
						cookie.Value = "zho";
						break;
				}
			}

			Session["Language"] = cookie.Value;


			// set the currency variable
			cookie = this.Context.Request.Cookies["Currency"];
			if (cookie == null)
			{
				cookie = new HttpCookie("Currency");
				cookie.Value = "usd";
				this.Context.Response.Cookies.Add(cookie);
			}

			cookie.Expires = DateTime.Now.AddDays(14);


			if (this.Context.Request.Params["c"] != null)
			{
				switch (this.Context.Request.Params["c"])
				{
					case "u":
						cookie.Value = "USD";
						break;
					case "e":
						cookie.Value = "EUR";
						break;
					case "r":
						cookie.Value = "RUB";
						break;
					case "c":
						cookie.Value = "CNY";
						break;
					case "i":
						cookie.Value = "INR";
						break;
				}
			}

			Session["Currency"] = cookie.Value.ToUpper();

			// set the agreement accepted variable
			cookie = this.Context.Request.Cookies["Agreement"];
			if (cookie == null)
			{
				cookie = new HttpCookie("Agreement");
				cookie.Value = "false";
				this.Context.Response.Cookies.Add(cookie);
			}

			cookie.Expires = DateTime.Now.AddDays(14);

			Session["Agreement"] = (cookie.Value == "true");

			Session["StartedAt"] = DateTime.UtcNow;

			string userAgent = (Request.ServerVariables["HTTP_USER_AGENT"] == null ? "" : Request.ServerVariables["HTTP_USER_AGENT"]);
			bool isMobile = Regex.IsMatch(userAgent, "(up.browser|up.link|mmp|symbian|smartphone|midp|wap|phone|windows ce|pda|mobile|mini|palm|ipad)", RegexOptions.IgnoreCase);
  
			LotteryService.Instance.Log(
				LogLevel.Information, "SESSIONSTART", "{0}: session started (ref:'{6}')",
				new SessionInfo(Request.ServerVariables["REMOTE_ADDR"], Session.SessionID), 
				isMobile,
				(Request.Cookies["#notificationemail"] == null ? "" : Request.Cookies["#notificationemail"].Value),
				Request.ServerVariables["REMOTE_ADDR"],
				Request.ServerVariables["HTTP_ACCEPT_LANGUAGE"],
				Request.ServerVariables["HTTP_USER_AGENT"],
				(Request.UrlReferrer == null ? "" : Request.UrlReferrer.ToString())
			);

			//var httpContextBase = new HttpContextWrapper(HttpContext.Current);
			//_555Lottery.Service.LotteryService.Instance.Initialize(httpContextBase, true);
		}

		protected void Session_End(object sender, EventArgs e)
		{
			DateTime StartedAt = (DateTime)Session["StartedAt"];
			LotteryService.Instance.Log(LogLevel.Information, "SESSIONEND", "{0}: session ended and lasted for {1}", new SessionInfo(null, Session.SessionID), (DateTime.UtcNow - StartedAt).ToString("c"));
		}
	}
}