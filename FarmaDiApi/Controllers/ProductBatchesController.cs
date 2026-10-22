using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductBatchesDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductBatchesController : BaseApiController
    {
        private readonly IProductBatchesService _productBatchesService;

        public ProductBatchesController(IProductBatchesService productBatchesService)
        {
            _productBatchesService = productBatchesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _productBatchesService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var batchesDtoCollection = serviceResponse.Data.Select(b => new GetAllProductBatchesDto
                {
                    BatchId = b.Id,
                    BatchNumer = b.BatchNumer,
                    ManufacturingDate = b.ManufacturingDate,
                    ExpirationDate = b.ExpirationDate,
                    Quantity = b.Quantity,
                    ProductId = b.oProduct.ProductId,
                    ProductGenericName = b.oProduct.GenericName,
                    ProductTradeName = b.oProduct.TradeName,
                    IsActive = b.IsActive,
                });

                return Ok(BuildCollectionResponse(batchesDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _productBatchesService.GetBatchesPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (batches, totalCount) = serviceResponse.Data;
                var batchesDtoCollection = batches.Select(b => new GetAllProductBatchesDto
                {
                    BatchId = b.Id,
                    BatchNumer = b.BatchNumer,
                    ManufacturingDate = b.ManufacturingDate,
                    ExpirationDate = b.ExpirationDate,
                    Quantity = b.Quantity,
                    ProductId = b.oProduct.ProductId,
                    ProductGenericName = b.oProduct.GenericName,
                    ProductTradeName = b.oProduct.TradeName,
                    IsActive = b.IsActive,
                });

                return Ok(BuildPagedResponse(batchesDtoCollection, totalCount, page, limit));
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

            var serviceResponse = await _productBatchesService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productBatchDto = new GetAllProductBatchesDto
                {
                    BatchId = serviceResponse.Data.Id,
                    BatchNumer = serviceResponse.Data.BatchNumer,
                    ManufacturingDate = serviceResponse.Data.ManufacturingDate,
                    ExpirationDate = serviceResponse.Data.ExpirationDate,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    ProductGenericName = serviceResponse.Data.oProduct.GenericName,
                    ProductTradeName = serviceResponse.Data.oProduct.TradeName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productBatchDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un lote asociado al Id proporcionado");
        }
    }
}
