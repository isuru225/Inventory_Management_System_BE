namespace TaskNest.Frontend.Models
{
    public class EditHistoryInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ItemName { get; set; }
        public int ChangedAmount { get; set; }
        public bool IsReducedAmount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
