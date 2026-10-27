using FarmaDiBusiness.DTOs.OperationalMetrics;
using FarmaDiCore.Common;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    /// <summary>
    /// Contrato de servicio para la gestión y exposición de métricas operacionales (KPIs)
    /// obtenidas desde cubos multidimensionales OLAP en Analysis Services.
    /// </summary>
    public interface IOlapMetricsService
    {
        Task<ServiceResponse<PurchasesKpiResponseDto>> GetPurchasesKpisAsync(int? year = null, int? month = null);
        Task<ServiceResponse<SalesKpiResponseDto>> GetSalesKpisAsync(int? year = null, int? month = null);
        Task<ServiceResponse<InventoryKpiResponseDto>> GetInventoryKpisAsync(int? daysToExpire = null);
        Task<ServiceResponse<OperationalMetricsSummaryDto>> GetOperationalMetricsSummaryAsync(int? year = null, int? month = null);
    }
}
