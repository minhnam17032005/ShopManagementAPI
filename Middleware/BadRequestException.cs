namespace Demo_Course_Management.Middleware
{
    public class BadRequestException : Exception
    {
        public List<string>? Errors { get; }

        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(List<string> errors)
            : base("Dữ liệu không hợp lệ.")
        {
            Errors = errors;
        }
    }
}
