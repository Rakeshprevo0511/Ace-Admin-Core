using Ace_Admin.Models;

namespace Ace_Admin.Dto
{
    public class VerifyPaymentResponseDTO
    {
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public double Amount { get; set; }
        public PayoutMethod Payout { get; set; }
        public Wallet Wallet { get; set; }
        public Transaction Transaction { get; set; }
    }
}
