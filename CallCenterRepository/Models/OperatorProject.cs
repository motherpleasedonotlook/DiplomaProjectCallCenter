using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("OperatorProjects")]
public class OperatorProject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkOperatorProject { get; set; }

    [ForeignKey("FkOperator")]
    public Operator Operator { get; set; }
    public int FkOperator { get; set; }

    [ForeignKey("FkProject")]
    public Project Project { get; set; }
    public int FkProject { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}