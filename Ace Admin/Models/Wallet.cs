using System.ComponentModel.DataAnnotations;

namespace Ace_Admin.Models
{
    public class Wallet
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public double Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
