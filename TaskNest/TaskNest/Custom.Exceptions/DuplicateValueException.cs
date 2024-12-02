namespace TaskNest.Custom.Exceptions
{
    public class DuplicateValueException : Exception
    {
        public DuplicateValueException() : base() { }
        public DuplicateValueException(string message) : base(message) { }
        public DuplicateValueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
