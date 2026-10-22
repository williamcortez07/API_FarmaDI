using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FarmaDiApi.Models;
using FarmaDiApi.Services;
using FarmaDiBusiness.DTOs;
using FarmaDiCore.Exceptions;

namespace FarmaDiApi.Middlewares;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditLoggingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        var timer = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        // 1. Permitir la lectura múltiple del flujo del Request Body
        context.Request.EnableBuffering();
        string requestBody = string.Empty;

        // Evitamos leer binarios o formularios multipart para optimizar memoria
        bool isMultipart = context.Request.HasFormContentType;

        if (!isMultipart && context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Rebobinar el stream para que los controladores puedan leerlo
        }

        // 2. Sanitizar datos sensibles en payloads entrantes (contraseñas, tokens)
        requestBody = SanitizarPayload(requestBody);

        Exception? caughtException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            await HandleExceptionAsync(context, ex, traceId);
        }
        finally
        {
            timer.Stop();

            // Clasificación semántica de severidad del log:
            // - Excepciones controladas de negocio (< 500): Warning
            // - Excepciones no controladas o errores 500+: Error
            // - Códigos HTTP 4xx normales: Warning
            // - Operaciones exitosas (2xx, 3xx): Information
            string level;
            if (caughtException is AppException appEx && appEx.StatusCode < StatusCodes.Status500InternalServerError)
            {
                level = "Warning";
            }
            else if (caughtException != null)
            {
                level = "Error";
            }
            else
            {
                level = context.Response.StatusCode >= 400 ? "Warning" : "Information";
            }

            var log = new SystemLog
            {
                TraceId = traceId,
                Timestamp = DateTime.UtcNow,
                Environment = _env.EnvironmentName,
                Level = level,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                UserId = ObtenerIdentificadorUsuario(context),
                Request = new RequestDetail
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                    Payload = string.IsNullOrWhiteSpace(requestBody) ? null : requestBody
                },
                Response = new ResponseDetail
                {
                    StatusCode = context.Response.StatusCode,
                    ElapsedMilliseconds = timer.ElapsedMilliseconds
                }
            };

            if (caughtException != null)
            {
                log.Exception = new ExceptionDetail
                {
                    Message = caughtException.Message,
                    StackTrace = caughtException.StackTrace,
                    Source = caughtException.Source ?? string.Empty
                };
            }

            // Inserción asíncrona segura en MongoDB para auditoría
            _ = logService.InsertLogAsync(log);
        }
    }

    /// <summary>
    /// Manejador centralizado de excepciones que garantiza que nunca se filtren trazas de pila ni errores SQL al cliente,
    /// devolviendo siempre la estructura unificada UnsuccessfulResponseDto.
    /// </summary>
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, string traceId)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";

        UnsuccessfulResponseDto responseDto;

        if (ex is AppException appEx)
        {
            // Excepción de dominio controlada
            context.Response.StatusCode = appEx.StatusCode;
            responseDto = new UnsuccessfulResponseDto
            {
                Code = appEx.StatusCode.ToString(),
                Message = appEx.Message,
                Details = appEx.Details ?? new { traceId, info = appEx.Message }
            };
        }
        else
        {
            // Excepción no controlada: mensaje genérico seguro protegiendo detalles internos
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            responseDto = new UnsuccessfulResponseDto
            {
                Code = StatusCodes.Status500InternalServerError.ToString(),
                Message = "Ocurrió un error inesperado en el servidor.",
                Details = new { traceId, info = "Error interno en la aplicación" }
            };
        }

        var json = JsonSerializer.Serialize(responseDto, SerializerOptions);
        await context.Response.WriteAsync(json);
    }

    private static string SanitizarPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return payload;

        // Ocultar valores de password, token, clave o secret mediante Regex
        var pattern = @"(?i)""(password|contraseña|token|secret|clave)""\s*:\s*""([^""]+)""";
        return Regex.Replace(payload, pattern, "\"$1\": \"******\"");
    }

    private static string ObtenerIdentificadorUsuario(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return "Anonimo";
        }

        // 1. Extraer ID del usuario desde claims estándar o JWT
        var id = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                 ?? context.User.FindFirst("sub")?.Value
                 ?? context.User.FindFirst("id")?.Value;

        // 2. Extraer nombre de usuario desde claims estándar o JWT
        var name = context.User.FindFirst(ClaimTypes.Name)?.Value
                   ?? context.User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
                   ?? context.User.FindFirst("name")?.Value
                   ?? context.User.FindFirst("unique_name")?.Value
                   ?? context.User.Identity?.Name;

        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
        {
            return $"{id} ({name})";
        }

        return id ?? name ?? "Autenticado";
    }
}
