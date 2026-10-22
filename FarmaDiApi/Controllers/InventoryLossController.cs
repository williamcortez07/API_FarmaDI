using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class InventoryLossController : BaseApiController
    {
        private readonly IInventoryLossService _inventoryLossService;

        public InventoryLossController(IInventoryLossService inventoryLossService)
        {
            _inventoryLossService = inventoryLossService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _inventoryLossService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoryLossDtoCollection = serviceResponse.Data.Select(il => new GetAllInventoryLossDto
                {
                    LowId = il.LowId,
                    BatchId = il.oBatch.Id,
                    BatchNumber = il.oBatch.BatchNumer,
                    Quantity = il.Quantity,
                    ProductId = il.oProduct.ProductId,
                    ProductGenericName = il.oProduct.GenericName,
                    ProductTradeName = il.oProduct.TradeName,
                    UserId = il.UserId,
                    UserName = il.UserName,
                    Reason = il.Reason,
                });

                return Ok(BuildCollectionResponse(inventoryLossDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _inventoryLossService.GetInventoryLossPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(il => new GetAllInventoryLossDto
                {
                    LowId = il.LowId,
                    BatchId = il.oBatch.Id,
                    BatchNumber = il.oBatch.BatchNumer,
                    Quantity = il.Quantity,
                    ProductId = il.oProduct.ProductId,
                    ProductGenericName = il.oProduct.GenericName,
                    ProductTradeName = il.oProduct.TradeName,
                    UserId = il.UserId,
                    UserName = il.UserName,
                    Reason = il.Reason,
                });

                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
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

            var serviceResponse = await _inventoryLossService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoryLossDto = new GetAllInventoryLossDto
                {
                    LowId = serviceResponse.Data.LowId,
                    BatchId = serviceResponse.Data.oBatch.Id,
                    BatchNumber = serviceResponse.Data.oBatch.BatchNumer,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    ProductGenericName = serviceResponse.Data.oProduct.GenericName,
                    ProductTradeName = serviceResponse.Data.oProduct.TradeName,
                    UserId = serviceResponse.Data.UserId,
                    UserName = serviceResponse.Data.UserName,
                    Reason = serviceResponse.Data.Reason,
                };

                return Ok(inventoryLossDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una baja asociada al Id proporcionado");
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddInventoryLossDto addInventoryLossDto)
        {
            if (addInventoryLossDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            if (addInventoryLossDto.Quantity <= 0)
            {
                return BadRequestError(
                    "Cantidad inferior a la requerida",
                    new { info = "La cantidad requerida para la operación debe ser mayor a 0" });
            }

            var serviceResponse = await _inventoryLossService.AddAsync(addInventoryLossDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var inventoryLossDto = new InventoryLossDto
                {
                    LowId = serviceResponse.Data.LowId,
                    BatchId = serviceResponse.Data.oBatch.Id,
                    Quantity = serviceResponse.Data.Quantity,
                    ProductId = serviceResponse.Data.oProduct.ProductId,
                    UserId = serviceResponse.Data.UserId,
                    Reason = serviceResponse.Data.Reason,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = inventoryLossDto.LowId },
                    inventoryLossDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El registro de baja no pudo completarse por conflicto de datos");
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
            const string mensaje = "Estamos mejorando nuestros sistemas, por favor inténtelo otro día";
            return Ok(new
            {
                Status = "Mantenimiento",
                Message = "Temporalmente se encuentra en mantenimiento. " + mensaje
            });
        }
    }
}
