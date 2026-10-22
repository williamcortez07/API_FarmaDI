using FarmaDiBusiness.DTOs.SaleDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class SalesController : BaseApiController
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateSaleDto dto)
        {
            if (dto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _saleService.InsertAsync(dto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data.InvoiceId },
                    serviceResponse.Data
                );
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Recurso no encontrado",
                customConflictMessage: "Conflicto al procesar la venta");
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequestError(
                    "Solicitud inválida",
                    new { info = "El ID de factura debe ser un número entero positivo." });
            }

            var serviceResponse = await _saleService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Recurso no encontrado");
        }

        [HttpGet]
        public async Task<IActionResult> GetSales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequestError(
                    "Solicitud inválida",
                    new { info = "Los parámetros 'pageNumber' y 'pageSize' deben ser enteros positivos mayores a cero." });
            }

            var serviceResponse = await _saleService.GetSalesPagedAsync(pageNumber, pageSize);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Recurso no encontrado");
        }
    }
}
