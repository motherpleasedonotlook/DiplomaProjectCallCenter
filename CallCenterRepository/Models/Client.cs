using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CallCenterRepository.Models;

public class Client
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkClient { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(12)]
    public string PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Client`s name is required.")]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [ForeignKey("FkProject")]
    public Project Project { get; set; }
    
    public int FkProject { get; set; }

    
    public ClientStatus Status { get; set; } = ClientStatus.NotProcessed;
    
    //конструктор
    public Client(string phoneNumber, string name, int fkProject)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));
        }
    
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Client name cannot be null or empty.", nameof(name));
        }

        PhoneNumber = phoneNumber;
        Name = name;
        FkProject = fkProject;  // Устанавливаем связь с проектом
    }
}
public enum ClientStatus
{
    Processed,
    NotProcessed,
    InvalidNumber,
    Recall
}
