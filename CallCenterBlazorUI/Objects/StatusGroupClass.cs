using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public class StatusGroupClass
{
    [JsonPropertyName("statusGroupId")]
    public int StatusGroupId { get; set; }
    [JsonPropertyName("statusGroupName")]
    public string? StatusGroupName { get; set; }
    [JsonPropertyName("statuses")]
    public List<StatusStruct>? Statuses { get; set; }
    
    // Создать новый статус и добавить в Statuses
    public async Task<bool> CreateAndAddStatusToGroupAsync(HttpClient http, string statusName)
    {
        try
        {
            var response = await http.PostAsync(
                $"proj-srv/status-groups/{StatusGroupId}/statuses?statusName={statusName}", null);

            if (!response.IsSuccessStatusCode) return false;
            var createdStatus = await response.Content.ReadFromJsonAsync<StatusStruct>();
            if (createdStatus == null) return false;
            Statuses ??= [];
            Statuses.Add(new StatusStruct 
            { 
                StatusId = createdStatus.StatusId, 
                StatusName = createdStatus.StatusName 
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Открепить статус (обновляется Statuses)
    public async Task<bool> DetachStatusAsync(HttpClient http, int statusId)
    {
        try
        {
            var response = await http.PutAsync(
                $"proj-srv/statuses/{statusId}/detach", 
                null);

            if (!response.IsSuccessStatusCode) return false;
            Statuses = Statuses?.Where(s => s.StatusId != statusId).ToList();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Переименовать группу статусов
    public async Task<bool> RenameGroupAsync(HttpClient http, string newName)
    {
        try
        {
            if (http == null)
                throw new InvalidOperationException("HttpClient is not initialized");

            var response = await http.PutAsync($"proj-srv/rename-group/{StatusGroupId}?newName={newName}", null);

            if (!response.IsSuccessStatusCode) return false;
            // Обновляем локальное имя при успехе
            this.StatusGroupName = newName;
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Переименовать статус в группе
    public async Task<bool> RenameStatusAsync(
        HttpClient http, 
        int statusId, 
        string newName)
    {
        try
        {
            if (http == null)
                throw new InvalidOperationException("HttpClient is not initialized");

            var response = await http.PutAsync($"proj-srv/rename-status/{statusId}?newName={newName}", null);

            if (!response.IsSuccessStatusCode) return false;
            // Обновляем локальное имя статуса при успехе
            var statusToUpdate = Statuses?.FirstOrDefault(s => s.StatusId == statusId);
            if (statusToUpdate != null)
            {
                statusToUpdate.StatusName = newName;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    
    // Статический: вернуть список всех свободных статусов
    public static async Task<List<StatusStruct>?> GetFreeStatusesAsync(HttpClient http)
    {
        try
        {
            var response = await http.GetAsync("proj-srv/statuses/free");

            if (!response.IsSuccessStatusCode) return null;
            var freeStatuses = await response.Content.ReadFromJsonAsync<List<StatusStruct>>();
            return freeStatuses?.Select(s => new StatusStruct 
            { 
                StatusId = s.StatusId, 
                StatusName = s.StatusName 
            }).ToList();
        }
        catch
        {
            return null;
        }
    }
    
    // Добавить свободный статус в эту группу (обновляется Statuses)
    public async Task<bool> AttachFreeStatusToGroupAsync(HttpClient http, int statusId)
    {
        try
        {
            var response = await http.PutAsync(
                $"proj-srv/status-groups/{StatusGroupId}/statuses/{statusId}/attach", 
                null);

            if (!response.IsSuccessStatusCode) return false;
            var attachedStatus = await response.Content.ReadFromJsonAsync<StatusStruct>();
            if (attachedStatus == null) return false;
            Statuses ??= [];
            Statuses.Add(new StatusStruct 
            { 
                StatusId = attachedStatus.StatusId, 
                StatusName = attachedStatus.StatusName 
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class StatusStruct
{
    [JsonPropertyName("statusId")]
    public int StatusId { get; set; }
    
    [JsonPropertyName("statusName")]
    public string? StatusName { get; set; }
}