namespace TaskNest.Custom.Exceptions
{
    public class InvalidRequestedDataException : Exception
    {
        public InvalidRequestedDataException() : base() { }
        public InvalidRequestedDataException(string message) : base(message) { }
        public InvalidRequestedDataException(string message, Exception innerException) : base(message, innerException) { }
    }
}
