using System.ComponentModel.DataAnnotations;

namespace Ace_Admin.Dto
{
    public class LoginView
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
