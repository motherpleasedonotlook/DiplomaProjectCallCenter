using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("Conversations")] 
public class Conversation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkTalk { get; set; }

    [Required(ErrorMessage = "Starting time is required.")]
    public DateTime TimeStarted { get; set; }

    [Required(ErrorMessage = "Ending time is required.")]
    public DateTime TimeEnded { get; set; }
    
    [Required(ErrorMessage = "Path to audio file is required.")]
    [MaxLength(1000)]
    public string PathToAudio { get; set; }

    // Внешние ключи
    [Required(ErrorMessage = "Operator ID is required.")]
    [ForeignKey("Operator")]
    public int FkOperator { get; set; }
    
    public Operator Operator { get; set; }

    [Required(ErrorMessage = "Client ID is required.")]
    [ForeignKey("Client")]
    public int FkClient { get; set; }
    
    public Client Client { get; set; }

    [Required(ErrorMessage = "Project ID is required.")]
    [ForeignKey("Project")]
    public int FkProject { get; set; }
    
    public Project Project { get; set; }

    public ICollection<ConversationStatus> SelectedStatuses { get; set; } = new List<ConversationStatus>();
    public ICollection<ClientNote> Notes { get; set; } = new List<ClientNote>();

    // Конструктор без параметров для EF Core
    public Conversation() { }

    // Основной конструктор
    public Conversation(
        DateTime timeStarted, 
        DateTime timeEnded, 
        string pathToAudio, 
        int fkOperator, 
        int fkClient,
        int fkProject)
    {
        if (string.IsNullOrWhiteSpace(pathToAudio))
        {
            throw new ArgumentException("Path to audio cannot be null or empty.", nameof(pathToAudio));
        }

        if (timeEnded < timeStarted)
        {
            throw new ArgumentException("End time cannot be earlier than start time.");
        }

        TimeStarted = timeStarted;
        TimeEnded = timeEnded;
        PathToAudio = pathToAudio;
        FkOperator = fkOperator;
        FkClient = fkClient;
        FkProject = fkProject;
    }

    // Дополнительный метод для добавления статусов
    public void AddStatus(ProjectStatus status)
    {
        SelectedStatuses.Add(new ConversationStatus
        {
            ProjectStatus = status,
            SelectedTime = DateTime.UtcNow
        });
    }
}