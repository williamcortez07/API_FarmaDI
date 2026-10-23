using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        private string GenerateTokenJWT(Users user, IEnumerable<string> roles)
        {
            var secretKey = _configuration["JWTSettings:SecretKey"] 
                         ?? _configuration["JwtSettings:SecretKey"]!;
            var issuer = _configuration["JWTSettings:Issuer"] 
                      ?? _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"] 
                        ?? _configuration["JwtSettings:Audience"];

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Mail ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Mail ?? string.Empty)
            };

            foreach (var role in roles)
            {
                if (string.IsNullOrWhiteSpace(role)) continue;

                var trimmedRole = role.Trim();

                // 1. Claim corto estándar JWT RFC 7519 para que el frontend y decodificadores lean 'role'
                claims.Add(new Claim("role", trimmedRole));

                // 2. Claim URI de Microsoft Identity para que ASP.NET Core resuelva ClaimsIdentity y User.IsInRole()
                claims.Add(new Claim(ClaimTypes.Role, trimmedRole));

                // 3. Normalización bidireccional entre Admin y Administrador para compatibilidad con políticas
                if (string.Equals(trimmedRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
                }
                else if (string.Equals(trimmedRole, "Administrador", StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(3);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                signingCredentials: credentials,
                expires: expires
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<Users>> RegisterAsync(AddUserDto newUser)
        {
            try
            {
                var existingUser = await _authRepository.GetByUserNameAsync(newUser.UserName);
                if (existingUser?.Data != null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.FileExisting,
                        Message = "Ya existe un usuario registrado con el mismo nombre."
                    };
                }

                var existingEmail = await _authRepository.GetByEmailAsync(newUser.Mail);
                if (existingEmail?.Data != null)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario registrado con el mismo correo electrónico."
                    };
                }

                var userToRegister = new Users
                {
                    UserName = newUser.UserName,
                    UserLastName = newUser.UserLastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    Mail = newUser.Mail,
                    UserPhone = newUser.UserPhone
                };

                var result = await _authRepository.RegisterAsync(userToRegister);

                return new ServiceResponse<Users>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Usuario registrado correctamente."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar el usuario."
                };
            }
        }

        public async Task<ServiceResponse<Users>> GetByEmailAsync(string mail)
        {
            try
            {
                var result = await _authRepository.GetByEmailAsync(mail);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                var messageCode = MessageCodes.Success;
                var message = string.Empty;

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        messageCode = MessageCodes.NotFound;
                        message = "No se encontró un usuario que corresponda al correo proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener el usuario.";
                        break;
                }

                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar el usuario por correo electrónico."
                };
            }
        }

        public async Task<ServiceResponse<Users>> GetByNameAsync(string name)
        {
            try
            {
                var result = await _authRepository.GetByUserNameAsync(name);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Users>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                var messageCode = MessageCodes.Success;
                var message = string.Empty;

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        messageCode = MessageCodes.NotFound;
                        message = "No se encontró un usuario que corresponda al nombre proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener el usuario.";
                        break;
                }

                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Users>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar el usuario por su nombre."
                };
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequestDto)
        {
            try
            {
                if (loginRequestDto == null ||
                    string.IsNullOrWhiteSpace(loginRequestDto.UserName) ||
                    string.IsNullOrWhiteSpace(loginRequestDto.Password))
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.BadRequest,
                        Message = "El nombre de usuario y la contraseña son requeridos."
                    };
                }

                // 1. Verificación y existencia del usuario en la base de datos
                var existentUser = await _authRepository.GetByUserNameAsync(loginRequestDto.UserName.Trim());

                if (existentUser?.Data == null || existentUser.OperationStatusCode != 0)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "El nombre de usuario o la contraseña son incorrectos."
                    };
                }

                // 2. Verificación de usuario activo
                if (!existentUser.Data.IsActive)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = "El usuario se encuentra inactivo. Comuníquese con el administrador."
                    };
                }

                // 3. Validación criptográfica de la contraseña ingresada contra el hash BCrypt
                var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, existentUser.Data.PasswordHash);
                if (!isPasswordValid)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message = "El nombre de usuario o la contraseña son incorrectos."
                    };
                }

                // 4. Recuperación y consolidación de roles asignados al usuario desde la base de datos
                var rolesResponse = await _authRepository.GetRolesByUserIdAsync(existentUser.Data.UserId);
                var userRoles = new List<string>();

                if (rolesResponse.Data != null)
                {
                    foreach (var r in rolesResponse.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(r) && !userRoles.Contains(r.Trim(), StringComparer.OrdinalIgnoreCase))
                        {
                            userRoles.Add(r.Trim());
                        }
                    }
                }

                // Si en la base de datos no tiene roles registrados (usuario huérfano), asignamos "Admin" por defecto
                if (userRoles.Count == 0)
                {
                    userRoles.Add("Admin");
                }

                // 5. Emisión y serialización del token JWT con claims de rol estandarizados y de Microsoft Identity
                var token = GenerateTokenJWT(existentUser.Data, userRoles);

                var loginResponse = new LoginResponseDto
                {
                    Id = existentUser.Data.UserId,
                    UserName = existentUser.Data.UserName,
                    Email = existentUser.Data.Mail,
                    Token = token,
                    Roles = userRoles
                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Data = loginResponse,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Login correcto."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el inicio de sesión."
                };
            }
        }
    }
}
