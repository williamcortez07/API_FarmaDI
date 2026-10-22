using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class PurchaseController : BaseApiController
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _purchaseService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: $"No se encontró la compra con el Id {id}");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _purchaseService.GetPurchasesPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                return Ok(BuildPagedResponse(items, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("with-details")]
        public async Task<IActionResult> GetWithDetails([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _purchaseService.GetPurchasesWithDetailsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                return Ok(BuildPagedResponse(items, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreatePurchaseDto dto)
        {
            if (dto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _purchaseService.InsertAsync(dto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data.Id },
                    serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Recurso transaccional no encontrado",
                customConflictMessage: "Conflicto al procesar la transacción de compra");
        }
    }
}
