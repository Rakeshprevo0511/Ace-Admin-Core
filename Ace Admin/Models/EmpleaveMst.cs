using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class EmpleaveMst
{
    public long Id { get; set; }

    public string? LeaveType { get; set; }

    public string? LeaveDate { get; set; }

    public string? Reason { get; set; }

    public DateTime? Addeddate { get; set; }

    public DateTime? Modifydate { get; set; }

    public long? EmpId { get; set; }

    public bool? IsProbation { get; set; }

    public long? ApprovedBy { get; set; }
}
