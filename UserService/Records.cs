namespace UserService;

public record ClientInfoDto
{
    public int ClientId { get; init; }
    public string? Name { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ProcessingStatus { get; init; }
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
