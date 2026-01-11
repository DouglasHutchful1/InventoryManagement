using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(255)]
        public string Action { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}