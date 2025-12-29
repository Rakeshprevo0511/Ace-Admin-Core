using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class Transaction
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public double Amount { get; set; }

    public string Status { get; set; } = null!;

    public string PaymentId { get; set; } = null!;

    public string ReferenceId { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
