using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OrderId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? PaymentMode { get; set; }

    public string Status { get; set; } = null!;

    public string? RawGatewayResponse { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
