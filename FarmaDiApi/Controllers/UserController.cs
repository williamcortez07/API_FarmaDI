using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUsersService _userService;

        public UserController(IUsersService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra un nuevo usuario con roles asignados (Solo Administradores).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
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

        /// <summary>
        /// Obtiene un usuario por su ID. Un administrador puede consultar cualquier usuario;
        /// un usuario de nivel inferior solo puede consultar sus propios datos.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var currentUserId = GetCurrentUserId();
            if (!IsAdminUser() && currentUserId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new UnsuccessfulResponseDto
                {
                    Code = StatusCodes.Status403Forbidden.ToString(),
                    Message = "No tiene autorización para visualizar los datos de otro usuario."
                });
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

        /// <summary>
        /// Actualiza los datos personales de un usuario EXCLUYENDO el rol.
        /// Un usuario regular solo puede actualizar su propio perfil; un administrador puede actualizar
        /// los datos de usuarios del sistema. En ningún caso este endpoint permite modificar el rol.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (updateUserDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var isAdmin = IsAdminUser();
            var currentUserId = GetCurrentUserId();

            if (!isAdmin && currentUserId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new UnsuccessfulResponseDto
                {
                    Code = StatusCodes.Status403Forbidden.ToString(),
                    Message = "No tiene autorización para actualizar los datos de otro usuario."
                });
            }

            var serviceResponse = await _userService.UpdateUserAsync(id, updateUserDto, isAdmin, currentUserId);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(serviceResponse.Data);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Usuario no encontrado",
                customConflictMessage: serviceResponse.Message);
        }

        /// <summary>
        /// Actualiza o reasigna el rol de un usuario.
        /// REGLA DE SEGURIDAD Y JERARQUÍA: Solo los usuarios con rol Admin pueden modificar
        /// el rol de usuarios de nivel inferior (ej. Vendedor, Empleado). No se permite degradar o
        /// modificar el rol de otro Administrador ni auto-modificarse.
        /// </summary>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto roleDto)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (roleDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var currentAdminId = GetCurrentUserId();
            var serviceResponse = await _userService.UpdateUserRoleAsync(id, roleDto.NewRoleId, currentAdminId);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    UserId = id,
                    AssignedRoleId = roleDto.NewRoleId,
                    Roles = serviceResponse.Data,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: serviceResponse.Message,
                customConflictMessage: serviceResponse.Message);
        }

        /// <summary>
        /// Obtiene todos los usuarios (Solo Administradores).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin")]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _userService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(BuildCollectionResponse(serviceResponse.Data, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        /// <summary>
        /// Obtiene listado paginado de usuarios (Solo Administradores).
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Administrador,Admin")]
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

        /// <summary>
        /// Asigna un rol adicional a un usuario (Solo Administradores).
        /// </summary>
        [HttpPost("assign-role")]
        [Authorize(Roles = "Administrador,Admin")]
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

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            return int.TryParse(idClaim, out var userId) ? userId : 0;
        }

        private bool IsAdminUser()
        {
            return User.IsInRole("Admin") || User.IsInRole("Administrador");
        }
    }
}
