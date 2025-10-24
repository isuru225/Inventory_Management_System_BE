namespace TaskNest.Custom.Exceptions
{
    //public class DuplicateValueException : Exception
    //{
    //    public DuplicateValueException() : base() { }
    //    public DuplicateValueException(string message) : base(message) { }
    //    public DuplicateValueException(string message, Exception innerException) : base(message, innerException) { }
    //}

    public class DuplicateValueException : Exception
    {
        public int ErrorCode { get; }   // Custom error code
        public string Errors { get; }

        public DuplicateValueException(int errorCode, string errors)
            : base("Values are duplicated.Value already exists")
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
