using CallCenterRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService;

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

//#1 Добавление клиентов
app.MapPost("project/add-clients", async (int projectId, List<Dictionary<string, string>> clientData) =>
{
    try
    {
        var result = await services.AddClientsToProject(projectId, clientData);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("AddClients");

//#2 Получение клиентов проекта
app.MapGet("projects/clients", async (int projectId) =>
{
    try
    {
        var clients = await services.GetProjectClients(projectId);
        return Results.Ok(clients);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetClients");
//#3 создать проект
app.MapPost("projects", async ([FromBody] CreateProjectRequest request) =>
{
    try
    {
        ProjectDto result = await services.CreateProjectAsync(
            request.ProjectName,
            request.ScriptText,
            request.CallInterval,
            request.StartTime,
            request.EndTime,
            request.TimeZoneOffset,
            request.AdminId,
            request.SelfOperatorProfileId);
        return Results.Created($"projects/{result.ProjectId}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("CreateProject");

//#4 Добавление операторов в проект
app.MapPost("project/add-operators", async (int projectId, List<int> operatorIds) =>
{
    try
    {
        var result = await services.AddOperatorsToProjectAsync(projectId, operatorIds);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("AddOperatorsToProject");

//#5 Получение проекта по ID
app.MapGet("project", async (int projectId) =>
{
    try
    {
        var project = await services.GetProjectByIdAsync(projectId);
        return Results.Ok(project);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetProjectById");

//#6 Редактирование проекта
app.MapPut("project/edit", async ( [FromBody] EditProjectRequest request) =>
{
    try
    {
        var result = await services.EditProjectAsync(
            request.ProjectId, request.Name, request.CallInterval, 
            request.StartTime, request.EndTime, request.TimeZoneOffset);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("EditProject");

//#7 редактировать скрипт проекта
app.MapPut("project/edit-scrypt", async ([FromBody] EditScriptRequest request) =>
{
    try
    {
        var result = await services.EditProjectScriptAsync(
            request.ProjectId,
            request.NewScript);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("EditProjectScript");

//#8 Переключение статуса проекта
app.MapPut("projects/status", async (int projectId) =>
{
    try
    {
        var result = await services.SwitchProjectStatusAsync(projectId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("SwitchProjectStatus");

//#9 Закрытие проекта
app.MapPut("projects/close", async (int projectId) =>
{
    try
    {
        var result = await services.CloseProjectAsync(projectId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("CloseProject");

//#10 открепление операторов от проекта
app.MapPut("projects/delete-operators", async (int projectId, List<int> operatorIds) =>
{
    try
    {
        var result = await services.RemoveOperatorsFromProjectAsync(projectId, operatorIds);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("RemoveOperatorsFromProject");

//#11 Получение проектов администратора
app.MapGet("projects", async (int adminId) =>
{
    try
    {
        var project = await services.GetProjects(adminId);
        return Results.Ok(project);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetProjects");

//#12 Получение проектов оператора
app.MapGet("operator/projects", async (int operatorId) =>
{
    try
    {
        var project = await services.GetOperatorProjectsAsync(operatorId);
        return Results.Ok(project);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
}).WithName("GetProjectsOperatorProjects");

// #13 - создать группу статусов со статусами
app.MapPost("status-groups", async ([FromBody] CreateStatusGroupRequest request) =>
{
    try
    {
        var result = await services.CreateStatusGroupAsync(
            request.StatusGroupName, 
            request.FkProject, 
            request.StatusNames);
        
        return Results.Created($"status-groups/{result.StatusGroupId}", result);
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
}).WithName("CreateStatusGroup");

// #14 - получить все статусы по ID группы статусов
app.MapGet("project/{projectId}/status-groups", async (int projectId) =>
{
    try
    {
        var statuses = await services.GetStatusGroupsAsync(projectId);
        return Results.Ok(statuses);
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
}).WithName("GetStatusesByGroupId");

// #15 - создать новый статус и добавить его в существующую группу
app.MapPost("status-groups/{groupId}/statuses", async (int groupId, string statusName) =>
{
    try
    {
        var status = await services.CreateStatusInGroupAsync(groupId, statusName);
        return Results.Created($"/status-groups/{groupId}/statuses/{status.StatusId}", status);
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
}).WithName("CreateStatusInGroup");

// #16 - открепить статус от группы
app.MapPut("statuses/{statusId}/detach", async (int statusId) =>
{
    try
    {
        var result = await services.DetachStatusFromGroupAsync(statusId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Database error: {ex.InnerException?.Message}");
        return Results.Problem("Database update error", statusCode: 500);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        return Results.Problem(ex.Message, statusCode: 500);
    }
}).WithName("DetachStatusFromGroup");

// #17 - получить список всех свободных статусов для звонка
app.MapGet("statuses/free", async (int callId) =>
{
    try
    {
        var statuses = await services.GetFreeStatusNamesByCallIdAsync(callId);
        return Results.Ok(statuses);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("GetFreeStatuses");

// #18 - найти и добавить в группу свободный статус
app.MapPut("status-groups/{groupId}/statuses/{statusId}/attach", async (int statusId, int groupId) =>
{
    try
    {
        var result = await services.AttachFreeStatusToGroupAsync(statusId, groupId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return Results.StatusCode(500);
    }
}).WithName("AttachFreeStatusToGroup");

// #19 - удалить группу статусов (открепляя статусы)
app.MapDelete("projects/{projectId}/status-groups/{groupId}", async (int groupId, int projectId) =>
{
    try
    {
        var result = await services.DetachStatusGroupFromProjectAsync(groupId, projectId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Database error: {ex.InnerException?.Message}");
        return Results.Problem("Database update error", statusCode: 500);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
        return Results.Problem(ex.Message, statusCode: 500);
    }
}).WithName("DetachStatusGroupFromProject");

// №20 изменить имя статус-группы
app.MapPut("rename-group/{groupId}", async (int groupId, string newName) =>
{
    try
    {
        var result = await services.RenameGroupAsync(groupId, newName);
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
}).WithName("RenameGroup");

//№23 изменить имя статуса
app.MapPut("rename-status/{statusId}", async (int statusId, string newName) =>
{
    try
    {
        var result = await services.RenameStatusAsync(statusId, newName);
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
}).WithName("RenameStatus");
app.Run();
