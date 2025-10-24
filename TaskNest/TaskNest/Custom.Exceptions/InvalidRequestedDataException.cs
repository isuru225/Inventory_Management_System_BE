namespace TaskNest.Custom.Exceptions
{
    public class InvalidRequestedDataException : Exception
    {
        public int ErrorCode { get; }   // Custom error code
        public IEnumerable<string> Errors { get; }

        public InvalidRequestedDataException(int errorCode, IEnumerable<string> errors)
            : base("DTO validation failed")
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
