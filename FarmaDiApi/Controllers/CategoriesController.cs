using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class CategoriesController : BaseApiController
    {
        private readonly ICategoriesService _categoryService;

        public CategoriesController(ICategoriesService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddCategoryDto addCategoryDto)
        {
            if (addCategoryDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _categoryService.AddAsync(addCategoryDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = categoryDto.CategoryId },
                    categoryDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre de la categoría ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _categoryService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var dtoCollection = serviceResponse.Data.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive
                });

                return Ok(BuildCollectionResponse(dtoCollection, serviceResponse.Message));
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

            var serviceResponse = await _categoryService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(categoryDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una categoría asociada al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto category)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (category == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _categoryService.UpdateAsync(id, category);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updated = new CategoryDto
                {
                    CategoryId = serviceResponse.Data.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updated);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una categoría con el Id proporcionado");
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

            var serviceResponse = await _categoryService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var categoryDto = new CategoryDto
                {
                    CategoryId = serviceResponse.Data.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    Description = serviceResponse.Data.CategoryDescription,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(categoryDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Categoría no encontrada");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _categoryService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    CategoryId = serviceResponse.Data.CategoryId,
                    CategoryName = serviceResponse.Data.CategoryName,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró categoría con el Id proporcionado");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _categoryService.GetCategoriesPaged(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Item1 != null)
            {
                var (categories, totalCount) = serviceResponse.Data;
                var categoriesCollection = categories.Select(c => new GetAllCategoriesDto
                {
                    Id = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.CategoryDescription,
                    IsActive = c.IsActive,
                });

                return Ok(BuildPagedResponse(categoriesCollection, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
