using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("ProjectStatusGroups")]
public class ProjectStatusGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkStatusGroup { get; set; }

    [Required]
    [MaxLength(100)]
    public string StatusGroupName { get; set; }

    // Связи
    [ForeignKey("FkProject")]
    public Project Project { get; set; }
    public int FkProject { get; set; }

    public ICollection<ProjectStatus> Statuses { get; set; } = new List<ProjectStatus>();
}