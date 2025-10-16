namespace Ace_Admin.Dto
{
    public sealed class EmpProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Title { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Location { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public int Views { get; set; }
        public int Followers { get; set; }
        public int Projects { get; set; }
        public string RevenueDisplay { get; set; } = ""; // e.g., "11k"
    }
}
