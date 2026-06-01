namespace Web_Đặt_lịch_phòng_khám.Services
{
    public interface IOpenAiService
    {
        Task<string> GetChatResponse(string userMessage);
    }
}