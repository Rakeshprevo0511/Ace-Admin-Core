using Ace_Admin.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Ace_Admin.Dto
{
    public class ChatHub : Hub
    {
        private readonly PracticeDbContext _context;
        private static readonly Dictionary<string, string> OnlineUsers = new(); // connectionId → userId

        public ChatHub(PracticeDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers[Context.ConnectionId] = userId;
                await Clients.All.SendAsync("UserOnline", userId);
            }

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            if (OnlineUsers.TryGetValue(Context.ConnectionId, out string userId))
            {
                OnlineUsers.Remove(Context.ConnectionId);
                await Clients.All.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(ex);
        }

        public Task<List<string>> GetOnlineUsers()
        {
            return Task.FromResult(OnlineUsers.Values.Distinct().ToList());
        }
        // Send a message
        public async Task SendMessage(string senderId, string receiverId, string message, string? messageType = "text", string? attachmentUrl = null)
        {
            string encryptedMessage = CryptoHelper.EncryptString(message);
            // Save to DB
            var userMessage = new UserMessage
            {
                UserId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                IsRead = false,
                MessageType = messageType,
                AttachmentUrl = attachmentUrl
            };

            _context.UserMessages.Add(userMessage);
            await _context.SaveChangesAsync();

            // Send to sender and receiver
            await Clients.Caller.SendAsync("ReceiveMessage", userMessage);
            await Clients.User(receiverId).SendAsync("ReceiveMessage", userMessage);
        }

        // Load chat history between two users
        public async Task<List<UserMessageDto>> LoadHistory(string userId, string receiverId)
        {
            return await _context.UserMessages
                .Where(m => (m.UserId == userId && m.ReceiverId == receiverId) ||
                            (m.UserId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new UserMessageDto
                {
                    UserId = m.UserId,
                    ReceiverId = m.ReceiverId,
                    Message = m.Message,
                    SentAt = m.SentAt.ToString("o"), // ISO 8601 format for JS Date
                    MessageType = m.MessageType,
                    AttachmentUrl = m.AttachmentUrl,
                    IsSeen = m.IsRead
                })
                .ToListAsync();
        }
        //encryted loadhistory
        //public async Task<List<UserMessageDto>> LoadHistory(string userId, string receiverId)
        //{
        //    var messages = await _context.UserMessages
        //        .Where(m => (m.UserId == userId && m.ReceiverId == receiverId) ||
        //                    (m.UserId == receiverId && m.ReceiverId == userId))
        //        .OrderBy(m => m.SentAt)
        //        .ToListAsync();

        //    return messages.Select(m => new UserMessageDto
        //    {
        //        UserId = m.UserId,
        //        ReceiverId = m.ReceiverId,
        //        Message = CryptoHelper.DecryptString(m.Message), // Decrypt here
        //        SentAt = m.SentAt.ToString("o"),
        //        MessageType = m.MessageType,
        //        AttachmentUrl = m.AttachmentUrl,
        //        IsSeen = m.IsRead
        //    }).ToList();
        //}
        public async Task MessageSeen(string messageId, string seenByUserId)
        {
            // Update DB
            var message = await _context.UserMessages.FindAsync(Convert.ToInt32(messageId));
            if (message != null)
            {
                message.IsRead = true;
                message.DeliveredAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // Notify sender
                await Clients.User(message.UserId.ToString())
                    .SendAsync("MessageSeenNotification", messageId, seenByUserId);
            }
        }
    }
}
