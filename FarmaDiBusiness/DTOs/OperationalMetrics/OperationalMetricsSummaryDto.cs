namespace FarmaDiBusiness.DTOs.OperationalMetrics
{
    /// <summary>
    /// Resumen ejecutivo que unifica los KPIs de Compras, Ventas e Inventario
    /// obtenidos desde el Cubo OLAP para el sistema web.
    /// </summary>
    public class OperationalMetricsSummaryDto
    {
        public PurchasesKpiResponseDto Purchases { get; set; } = new();
        public SalesKpiResponseDto Sales { get; set; } = new();
        public InventoryKpiResponseDto Inventory { get; set; } = new();
    }
}
