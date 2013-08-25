using System;
using System.Collections.Generic;

namespace _555Lottery.DataModel
{
	[Flags]
	public enum TicketLotState
	{
		NotSet = 0,

		WaitingForPayment = 1,
		InvalidTimeUp = 2,
		InvalidUnknownAmountReceived = 4,
		InvalidConfirmedTooLate = 8,
		RefundInitiated = 16,

		ConfirmedNotEvaluated = 32,
		EvaluatedNotWon = 64,
		EvaluatedWonPaymentPending = 128,
		WonPaymentInitiated = 256,

		RefundConfirmed = 512,
		PaymentConfirmed = 1024,

		TooFewConfirmations = 2048,
	}
}