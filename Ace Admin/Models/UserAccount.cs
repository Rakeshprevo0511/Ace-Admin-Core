using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class UserAccount
{
    public int Accid { get; set; }

    public int? Empid { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Role Role { get; set; } = null!;
}
