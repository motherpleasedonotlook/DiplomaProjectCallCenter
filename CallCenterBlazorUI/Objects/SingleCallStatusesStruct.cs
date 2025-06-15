using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public struct SingleCallStatusesStruct
{
    [JsonPropertyName("statusGroupId")]
    public int StatusGroupId { get; set; }
    [JsonPropertyName("statusGroupName")]
    public string? StatusGroupName { get; set; }
    [JsonPropertyName("statusId")]
    public int StatusId { get; set; }
    [JsonPropertyName("statusName")]
    public string? StatusName { get; set; }
    [JsonPropertyName("timeStatusSelected")]
    public DateTime TimeStatusSelected { get; set; }
}
