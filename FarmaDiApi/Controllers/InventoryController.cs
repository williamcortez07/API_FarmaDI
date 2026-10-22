using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class InventoryController : BaseApiController
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _inventoryService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoryDtoCollection = serviceResponse.Data.Select(i => new GetAllInventoryDto
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.oproduct.ProductId,
                    ProductGenericname = i.oproduct.GenericName,
                    SalePrice = i.SalePrice,
                    PurchasePrice = i.PurchasePrice,
                    CriticalStock = i.CriticalStock
                });

                return Ok(BuildCollectionResponse(inventoryDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _inventoryService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoryDto = new GetAllInventoryDto
                {
                    InventoryId = serviceResponse.Data.InventoryId,
                    ProductId = serviceResponse.Data.oproduct.ProductId,
                    ProductGenericname = serviceResponse.Data.oproduct.GenericName,
                    SalePrice = serviceResponse.Data.SalePrice,
                    PurchasePrice = serviceResponse.Data.PurchasePrice,
                    CriticalStock = serviceResponse.Data.CriticalStock,
                };

                return Ok(inventoryDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: $"No se encontró ningún dato relacionado al Id {id}");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _inventoryService.GetInventoryPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                return Ok(BuildPagedResponse(items, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("with-details")]
        public async Task<IActionResult> GetWithDetails(
            [FromQuery] int? categoryId,
            [FromQuery] string? estado,
            [FromQuery] int? brandId,
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? fechaCorte,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _inventoryService.GetInventoryWithDetailsAsync(
                page,
                limit,
                categoryId,
                estado,
                brandId,
                supplierId,
                fechaCorte);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                return Ok(BuildPagedResponse(items, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("{productId}/details")]
        public async Task<IActionResult> GetByIdWithDetails(int productId)
        {
            if (!TryValidatePositiveId(productId, "ProductId", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _inventoryService.GetByIdWithDetailsAsync(productId);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: $"No se encontró inventario con detalles para el producto con Id {productId}");
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] int? categoryId,
            [FromQuery] string? estado,
            [FromQuery] int? brandId,
            [FromQuery] int? supplierId,
            [FromQuery] DateTime? fechaCorte,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _inventoryService.GetDashboardAsync(
                page,
                limit,
                categoryId,
                estado,
                brandId,
                supplierId,
                fechaCorte);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var dashboardData = serviceResponse.Data;
                int totalCount = dashboardData.Summary.TotalProductos;
                int totalPages = (int)Math.Ceiling(totalCount / (double)limit);

                var apiResponse = new ApiResponse<InventoryDashboardDto>
                {
                    Data = dashboardData,
                    Meta = new GetPagedDto
                    {
                        TotalItems = totalCount,
                        TotalPages = totalPages,
                        CurrentPage = page,
                        ItemsPerPage = limit
                    }
                };

                return Ok(apiResponse);
            }

            return MapServiceError(serviceResponse);
        }
    }
}
