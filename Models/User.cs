using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Firstname { get; set; }

        [Required, MaxLength(100)]
        public string Surname { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required, MaxLength(255)]
        public string Password { get; set; }
        [NotMapped]
        public string ConfirmPassword { get; set; }

        public bool Active { get; set; } = true;

        public DateTime? CreationDate { get; set; }

        public int UserType { get; set; } = 0;

        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<Customer> Customers { get; set; }
        public ICollection<Supplier> Suppliers { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; }
    }
}