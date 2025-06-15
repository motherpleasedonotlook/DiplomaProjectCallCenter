using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Blazored.LocalStorage;

namespace CallCenterBlazorUI.Objects;

public class AdminProfileClass
{
    [JsonPropertyName("pkAdmin")] public int IdAdmin { get; set; }

    [JsonPropertyName("username")] public string? AdminUsername { get; set; }

    [JsonPropertyName("password")] public string? AdminsPassword { get; set; }

    [JsonPropertyName("created")] public DateTime AdminProfileCreated { get; set; }

    [JsonPropertyName("isActive")] public bool IsAdmProfileActive { get; set; }

    [JsonPropertyName("selfOperatorProfileId")]
    public int SelfOperatorProfileId { get; set; }

    public bool WorkSession { get; set; } = true; //при входе в акк активна рабочая сессия
    public bool OnlineSession { get; set; } //но мы еще не на линии
    public List<OperatorProfileClass>? MyOperators { get; set; }
    public List<ProjectDetailsClass>? MyProjects { get; set; }
    private ILocalStorageService? _localStorage;
    private HttpClient? _http;

    [JsonConstructor]
    public AdminProfileClass()
    {
    }

    public void InitializeServices(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
        InitializeFromStorage().ConfigureAwait(false);
    }

    public void Initialize(int idAdmin, string? username,
        DateTime created, bool isActive, int selfOperatorProfileId)
    {
        IdAdmin = idAdmin;
        AdminUsername = username;
            //AdminsPassword = password;
        AdminProfileCreated = created;
        IsAdmProfileActive = isActive;
        SelfOperatorProfileId = selfOperatorProfileId;
    }

    public void Reset()
    {
        IdAdmin = 0;
        AdminUsername = string.Empty;
        AdminsPassword = string.Empty;
        AdminProfileCreated = default;
        IsAdmProfileActive = false;
        WorkSession = false;
        OnlineSession = false;
        MyOperators = null;
        MyProjects = null;
        SelfOperatorProfileId = -1;
    }

    //копирование объектов класса
    public static void CopyProfileData(AdminProfileClass source, AdminProfileClass target)
    {
        target.IdAdmin = source.IdAdmin;
        target.AdminUsername = source.AdminUsername;
        target.AdminsPassword = source.AdminsPassword;
        target.AdminProfileCreated = source.AdminProfileCreated;
        target.IsAdmProfileActive = source.IsAdmProfileActive;
        target.SelfOperatorProfileId = source.SelfOperatorProfileId;
        target.WorkSession = source.WorkSession;
        target.OnlineSession = source.OnlineSession;
    }


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

