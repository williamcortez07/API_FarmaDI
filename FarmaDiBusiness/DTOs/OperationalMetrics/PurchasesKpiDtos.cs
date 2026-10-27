using System;
using System.Collections.Generic;

namespace FarmaDiBusiness.DTOs.OperationalMetrics
{
    /// <summary>
    /// KPI 1.1: Representa la participación porcentual en compras de un proveedor.
    /// </summary>
    public class SupplierPurchaseShareDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalSpend { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SharePercentage { get; set; }
    }

    /// <summary>
    /// KPI 1.2: Variación porcentual del gasto en compras respecto a períodos anteriores.
    /// </summary>
    public class PurchaseSpendVariationDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal CurrentSpend { get; set; }
        public decimal? PreviousSpend { get; set; }
        public decimal? VariationPercentage { get; set; }
    }

    /// <summary>
    /// KPI 1.3: Margen y ganancia por producto y proveedor adquirido.
    /// </summary>
    public class ProductSupplierMarginDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchasedAmount { get; set; }
        public int QuantityPurchased { get; set; }
        public decimal AverageUnitCost { get; set; }
        public decimal? EstimatedSalesPrice { get; set; }
        public decimal? EstimatedMarginPercentage { get; set; }
    }

    /// <summary>
    /// Contenedor consolidado para todos los KPIs operacionales de compras.
    /// </summary>
    public class PurchasesKpiResponseDto
    {
        public List<SupplierPurchaseShareDto> SupplierShares { get; set; } = new();
        public List<PurchaseSpendVariationDto> SpendVariations { get; set; } = new();
        public List<ProductSupplierMarginDto> ProductSupplierMargins { get; set; } = new();
        public decimal TotalOverallSpend { get; set; }
        public int TotalItemsPurchased { get; set; }
    }
}
