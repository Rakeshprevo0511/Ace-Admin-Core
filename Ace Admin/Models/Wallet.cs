using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class Wallet
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public double Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee User { get; set; } = null!;
}
