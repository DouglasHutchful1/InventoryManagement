using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
       
        [ForeignKey("CreatedBy")]  
        [BindNever]
        public User Creator { get; set; }  

        // Navigation
        [NotMapped]
        [BindNever]
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}