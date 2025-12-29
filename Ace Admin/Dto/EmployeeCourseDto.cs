namespace Ace_Admin.Dto
{
    public class EmployeeCourseDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public TimeOnly? StudyHours { get; set; }
    }
}
