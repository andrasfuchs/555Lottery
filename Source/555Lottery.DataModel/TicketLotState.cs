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

		/// <summary>
		/// The deadline is up, and no bitcoin transaction was detected
		/// </summary>
		InvalidTimeUp = 2,

		/// <summary>
		/// We got some money, but the amount was invalid, so we need to invalidate the ticket and refund the money
		/// </summary>
		InvalidUnknownAmountReceived = 4,

		/// <summary>
		/// The transaction was initiated after the deadline, so the ticketlot is invalid
		/// </summary>
		InvalidConfirmedTooLate = 8,
		
		/// <summary>
		/// The refund was initiated, but it's not confirmed by the bitcoin network yet
		/// </summary>
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
		EvaluatedPrizePaymentPending = 128,
		
		/// <summary>
		/// The prize payment was initiated, but it's not confirmed by the bitcoin network yet
		/// </summary>
		PrizePaymentInitiated = 256,

		/// <summary>
		/// Refund payment is confirmed by the bitcoin network
		/// </summary>
		RefundConfirmed = 512,
		
		/// <summary>
		/// We get here anytime before the deadline if the ticketlot's transaction has at least 6 confirmations
		/// </summary>
		PaymentConfirmed = 1024,

		/// <summary>
		/// We get here anytime before the deadline if the ticketlot's transaction has 1 to 5 confirmations
		/// </summary>
		TooFewConfirmations = 2048,

		/// <summary>
		/// Prize payment is confirmed by the bitcoin network
		/// </summary>
		PrizePaymentConfirmed = 4096,
	}
}