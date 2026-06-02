namespace ShopManagementAPI.DTOs.Common
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public List<string>? Errors { get; set; }
    }
}
