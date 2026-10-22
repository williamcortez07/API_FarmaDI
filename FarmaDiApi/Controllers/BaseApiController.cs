using FarmaDiBusiness.DTOs;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    /// <summary>
    /// Controlador base que estandariza las respuestas, mapeos de errores de negocio y paginación
    /// en todos los endpoints de la API, garantizando consistencia, código limpio y alta mantenibilidad.
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Mapea los códigos de resultado de la capa de negocio (ServiceResponse) a las respuestas HTTP semánticas
        /// apropiadas, manteniendo el contrato de respuesta unificado UnsuccessfulResponseDto.
        /// </summary>
        protected IActionResult MapServiceError<T>(
            ServiceResponse<T> serviceResponse,
            string? customNotFoundMessage = null,
            string? customConflictMessage = null)
        {
            var unsuccessfulResponse = new UnsuccessfulResponseDto();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unsuccessfulResponse.Code = StatusCodes.Status404NotFound.ToString();
                    unsuccessfulResponse.Message = customNotFoundMessage ?? serviceResponse.Message ?? "Recurso no encontrado";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = serviceResponse.Message ?? "No se encontró el recurso solicitado" };
                    return NotFound(unsuccessfulResponse);

                case MessageCodes.Conflict:
                case MessageCodes.DuplicateData:
                case MessageCodes.FileExisting:
                    unsuccessfulResponse.Code = StatusCodes.Status409Conflict.ToString();
                    unsuccessfulResponse.Message = customConflictMessage ?? serviceResponse.Message ?? "El registro no pudo completarse por un conflicto de datos";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = serviceResponse.Message ?? "Hubo conflicto con registros existentes" };
                    return Conflict(unsuccessfulResponse);

                case MessageCodes.ErrorValidation:
                case MessageCodes.BadRequest:
                    unsuccessfulResponse.Code = StatusCodes.Status400BadRequest.ToString();
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Los datos proporcionados no son válidos";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = serviceResponse.Message ?? "Error en la validación de la solicitud" };
                    return BadRequest(unsuccessfulResponse);

                case MessageCodes.Unauthorized:
                case MessageCodes.Authentication:
                    unsuccessfulResponse.Code = StatusCodes.Status401Unauthorized.ToString();
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "Credenciales inválidas o no autorizadas";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = "No posee autorización para esta operación" };
                    return Unauthorized(unsuccessfulResponse);

                case MessageCodes.NoData:
                    // Preserva el comportamiento existente del backend donde no haber datos devuelve 200 con payload informativo
                    unsuccessfulResponse.Code = StatusCodes.Status200OK.ToString();
                    unsuccessfulResponse.Message = serviceResponse.Message ?? "No se encontraron registros";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = "Temporalmente no hay registros en la BD" };
                    return Ok(unsuccessfulResponse);

                case MessageCodes.ErrorDataBase:
                default:
                    unsuccessfulResponse.Code = StatusCodes.Status500InternalServerError.ToString();
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = serviceResponse.Details ?? new { info = serviceResponse.Message ?? "Error interno en la aplicación" };
                    return StatusCode(StatusCodes.Status500InternalServerError, unsuccessfulResponse);
            }
        }

        /// <summary>
        /// Genera una respuesta estándar de BadRequest (400) encapsulada en UnsuccessfulResponseDto.
        /// </summary>
        protected IActionResult BadRequestError(string message, object? details = null)
        {
            return BadRequest(new UnsuccessfulResponseDto
            {
                Code = StatusCodes.Status400BadRequest.ToString(),
                Message = message,
                Details = details ?? new { info = message }
            });
        }

        /// <summary>
        /// Valida que un identificador entero de ruta sea mayor a cero de manera consistente.
        /// </summary>
        protected bool TryValidatePositiveId(int id, string paramName, out IActionResult? errorResult)
        {
            if (id <= 0)
            {
                errorResult = BadRequestError(
                    $"{paramName} proporcionado debe de ser mayor a 0",
                    new { info = "Error en el formato de valor enviado" });
                return false;
            }

            errorResult = null;
            return true;
        }

        /// <summary>
        /// Normaliza los parámetros de paginación para evitar desbordes de memoria o páginas negativas.
        /// </summary>
        protected static void NormalizePagination(ref int page, ref int limit, int defaultLimit = 10, int maxLimit = 100)
        {
            if (page <= 0) page = 1;
            if (limit <= 0) limit = defaultLimit;
            if (limit > maxLimit) limit = maxLimit;
        }

        /// <summary>
        /// Construye una respuesta paginada estandarizada en ApiResponse<T>.
        /// </summary>
        protected static ApiResponse<IEnumerable<T>> BuildPagedResponse<T>(
            IEnumerable<T> items,
            int totalCount,
            int page,
            int limit)
        {
            var list = items as IList<T> ?? items.ToList();
            int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            return new ApiResponse<IEnumerable<T>>
            {
                Data = list,
                Meta = new GetPagedDto
                {
                    TotalItems = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    ItemsPerPage = limit
                }
            };
        }

        /// <summary>
        /// Construye una respuesta de colección estandarizada con metadatos de conteo total.
        /// </summary>
        protected static ApiResponse<IEnumerable<T>> BuildCollectionResponse<T>(
            IEnumerable<T> items,
            string? message = null)
        {
            var list = items as IList<T> ?? items.ToList();

            return new ApiResponse<IEnumerable<T>>
            {
                Data = list,
                Meta = new
                {
                    TotalAmount = list.Count,
                    message = message ?? "Operación exitosa"
                }
            };
        }
    }
}
