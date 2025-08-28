using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class UserToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }
}
