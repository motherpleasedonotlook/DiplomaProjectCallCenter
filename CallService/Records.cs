namespace CallService;

public record ClientInfoDto
{
    public int ClientId { get; init; }
    public string? Name { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ProcessingStatus { get; init; }
}

public record CallStatusDto
{
    public int StatusGroupId { get; init; }
    public string? StatusGroupName { get; init; }
    public int StatusId { get; init; }
    public string? StatusName { get; init; }
    public DateTime TimeStatusSelected { get; init; }
}

public record ClientNoteDto
{
    public int NoteId{ get; init; }
    public string? Text{ get; init; }
    public DateTime? DateWritten { get; init; }
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