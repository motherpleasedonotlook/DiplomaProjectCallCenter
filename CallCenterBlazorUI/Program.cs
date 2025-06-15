using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CallCenterBlazorUI;
using Blazored.LocalStorage;
using CallCenterBlazorUI.Objects;
using OfficeOpenXml;

// Для версии 7.x
ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// настройка базового адреса
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5276/") 
});

// Регистрируем Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Регистрируем сервисы с ленивой инициализацией
builder.Services.AddScoped<AdminProfileClass>(sp => 
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var http = sp.GetRequiredService<HttpClient>();
    var profile = new AdminProfileClass();
    profile.InitializeServices(localStorage, http);
    return profile;
});

builder.Services.AddScoped<OperatorProfileClass>(sp => 
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var http = sp.GetRequiredService<HttpClient>();
    var profile = new OperatorProfileClass();
    profile.InitializeServices(localStorage, http);
    return profile;
});

await builder.Build().RunAsync();