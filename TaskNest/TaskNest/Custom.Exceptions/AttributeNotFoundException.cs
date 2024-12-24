namespace TaskNest.Custom.Exceptions
{
    public class AttributeNotFoundException : Exception
    {
        public AttributeNotFoundException() : base() { }
        public AttributeNotFoundException(string message) : base(message) { }
        public AttributeNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
