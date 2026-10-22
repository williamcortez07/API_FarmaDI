using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class BrandsController : BaseApiController
    {
        private readonly IBrandsService _brandsService;

        public BrandsController(IBrandsService brandsService)
        {
            _brandsService = brandsService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddBrandDto addBrandDto)
        {
            if (addBrandDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _brandsService.AddAsync(addBrandDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var brandDto = new BrandDto
                {
                    BrandId = serviceResponse.Data.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = brandDto.BrandId },
                    brandDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre de la marca ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _brandsService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var brandsDtoCollection = serviceResponse.Data.Select(b => new GetAllBrandsDto
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    BrandDescription = b.Description,
                    IsActive = b.IsActive
                });

                return Ok(BuildCollectionResponse(brandsDtoCollection, serviceResponse.Message));
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

            var serviceResponse = await _brandsService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var brandDto = new BrandDto
                {
                    BrandId = serviceResponse.Data.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(brandDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una marca asociada al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandDto brand)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (brand == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _brandsService.UpdateAsync(id, brand);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updated = new BrandDto
                {
                    BrandId = serviceResponse.Data.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updated);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una marca con el Id proporcionado");
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

            var serviceResponse = await _brandsService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var brandDto = new BrandDto
                {
                    BrandId = serviceResponse.Data.BrandId,
                    Name = serviceResponse.Data.BrandName,
                    Description = serviceResponse.Data.Description,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(brandDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Marca no encontrada");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _brandsService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    BrandId = serviceResponse.Data.BrandId,
                    BrandName = serviceResponse.Data.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una marca con el Id proporcionado");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _brandsService.GetBrandsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Item1 != null)
            {
                var (brands, totalCount) = serviceResponse.Data;
                var brandsCollection = brands.Select(b => new GetAllBrandsDto
                {
                    BrandId = b.BrandId,
                    BrandName = b.BrandName,
                    BrandDescription = b.Description,
                    IsActive = b.IsActive
                });

                return Ok(BuildPagedResponse(brandsCollection, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
