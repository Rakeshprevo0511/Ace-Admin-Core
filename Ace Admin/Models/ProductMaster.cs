using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class ProductMaster
{
    public long ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int QuantityInStock { get; set; }

    public long? ParentId { get; set; }

    public long? ChildId { get; set; }
}
