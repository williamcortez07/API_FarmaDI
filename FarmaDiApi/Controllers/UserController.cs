using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Admin")]
    public class UserController : BaseApiController
    {
        private readonly IUsersService _userService;

        public UserController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterUserRolesDto dto)
        {
            if (dto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _userService.RegisterUserWithRolesAsync(dto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var dataResponse = new UserRolResponseDto
                {
                    UserName = serviceResponse.Data.Users.UserName,
                    UserLastName = serviceResponse.Data.Users.UserLastName,
                    Mail = serviceResponse.Data.Users.Mail,
                    UserPhone = serviceResponse.Data.Users.UserPhone,
                    Roles = serviceResponse.Data.Roles.Select(dt => new RolesResponseDto
                    {
                        RolId = dt.Id,
                        RolName = dt.RolName
                    }).ToList()
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = serviceResponse.Data.Users.UserId },
                    dataResponse);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Uno o varios de los Roles proporcionados no existen.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _userService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Usuario no encontrado");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _userService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(BuildCollectionResponse(serviceResponse.Data, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _userService.GetUsersPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                return Ok(BuildPagedResponse(items, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleAssignmentDto assignmentDto)
        {
            if (assignmentDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var result = await _userService.AssignRoleToUserAsync(assignmentDto.UserId, assignmentDto.RoleId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return MapServiceError(result);
        }
    }
}
