using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.DataModel
{
	public enum TicketLotState { NotSet = 0, WaitingForPayment = 1, PaymentConfirmed = 2, Evaluated = 3 }
}