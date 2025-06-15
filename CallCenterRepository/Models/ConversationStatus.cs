using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("ConversationStatuses")]
public class ConversationStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkConversationStatus { get; set; }

    [ForeignKey("FkTalk")]
    public Conversation Conversation { get; set; }
    public int FkTalk { get; set; }

    [ForeignKey("FkProjectStatus")]
    public ProjectStatus ProjectStatus { get; set; }
    public int FkProjectStatus { get; set; }

    public DateTime SelectedTime { get; set; } = DateTime.UtcNow; // Когда статус был выбран
}