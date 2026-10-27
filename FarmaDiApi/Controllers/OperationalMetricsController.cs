using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.OperationalMetrics;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    /// <summary>
    /// Controlador responsable de exponer las métricas operacionales y KPIs del negocio
    /// calculadas a partir de los cubos multidimensionales OLAP en Analysis Services.
    /// </summary>
    [Route("api/[controller]")]
    public class OperationalMetricsController : BaseApiController
    {
        private readonly IOlapMetricsService _metricsService;

        public OperationalMetricsController(IOlapMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        /// <summary>
        /// Obtiene los KPIs de compras:
        /// 1. Participación de compras por proveedor (%)
        /// 2. Variación del gasto en compras por período (%)
        /// 3. Ganancia / Margen estimado por producto y proveedor
        /// </summary>
        /// <param name="year">Año de consulta opcional para acotar el cálculo.</param>
        /// <param name="month">Mes de consulta opcional (1 a 12).</param>
        [HttpGet("purchases")]
        [ProducesResponseType(typeof(PurchasesKpiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPurchasesMetrics([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            var serviceResponse = await _metricsService.GetPurchasesKpisAsync(year, month);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene los KPIs de ventas:
        /// 1. Variación de las ventas por período (%)
        /// 2. Margen bruto de ventas (%)
        /// 3. Participación de ventas por categoría (%)
        /// </summary>
        /// <param name="year">Año de consulta opcional para acotar el cálculo.</param>
        /// <param name="month">Mes de consulta opcional (1 a 12).</param>
        [HttpGet("sales")]
        [ProducesResponseType(typeof(SalesKpiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSalesMetrics([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            var serviceResponse = await _metricsService.GetSalesKpisAsync(year, month);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene los KPIs de inventario:
        /// 1. Índice de rotación de inventario por producto
        /// 2. Porcentaje de inventario próximo a vencer (%)
        /// 3. Tasa de disponibilidad operativa de productos (%)
        /// </summary>
        /// <param name="daysToExpire">Días límite para considerar un lote como próximo a vencer (por defecto 90 días).</param>
        [HttpGet("inventory")]
        [ProducesResponseType(typeof(InventoryKpiResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryMetrics([FromQuery] int? daysToExpire = 90)
        {
            var serviceResponse = await _metricsService.GetInventoryKpisAsync(daysToExpire);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Resumen consolidado de todos los KPIs operacionales (Compras, Ventas e Inventario)
        /// para el panel central de analítica del sistema web.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(OperationalMetricsSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnsuccessfulResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMetricsSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            var serviceResponse = await _metricsService.GetOperationalMetricsSummaryAsync(year, month);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(serviceResponse);
        }
    }
}
