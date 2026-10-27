using System;
using System.Collections.Generic;

namespace FarmaDiBusiness.DTOs.OperationalMetrics
{
    /// <summary>
    /// KPI 2.1: Variación porcentual de las ventas respecto al período previo.
    /// </summary>
    public class SalesVariationDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal CurrentSales { get; set; }
        public decimal? PreviousSales { get; set; }
        public decimal? VariationPercentage { get; set; }
    }

    /// <summary>
    /// KPI 2.2: Margen bruto de ventas expresado en valor monetario y porcentual.
    /// </summary>
    public class GrossSalesMarginDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossMargin { get; set; }
        public decimal GrossMarginPercentage { get; set; }
    }

    /// <summary>
    /// KPI 2.3: Participación porcentual de ventas por categoría de producto.
    /// </summary>
    public class CategorySalesShareDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SharePercentage { get; set; }
    }

    /// <summary>
    /// Contenedor consolidado para todos los KPIs operacionales de ventas.
    /// </summary>
    public class SalesKpiResponseDto
    {
        public List<SalesVariationDto> SalesVariations { get; set; } = new();
        public List<GrossSalesMarginDto> GrossMargins { get; set; } = new();
        public List<CategorySalesShareDto> CategoryShares { get; set; } = new();
        public decimal TotalOverallRevenue { get; set; }
        public int TotalItemsSold { get; set; }
    }
}
