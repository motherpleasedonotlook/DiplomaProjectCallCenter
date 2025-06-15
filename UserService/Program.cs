using CallCenterRepository;
using Microsoft.AspNetCore.Mvc;
using UserService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var context = new ApplicationDbContext();
var services = new Services(context);

// #1 получить профиль админа
app.MapGet("admin/{adminId}", async (int adminId) =>
{
    try
    {
        var existingAdmin = await services.GetAdminProfile(adminId);
        Console.WriteLine("UserService method Admin may be completed (or maybe not...)");
        return Results.Ok(existingAdmin); //возвращаем данные профиля
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
}).WithName("Admin");

//#2 все операторы админа
app.MapGet("admin/{adminId}/operators", async (int adminId) =>
{
    try
    {
        var operators = await services.GetAllOperatorsByAdminAsync(adminId);
        return Results.Ok(operators); //возвращаем операторов
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
}).WithName("GetAllOperators");

//#3 получить профиль оператора по id
app.MapGet("operators/{operatorId}", async (int operatorId) =>
{
    try
    {
        var operator_ = await services.GetOperatorByAdminAsync(operatorId);
        return Results.Ok(operator_); //возвращаем оператора
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
}).WithName("GetOperator");

//#4 изменить имя оператора
app.MapPut("operators/{operatorId}/username", async (int operatorId, string newUsername) =>
{
    try
    {
        var editedOperatorId = await services.EditOperatorUsernameAsync(operatorId,newUsername);
        return Results.Ok(editedOperatorId); //возвращаем id оператора
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
}).WithName("EditOperatorUsername");

//#5 переключить статус оператора
app.MapPut("operators/{operatorId}/status", async (int operatorId) =>
{
    try
    {
        var editedOperatorId = await services.SwitchOperatorStatusAsync(operatorId);
        return Results.Ok(editedOperatorId); //возвращаем id оператора
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
}).WithName("SwitchOperatorStatus");

//#6 Получение операторов проекта
app.MapGet("project/operators", async (int projectId) =>
{
    try
    {
        var operators = await services.GetOperatorsByProjectIdAsync(projectId);
        return Results.Ok(operators);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetProjectOperators");

//#7 Активные операторы админа, не принадлежащие проекту N
app.MapGet("admin/available-operators", async (int adminId, int projectId)=>
{
    try
    {
        var operators = await services.GetAvailableOperatorsForProjectAsync(adminId, projectId);
        return Results.Ok(operators);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("AvailableOperatorsToAdd");

app.Run();
