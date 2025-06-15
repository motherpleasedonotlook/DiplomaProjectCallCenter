using CallCenterRepository;
using RatingService;

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
var context = new ApplicationDbContext();
var services = new Services(context);

//#1 выставить оценку звонку
app.MapPost("call/rating", async (int callId, int type, int score) =>
{
    try
    {
        var rateUnit = await services.PutGrade(callId, type, score);
        Console.WriteLine("RatingService method PutGrade may be completed (or maybe not...)");
        return Results.Ok(rateUnit); 
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.BadRequest(ex.Message);
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
}).WithName("PutGrade");

//#2 редактировать оценку
app.MapPut("call/rating", async (int callId, int type, int newScore) =>
{
    try
    {
        var rateUnit = await services.EditGrade(callId, type, newScore);
        Console.WriteLine("RatingService method EditGrade may be completed (or maybe not...)");
        return Results.Ok(rateUnit); 
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine(ex.Message);
        return Results.BadRequest(ex.Message);
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
}).WithName("EditGrade");

//#3 список оценок конкретного звонка
app.MapGet("call/rating", async (int callId) =>
{
    try
    {
        var rateUnit = await services.GetGrade(callId);
        Console.WriteLine("RatingService method ReturnGrade may be completed (or maybe not...)");
        return Results.Ok(rateUnit);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.Data);
        return Results.StatusCode(500);
    }
}).WithName("GetGrade");

//#4 получить рейтинг конкретного оператора
app.MapGet("operator/ratings", async (int operatorId) =>
{
    try
    {
        var rating = await services.GetOperatorRatingFull(operatorId);
        Console.WriteLine("RatingService method GetOperatorsRatings may be completed (or maybe not...)");
        return Results.Ok(rating); 
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
}).WithName("GetOperatorsRatings");

//#5 получить рейтинг всех операторов конкретного администратора
app.MapGet("admin/{adminId}/ratings", async (int adminId, bool isActive=true) =>
{
    try
    {
        var ratings = await services.GetActiveOperatorsRatingByAdminAsync(adminId, isActive);
        Console.WriteLine("RatingService method GetAllOperatorsRatings may be completed (or maybe not...)");
        return Results.Ok(ratings); //возвращаем операторов
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
}).WithName("GetAllOperatorsRatings");

app.Run();
