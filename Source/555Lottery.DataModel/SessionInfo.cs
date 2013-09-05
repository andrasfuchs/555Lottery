using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _555Lottery.DataModel
{
	public class SessionInfo
	{
		public string IPAddress { get; set; }
		public string SessionId { get; set; }

		public SessionInfo(string ipAddress, string sessionId)
		{
			this.IPAddress = ipAddress;
			this.SessionId = sessionId;
		}

		public override string ToString()
		{
			return this.SessionId;
		}
	}
}
