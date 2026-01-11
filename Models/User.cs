using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models;

[Table("User")] 

public class User
{
    public int Id { get; set; }
    
    public string Firstname { get; set; }
    
    public string Surname { get; set; }
    
    public string Email { get; set; }
        
    public string Username { get; set; }
    
    public string Password { get; set; }
    [NotMapped]

    public string ConfirmPassword { get; set; }

    public bool Active { get; set; }
    
    public int UserType { get; set; } 
    
    public DateTime? CreationDate { get; set; }
}
