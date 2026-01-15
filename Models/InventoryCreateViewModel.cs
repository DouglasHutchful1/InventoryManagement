using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models;

public class InventoryCreateViewModel
{
    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MaxLength(50)]
    public string SKU { get; set; }

    [MaxLength(50)]
    public string Category { get; set; }

    public int Quantity { get; set; } = 0;
    public int ReorderLevel { get; set; } = 10;
    public decimal? Price { get; set; }
}
