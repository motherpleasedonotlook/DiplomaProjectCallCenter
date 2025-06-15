namespace RatingService;

public record GradesDto
{
    public int PkGrade { get; init; }
    public string? GradeType { get; init; }
    public int Score { get; init; }
    public int FkConversation { get; init; }
}

public record OperatorRatingsDto
{
    public int OperatorId { get; init; }
    public string? OperatorName { get; init; }
    public double FullScore { get; init; }
    public Dictionary<string, double>? Ratings { get; init; }
}