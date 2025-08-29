using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string EmpName { get; set; } = null!;

    public string? Position { get; set; }

    public string? Location { get; set; }

    public int? Age { get; set; }

    public int? Salary { get; set; }

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string? Password { get; set; }

    public string? Username { get; set; }

    public DateTime? JoiningDate { get; set; }

    public string? FilePathPic { get; set; }

    public virtual ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();
}
