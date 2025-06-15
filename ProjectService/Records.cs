namespace ProjectService;

public record ProjectDto
{
    public int ProjectId{ get; init;}
    public string? ProjectName { get; init;}
    public DateTime ProjectCreated { get; init;}
    public DateTime ProjectLastUpdate{ get; init;}
    public bool IsProjActive{ get; init;}
    public string? ProjectScriptText{ get; init;}
    public int ProjectCallInterval { get; init;}
    public TimeOnly ProjectStartsAt{ get; init;}
    public TimeOnly ProjectEndsAt{ get; init;}
    public int ProjectTimeOffset{ get; init;}
    public DateTime? ProjectClosedAt{ get; init;}
}
public record AdminDto
{
    public int PkAdmin { get; init; }
    public DateTime Created { get; init; }
    public bool IsActive { get; init; }
    public string? Password { get; init; }
    public string? Username { get; init; }
    public int SelfOperatorProfileId { get; init; }
}

public record OperatorDto
{
    public int PkOperator { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public DateTime Created { get; init; }
    public bool IsActive { get; init; }
    public int FkAdmin { get; init; }
}
public record CreateProjectRequest(
    string ProjectName,
    string? ScriptText,
    int CallInterval,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int TimeZoneOffset,
    int AdminId,
    int SelfOperatorProfileId);
    
public record EditScriptRequest(
    int ProjectId,
    string NewScript
);

public record EditProjectRequest(
    int ProjectId,
    string? Name,
    int? CallInterval,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? TimeZoneOffset
);

public record ClientInfoDto
{
    public int ClientId { get; init; }
    public string? Name { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ProcessingStatus { get; init; }
}

public record ConversationDto
{
    public int CallId { get; init; }
    public int ProjectId { get; init;}
    public int OperatorId { get; init; }
    public int ClientId { get; init; }
    public DateTime CallStartedAt { get; init; }
    public DateTime CallEndedAt { get; init; }
    public string? PathToCallRecord { get; init; }
}
public record StatusGroupDto
{
    public int StatusGroupId { get; init; }
    public string? StatusGroupName { get; init; } 
    
    public List<StatusDto>? Statuses { get; set; }
}
public record StatusDto
{
    public int StatusId { get; init; }
    public string? StatusName { get; init; }
}
public record CreateStatusGroupRequest
{
    public string StatusGroupName { get; init; }
    public int FkProject { get; init; }
    public List<string>? StatusNames { get; init; }
}