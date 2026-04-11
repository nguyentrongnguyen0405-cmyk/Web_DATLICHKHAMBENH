namespace Web_Đặt_lịch_phòng_khám.Services
{
    public interface IQRCodeService
    {
        byte[] GenerateQRCode(string text);
    }
}