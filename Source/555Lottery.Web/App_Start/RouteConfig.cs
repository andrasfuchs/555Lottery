﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace _555Lottery.Web
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "CheckHash",
				url: "CheckHash/{sequence}/{hash}",
				defaults: new { controller = "Home", action = "CheckHash" }
			);

			routes.MapRoute(
				name: "Affiliate",
				url: "Affiliate/{userId}/{date}",
				defaults: new { controller = "Home", action = "Affiliate", userId = UrlParameter.Optional, date = UrlParameter.Optional }
			);

			routes.MapRoute(
				name: "Default",
				url: "{action}/{id}",
				defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			);

			//routes.MapRoute(
			//	name: "Default",
			//	url: "{controller}/{action}/{id}",
			//	defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
			//);
		}
	}
}