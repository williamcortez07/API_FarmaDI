using FarmaDiBusiness.DTOs.StockDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class StockController : BaseApiController
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _stockService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var stockDtoCollection = serviceResponse.Data.Select(c => new StockDto
                {
                    StockId = c.Id,
                    BachtId = c.BatchId.Id,
                    BatchNumber = c.BatchId.BatchNumer,
                    ProductId = c.ProductId,
                    AvailableQuantity = c.AvailableQuantity,
                });

                return Ok(BuildCollectionResponse(stockDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _stockService.GetStockPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var stockDtoCollection = items.Select(c => new StockDto
                {
                    StockId = c.Id,
                    BachtId = c.BatchId.Id,
                    BatchNumber = c.BatchId.BatchNumer,
                    ProductId = c.ProductId,
                    AvailableQuantity = c.AvailableQuantity,
                });

                return Ok(BuildPagedResponse(stockDtoCollection, totalCount, page, limit));
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

            var serviceResponse = await _stockService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var stockDto = new StockDto
                {
                    StockId = serviceResponse.Data.Id,
                    BachtId = serviceResponse.Data.BatchId.Id,
                    BatchNumber = serviceResponse.Data.BatchId.BatchNumer,
                    ProductId = serviceResponse.Data.ProductId,
                    AvailableQuantity = serviceResponse.Data.AvailableQuantity,
                };

                return Ok(stockDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró ningún registro asociado al Id proporcionado");
        }
    }
}
