using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SahibindenClone.WebUI.Errors;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // İstemci isteği iptal etti; tamamlanmış bir HTTP cevabı yazılamaz.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "İstek işlenirken yakalanmamış bir hata oluştu. TraceId: {TraceId}", context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            var (statusCode, message) = exception switch
            {
                BadHttpRequestException badRequest => (badRequest.StatusCode, "İstek biçimi geçersiz."),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Bu işlem için yetkiniz yok."),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "İstenen kaynak bulunamadı."),
                _ => (StatusCodes.Status500InternalServerError, "Beklenmeyen bir sunucu hatası oluştu.")
            };

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse(message, statusCode));
        }
    }
}

public sealed class ApiErrorResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is IStatusCodeActionResult { StatusCode: >= 400 } statusResult)
        {
            var statusCode = statusResult.StatusCode!.Value;
            var error = context.Result is ObjectResult objectResult
                ? GetErrorMessage(objectResult.Value)
                : DefaultMessage(statusCode);

            context.Result = new ObjectResult(new ApiErrorResponse(error, statusCode))
            {
                StatusCode = statusCode
            };
        }

        await next();
    }

    private static string GetErrorMessage(object? value)
    {
        if (value is null) return DefaultMessage(StatusCodes.Status400BadRequest);
        if (value is string text && !string.IsNullOrWhiteSpace(text)) return text;

        try
        {
            var element = System.Text.Json.JsonSerializer.SerializeToElement(value);
            if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                return element.GetString() ?? DefaultMessage(StatusCodes.Status400BadRequest);

            if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var message = FindStringProperty(element, "error", "message", "title");
                if (!string.IsNullOrWhiteSpace(message)) return message;

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name.Equals("errors", StringComparison.OrdinalIgnoreCase))
                    {
                        var validationErrors = FlattenStrings(property.Value).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
                        var combined = string.Join(" ", validationErrors);
                        if (!string.IsNullOrWhiteSpace(combined)) return combined;
                    }
                }
            }
            else if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var combined = string.Join(" ", FlattenStrings(element).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct());
                if (!string.IsNullOrWhiteSpace(combined)) return combined;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Use the stable fallback for response objects that cannot be represented as JSON.
        }

        return DefaultMessage(StatusCodes.Status400BadRequest);
    }

    private static string? FindStringProperty(System.Text.Json.JsonElement element, params string[] names)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (names.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) &&
                property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                return property.Value.GetString();
        }
        return null;
    }

    private static IEnumerable<string> FlattenStrings(System.Text.Json.JsonElement element)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var value = element.GetString();
            if (value is not null) yield return value;
        }
        else if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                foreach (var value in FlattenStrings(item)) yield return value;
        }
        else if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
                foreach (var value in FlattenStrings(property.Value)) yield return value;
        }
    }

    private static string DefaultMessage(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "İstek geçersiz.",
        StatusCodes.Status401Unauthorized => "Kimlik doğrulaması gerekiyor.",
        StatusCodes.Status403Forbidden => "Bu işlem için yetkiniz yok.",
        StatusCodes.Status404NotFound => "İstenen kaynak bulunamadı.",
        StatusCodes.Status409Conflict => "İstek mevcut durumla çakışıyor.",
        _ => "İstek işlenemedi."
    };
}
