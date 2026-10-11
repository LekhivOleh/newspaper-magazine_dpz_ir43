using CURSOVA.Application.Common;
using CURSOVA.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CURSOVA.Infrastructure.Web;

public class ApiExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
            return false;

        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Не знайдено"),
            ConflictException => (StatusCodes.Status409Conflict, "Конфлікт"),
            DomainException => (StatusCodes.Status400BadRequest, "Некоректні дані"),
            _ => (0, "")
        };

        if (status == 0)
            return false;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message }
        });
    }
}
