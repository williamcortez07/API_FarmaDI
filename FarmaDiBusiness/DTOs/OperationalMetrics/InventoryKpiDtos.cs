using System;
using System.Collections.Generic;

namespace FarmaDiBusiness.DTOs.OperationalMetrics
{
    /// <summary>
    /// KPI 3.1: Índice de rotación de inventario calculado a nivel de producto.
    /// </summary>
    public class ProductInventoryTurnoverDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int BatchInitialQuantity { get; set; }
        public int DispatchedQuantity { get; set; }
        public decimal TurnoverRatio { get; set; }
    }

    /// <summary>
    /// KPI 3.2: Información de lotes próximos a vencer y su impacto porcentual.
    /// </summary>
    public class ExpiringInventoryDto
    {
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public int AvailableQuantity { get; set; }
        public int BatchInitialQuantity { get; set; }
        public decimal NearExpirationPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// KPI 3.3: Tasa de disponibilidad operativa de productos en catálogo.
    /// </summary>
    public class ProductAvailabilityDto
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int CriticalStockProducts { get; set; }
        public decimal AvailabilityRatePercentage { get; set; }
    }

    /// <summary>
    /// Contenedor consolidado para todos los KPIs operacionales de inventario.
    /// </summary>
    public class InventoryKpiResponseDto
    {
        public List<ProductInventoryTurnoverDto> ProductTurnovers { get; set; } = new();
        public List<ExpiringInventoryDto> ExpiringBatches { get; set; } = new();
        public decimal OverallExpiringPercentage { get; set; }
        public ProductAvailabilityDto Availability { get; set; } = new();
        public int TotalInventoryItems { get; set; }
    }
}
