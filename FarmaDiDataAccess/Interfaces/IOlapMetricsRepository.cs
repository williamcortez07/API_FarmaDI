using FarmaDiCore.Common;
using FarmaDiCore.Entities.OperationalMetrics;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    /// <summary>
    /// Contrato de repositorio para la extracción y procesamiento de métricas operacionales
    /// desde los cubos OLAP de Analysis Services mediante ADOMD.NET.
    /// </summary>
    public interface IOlapMetricsRepository
    {
        Task<RepositoryResponse<PurchasesKpiModel>> GetPurchasesKpisAsync(int? year = null, int? month = null);
        Task<RepositoryResponse<SalesKpiModel>> GetSalesKpisAsync(int? year = null, int? month = null);
        Task<RepositoryResponse<InventoryKpiModel>> GetInventoryKpisAsync(int? daysToExpire = null);
        Task<RepositoryResponse<OperationalMetricsSummaryModel>> GetOperationalMetricsSummaryAsync(int? year = null, int? month = null);
    }
}
