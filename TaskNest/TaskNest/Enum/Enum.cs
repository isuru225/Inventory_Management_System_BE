namespace TaskNest.Enum
{
    public enum NotificationEnum
    {
        NOTIFICATION_TYPE_REORDER = 0,
        NOTIFICATION_TYPE_EXPIRE = 1

    }

    public enum ItemType 
    {
        RAW_DRUG = 0,
        FINISHED_DRUG = 1,
        GENERAL = 2
    }

    public enum ErrorCodes : int
    {
        INVALID_PASSWORD = 100,
        INVALID_EMAIL = 101
    }
}
