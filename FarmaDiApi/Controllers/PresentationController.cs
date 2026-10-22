using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class PresentationController : BaseApiController
    {
        private readonly IPresentationService _presentationService;

        public PresentationController(IPresentationService presentationService)
        {
            _presentationService = presentationService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddPresentationDto addPresentationDto)
        {
            if (addPresentationDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _presentationService.AddAsync(addPresentationDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = presentationDto.Id },
                    presentationDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre de la presentación ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _presentationService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var presentationDtoCollection = serviceResponse.Data.Select(p => new GetAllPresentationDto
                {
                    Id = p.Id,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitMeasure = p.UnitMeasure,
                    IsActive = p.IsActive
                });

                return Ok(BuildCollectionResponse(presentationDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _presentationService.GetPresentationsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (presentations, totalCount) = serviceResponse.Data;
                var presentationDtoCollection = presentations.Select(p => new GetAllPresentationDto
                {
                    Id = p.Id,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitMeasure = p.UnitMeasure,
                    IsActive = p.IsActive
                });

                return Ok(BuildPagedResponse(presentationDtoCollection, totalCount, page, limit));
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

            var serviceResponse = await _presentationService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return Ok(presentationDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró una presentación asociada al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePresentationDto data)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (data == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _presentationService.UpdateAsync(id, data);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updatedPresentation = new PresentationDto
                {
                    Id = serviceResponse.Data.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedPresentation);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró presentación con el Id proporcionado");
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

            var serviceResponse = await _presentationService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var presentationDto = new PresentationDto
                {
                    Id = serviceResponse.Data.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                };

                return Ok(presentationDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Presentación no encontrada");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _presentationService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    Id = serviceResponse.Data.Id,
                    Description = serviceResponse.Data.Description,
                    Quantity = serviceResponse.Data.Quantity,
                    UnitMeasure = serviceResponse.Data.UnitMeasure,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró presentación con el Id proporcionado");
        }
    }
}
