using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class OtpRecord
{
    public int Id { get; set; }

    public int EmpId { get; set; }

    public string Email { get; set; } = null!;

    public string Otp { get; set; } = null!;

    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; }
}
