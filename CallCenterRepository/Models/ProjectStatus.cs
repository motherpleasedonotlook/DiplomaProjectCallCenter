using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("ProjectStatuses")]
public class ProjectStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkProjectStatus { get; set; }

    [Required]
    [MaxLength(100)]
    public string StatusName { get; set; }

    // Связи
    [ForeignKey("FkStatusGroup")]
    public ProjectStatusGroup StatusGroup { get; set; }
    public int? FkStatusGroup { get; set; }
}