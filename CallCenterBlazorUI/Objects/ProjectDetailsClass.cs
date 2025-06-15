using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CallCenterBlazorUI.Objects;

public class ProjectDetailsClass
{
    [JsonPropertyName("projectId")]
    public int ProjectId{ get; set;}
    [JsonPropertyName("projectName")]
    public string? ProjectName { get; set;}
    [JsonPropertyName("projectCreated")]
    public DateTime ProjectCreated { get; set;}
    [JsonPropertyName("projectLastUpdate")]
    public DateTime ProjectLastUpdate{ get; set;}
    [JsonPropertyName("isProjActive")]
    public bool IsProjActive{ get; set;}
    [JsonPropertyName("projectScriptText")]
    public string? ProjectScriptText{ get; set;}
    [JsonPropertyName("projectCallInterval")]
    public int ProjectCallInterval { get; set;}
    [JsonPropertyName("projectStartsAt")]
    public TimeOnly ProjectStartsAt{ get; set;}
    [JsonPropertyName("projectEndsAt")]
    public TimeOnly ProjectEndsAt{ get; set;}
    [JsonPropertyName("projectTimeOffset")]
    public int ProjectTimeOffset{ get; set;}
    [JsonPropertyName("projectClosedAt")]
    public DateTime? ProjectClosedAtNullable { get; set; }

    [JsonIgnore] public DateTime ProjectClosedAt => ProjectClosedAtNullable ?? DateTime.MinValue;
    
    //список статус-групп
    [JsonIgnore] public List<StatusGroupClass>? ProjectStatuses { get; set; }

