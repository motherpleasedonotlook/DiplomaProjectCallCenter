using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Blazored.LocalStorage;

namespace CallCenterBlazorUI.Objects;

public class OperatorProfileClass
{
    [JsonPropertyName("pkOperator")] 
    public int OperatorId { get; set; }

    [JsonPropertyName("username")] 
    public string? OperatorUsername { get; set; }

    [JsonPropertyName("password")] 
    public string? OperatorsPassword { get; set; }

    [JsonPropertyName("created")] 
    public DateTime OperatorProfileCreated { get; set; }

    [JsonPropertyName("isActive")] 
    public bool IsOprProfileActive { get; set; }
    
    [JsonPropertyName("fkAdmin")] 
    public int FkAdmin { get; set; }

    public bool WorkSession { get; set; } = true; //при входе в акк активна рабочая сессия
    public bool OnlineSession { get; set; } //но мы еще не на линии
    public List<ProjectDetailsClass>? MyProjects { get; set; }
    private ILocalStorageService? _localStorage;
    private HttpClient? _http;
    [JsonConstructor]
    public OperatorProfileClass() { }

    private async Task InitializeFromStorage()
    {
        try 
        {
            await LoadFromLocalStorage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from localStorage: {ex.Message}");
            Reset();
        }
    }
    public void InitializeServices(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
        InitializeFromStorage().ConfigureAwait(false);
    }
    
    //инициализировать объект
    public void Initialize(
        int operatorId,
        string? operatorUsername,
        string? operatorsPassword,
        DateTime operatorProfileCreated,
        bool isOprProfileActive,
        int fkAdmin)
    {
        OperatorId = operatorId;
        OperatorUsername = operatorUsername;
        OperatorsPassword = operatorsPassword;
        OperatorProfileCreated = operatorProfileCreated;
        IsOprProfileActive = isOprProfileActive;
        FkAdmin = fkAdmin;
    }
    
    //обнулить объект
    public void Reset()
    {
        OperatorId = 0;
        OperatorUsername = string.Empty;
        OperatorsPassword = string.Empty;
        OperatorProfileCreated = default;
        IsOprProfileActive = false;
        WorkSession = false;
        OnlineSession = false;
        FkAdmin = 0;
    }
    
    
    //вход
    public async Task<bool> Login(string username, string password, HttpClient http)
    {
        try
        {
            var url = $"/log-srv/operator/login?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode) return false;
            var jsonString = await response.Content.ReadAsStringAsync();
            var tempProfile = JsonSerializer.Deserialize<OperatorProfileClass>(jsonString);

            if (tempProfile == null) return false;
            Initialize(
                tempProfile.OperatorId,
                tempProfile.OperatorUsername,
                tempProfile.OperatorsPassword,
                tempProfile.OperatorProfileCreated,
                tempProfile.IsOprProfileActive,
                tempProfile.FkAdmin
            );
            return true;
        }
        catch
        {
            Reset();
            throw;
        }
    }

    //получить профиль оператора
    public async Task LoadOperatorProfile(int operatorId, HttpClient http)
    {
        try
        {
            var operatorUrl = $"/usr-srv/operators/{operatorId}";
            var operatorResponse = await http.GetAsync(operatorUrl);
            if (operatorResponse.IsSuccessStatusCode)
            {
                var operatorJson = await operatorResponse.Content.ReadAsStringAsync();
                var tempProfile = JsonSerializer.Deserialize<OperatorProfileClass>(operatorJson);
                if (tempProfile != null)
                {
                    Initialize(
                        tempProfile.OperatorId,
                        tempProfile.OperatorUsername,
                        tempProfile.OperatorsPassword,
                        tempProfile.OperatorProfileCreated,
                        tempProfile.IsOprProfileActive,
                        tempProfile.FkAdmin
                    );
                }
            }
        }
        catch
        {
            Reset();
            throw;
        }
    }
    
    //сохранить состояние в локальную память
    public async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.SetItemAsync("OperatorProfile", this);
    }
    
    //загрузить состояние из локальной памяти
    public async Task LoadFromLocalStorage()
    {
        if (_localStorage == null)
            throw new InvalidOperationException("LocalStorage service not initialized");
    
        var savedProfile = await _localStorage.GetItemAsync<OperatorProfileClass>("AdminProfile");
        if (savedProfile != null)
        {
            Initialize(
                savedProfile.OperatorId,
                savedProfile.OperatorUsername,
                savedProfile.OperatorsPassword,
                savedProfile.OperatorProfileCreated,
                savedProfile.IsOprProfileActive,
                savedProfile.FkAdmin
            );
            WorkSession = savedProfile.WorkSession;
            OnlineSession = savedProfile.OnlineSession;
        }
    }
    
    //удалить состояние из локальной памяти
    public async Task ClearLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.RemoveItemAsync("OperatorProfile");
    }

    //установить/переустановить свойство MyProjects
    public async Task SetMyProjects(HttpClient http)
    {
        try
        {
            if (http == null) throw new InvalidOperationException("HttpClient is not initialized");
            var url = $"/proj-srv/operator/projects?operatorId={OperatorId}";
            var projects = await http.GetFromJsonAsync<List<ProjectDetailsClass>>(url);
            MyProjects = projects?.OrderBy(proj => proj.ProjectCreated).ToList() ?? [];
        }
        catch
        {
            MyProjects = new List<ProjectDetailsClass>();
            throw;
        }
    }

    //изменить имя оператора
    public async Task<string> ChangeOperatorsUsername(string newUsername, HttpClient http)
    {
        if (string.IsNullOrWhiteSpace(newUsername)) return "Не оставляйте имя пустым...";
        var response = await http.PutAsync($"/usr-srv/operators/{OperatorId}/username?newUsername={Uri.EscapeDataString(newUsername)}", null);
        if (response.IsSuccessStatusCode) //успешно
        {
            OperatorUsername = newUsername;
            return "Имя пользователя успешно изменено";
        }
        if (response.StatusCode == HttpStatusCode.Conflict)//не успешно
        {
            return "Имя пользователя занято";
        }
        //ошибка сервера, скорее всего не отвечает, тк отключен
        Console.WriteLine($"Произошла ошибка при изменении имени: {response.StatusCode.ToString()}");
        return $"Произошла ошибка при изменении имени: {response.StatusCode.ToString()}";
    }

    //переключить статус
    public async Task<bool> SwitchStatus(HttpClient http)
    {
        var response = await http.PutAsync($"/usr-srv/operators/{OperatorId}/status", null);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("response.StatusCode.ToString()");
        IsOprProfileActive = !IsOprProfileActive;
        return IsOprProfileActive;

    }
    
    //получить все звонки оператора за указанное время
    public async Task<List<CallDetailsClass>> GetMyCalls(HttpClient http, DateTime starts, DateTime ends)
    {
        var startEncoded = Uri.EscapeDataString(starts.ToString("o"));
        var endEncoded = Uri.EscapeDataString(ends.ToString("o"));
    
        var url = $"/call-srv/operator/conversations?operatorId={OperatorId}&startDate={startEncoded}&endDate={endEncoded}";
    
        var result = await http.GetFromJsonAsync<List<CallDetailsClass>>(url);
        if (result == null) 
            throw new InvalidOperationException("Ошибка при загрузке данных о записанных звонках");
        return result;
    }
    
    //получить все звонки связанные с проектом N за указанное время
}