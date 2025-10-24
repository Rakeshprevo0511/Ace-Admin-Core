using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class EmployeeCourse
{
    public int EmployeeCourseId { get; set; }

    public int EmployeeId { get; set; }

    public int CourseId { get; set; }

    public DateTime CompletionDate { get; set; }

    public string Status { get; set; } = null!;

    public TimeOnly? StudyHours { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
