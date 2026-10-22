using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] AddUserDto newuser)
        {
            if (newuser == null)
            {
                return BadRequestError("El payload del usuario no puede ser nulo.");
            }

            var serviceResponse = await _authService.RegisterAsync(newuser);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var userDto = new AddUserDto
                {
                    UserName = serviceResponse.Data.UserName,
                    UserLastName = serviceResponse.Data.UserLastName,
                    Mail = serviceResponse.Data.Mail,
                    UserPhone = serviceResponse.Data.UserPhone
                };

                return CreatedAtAction(
                    nameof(Register),
                    new { id = serviceResponse.Data.UserId },
                    userDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre de usuario o correo ya existe");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (loginRequest == null)
            {
                return BadRequestError("El payload de autenticación no puede ser nulo.");
            }

            var serviceResponse = await _authService.LoginAsync(loginRequest);

            if (serviceResponse.IsSuccess)
            {
                return Ok(serviceResponse.Data);
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound ||
                serviceResponse.MessageCode == MessageCodes.Unauthorized)
            {
                return Unauthorized(new UnsuccessfulResponseDto
                {
                    Code = StatusCodes.Status401Unauthorized.ToString(),
                    Message = "Credenciales inválidas",
                    Details = new { info = serviceResponse.Message ?? "El nombre de usuario o la contraseña son incorrectos" }
                });
            }

            return MapServiceError(serviceResponse);
        }
    }
}
