namespace LoginService;

public record OperatorDto
{
    public int PkOperator { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public DateTime Created { get; init; }
    public bool IsActive { get; init; }
    public int FkAdmin { get; init; }
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