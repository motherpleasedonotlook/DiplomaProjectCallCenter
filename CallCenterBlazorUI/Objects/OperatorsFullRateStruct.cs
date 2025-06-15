using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public struct OperatorsFullRateStruct
{
    [JsonPropertyName("operatorId")]
    public int OperatorId { get; set; }
    [JsonPropertyName("operatorName")]
    public string? OperatorName { get; set; }
    [JsonPropertyName("fullScore")]
    public double FullScore { get; set; }
    [JsonPropertyName("ratings")]
    public Dictionary<string, double>? Ratings { get; set; }
}
public struct GradeStruct
{
    [JsonPropertyName("pkGrade")]
    public int PkGrade { get; init; }
    [JsonPropertyName("gradeType")]
    public string? GradeType { get; init; }
    [JsonPropertyName("score")]
    public int Score { get; init; }
    [JsonPropertyName("fkConversation")]
    public int FkConversation { get; init; }
}