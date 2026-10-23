using FarmaDiApi.Models;
using FarmaDiApi.Services;
using FarmaDiBusiness.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    /// <summary>
    /// Controlador para la consulta, filtrado y auditoría técnica de los logs registrados en MongoDB Atlas.
    /// Protegido con privilegios administrativos para salvaguardar la información técnica y de trazabilidad.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Admin")]
    public class LogsController : BaseApiController
    {
        private readonly ILogService _logService;

        // Conjunto validado de etiquetas de severidad aceptadas por el sistema
        private static readonly HashSet<string> AllowedLevels = new(StringComparer.OrdinalIgnoreCase)
        {
            "Information",
            "Warning",
            "Error"
        };

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Obtiene todos los logs registrados con paginación, permitiendo además filtrados opcionales combinados.
        /// </summary>
        /// <param name="page">Número de página (inicia en 1).</param>
        /// <param name="limit">Cantidad de registros por página (por defecto 10, máximo 100).</param>
        /// <param name="level">Filtro opcional por severidad: Information, Warning o Error.</param>
        /// <param name="startDate">Filtro opcional de fecha inicial.</param>
        /// <param name="endDate">Filtro opcional de fecha final.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición HTTP.</param>
        /// <response code="200">Retorna la colección paginada de logs solicitada.</response>
        /// <response code="400">Si los parámetros de filtrado no son válidos.</response>
        /// <response code="401">Si el usuario no está autenticado o carece de permisos de Administrador.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? level = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(ref page, ref limit);

            if (!string.IsNullOrWhiteSpace(level) && !AllowedLevels.Contains(level))
            {
                return BadRequestError(
                    $"El nivel de log '{level}' no es válido. Los niveles permitidos son: Information, Warning, Error.",
                    new { info = "Nivel de severidad no reconocido", allowedValues = AllowedLevels });
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequestError(
                    "La fecha de inicio (startDate) no puede ser posterior a la fecha final (endDate).",
                    new { info = "Rango de fechas inconsistente" });
            }

            var serviceResponse = await _logService.GetLogsPagedAsync(page, limit, level, startDate, endDate, cancellationToken);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(x => x.ToDto());
                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene los logs filtrados por etiqueta de severidad dinámica (Information, Warning o Error) con paginación.
        /// </summary>
        /// <param name="level">Etiqueta de severidad: Information, Warning o Error.</param>
        /// <param name="page">Número de página (inicia en 1).</param>
        /// <param name="limit">Cantidad de registros por página.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición HTTP.</param>
        [HttpGet("level/{level}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByLevel(
            [FromRoute] string level,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(ref page, ref limit);

            if (string.IsNullOrWhiteSpace(level) || !AllowedLevels.Contains(level))
            {
                return BadRequestError(
                    $"El nivel de log '{level}' no es válido. Los niveles permitidos son: Information, Warning, Error.",
                    new { info = "Nivel de severidad no reconocido", allowedValues = AllowedLevels });
            }

            var serviceResponse = await _logService.GetLogsByLevelPagedAsync(level, page, limit, cancellationToken);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(x => x.ToDto());
                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene exclusivamente los logs de severidad Information con paginación.
        /// </summary>
        [HttpGet("information")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInformationLogs(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            return await GetByLevel("Information", page, limit, cancellationToken);
        }

        /// <summary>
        /// Obtiene exclusivamente los logs de severidad Warning con paginación.
        /// </summary>
        [HttpGet("warning")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWarningLogs(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            return await GetByLevel("Warning", page, limit, cancellationToken);
        }

        /// <summary>
        /// Obtiene exclusivamente los logs de severidad Error con paginación.
        /// </summary>
        [HttpGet("error")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetErrorLogs(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            return await GetByLevel("Error", page, limit, cancellationToken);
        }

        /// <summary>
        /// Obtiene los logs registrados en el día en curso (UTC) con paginación.
        /// </summary>
        /// <param name="page">Número de página (inicia en 1).</param>
        /// <param name="limit">Cantidad de registros por página.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición HTTP.</param>
        [HttpGet("today")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogsToday(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _logService.GetLogsTodayPagedAsync(page, limit, cancellationToken);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(x => x.ToDto());
                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene los logs registrados dentro de un rango de fechas especificado con paginación.
        /// </summary>
        /// <param name="startDate">Fecha inicial del rango.</param>
        /// <param name="endDate">Fecha final del rango.</param>
        /// <param name="page">Número de página (inicia en 1).</param>
        /// <param name="limit">Cantidad de registros por página.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición HTTP.</param>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLogsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            NormalizePagination(ref page, ref limit);

            if (startDate > endDate)
            {
                return BadRequestError(
                    "La fecha de inicio (startDate) no puede ser posterior a la fecha final (endDate).",
                    new { info = "Rango de fechas inválido", startDate, endDate });
            }

            var serviceResponse = await _logService.GetLogsByDateRangePagedAsync(startDate, endDate, page, limit, cancellationToken);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(x => x.ToDto());
                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
