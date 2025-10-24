namespace TaskNest.Custom.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public int ErrorCode { get; }   // Custom error code
        public string Errors { get; }

        public UserNotFoundException(int errorCode, string errors)
            : base("User not found!")
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
