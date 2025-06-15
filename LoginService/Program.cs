using CallCenterRepository;
using LoginService;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
var context = new ApplicationDbContext();
var services = new Services(context);


//#1 регистрация админа
app.MapPost("registration", async (string username, string password) =>
{
    try
    {
        var newAdminId = await services.RegisterAdminAsync(username, password);
        Console.WriteLine("LoginService method AdminRegistration may be completed (or maybe not...)");
        return Results.Ok(newAdminId); // Возвращаем результат успешной регистрации
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("AdminRegistration");

//#2 добавить профиль оператора
app.MapPost("admin/{adminId}/operators", async (int adminId, string username, string password) =>
{
    try
    {
        var newOperator = await services.AddOperatorAsync(adminId, username, password);
        Console.WriteLine("LoginService method AddOperator may be completed (or maybe not...)");
        return Results.Ok(newOperator);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.NotFound(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("AddOperator");

//#3 получить профиль оператора по паролю и имени
app.MapGet("operator/login", async (string username, string password) =>
{
    try
    {
        var existingOperatorId = await services.CheckOperatorAsync(username, password);
        Console.WriteLine("LoginService method AdminLogin may be completed (or maybe not...)");
        return Results.Ok(existingOperatorId); //возвращаем данные профиля
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("OperatorLogin");

//#4 получить профиль админа по паролю и имени (логин)
app.MapGet("admin/login", async (string username, string password) =>
{
    try
    {
        var existingAdminId = await services.CheckAdminAsync(username, password);
        Console.WriteLine("LoginService method AdminLogin may be completed (or maybe not...)");
        return Results.Ok(existingAdminId); //возвращаем данные профиля
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("AdminLogin");

//#5 смена пароля администратора 
app.MapPut("admin/{adminId}/password", async (int adminId, string oldPassword, string newPassword) =>
{
    try
    {
        var newPass = await services.EditAdminsPassword(adminId,oldPassword,newPassword);
        Console.WriteLine("LoginService method EditPassword may be completed (or maybe not...)");
        return Results.Ok(newPass);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.NotFound(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("EditPassword");

app.Run();
