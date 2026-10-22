using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.SupplierDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class SuppliersController : BaseApiController
    {
        private readonly ISupplierService _suppliersService;

        public SuppliersController(ISupplierService supplier)
        {
            _suppliersService = supplier;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddSupplierDto addsupplierDto)
        {
            if (addsupplierDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _suppliersService.AddAsync(addsupplierDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var supplierDto = new SupplierDto
                {
                    SupplierId = serviceResponse.Data.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    RNC = serviceResponse.Data.RNC,
                    Mail = serviceResponse.Data.Mail,
                    SupplierPhone = serviceResponse.Data.SupplierPhone,
                    SupplierAddress = serviceResponse.Data.SupplierAddress,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = supplierDto.SupplierId },
                    supplierDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre o RNC del proveedor ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _suppliersService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var suppliersDtoCollection = serviceResponse.Data.Select(c => new GetAllSupplierDto
                {
                    SupplierId = c.SupplierId,
                    SupplierName = c.SupplierName,
                    RNC = c.RNC,
                    Mail = c.Mail,
                    SupplierPhone = c.SupplierPhone,
                    SupplierAddress = c.SupplierAddress,
                    IsActive = c.IsActive,
                });

                return Ok(BuildCollectionResponse(suppliersDtoCollection, serviceResponse.Message));
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

            var serviceResponse = await _suppliersService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var supplierDto = new SupplierDto
                {
                    SupplierId = serviceResponse.Data.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    RNC = serviceResponse.Data.RNC,
                    Mail = serviceResponse.Data.Mail,
                    SupplierPhone = serviceResponse.Data.SupplierPhone,
                    SupplierAddress = serviceResponse.Data.SupplierAddress,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(supplierDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un proveedor asociado al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto datasupplier)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (datasupplier == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _suppliersService.UpdateAsync(id, datasupplier);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updatedSupplier = new SupplierDto
                {
                    SupplierId = serviceResponse.Data.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    RNC = serviceResponse.Data.RNC,
                    Mail = serviceResponse.Data.Mail,
                    SupplierPhone = serviceResponse.Data.SupplierPhone,
                    SupplierAddress = serviceResponse.Data.SupplierAddress,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedSupplier);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un proveedor con el Id proporcionado");
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequestError(
                    "El dato proporcionado no es válido",
                    new { Error = "El nombre no puede ser nulo o vacío" });
            }

            var serviceResponse = await _suppliersService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var supplierDto = new SupplierDto
                {
                    SupplierId = serviceResponse.Data.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    RNC = serviceResponse.Data.RNC,
                    Mail = serviceResponse.Data.Mail,
                    SupplierPhone = serviceResponse.Data.SupplierPhone,
                    SupplierAddress = serviceResponse.Data.SupplierAddress,
                };

                return Ok(supplierDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Proveedor no encontrado");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _suppliersService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    SupplierId = serviceResponse.Data.SupplierId,
                    SupplierName = serviceResponse.Data.SupplierName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un proveedor con el Id proporcionado");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _suppliersService.GetSuppliersPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Item1 != null)
            {
                var (suppliers, totalCount) = serviceResponse.Data;
                var suppliersCollection = suppliers.Select(b => new GetAllSupplierDto
                {
                    SupplierId = b.SupplierId,
                    SupplierName = b.SupplierName,
                    RNC = b.RNC,
                    SupplierPhone = b.SupplierPhone,
                    Mail = b.Mail,
                    SupplierAddress = b.SupplierAddress,
                    IsActive = b.IsActive
                });

                return Ok(BuildPagedResponse(suppliersCollection, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