    // Редактирование основных параметров проекта
    public async Task<bool> EditProjectAsync(HttpClient http,
        string? newName = null,
        int? newCallInterval = null,
        TimeOnly? newStartTime = null,
        TimeOnly? newEndTime = null,
        int? newTimeZoneOffset = null)
    {
        try
        {
            var response = await http.PutAsJsonAsync("proj-srv/project/edit", new
            {
                projectId = ProjectId,
                name = newName ?? ProjectName,
                callInterval = newCallInterval ?? ProjectCallInterval,
                startTime = newStartTime ?? ProjectStartsAt,
                endTime = newEndTime ?? ProjectEndsAt,
                timeZoneOffset = newTimeZoneOffset ?? ProjectTimeOffset
            });

            if (!response.IsSuccessStatusCode) return false;
            // Обновляем локальные свойства
            if (newName != null) ProjectName = newName;
            if (newCallInterval != null) ProjectCallInterval = newCallInterval.Value;
            if (newStartTime != null) ProjectStartsAt = newStartTime.Value;
            if (newEndTime != null) ProjectEndsAt = newEndTime.Value;
            if (newTimeZoneOffset != null) ProjectTimeOffset = newTimeZoneOffset.Value;
                
            ProjectLastUpdate = DateTime.UtcNow;
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    /// Редактирование скрипта проекта
    public async Task<bool> EditScriptAsync(HttpClient http, string newScript)
    {
        try
        {
            var response = await http.PutAsJsonAsync("proj-srv/project/edit-scrypt", new
            {
                projectId = ProjectId,
                newScript
            });

            if (!response.IsSuccessStatusCode) return false;
            ProjectScriptText = newScript;
            ProjectLastUpdate = DateTime.UtcNow;
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    // Переключение статуса проекта
    public async Task<bool> ToggleStatusAsync(HttpClient http)
    {
        try
        {
            var response = await http.PutAsync($"proj-srv/projects/status?projectId={ProjectId}", null);

            if (!response.IsSuccessStatusCode) return false;
            IsProjActive = !IsProjActive;
            ProjectLastUpdate = DateTime.UtcNow;
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    // Закрытие проекта
    public async Task<bool> CloseProjectAsync(HttpClient http)
    {
        try
        {
            var response = await http.PutAsync($"proj-srv/projects/close?projectId={ProjectId}", null);

            if (!response.IsSuccessStatusCode) return false;
            IsProjActive = false;
            ProjectClosedAtNullable = DateTime.UtcNow;
            ProjectLastUpdate = DateTime.UtcNow;
            return true;

        }
        catch
        {
            return false;
        }
    }
    
    //Установить/переустановить свойство ProjectStatuses
    public async Task GetProjectStatusGroupsAsync(HttpClient http)
    {
        var url = $"proj-srv/project/{ProjectId}/status-groups";
        try
        {
            var response = await http.GetAsync(url);
        
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content); // Логируем сырой ответ
            
                ProjectStatuses = await response.Content.ReadFromJsonAsync<List<StatusGroupClass>>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading status groups: {ex}");
            ProjectStatuses = new List<StatusGroupClass>();
        }
    }
    
    //создать группу статусов
    public async Task AddStatusGroupAsync(HttpClient http,
        string statusGroupName,
        List<string>? statusNames = null)
    {
        const string url = "proj-srv/status-groups";
    
        var requestData = new {
            statusGroupName,
            fkProject = ProjectId,
            statusNames
        };

        var response = await http.PostAsJsonAsync(url, requestData);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error: {response.StatusCode} - {errorContent}");
        }

        var createdGroup = await response.Content.ReadFromJsonAsync<StatusGroupClass>();
        ProjectStatuses ??= new List<StatusGroupClass>();
        ProjectStatuses.Add(createdGroup!);
    }
    
    //Удалить группу статусов
    public async Task<bool> DeleteStatusGroupAsync(HttpClient http, int groupId)
    {
        var url = $"proj-srv/projects/{ProjectId}/status-groups/{groupId}";
        try
        {
            if (http == null) 
                throw new InvalidOperationException("HttpClient is not initialized");

            var response = await http.DeleteAsync(url);

            if (!response.IsSuccessStatusCode) return false;
            // удаляем группу из списка, если запрос успешен
            ProjectStatuses = ProjectStatuses?.Where(g => g.StatusGroupId != groupId)
                .ToList();
            return true;
        }
        catch
        {
            return false;
        }
    }
    //получить участников проекта
    public async Task<List<OperatorProfileClass>> GetProjectOperatorsAsync(HttpClient http)
    {
        if (http == null) throw new InvalidOperationException("HttpClient is not initialized");
        try
        {
            var url = $"/usr-srv/project/operators?projectId={ProjectId}";
            var participants = await http.GetFromJsonAsync<List<OperatorProfileClass>>(url);
            if (participants == null) throw new Exception("Проект не найден, возможно, неактуальные данные в кеше");
            return participants;
        }
        catch
        {
            return [];
        }
    }
    //добавить оператора в проект
    public async Task<bool> AddOperatorsToProjectAsync(
        HttpClient http, 
        List<int> operatorIds)
    {
        if (http == null)
            throw new ArgumentNullException(nameof(http), "HttpClient is required");

        if (operatorIds == null || !operatorIds.Any())
            return false;

        try
        {
            var response = await http.PostAsJsonAsync("proj-srv/project/add-operators?projectId=" + ProjectId, operatorIds);

            if (response.IsSuccessStatusCode)
            {
                // обновляем локальный кеш участников проекта
                await GetProjectOperatorsAsync(http);
                return true;
            }

            Console.WriteLine($"Error adding operators. Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in AddOperatorsToProjectAsync: {ex.Message}");
            return false;
        }
    }
    
    //удалить оператора из проекта
    public async Task<bool> RemoveOperatorsFromProjectAsync(
        HttpClient http, 
        List<int> operatorIds)
    {
        if (http == null)
            throw new ArgumentNullException(nameof(http), "HttpClient is required");

        if (operatorIds == null || !operatorIds.Any())
            return false;

        try
        {
            var response = await http.PutAsJsonAsync("proj-srv/projects/delete-operators?projectId=" + ProjectId, operatorIds);

            if (response.IsSuccessStatusCode)
            {
                // обновляем локальный кеш участников проекта
                await GetProjectOperatorsAsync(http);
                return true;
            }

            Console.WriteLine($"Error removing operators. Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in RemoveOperatorsFromProjectAsync: {ex.Message}");
            return false;
        }
    }
    
    //Получить звонки по проекту за интервал времени
    public async Task<List<CallDetailsClass>> GetCallsByProject(HttpClient http, DateTime startDate, DateTime endDate)
    {
        var startUtc = startDate.ToUniversalTime().ToString("o");
        var endUtc = endDate.ToUniversalTime().ToString("o");

        var url = $"call-srv/projects/conversations?projectId={ProjectId}&startDate={Uri.EscapeDataString(startUtc)}&endDate={Uri.EscapeDataString(endUtc)}";
        var result = await http.GetFromJsonAsync<List<CallDetailsClass>>(url);
        if (result == null) 
            throw new InvalidOperationException("Ошибка при загрузке данных о записанных звонках");
        return result;
    }
    //Получить клиентов проекта
    public async Task<List<ClientCardStruct>> GetClientsAsync(HttpClient http)
    {
        try
        {
            var url = $"/proj-srv/projects/clients?projectId={ProjectId}";
            var result = await http.GetFromJsonAsync<List<ClientCardStruct>>(url);
            Console.WriteLine($"Загружено клиентов: {result.Count}");
            Console.WriteLine($"Request URL: {url}");
            return result;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }
    }
    //Добавить клиентов
    public async Task<int> AddClientsAsync(HttpClient http, List<Dictionary<string, string>> clients)
    {
        try
        {
            var response = await http.PostAsJsonAsync(
                $"/proj-srv/project/add-clients?projectId={ProjectId}",
                clients
            );
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<int>();
                Console.WriteLine($"Добавлено клиентов:{result}");
                return result;
            }
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка в AddClientsAsync: {error}");
            return -1;
        }
        catch(Exception exception)
        {
            Console.WriteLine($"Exception в AddClientsAsync: {exception.Message}");
            return -1;
        }
    }
}
