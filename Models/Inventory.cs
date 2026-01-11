using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string SKU { get; set; }

        [MaxLength(50)]
        public string Category { get; set; }

        public int Quantity { get; set; } = 0;

        public int ReorderLevel { get; set; } = 10;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public int CreatedBy { get; set; }
        public User User { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}