namespace Demo_Course_Management.Middleware
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
