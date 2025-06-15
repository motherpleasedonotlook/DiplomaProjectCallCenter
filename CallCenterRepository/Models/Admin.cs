using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;

namespace CallCenterRepository.Models;
[Table("Admins")]
public class Admin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkAdmin { get; set; }
    
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(30)]
    public string Username { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(100)]
    public string Password { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public int SelfOperatorProfile { get; set; }
    
    public Admin(string username, string password, int selfOperatorProfile)
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
        SelfOperatorProfile = selfOperatorProfile;
    }
}