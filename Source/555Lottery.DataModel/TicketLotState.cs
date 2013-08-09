using System;
using System.Collections.Generic;

namespace _555Lottery.DataModel
{
	public enum TicketLotState { NotSet = 0, WaitingForPayment = 1, PaymentConfirmed = 2, Evaluated = 3, NotEnoughConfirmations = 4, TooLateFirstConfirmation = 5 }
}