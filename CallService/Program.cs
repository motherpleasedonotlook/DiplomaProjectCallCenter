using CallCenterRepository;
using CallService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

// #1 - получить все звонки оператора за интервал времени
app.MapGet($"operator/conversations", async (
    int operatorId,
    DateTime startDate,
    DateTime endDate) =>
{
    try
    {
        var conversations = await services.GetOperatorProjectConversationsAsync(
            operatorId, startDate, endDate);
        return Results.Ok(conversations);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("GetOperatorConversations");

// #2 - изменить статус звонка
app.MapPut("call/change-status", async (
    int callId, int newStatusId) =>
{
    try
    {
        var result = await services.ChangeConversationStatusAsync(callId, newStatusId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("ChangeConversationStatus");

// #3 - сохранить звонок
app.MapPost("conversations", async (DateTime timeStarted,
    DateTime timeEnded,
    string pathToAudio,
    int operatorId,
    int clientId,
    int projectId,
    string clientProcessingStatus,
    List<int> statusIds) =>
{
    try
    {
        var result = await services.SaveConversationAsync(
            timeStarted,
            timeEnded,
            pathToAudio,
            operatorId,
            clientId,
            projectId,
            clientProcessingStatus,
            statusIds);
        return Results.Created($"conversations/{result.CallId}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("SaveConversation");

// #4 - получить все статусы звонка по ID звонка
app.MapGet("call/statuses", async (int callId) =>
{
    try
    {
        var statuses = await services.GetCallStatusesAsync(callId);
        return Results.Ok(statuses);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("GetCallStatuses");

// #5 - получить заметку по ID звонка
app.MapGet("call/note", async (int callId) =>
{
    try
    {
        var note = await services.GetNoteByCallIdAsync(callId);
        return Results.Ok(note);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("GetNoteByCallId");

//#6 Получение клиента по ID
app.MapGet("call/client", async (int clientId) =>
{
    try
    {
        var client = await services.GetClientByCallIdAsync(clientId);
        return Results.Ok(client);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetClientByCallId");

// #7 - получить все звонки в рамках проекта за интервал времени
app.MapGet("/projects/conversations", async (
    int projectId,
    DateTime startDate,
    DateTime endDate) =>
{
    Console.WriteLine($"Получены параметры: startDate={startDate}, endDate={endDate}");
    try
    {
        var conversations = await services.GetProjectConversationsAsync(projectId, startDate, endDate);
        return Results.Ok(conversations);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("GetProjectConversations");

// #8 изменить статус клиента 
app.MapPut("client/change-status", async (
    int clientId, string newStatus) =>
{
    try
    {
        var result = await services.UpdateClientStatusAsync(clientId, newStatus);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("ChangeClientStatus");

//#9 оставить заметку
app.MapPost("call/{callId}/leave-note", async (int callId, string note) =>
{
    try
    {
        var result = await services.LeaveNoteAsync(callId, note);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("LeaveNote");

//#10 редактировать заметку
app.MapPut("call/{callId}/edit-note", async (int callId, string note) =>
{
    try
    {
        var result = await services.EditNoteAsync(callId, note);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("EditNote");


app.Run();
