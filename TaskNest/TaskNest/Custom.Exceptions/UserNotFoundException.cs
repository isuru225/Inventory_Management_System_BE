namespace TaskNest.Custom.Exceptions
{
    public class UserNotFoundException : AppException
    {
        public UserNotFoundException(int errorCode, string message)
       : base(errorCode, message) { }
    }
}
