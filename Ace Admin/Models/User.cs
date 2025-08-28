﻿using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public long? Age { get; set; }
}
