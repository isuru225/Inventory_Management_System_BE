namespace TaskNest.Custom.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public int ErrorCode { get; }   // Custom error code
        public string Errors { get; }

        public InvalidCredentialsException(int errorCode, string errors)
            : base("Password is incorrect!")
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
