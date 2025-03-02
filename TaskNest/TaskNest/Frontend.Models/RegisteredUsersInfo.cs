namespace TaskNest.Frontend.Models
{
    public class RegisteredUsersInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string MobileNumber { get; set; }
    }
}
