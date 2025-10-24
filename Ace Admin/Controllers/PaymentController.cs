using Ace_Admin.Dto;
using Ace_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Ace_Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PracticeDbContext _context;
        private readonly IConfiguration _config;
        public PaymentController(ILogger<HomeController> logger, PracticeDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDTO req)
        {
            try
            {
                long userId = 1; // TODO: Replace with actual authenticated user ID

                // 🔍 Validate input
                if (string.IsNullOrEmpty(req.RazorpayOrderId) ||
                    string.IsNullOrEmpty(req.RazorpayPaymentId) ||
                    string.IsNullOrEmpty(req.RazorpaySignature))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Missing required fields"));
                }

                // 💳 Mock payment details (replace this with actual Razorpay API verification)
                var payment = new
                {
                    Id = req.RazorpayPaymentId,
                    Method = req.Method?.ToLower() ?? "bank",
                    Amount = 500000, // paise
                    Email = "user@example.com",
                    Card = new { Network = "Visa", Last4 = "4242", Type = "credit", Name = "John Doe" },
                    Bank = "HDFC",
                    Vpa = "test@upi"
                };

                // 💾 Save payout method
                var payout = new PayoutMethod
                {
                    UserId = userId,
                    IsDefault = false,
                    IsEnable = true,
                    IsDeleted = false
                };

                switch (payment.Method)
                {
                    case "upi":
                        payout.Type = "upi";
                        payout.UpiId = payment.Vpa;
                        break;

                    case "card":
                        payout.Type = "card";
                        payout.HolderName = payment.Card.Name;
                        break;

                    case "bank":
                        payout.Type = "bank";
                        payout.AccountNo = payment.Bank;
                        payout.HolderName = payment.Email;
                        break;

                    default:
                        return BadRequest(ApiResponse<object>.BadRequest("Unsupported payment method"));
                }

                _context.PayoutMethods.Add(payout);
                await _context.SaveChangesAsync();

                // 💰 Update or create wallet
                double amount = payment.Amount / 100.0;
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    wallet = new Wallet { UserId = userId, Balance = amount };
                    _context.Wallets.Add(wallet);
                }
                else
                {
                    wallet.Balance += amount;
                }

                await _context.SaveChangesAsync();

                // 🧾 Record transaction
                var transaction = new Transaction
                {
                    UserId = userId,
                    Type = "deposit",
                    Amount = amount,
                    Status = "success",
                    PaymentId = payout.Id.ToString(),
                    ReferenceId = payment.Id,
                    Description = $"Deposit of ₹{amount} via {payment.Method}"
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // 📦 Prepare response data
                var responseData = new VerifyPaymentResponseDTO
                {
                    OrderId = req.RazorpayOrderId,
                    PaymentId = req.RazorpayPaymentId,
                    Amount = amount,
                    Payout = payout,
                    Wallet = wallet,
                    Transaction = transaction
                };

                // ✅ Return standardized success response
                return Ok(ApiResponse<VerifyPaymentResponseDTO>.Ok(
                    responseData,
                    $"Payment verified ({payment.Method}), wallet updated, transaction recorded"
                ));
            }
            catch (Exception ex)
            {
                // ❌ Catch any unexpected errors
                return StatusCode(500, ApiResponse<object>.InternalServerError($"Error verifying payment: {ex.Message}"));
            }
        }

        

    }

}
