namespace Ace_Admin.Models
{
    public class Transaction
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } // "deposit" or "withdraw"
        public double Amount { get; set; }
        public string Status { get; set; } // "pending", "success", "failed"
        public string PaymentId { get; set; } // payout id
        public string ReferenceId { get; set; } // Razorpay payment id
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
