using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class NotificationInfo
    {
        [Required(ErrorMessage = "Item type is required!")]
        public int ItemType { get; set; }
        [Required(ErrorMessage = "Notification type is required!")]
        public int NotificationType { get; set; }
        [Required(ErrorMessage = "Notification created time is required.")]
        public DateTime CreatedAt { get; set; }
        [Required(ErrorMessage = "Item name is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Item name must be alphanumeric.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Item name must be at most 20 characters.")]
        public string ItemName { get; set; }
    }
}
