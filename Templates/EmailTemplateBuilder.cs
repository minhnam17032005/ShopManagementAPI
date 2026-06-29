namespace ShopManagement.Infrastructure.Templates;

public static class EmailTemplateBuilder
{
    public static string BuildOtpTemplate(
        string title,
        string message,
        string otp,
        int expiredMinutes)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset='utf-8'>
        <title>{title}</title>
        </head>

        <body style='font-family:Arial;background:#f5f5f5;padding:20px;'>

        <div style='max-width:600px;background:white;margin:auto;padding:30px;border-radius:10px;'>

        <h2 style='color:#2563eb'>
        Shop Management
        </h2>

        <p>Xin chào,</p>

        <p>{message}</p>

        <div style='
        font-size:32px;
        font-weight:bold;
        letter-spacing:8px;
        text-align:center;
        padding:20px;
        background:#eff6ff;
        border-radius:8px;
        margin:20px 0;'>
        {otp}
        </div>

        <p>
        OTP có hiệu lực trong
        <strong>{expiredMinutes} phút</strong>.
        </p>

        <p>Không chia sẻ mã này cho bất kỳ ai.</p>

        <hr>

        <p style='color:gray;font-size:12px'>
        Shop Management System
        </p>

        </div>

        </body>
        </html>";
    }
}