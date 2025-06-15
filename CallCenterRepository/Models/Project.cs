using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterRepository.Models;

[Table("Projects")]
public class Project
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PkProject { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProjectName { get; set; }

    [Required]
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    [Required]
    public bool ProjectStatus { get; set; } = true;

    public string? ScriptText { get; set; }

    [Required]
    public int CallInterval { get; set; } // в секундах

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    [Required]
    public int TimeZoneOffset { get; set; } // смещение в часах

    public DateTime? Closed { get; set; }

    // Связи
    [ForeignKey("FkAdmin")]
    public Admin Admin { get; set; }
    public int FkAdmin { get; set; }

    public ICollection<OperatorProject> OperatorProjects { get; set; } = new List<OperatorProject>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<ProjectStatusGroup> StatusGroups { get; set; } = new List<ProjectStatusGroup>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public Project(
        string projectName,
        int callInterval,
        TimeOnly startTime,
        TimeOnly endTime,
        int timeZoneOffset,
        int fkAdmin,
        DateTime created = default,
        bool projectStatus = true,
        string? scriptText = null,
        DateTime? closed = null)
    {
        ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        CallInterval = callInterval;
        StartTime = startTime;
        EndTime = endTime;
        TimeZoneOffset = timeZoneOffset;
        FkAdmin = fkAdmin;
        Created = created == default ? DateTime.UtcNow : created;
        ProjectStatus = projectStatus;
        ScriptText = scriptText;
        Closed = closed;
    }
}