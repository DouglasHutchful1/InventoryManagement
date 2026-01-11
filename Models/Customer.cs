using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; }
        public User User { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}