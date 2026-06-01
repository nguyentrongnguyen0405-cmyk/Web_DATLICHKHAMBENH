using System.Text;
using System.Text.Json;

namespace Web_Đặt_lịch_phòng_khám.Services
{
    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint = "https://api.openai.com/v1/chat/completions";

        public OpenAiService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("OpenAI API key is missing. Add 'OpenAI:ApiKey' in appsettings.json");
        }

        public async Task<string> GetChatResponse(string userMessage)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Bạn là trợ lý ảo của phòng khám Đặt lịch. Hãy trả lời các câu hỏi về đặt lịch khám, triệu chứng, giá dịch vụ, bác sĩ, giờ làm việc, địa chỉ, thanh toán... Hãy trả lời ngắn gọn, thân thiện, bằng tiếng Việt. Nếu câu hỏi không liên quan, hãy từ chối nhẹ nhàng." },
                    new { role = "user", content = userMessage }
                },
                max_tokens = 300,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(_endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Lỗi OpenAI: {responseString}";

            using var doc = JsonDocument.Parse(responseString);
            var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return reply ?? "Xin lỗi, tôi chưa có câu trả lời.";
        }
    }
}