    //сохранить состояние в локальную память
    public async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.SetItemAsync("AdminProfile", this);
    }

    //загрузить состояние из локальной памяти
    public async Task LoadFromLocalStorage()
    {
        if (_localStorage == null)
            throw new InvalidOperationException("LocalStorage service not initialized");

        var savedProfile = await _localStorage.GetItemAsync<AdminProfileClass>("AdminProfile");
        if (savedProfile != null)
        {
            Initialize(
                savedProfile.IdAdmin,
                savedProfile.AdminUsername,
                //savedProfile.AdminsPassword,
                savedProfile.AdminProfileCreated,
                savedProfile.IsAdmProfileActive,
                savedProfile.SelfOperatorProfileId
            );
            WorkSession = savedProfile.WorkSession;
            OnlineSession = savedProfile.OnlineSession;
        }
    }

    //очистить состояние
    public async Task ClearLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.RemoveItemAsync("AdminProfile");
    }

    //вход
    public async Task<bool> Login(string username, string password, HttpClient http)
    {
        try
        {
            var url =
                $"/log-srv/admin/login?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode) return false;
            var jsonString = await response.Content.ReadAsStringAsync();
            var tempProfile = JsonSerializer.Deserialize<AdminProfileClass>(jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tempProfile == null) return false;
            Initialize(
                tempProfile.IdAdmin,
                tempProfile.AdminUsername,
                //tempProfile.AdminsPassword,
                tempProfile.AdminProfileCreated,
                tempProfile.IsAdmProfileActive,
                tempProfile.SelfOperatorProfileId
            );
            return true;
        }
        catch
        {
            Reset();
            throw;
        }
    }

    //регистрация
    public async Task<bool> Register(string username, string password, HttpClient http)
    {
        var url =
            $"/log-srv/registration?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
        var response = await http.PostAsync(url, null);
        return response.IsSuccessStatusCode;
    }

    //сменить пароль
    public async Task<string?> ChangePassword(string oldPassword, string? newPassword, HttpClient http)
    {
        if (http == null)
            throw new InvalidOperationException("HttpClient is not initialized");
        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            return "Заполните поля...";
        var url =
            $"/log-srv/admin/{IdAdmin}/password?oldPassword={Uri.EscapeDataString(oldPassword)}&newPassword={Uri.EscapeDataString(newPassword)}";
        var response = await http.PutAsync(url, null);

        if (!response.IsSuccessStatusCode)
            return response.StatusCode == HttpStatusCode.Conflict
                ? "Неверный пароль."
                : $"Ошибка: {response.StatusCode}";
        newPassword = await response.Content.ReadFromJsonAsync<string>();
        AdminsPassword = newPassword ?? throw new InvalidOperationException();
        return "Пароль успешно изменен.";
    }

    //обновить список операторов
    public async Task RenewOperatorsList(HttpClient http)
    {
        if (IdAdmin <= 0)
        {
            throw new InvalidOperationException("Id не инициализирован");
        }

        if (http == null) throw new InvalidOperationException("HttpClient не инициализирован");
        try
        {
            var url = $"/usr-srv/admin/{IdAdmin}/operators";
            var operators = await http.GetFromJsonAsync<List<OperatorProfileClass>>(url);
            MyOperators = operators?.OrderBy(o => o.OperatorProfileCreated).ToList() ?? [];
        }
        catch
        {
            MyOperators = new List<OperatorProfileClass>();
            throw;
        }
    }

    //только активные операторы
    public List<OperatorProfileClass> ShowActiveOperatorsOnly()
    {
        return MyOperators?
            .Where(o => o.IsOprProfileActive)
            .OrderBy(o => o.OperatorProfileCreated)
            .ToList() ?? [];
    }

    //добавление нового оператора
    public async Task<string?> AddNewOperator(string username, string password, HttpClient http)
    {
        if (http == null) throw new InvalidOperationException("HttpClient is not initialized");
        var url =
            $"/log-srv/admin/{IdAdmin}/operators?username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";
        var response = await http.PostAsync(url, null);
        if (response.IsSuccessStatusCode)
        {
            await RenewOperatorsList(http);
            return "Оператор успешно добавлен.";
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return "Оператор с таким именем уже существует.";
        }

        Console.WriteLine($"Произошла ошибка при добавлении нового оператора: {response.StatusCode.ToString()}");
        return $"Произошла ошибка при добавлении нового оператора: {response.StatusCode.ToString()}";
    }

    //инициализировать/перезаписать свойство MyProjects
    public async Task SetMyProjects(HttpClient http)
    {
        var url = $"/proj-srv/projects?adminId={IdAdmin}";
        try
        {
            if (http == null) throw new InvalidOperationException("HttpClient is not initialized");
            var projects = await http.GetFromJsonAsync<List<ProjectDetailsClass>>(url);
            MyProjects = projects?.OrderBy(proj => proj.ProjectCreated).ToList() ?? [];
        }
        catch
        {
            MyProjects = new List<ProjectDetailsClass>();
            throw;
        }

    }

    //Добавить проект (+переустановить MyProjects)
    public async Task<bool> AddNewProject(HttpClient http, string projectName, string? scriptText, int callInterval,
        TimeOnly startTime, TimeOnly endTime, int timeZoneOffset)
    {
        try
        {
            var requestData = new
            {
                projectName,
                scriptText,
                callInterval,
                startTime = startTime.ToString("HH:mm:ss"),
                endTime = endTime.ToString("HH:mm:ss"),
                timeZoneOffset,
                adminId = IdAdmin,
                selfOperatorProfileId = SelfOperatorProfileId
            };
            var response = await http.PostAsJsonAsync("/proj-srv/projects", requestData);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка при создании проекта: {errorMessage}");
                return false;
            }

            var newProject = await response.Content.ReadFromJsonAsync<ProjectDetailsClass>();
            if (newProject == null)
            {
                Console.WriteLine("Не удалось десериализовать созданный проект");
                return false;
            }

            await SetMyProjects(http);
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка при создании проекта: {exception.Message}");
            return false;
        }
    }

    //Получить проект по id
    public async Task<ProjectDetailsClass> GetCurrentProject(HttpClient http, int projectId)
    {
        var url = $"/proj-srv/project?projectId={projectId}";
        try
        {
            if (http == null) throw new InvalidOperationException("HttpClient is not initialized");
            var currentProject = await http.GetFromJsonAsync<ProjectDetailsClass>(url);
            if (currentProject != null) return currentProject;
            throw new Exception("Возвращен null");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            throw;
        }

    }
    
    //получить всех операторов админа, не связанных с проектом N
    public async Task<List<OperatorProfileClass>?> GetAvailableOperatorsForProjectAsync(HttpClient http, int projectId)
    {
        try
        {
            if (http == null)
                throw new ArgumentNullException(nameof(http), "HttpClient is required");

            var url = $"usr-srv/admin/available-operators?adminId={IdAdmin}&projectId={projectId}";
            var response = await http.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var operators = await response.Content.ReadFromJsonAsync<List<OperatorProfileClass>>();
                return operators;
            }
            Console.WriteLine($"Ошибка получения доступных операторов: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception в методе GetAvailableOperatorsForProjectAsync: {ex.Message}");
            return null;
        }
    }
    
}


