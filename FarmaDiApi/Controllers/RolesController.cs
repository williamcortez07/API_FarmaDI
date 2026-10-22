using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Admin")]
    public class RolesController : BaseApiController
    {
        private readonly IRolService _rolService;

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddRolDto addRolDto)
        {
            if (addRolDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _rolService.AddRolAsync(addRolDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var rolDto = new RolDto
                {
                    RolId = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetByName),
                    new { name = rolDto.RolName },
                    rolDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre del rol ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _rolService.GetAllRolsAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var rolesDtoCollection = serviceResponse.Data.Select(r => new GetAllRolsDto
                {
                    Id = r.Id,
                    RolName = r.RolName,
                    IsActive = r.IsActive
                });

                return Ok(BuildCollectionResponse(rolesDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _rolService.GetRolesPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var rolesDtoCollection = items.Select(r => new GetAllRolsDto
                {
                    Id = r.Id,
                    RolName = r.RolName,
                    IsActive = r.IsActive
                });

                return Ok(BuildPagedResponse(rolesDtoCollection, totalCount, page, limit));
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

            var serviceResponse = await _rolService.GetRolByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var rolDto = new GetAllRolsDto
                {
                    Id = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };

                return Ok(rolDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un rol asociado al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRolDto dataRol)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (dataRol == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _rolService.UpdateRolAsync(id, dataRol);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updatedRol = new RolDto
                {
                    RolId = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };

                return Ok(updatedRol);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un rol con el Id proporcionado");
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

            var serviceResponse = await _rolService.GetRolByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var rolDto = new GetAllRolsDto
                {
                    Id = serviceResponse.Data.Id,
                    RolName = serviceResponse.Data.RolName,
                    IsActive = serviceResponse.Data.IsActive
                };

                return Ok(rolDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Rol no encontrado");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _rolService.SetRolStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    serviceResponse.Data.Id,
                    serviceResponse.Data.RolName,
                    serviceResponse.Data.IsActive,
                    serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un rol con el Id proporcionado");
        }
    }
}
