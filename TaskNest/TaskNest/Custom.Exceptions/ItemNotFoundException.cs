namespace TaskNest.Custom.Exceptions
{
    public class ItemNotFoundException : Exception
    {
        public int ErrorCode { get; }   // Custom error code
        public string Errors { get; }

        public ItemNotFoundException(int errorCode, string errors)
            : base("Item not found!")
        {
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
