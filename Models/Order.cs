using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public decimal? TotalAmount { get; set; }

        public int CreatedBy { get; set; }
        public User User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}