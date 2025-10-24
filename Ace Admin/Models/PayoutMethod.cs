using System.ComponentModel.DataAnnotations;

namespace Ace_Admin.Models
{
    public class PayoutMethod
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } // "upi", "bank", "card"
        public string? UpiId { get; set; }
        public string? AccountNo { get; set; }
        public string? HolderName { get; set; }
        public string? CardNetwork { get; set; }
        public string? CardLast4 { get; set; }
        public string? CardType { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsEnable { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
