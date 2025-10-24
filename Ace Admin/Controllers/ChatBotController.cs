using System;
using System.Collections.Generic;
using Ace_Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Ace_Admin.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // JWT required
    public class ChatBotController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string OPENAI_API_KEY = "your-api-key-here";
        private const string OPENAI_ENDPOINT = "https://api.openai.com/v1/chat/completions";

        // POST: api/ChatBot/chat
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            // Build conversation history
            var messages = new List<object>
            {
                new
                {
                    role = "system",
                    content = "You are a helpful AI assistant for an employee management system. " +
                              "Answer questions about the system, navigation, and features concisely and professionally."
                }
            };

            if (request.History != null && request.History.Any())
            {
                foreach (var msg in request.History.TakeLast(10))
                    messages.Add(new { role = msg.Role, content = msg.Content });
            }

            messages.Add(new { role = "user", content = request.Message });

            string responseText = await CallOpenAI(messages);

            return Ok(new
            {
                success = true,
                response = responseText,
                timestamp = DateTime.UtcNow
            });
        }

        private async Task<string> CallOpenAI(List<object> messages)
        {
            try
            {
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages,
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_KEY}");

                var response = await httpClient.PostAsync(OPENAI_ENDPOINT, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"OpenAI API error: {responseString}";

                var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseString);
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated";
            }
            catch
            {
                return GetFallbackResponse();
            }
        }
        // POST: api/ChatBot/chat
        [HttpPost("ChatFree")]
        public IActionResult ChatFree([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            string responseText = GetDummyResponse(request.Message);

            return Ok(new
            {
                success = true,
                response = responseText,
                timestamp = DateTime.UtcNow
            });
        }

        private string GetDummyResponse(string message)
        {
            var lower = message.ToLower().Trim();

            // HELP / CAPABILITIES
            if (lower.Contains("help") || lower.Contains("what can you") || lower.Contains("capabilities") ||
                IsWordMatch(lower, "help") || IsWordMatch(lower, "capabilities"))
            {
                return "I can help you with:\n\n" +
                       "📊 **Dashboard & Navigation**\n" +
                       "• Understanding your dashboard metrics\n" +
                       "• Navigating different sections\n" +
                       "• Finding specific features\n\n" +
                       "👤 **Profile & Account**\n" +
                       "• Viewing and editing your profile\n" +
                       "• Changing account settings\n" +
                       "• Managing your preferences\n\n" +
                       "🔐 **Security**\n" +
                       "• Login and logout procedures\n" +
                       "• Password management\n" +
                       "• Session handling\n\n" +
                       "What would you like to know more about?";
            }
            // DASHBOARD
            else if (lower.Contains("dashboard") || lower.Contains("main page") || lower.Contains("home") ||
                     IsWordMatch(lower, "dashboard") || IsWordMatch(lower, "home"))
            {
                return "**Dashboard Overview** 📊\n\n" +
                       "Your dashboard displays:\n" +
                       "• Key performance metrics and statistics\n" +
                       "• Recent activity and notifications\n" +
                       "• Quick access to important features\n\n" +
                       "💡 **Tip:** Use the sidebar menu on the left to navigate to different sections of the application.";
            }
            // PROFILE / ACCOUNT
            else if (lower.Contains("profile") || lower.Contains("account") || lower.Contains("personal info") ||
                     lower.Contains("edit my profile") || lower.Contains("update profile") || lower.Contains("change profile") ||
                     IsWordMatch(lower, "profile") || IsWordMatch(lower, "account"))
            {
                return "**Managing Your Profile** 👤\n\n" +
                       "To view or edit your profile:\n" +
                       "1. Click on your avatar in the top-right corner\n" +
                       "2. Select **'Profile'** from the dropdown menu\n" +
                       "3. Update your information and save changes\n\n" +
                       "You can update your name, email, photo, and other personal details.";
            }
            // LOGOUT
            else if (lower.Contains("logout") || lower.Contains("sign out") || lower.Contains("log out") ||
                     IsWordMatch(lower, "logout") || IsWordMatch(lower, "sign out"))
            {
                return "**Logging Out** 🚪\n\n" +
                       "To logout securely:\n" +
                       "1. Click on your avatar in the top-right corner\n" +
                       "2. Select **'Logout'** from the dropdown menu\n\n" +
                       "This will end your session and you'll need to login again to access the system.";
            }
            // SETTINGS
            else if (lower.Contains("settings") || lower.Contains("preferences") || lower.Contains("configure") ||
                     IsWordMatch(lower, "settings") || IsWordMatch(lower, "preferences"))
            {
                return "**Settings & Preferences** ⚙️\n\n" +
                       "You can access settings by:\n" +
                       "• Clicking the gear icon ⚙️ in the navigation bar\n" +
                       "• Or through your profile menu in the top-right\n\n" +
                       "Here you can customize your experience and manage application settings.";
            }
            // GREETINGS
            else if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey") ||
                     IsWordMatch(lower, "hello") || IsWordMatch(lower, "hi"))
            {
                return "Hello! 👋 Welcome to Ace Admin!\n\n" +
                       "I'm your AI assistant, here to help you navigate and use the system effectively.\n\n" +
                       "Feel free to ask me about:\n" +
                       "• Features and functionality\n" +
                       "• Navigation and menu items\n" +
                       "• Account management\n" +
                       "• Or anything else you'd like to know!";
            }
            // THANKS
            else if (lower.Contains("thank") || lower.Contains("thanks") || IsWordMatch(lower, "thank") || IsWordMatch(lower, "thanks"))
            {
                return "You're very welcome! 😊\n\n" +
                       "I'm always here to help. If you have any more questions, don't hesitate to ask!";
            }
            // DEFAULT / FALLBACK
            else
            {
                return "I'm here to help! 🤖\n\n" +
                       "I can assist you with information about:\n\n" +
                       "📊 **Dashboard** - Overview and metrics\n" +
                       "👤 **Profile** - Account management\n" +
                       "🧭 **Navigation** - Finding features\n" +
                       "⚙️ **Settings** - Preferences and configuration\n" +
                       "🔔 **Notifications** - Alerts and messages\n" +
                       "✅ **Tasks** - Project management\n\n" +
                       "What specific feature would you like to know about?";
            }
        }
        private string GetFallbackResponse()
        {
            var fallbackResponses = new[]
            {
                "I'm here to help! Could you please rephrase your question?",
                "I'm currently experiencing some issues. Please try again shortly.",
                "I'd love to help! Can you provide more details?"
            };
            return fallbackResponses[new Random().Next(fallbackResponses.Length)];
        }
        private bool IsWordMatch(string input, string target, int maxDistance = 2)
        {
            return LevenshteinDistance(input, target) <= maxDistance;
        }
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t))
                return s.Length;

            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[s.Length, t.Length];
        }
    }

    // Models
    public class ChatRequest
    {
        public string Message { get; set; }
        public List<ChatMessage> History { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
