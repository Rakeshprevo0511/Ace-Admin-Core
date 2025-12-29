using System;
using System.Collections.Generic;

namespace Ace_Admin.Models;

public partial class UserMessage
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public string? ReceiverId { get; set; }

    public bool IsRead { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public int? ChatRoomId { get; set; }

    public string? AttachmentUrl { get; set; }

    public string? MessageType { get; set; }
}
