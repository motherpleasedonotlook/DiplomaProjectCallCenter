using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CallCenterRepository.Models;

public class ClientNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkNote { get; set; }

    [Required(ErrorMessage = "Note created date is required.")]
    public DateTime DateWritten { get; set; }
    
    [Required(ErrorMessage = "Note text is required.")]
    [MaxLength(200)]
    public string Text { get; set; }
    
    [Required(ErrorMessage = "Conversation ID is required.")]
    [ForeignKey(nameof(Conversation))]
    public int FkConversation { get; set; }
    
    public Conversation Conversation { get; set; }

    public ClientNote(DateTime dateWritten, string text, int fkConversation)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Path to audio cannot be null or empty.", nameof(text));
        }

        DateWritten = dateWritten;
        Text = text;
        FkConversation = fkConversation;
    }
}