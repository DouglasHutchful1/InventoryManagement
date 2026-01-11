namespace InventoryManagementSystem.Models;

public class Sale
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal SaleAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; }
    public Inventory Product { get; set; }
}