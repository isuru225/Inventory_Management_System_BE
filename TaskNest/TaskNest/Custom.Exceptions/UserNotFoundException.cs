namespace TaskNest.Custom.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public int ErrorCode { get; }

        public UserNotFoundException(int errorCode)
            : base($"An error occurred with code {errorCode}.")
        {
            ErrorCode = errorCode;
        }

        public UserNotFoundException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public UserNotFoundException(int errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }
    }
}
