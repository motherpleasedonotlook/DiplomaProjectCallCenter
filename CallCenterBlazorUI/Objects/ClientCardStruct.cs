using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public struct ClientCardStruct
{
    [JsonPropertyName("clientId")]
    public int ClientId { get; set; }
    [JsonPropertyName("name")]
    public string ClientName { get; set; }
    [JsonPropertyName("phoneNumber")]
    public string ClientPhoneNumber { get; set; }
    [JsonPropertyName("processingStatus")]
    public string? ClientProcessingStatus { get; set; }
}
/*
ClientProcessingStatus бывает 4 значений: 
"Processed",
"NotProcessed",
"InvalidNumber",
"Recall"
*/