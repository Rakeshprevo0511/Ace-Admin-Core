namespace Ace_Admin.Dto
{
    public class VerifyPaymentDTO
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string? Method { get; set; } // "upi", "bank", "card" for mocking
    }
}
