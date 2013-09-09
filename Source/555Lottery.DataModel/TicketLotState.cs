using System;
using System.Collections.Generic;

namespace _555Lottery.DataModel
{
	[Flags]
	public enum TicketLotState
	{
		NotSet = 0,

		/// <summary>
		/// Initial state after lot creation
		/// </summary>
		WaitingForPayment = 1,
		InvalidTimeUp = 2,
		InvalidUnknownAmountReceived = 4,
		InvalidConfirmedTooLate = 8,
		RefundInitiated = 16,

		/// <summary>
		/// At the deadline we get here if the previous state was PaymentConfirmed
		/// </summary>
		ConfirmedNotEvaluated = 32,

		/// <summary>
		/// We get here from ConfirmNotEvaluated state if the ticketlot has at least one winning game
		/// </summary>
		EvaluatedNotWon = 64,

		/// <summary>
		/// We get here from ConfirmNotEvaluated state if the ticketlot doesn't have any games which won prizes
		/// </summary>
		EvaluatedWonPaymentPending = 128,
		WonPaymentInitiated = 256,


		RefundConfirmed = 512,
		
		/// <summary>
		/// We get here anytime before the deadline if the ticketlot's transaction has at least 6 confirmations
		/// </summary>
		PaymentConfirmed = 1024,

		/// <summary>
		/// We get here anytime before the deadline if the ticketlot's transaction has 1 to 5 confirmations
		/// </summary>
		TooFewConfirmations = 2048,
	}
}