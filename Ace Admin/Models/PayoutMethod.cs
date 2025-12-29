using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class PayoutMethod
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public string? UpiId { get; set; }

    public string? AccountNo { get; set; }

    public string? HolderName { get; set; }

    public string? CardNetwork { get; set; }

    public string? CardLast4 { get; set; }

    public string? CardType { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnable { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
}
