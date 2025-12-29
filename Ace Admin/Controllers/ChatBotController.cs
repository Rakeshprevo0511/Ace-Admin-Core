using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        #region --- Chat with OpenAI API ---
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            var messages = new List<object>
            {
                new { role = "system", content = "You are a helpful AI assistant for an employee management system. Answer questions about the system, navigation, and features concisely and professionally." }
            };

            if (request.History?.Any() == true)
                foreach (var msg in request.History.TakeLast(10))
                    messages.Add(new { role = msg.Role, content = msg.Content });

            messages.Add(new { role = "user", content = request.Message });

            string responseText = await CallOpenAI(messages);

            return Ok(new { success = true, response = responseText, timestamp = DateTime.UtcNow });
        }

        private async Task<string> CallOpenAI(List<object> messages)
        {
            try
            {
                var requestBody = new { model = "gpt-3.5-turbo", messages, max_tokens = 500, temperature = 0.7 };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OPENAI_API_KEY}");

                var response = await httpClient.PostAsync(OPENAI_ENDPOINT, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"OpenAI API error: {responseString}";

                var result = JsonConvert.DeserializeObject<OpenAIResponse>(responseString);
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response generated";
            }
            catch { return GetFallbackResponse(); }
        }
        #endregion

        #region --- Local Dummy Chat (Offline Mode) ---
        [HttpPost("ChatFree")]
        public IActionResult ChatFree([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { success = false, message = "Message is required" });

            return Ok(new { success = true, response = GetDummyResponse(request.Message), timestamp = DateTime.UtcNow });
        }
        #endregion

        #region --- Dummy AI Logic ---
        private string GetDummyResponse(string message)
        {
            var lower = message.ToLower().Trim();

            if (ContainsAny(lower, "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening"))
                return "Hello! 👋 Welcome to **Ace Admin**!\n\nI'm your AI assistant. Type **'help'** to see what I can do.";

            if (ContainsAny(lower, "help", "capabilities", "assist me"))
                return "**I can help you with:** Dashboard, Profile, Security, Tasks, Reports, and Settings. Try asking: *'How do I change my password?*";

            if (ContainsAny(lower, "dashboard", "home", "overview"))
                return "**Dashboard Overview:** Shows KPIs, activities, and quick actions. Use the sidebar to navigate.";

            if (ContainsAny(lower, "profile", "account"))
                return "**Profile Management:** Go to Profile → Edit → Update info and save.";

            if (ContainsAny(lower, "password", "security"))
                return "**Password & Security:** Profile → Security → Change password. Must be 8+ chars with symbols.";

            if (ContainsAny(lower, "logout", "sign out"))
                return "**Logout:** Click avatar → Logout. Ends session and clears temp data.";

            if (ContainsAny(lower, "settings", "preferences"))
                return "**Settings:** Gear icon → Change theme, language, notifications, and privacy.";

            if (ContainsAny(lower, "task", "project"))
                return "**Tasks:** Tasks → +New → Fill details → Create. Track progress in Projects tab.";

            if (ContainsAny(lower, "report", "analytics"))
                return "**Reports:** Reports → Choose type → Filter by date → Export to PDF/Excel.";

            if (ContainsAny(lower, "support", "contact"))
                return "**Support:** Email support@aceadmin.com or use Help → Report Issue.";

            if (ContainsAny(lower, "thank", "thanks"))
                return "You're welcome! 😊 Always here to help.";

            if (ContainsAny(lower, "bye", "goodbye"))
                return "Goodbye! 👋 Come back anytime.";

            return "I'm here to help! 🤖 Try asking: *'How do I create a task?'* or *'Show me the dashboard features.'*";
        }
        #endregion

        #region --- Utility Methods ---
        private bool ContainsAny(string input, params string[] keywords) => keywords.Any(k => input.Contains(k));
        private string GetFallbackResponse() => new[] { "I'm here to help!", "Please try again later.", "Can you rephrase that?" }[new Random().Next(3)];
        private bool IsWordMatch(string input, string target, int maxDistance = 2) => LevenshteinDistance(input, target) <= maxDistance;

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }

            return d[s.Length, t.Length];
        }
        #endregion
    }

    #region --- Models ---
    public class ChatRequest { public string Message { get; set; } public List<ChatMessage> History { get; set; } }
    public class ChatMessage { public string Role { get; set; } public string Content { get; set; } }
    public class OpenAIResponse { public List<Choice> Choices { get; set; } }
    public class Choice { public Message Message { get; set; } }
    public class Message { public string Role { get; set; } public string Content { get; set; } }
    #endregion
}
