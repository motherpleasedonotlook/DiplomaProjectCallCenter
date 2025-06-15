using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CallCenterRepository.Models;

public class Operator
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkOperator { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(30)]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(20)]
    public string Password { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    //внешний ключ
    [Required(ErrorMessage = "Admin ID is required.")]
    [ForeignKey("FkAdmin")]
    public int FkAdmin { get; set; }
    
    public Admin Admin { get; set; }
    
    //конструктор
    public Operator(string username, string password, int fkAdmin)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        }
        
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        Username = username;
        Password = password;
        FkAdmin = fkAdmin;
    }
}