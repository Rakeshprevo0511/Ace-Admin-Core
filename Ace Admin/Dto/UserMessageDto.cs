namespace Ace_Admin.Dto
{
    public class UserMessageDto
    {
        public string UserId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public string SentAt { get; set; } // <-- string for safe JS parsing
        public string DeliveredAt { get; set; }// <-- string for safe JS parsing
        public string MessageType { get; set; }
        public bool IsSeen { get; set; }
        public string? AttachmentUrl { get; set; }
    }
}
