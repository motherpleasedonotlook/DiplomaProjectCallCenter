using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public struct NoteStruct
{
    [JsonPropertyName("noteId")]
    public int NoteId{ get; set; }
    [JsonPropertyName("text")]
    public string? Text{ get; set; }
    [JsonPropertyName("dateWritten")]
    public DateTime? DateWritten { get; set; }
}