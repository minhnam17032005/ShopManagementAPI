namespace ShopManagementAPI.Configurations
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = null!;   // Tên người gửi
        public string SenderEmail { get; set; } = null!;  // Email gửi
        public string Password { get; set; } = null!;     // App Password
        public string SmtpServer { get; set; } = null!;   // SMTP
        public int Port { get; set; }                     // Port SMTP
    }
}
