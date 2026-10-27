using FarmaDiBusiness.DTOs.OperationalMetrics;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities.OperationalMetrics;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    /// <summary>
    /// Servicio de negocio encargado de coordinar la validación y transformación
    /// de los KPIs operacionales extraídos desde el motor OLAP.
    /// </summary>
    public class OlapMetricsService : IOlapMetricsService
    {
        private readonly IOlapMetricsRepository _olapRepository;

        public OlapMetricsService(IOlapMetricsRepository olapRepository)
        {
            _olapRepository = olapRepository;
        }

        public async Task<ServiceResponse<PurchasesKpiResponseDto>> GetPurchasesKpisAsync(int? year = null, int? month = null)
        {
            var serviceResponse = new ServiceResponse<PurchasesKpiResponseDto>();

            // Validación defensiva de parámetros temporales
            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorValidation;
                serviceResponse.Message = "El año especificado debe estar entre el 2000 y el 2100.";
                return serviceResponse;
            }

            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorValidation;
                serviceResponse.Message = "El mes especificado debe estar entre 1 y 12.";
                return serviceResponse;
            }

            var repoResponse = await _olapRepository.GetPurchasesKpisAsync(year, month);

            if (repoResponse.OperationStatusCode != 200 || repoResponse.Data == null)
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorDataBase;
                serviceResponse.Message = repoResponse.Message ?? "Error al obtener métricas de compras desde el cubo.";
                return serviceResponse;
            }

            var model = repoResponse.Data;
            var dto = new PurchasesKpiResponseDto
            {
                TotalOverallSpend = model.TotalOverallSpend,
                TotalItemsPurchased = model.TotalItemsPurchased,
                SupplierShares = model.SupplierShares.Select(s => new SupplierPurchaseShareDto
                {
                    SupplierName = s.SupplierName,
                    TotalSpend = s.TotalSpend,
                    TotalQuantity = s.TotalQuantity,
                    SharePercentage = s.SharePercentage
                }).ToList(),
                SpendVariations = model.SpendVariations.Select(v => new PurchaseSpendVariationDto
                {
                    Period = v.Period,
                    CurrentSpend = v.CurrentSpend,
                    PreviousSpend = v.PreviousSpend,
                    VariationPercentage = v.VariationPercentage
                }).ToList(),
                ProductSupplierMargins = model.ProductSupplierMargins.Select(m => new ProductSupplierMarginDto
                {
                    ProductName = m.ProductName,
                    SupplierName = m.SupplierName,
                    TotalPurchasedAmount = m.TotalPurchasedAmount,
                    QuantityPurchased = m.QuantityPurchased,
                    AverageUnitCost = m.AverageUnitCost,
                    EstimatedSalesPrice = m.EstimatedSalesPrice,
                    EstimatedMarginPercentage = m.EstimatedMarginPercentage
                }).ToList()
            };

            serviceResponse.Data = dto;
            serviceResponse.IsSuccess = true;
            serviceResponse.Success = true;
            serviceResponse.MessageCode = MessageCodes.Success;
            serviceResponse.Message = "Métricas operacionales de compras calculadas correctamente.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<SalesKpiResponseDto>> GetSalesKpisAsync(int? year = null, int? month = null)
        {
            var serviceResponse = new ServiceResponse<SalesKpiResponseDto>();

            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorValidation;
                serviceResponse.Message = "El año especificado debe estar entre el 2000 y el 2100.";
                return serviceResponse;
            }

            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorValidation;
                serviceResponse.Message = "El mes especificado debe estar entre 1 y 12.";
                return serviceResponse;
            }

            var repoResponse = await _olapRepository.GetSalesKpisAsync(year, month);

            if (repoResponse.OperationStatusCode != 200 || repoResponse.Data == null)
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorDataBase;
                serviceResponse.Message = repoResponse.Message ?? "Error al obtener métricas de ventas desde el cubo.";
                return serviceResponse;
            }

            var model = repoResponse.Data;
            var dto = new SalesKpiResponseDto
            {
                TotalOverallRevenue = model.TotalOverallRevenue,
                TotalItemsSold = model.TotalItemsSold,
                SalesVariations = model.SalesVariations.Select(v => new SalesVariationDto
                {
                    Period = v.Period,
                    CurrentSales = v.CurrentSales,
                    PreviousSales = v.PreviousSales,
                    VariationPercentage = v.VariationPercentage
                }).ToList(),
                GrossMargins = model.GrossMargins.Select(g => new GrossSalesMarginDto
                {
                    Period = g.Period,
                    TotalRevenue = g.TotalRevenue,
                    TotalCost = g.TotalCost,
                    GrossMargin = g.GrossMargin,
                    GrossMarginPercentage = g.GrossMarginPercentage
                }).ToList(),
                CategoryShares = model.CategoryShares.Select(c => new CategorySalesShareDto
                {
                    CategoryName = c.CategoryName,
                    TotalRevenue = c.TotalRevenue,
                    TotalQuantity = c.TotalQuantity,
                    SharePercentage = c.SharePercentage
                }).ToList()
            };

            serviceResponse.Data = dto;
            serviceResponse.IsSuccess = true;
            serviceResponse.Success = true;
            serviceResponse.MessageCode = MessageCodes.Success;
            serviceResponse.Message = "Métricas operacionales de ventas calculadas correctamente.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<InventoryKpiResponseDto>> GetInventoryKpisAsync(int? daysToExpire = null)
        {
            var serviceResponse = new ServiceResponse<InventoryKpiResponseDto>();

            if (daysToExpire.HasValue && (daysToExpire.Value < 1 || daysToExpire.Value > 3650))
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorValidation;
                serviceResponse.Message = "El umbral de días para vencimiento debe estar entre 1 y 3650 días.";
                return serviceResponse;
            }

            var repoResponse = await _olapRepository.GetInventoryKpisAsync(daysToExpire);

            if (repoResponse.OperationStatusCode != 200 || repoResponse.Data == null)
            {
                serviceResponse.IsSuccess = false;
                serviceResponse.Success = false;
                serviceResponse.MessageCode = MessageCodes.ErrorDataBase;
                serviceResponse.Message = repoResponse.Message ?? "Error al obtener métricas de inventario desde el cubo.";
                return serviceResponse;
            }

            var model = repoResponse.Data;
            var dto = new InventoryKpiResponseDto
            {
                TotalInventoryItems = model.TotalInventoryItems,
                OverallExpiringPercentage = model.OverallExpiringPercentage,
                Availability = new ProductAvailabilityDto
                {
                    TotalProducts = model.Availability.TotalProducts,
                    InStockProducts = model.Availability.InStockProducts,
                    OutOfStockProducts = model.Availability.OutOfStockProducts,
                    CriticalStockProducts = model.Availability.CriticalStockProducts,
                    AvailabilityRatePercentage = model.Availability.AvailabilityRatePercentage
                },
                ProductTurnovers = model.ProductTurnovers.Select(t => new ProductInventoryTurnoverDto
                {
                    ProductName = t.ProductName,
                    AvailableQuantity = t.AvailableQuantity,
                    BatchInitialQuantity = t.BatchInitialQuantity,
                    DispatchedQuantity = t.DispatchedQuantity,
                    TurnoverRatio = t.TurnoverRatio
                }).ToList(),
                ExpiringBatches = model.ExpiringBatches.Select(b => new ExpiringInventoryDto
                {
                    BatchNumber = b.BatchNumber,
                    ProductName = b.ProductName,
                    ExpirationDate = b.ExpirationDate,
                    DaysUntilExpiration = b.DaysUntilExpiration,
                    AvailableQuantity = b.AvailableQuantity,
                    BatchInitialQuantity = b.BatchInitialQuantity,
                    NearExpirationPercentage = b.NearExpirationPercentage,
                    Status = b.Status
                }).ToList()
            };

            serviceResponse.Data = dto;
            serviceResponse.IsSuccess = true;
            serviceResponse.Success = true;
            serviceResponse.MessageCode = MessageCodes.Success;
            serviceResponse.Message = "Métricas operacionales de inventario calculadas correctamente.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<OperationalMetricsSummaryDto>> GetOperationalMetricsSummaryAsync(int? year = null, int? month = null)
        {
            var serviceResponse = new ServiceResponse<OperationalMetricsSummaryDto>();

            var purchasesTask = GetPurchasesKpisAsync(year, month);
            var salesTask = GetSalesKpisAsync(year, month);
            var inventoryTask = GetInventoryKpisAsync();

            await Task.WhenAll(purchasesTask, salesTask, inventoryTask);

            var purchasesRes = await purchasesTask;
            var salesRes = await salesTask;
            var inventoryRes = await inventoryTask;

            var summary = new OperationalMetricsSummaryDto();

            if (purchasesRes.IsSuccess && purchasesRes.Data != null)
            {
                summary.Purchases = purchasesRes.Data;
            }

            if (salesRes.IsSuccess && salesRes.Data != null)
            {
                summary.Sales = salesRes.Data;
            }

            if (inventoryRes.IsSuccess && inventoryRes.Data != null)
            {
                summary.Inventory = inventoryRes.Data;
            }

            serviceResponse.Data = summary;
            serviceResponse.IsSuccess = true;
            serviceResponse.Success = true;
            serviceResponse.MessageCode = MessageCodes.Success;
            serviceResponse.Message = "Resumen de métricas operacionales consolidado exitosamente.";

            return serviceResponse;
        }
    }
}
