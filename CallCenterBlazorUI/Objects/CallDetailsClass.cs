using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public class CallDetailsClass
{
    [JsonPropertyName("callId")]
    public int CallId { get; set; }
    
    [JsonPropertyName("projectId")]
    public int ProjectId { get; set;}
    
    [JsonPropertyName("operatorId")]
    public int OperatorId { get; set;}
    
    [JsonPropertyName("clientId")]
    public int ClientId { get; set;}
    
    [JsonPropertyName("callStartedAt")] 
    public DateTime CallStartedAt { get; set; }
    
    [JsonPropertyName("callEndedAt")]
    public DateTime CallEndedAt { get; set; }
    
    [JsonPropertyName("pathToCallRecord")]
    public string? PathToCallRecord { get; set; }

    public CallDetailsClass(
        int callId, int projectId, int operatorId, DateTime callStartedAt,
        DateTime callEndedAt, string pathToCallRecord, int clientId
        )
    {
        CallId = callId;
        ProjectId = projectId;
        OperatorId = operatorId;
        ClientId = clientId;
        CallStartedAt = callStartedAt;
        CallEndedAt = callEndedAt;
        PathToCallRecord = pathToCallRecord;
    }
    
    //получить клиента по id
    public async Task<ClientCardStruct> GetClientsDataAsync(HttpClient http)
    {
        try
        {
            var url = $"/call-srv/call/client?clientId={ClientId}";
            var result = await http.GetFromJsonAsync<ClientCardStruct>(url);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    //изменить статус клиента
    public async Task<bool> ChangeClientStatusAsync(HttpClient http,string newStatus)
    {
        var url = $"/call-srv/client/change-status?clientId={ClientId}&newStatus={newStatus}";
        var response = await http.PutAsync(url, null);
        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(response.StatusCode);
        return false;
    }
    
    //получить выбранные статусы в статус-группах
    public async Task<List<SingleCallStatusesStruct>> GetStatusGroupsAsync(HttpClient http)
    {
        try
        {
            var url = $"/call-srv/call/statuses?callId={CallId}";
            var result = await http.GetFromJsonAsync<List<SingleCallStatusesStruct>>(url);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    //получить имена статусов из удаленных групп, если есть
    public async Task<List<string>> GetFreeStatusesAsync(HttpClient http)
    {
        try
        {
            var url = $"/proj-srv/statuses/free?callId={CallId}";
            var result = await http.GetFromJsonAsync<List<string>>(url);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    //изменить статус завершения звонка
    public async Task<bool> ChangeCallStatusAsync(HttpClient http, int newStatusId)
    {
        var url = $"/call-srv/call/change-status?callId={CallId}&newStatusId={newStatusId}";
        var response = await http.PutAsync(url, null);
        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(response.StatusCode);
        return false;
    }
    //получить заметку по id звонка (может вернуть null, если заметки нет)
    public async Task<NoteStruct> GetNoteAsync(HttpClient http)
    {
        try
        {
            var url = $"/call-srv/call/note?callId={CallId}";
            var result = await http.GetFromJsonAsync<NoteStruct>(url);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    
    //оставить заметку по id звонка
    public async Task<bool> LeaveNoteAsync(HttpClient http, string note)
    {
        try
        {
            var response = await http.PostAsync($"call-srv/call/{CallId}/leave-note?note={note}", null);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Console.WriteLine($"Ошибка при добавлении заметки: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception в LeaveNoteAsync: {ex.Message}");
            return false;
        }
    }
    //редактировать заметку
    public async Task<bool>EditNoteAsync(HttpClient http, string newNote)
    {
        var url = $"/call-srv/call/{CallId}/edit-note?note={newNote}";
        var response = await http.PutAsync(url, null);
        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(response.StatusCode);
        return false;
    }
    //получить список оценок звонка (типа может вернуть null, чтобы желтая линия меня не бесила, но вщ не должна)
    public async Task<List<GradeStruct>?> GetGradeListAsync(HttpClient http)
    {
        try
        {
            var url = $"/rate-srv/call/rating?callId={CallId}";
            var result = await http.GetFromJsonAsync<List<GradeStruct>>(url);
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    //редактировать оценку
    public async Task<bool> EditGradeAsync(HttpClient http, int type, int newScore)
    {
        var url = $"/rate-srv/call/rating?callId={CallId}&type={type}&newScore={newScore}";
        var response = await http.PutAsync(url, null);
        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(response.StatusCode);
        return false;
    }
  
    //поставить оценку
    public async Task<bool> PutGradeAsync(HttpClient http, int type, int score)
    {
        try
        {
            var response = await http.PostAsync($"/rate-srv/call/rating?callId={CallId}&type={type}&score={score}",null);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            Console.WriteLine($"Ошибка при добавлении оценки: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception в LeaveNoteAsync: {ex.Message}");
            return false;
        }
    }
}