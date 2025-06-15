using Microsoft.EntityFrameworkCore;

namespace ProjectService;

public static class EndpointHandler
{
    public static async Task<IResult> HandleRequest(
        Func<Task<object>> action,
        Func<Exception, IResult>? customErrorHandler = null)
    {
        try
        {
            var result = await action();
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Database error: {ex.InnerException?.Message}");
            return Results.Problem("Database update error", statusCode: 500);
        }
        catch (Exception ex)
        {
            if (customErrorHandler != null)
                return customErrorHandler(ex);
                
            Console.WriteLine(ex.Message);
            return Results.Problem(ex.Message, statusCode: 500);
        }
    }

    public static async Task<IResult> HandleCreatedRequest(
        Func<Task<object>> action,
        string routePattern,
        Func<object, string> idSelector)
    {
        try
        {
            var result = await action();
            return Results.Created($"{routePattern}/{idSelector(result)}", result);
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
    }
}