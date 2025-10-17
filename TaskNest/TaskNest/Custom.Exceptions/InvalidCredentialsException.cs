namespace TaskNest.Custom.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException(int errorCode, string message)
        : base(errorCode, message) { }
    }
}
