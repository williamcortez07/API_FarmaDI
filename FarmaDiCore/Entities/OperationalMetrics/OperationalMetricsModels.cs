using System;
using System.Collections.Generic;

namespace FarmaDiCore.Entities.OperationalMetrics
{
    public class SupplierPurchaseShare
    {
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalSpend { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SharePercentage { get; set; }
    }

    public class PurchaseSpendVariation
    {
        public string Period { get; set; } = string.Empty;
        public decimal CurrentSpend { get; set; }
        public decimal? PreviousSpend { get; set; }
        public decimal? VariationPercentage { get; set; }
    }

    public class ProductSupplierMargin
    {
        public string ProductName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchasedAmount { get; set; }
        public int QuantityPurchased { get; set; }
        public decimal AverageUnitCost { get; set; }
        public decimal? EstimatedSalesPrice { get; set; }
        public decimal? EstimatedMarginPercentage { get; set; }
    }

    public class PurchasesKpiModel
    {
        public List<SupplierPurchaseShare> SupplierShares { get; set; } = new();
        public List<PurchaseSpendVariation> SpendVariations { get; set; } = new();
        public List<ProductSupplierMargin> ProductSupplierMargins { get; set; } = new();
        public decimal TotalOverallSpend { get; set; }
        public int TotalItemsPurchased { get; set; }
    }

    public class SalesVariation
    {
        public string Period { get; set; } = string.Empty;
        public decimal CurrentSales { get; set; }
        public decimal? PreviousSales { get; set; }
        public decimal? VariationPercentage { get; set; }
    }

    public class GrossSalesMargin
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossMargin { get; set; }
        public decimal GrossMarginPercentage { get; set; }
    }

    public class CategorySalesShare
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SharePercentage { get; set; }
    }

    public class SalesKpiModel
    {
        public List<SalesVariation> SalesVariations { get; set; } = new();
        public List<GrossSalesMargin> GrossMargins { get; set; } = new();
        public List<CategorySalesShare> CategoryShares { get; set; } = new();
        public decimal TotalOverallRevenue { get; set; }
        public int TotalItemsSold { get; set; }
    }

    public class ProductInventoryTurnover
    {
        public string ProductName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int BatchInitialQuantity { get; set; }
        public int DispatchedQuantity { get; set; }
        public decimal TurnoverRatio { get; set; }
    }

    public class ExpiringInventory
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

    public class ProductAvailability
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int CriticalStockProducts { get; set; }
        public decimal AvailabilityRatePercentage { get; set; }
    }

    public class InventoryKpiModel
    {
        public List<ProductInventoryTurnover> ProductTurnovers { get; set; } = new();
        public List<ExpiringInventory> ExpiringBatches { get; set; } = new();
        public decimal OverallExpiringPercentage { get; set; }
        public ProductAvailability Availability { get; set; } = new();
        public int TotalInventoryItems { get; set; }
    }

    public class OperationalMetricsSummaryModel
    {
        public PurchasesKpiModel Purchases { get; set; } = new();
        public SalesKpiModel Sales { get; set; } = new();
        public InventoryKpiModel Inventory { get; set; } = new();
    }
}
