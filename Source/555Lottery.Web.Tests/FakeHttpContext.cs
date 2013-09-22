using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace _555Lottery.Web.Tests
{
	public static class MockHelper
	{
		public static HttpContext FakeHttpContext()
		{
			var httpRequest = new HttpRequest("", "http://kindermusik/", "");
			var stringWriter = new StringWriter();
			var httpResponce = new HttpResponse(stringWriter);
			var httpContext = new HttpContext(httpRequest, httpResponce);

			var sessionContainer = new HttpSessionStateContainer("lotteryunittestsessionid", new SessionStateItemCollection(),
													new HttpStaticObjectsCollection(), 10, true,
													HttpCookieMode.AutoDetect,
													SessionStateMode.InProc, false);

			httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
										BindingFlags.NonPublic | BindingFlags.Instance,
										null, CallingConventions.Standard,
										new[] { typeof(HttpSessionStateContainer) },
										null)
								.Invoke(new object[] { sessionContainer });

			return httpContext;
		}
	}
}
