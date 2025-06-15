using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CallCenterRepository.Models;

public class ConversationGrade
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkGrade { get; set; }

    [Required(ErrorMessage = "Grade type is required.")]
    public ConversationGradeType GradeType { get; set; }

    [Required(ErrorMessage = "Score value is required.")]
    [Range(0, 5)]
    public int Score { get; set; }

    [Required(ErrorMessage = "Conversation ID is required.")]
    [ForeignKey("FkConversation")]
    public int FkConversation { get; set; }
    
    public Conversation Conversation { get; set; }
    
    //конструктор
    public ConversationGrade(ConversationGradeType gradeType, int score, int fkConversation)
    {
        if (!Enum.IsDefined(gradeType))
        {
            throw new ArgumentException("Invalid grade type.", nameof(gradeType));
        }
        GradeType = gradeType;
        Score = score;
        FkConversation = fkConversation;
    }
}
public enum ConversationGradeType
{
    Speech,
    ScriptFollowing,
    Accuracy,
    Statuses
}