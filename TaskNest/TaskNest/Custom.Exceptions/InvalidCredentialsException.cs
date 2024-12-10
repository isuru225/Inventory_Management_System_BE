namespace TaskNest.Custom.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public int ErrorCode { get; }

        public InvalidCredentialsException(int errorCode)
            : base($"An error occurred with code {errorCode}.")
        {
            ErrorCode = errorCode;
        }

        public InvalidCredentialsException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public InvalidCredentialsException(int errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }
    }
}
