using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiBusiness.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _userRepository;
        private readonly IRolesRepository _rolesRepository;

        public UsersService(IUsersRepository userRepository, IRolesRepository rolesRepository)
        {
            _userRepository = userRepository;
            _rolesRepository = rolesRepository;
        }

        public async Task<ServiceResponse<RolesUers>> RegisterUserWithRolesAsync(RegisterUserRolesDto userDto)
        {
            try
            {
                // 1. Validar disponibilidad del nombre de usuario
                var existingNameUser = await _userRepository.GetByUserNameAsync(userDto.UserName);
                if (existingNameUser?.Data != null)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El nombre de usuario ya está en uso."
                    };
                }

                // 2. Validar disponibilidad del correo electrónico
                var existingEmailUser = await _userRepository.GetByEmailAsync(userDto.Mail);
                if (existingEmailUser?.Data != null)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El correo electrónico ya está en uso."
                    };
                }

                // 3. Validar existencia de cada uno de los roles solicitados
                foreach (var roleDto in userDto.RolesIds)
                {
                    var existingRol = await _rolesRepository.GetByIdAsync(roleDto.IdRoles);
                    if (existingRol?.Data == null)
                    {
                        return new ServiceResponse<RolesUers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"El rol con ID {roleDto.IdRoles} no existe."
                        };
                    }
                }

                // 4. Mapear y encriptar contraseña de forma segura
                var newUser = new Users
                {
                    UserName = userDto.UserName,
                    UserLastName = userDto.UserLastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    Mail = userDto.Mail,
                    UserPhone = userDto.UserPhone
                };

                var rolesList = userDto.RolesIds.Select(r => new Roles
                {
                    Id = r.IdRoles
                }).ToList();

                var result = await _userRepository.RegisterUserWithRolesAsync(newUser, rolesList);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<RolesUers>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Usuario registrado exitosamente con roles."
                    };
                }

                return new ServiceResponse<RolesUers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Error al registrar el usuario con roles."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<RolesUers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el registro del usuario."
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<GetUsers>>> GetAllAsync()
        {
            try
            {
                var repositoryResult = await _userRepository.GetAllAsync();

                if (repositoryResult.OperationStatusCode != 0)
                {
                    return new ServiceResponse<IEnumerable<GetUsers>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Error al consultar la base de datos."
                    };
                }

                var usersDto = repositoryResult.Data!.Select(user => new GetUsers
                {
                    Id = user.UserId,
                    UserName = user.UserName,
                    UserLastName = user.UserLastName,
                    Mail = user.Mail,
                    UserPhone = user.UserPhone,
                    IsActive = user.IsActive,
                    Roles = user.Roles.Select(rol => new RolesResponseDto
                    {
                        RolId = rol.Id,
                        RolName = rol.RolName
                    }).ToList()
                }).ToList();

                return new ServiceResponse<IEnumerable<GetUsers>>
                {
                    Data = usersDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Listado obtenido correctamente."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<GetUsers>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener los usuarios."
                };
            }
        }

        public async Task<ServiceResponse<GetUsers>> GetByIdAsync(int id)
        {
            try
            {
                var repositoryResult = await _userRepository.GetByIdAsync(id);
                if (repositoryResult.OperationStatusCode != 0 || repositoryResult.Data == null)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "Usuario no encontrado."
                    };
                }

                var user = repositoryResult.Data;
                var userDto = new GetUsers
                {
                    Id = user.UserId,
                    UserName = user.UserName,
                    UserLastName = user.UserLastName,
                    Mail = user.Mail,
                    UserPhone = user.UserPhone,
                    IsActive = user.IsActive,
                    Roles = user.Roles.Select(rol => new RolesResponseDto
                    {
                        RolId = rol.Id,
                        RolName = rol.RolName
                    }).ToList()
                };

                return new ServiceResponse<GetUsers>
                {
                    Data = userDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Usuario obtenido correctamente."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<GetUsers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener el usuario."
                };
            }
        }

        public async Task<ServiceResponse<Users>> GetUSerByNameAsync(string name)
        {
            try
            {
                // CORREGIDO: Todo el flujo envuelto en try-catch estratégico para evitar crashes
                var result = await _userRepository.GetByUserNameAsync(name);

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
                    Message = "Ocurrió un error inesperado al consultar el usuario por nombre."
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId)
        {
            try
            {
                // CORREGIDO: Removido el bloque masivo de duplicidad comentado (zombi)

                // 1. Validación de existencia del usuario
                var userResult = await _userRepository.GetByIdAsync(userId);
                if (userResult?.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El usuario con ID {userId} no fue encontrado."
                    };
                }

                // 2. Validación de existencia del rol
                var roleResult = await _rolesRepository.GetByIdAsync(roleId);
                if (roleResult?.Data == null)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El rol con ID {roleId} no fue encontrado."
                    };
                }

                // 3. Persistencia de asignación e interpretación de estados relacionales
                var repositoryResult = await _userRepository.AssignRoleToUserAsync(userId, roleId);

                if (repositoryResult.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = repositoryResult.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Rol asignado correctamente."
                    };
                }
                else if (repositoryResult.OperationStatusCode == 50003)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El usuario ya tiene asignado este rol."
                    };
                }
                else
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = $"Error al asignar el rol. Código: {repositoryResult.OperationStatusCode}"
                    };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la solicitud de asignación."
                };
            }
        }
    
        
        public async Task<ServiceResponse<GetUsers>> UpdateUserAsync(int id, UpdateUserDto userDto, bool isAdmin = true, int currentUserId = 0)
        {
            try
            {
                // 1. Validar existencia del usuario a actualizar
                var existingUserResponse = await _userRepository.GetByIdAsync(id);
                if (existingUserResponse?.Data == null || existingUserResponse.Data.UserId == 0)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "Usuario no encontrado."
                    };
                }

                var currentUser = existingUserResponse.Data;

                // 2. REGLA DE JERARQUÍA Y ESTADO (IsActive):
                // Se mantiene la misma lógica: solo los admin pueden alterar el estado de usuarios de nivel inferior.
                bool targetIsActive = currentUser.IsActive;

                if (isAdmin)
                {
                    var isTargetAdmin = currentUser.Roles != null && currentUser.Roles.Any(r =>
                        string.Equals(r.RolName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(r.RolName, "Administrador", StringComparison.OrdinalIgnoreCase));

                    if (isTargetAdmin && id != currentUserId)
                    {
                        return new ServiceResponse<GetUsers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Restricción de jerarquía: Solo los usuarios con rol Admin pueden modificar a usuarios de nivel inferior (ej. Vendedor, Empleado). No está permitido modificar a otro Administrador."
                        };
                    }

                    if (id == currentUserId && !userDto.IsActive)
                    {
                        return new ServiceResponse<GetUsers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "No está permitido auto-desactivar su propia cuenta administrativa para prevenir la pérdida de acceso al sistema."
                        };
                    }

                    targetIsActive = userDto.IsActive;
                }
                else
                {
                    // Si el usuario no es admin, no puede modificar su propio estado IsActive; se preserva el estado actual
                    targetIsActive = currentUser.IsActive;
                }

                // 3. Validar que el nuevo UserName no esté en conflicto con otro usuario
                if (!string.Equals(currentUser.UserName, userDto.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    var userByName = await _userRepository.GetByUserNameAsync(userDto.UserName);
                    if (userByName?.Data != null && userByName.Data.UserId != id)
                    {
                        return new ServiceResponse<GetUsers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "El nombre de usuario ya está en uso por otra cuenta."
                        };
                    }
                }

                // 4. Validar que el nuevo Mail no esté en conflicto con otro usuario
                if (!string.Equals(currentUser.Mail, userDto.Mail, StringComparison.OrdinalIgnoreCase))
                {
                    var userByMail = await _userRepository.GetByEmailAsync(userDto.Mail);
                    if (userByMail?.Data != null && userByMail.Data.UserId != id)
                    {
                        return new ServiceResponse<GetUsers>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "El correo electrónico ya está en uso por otra cuenta."
                        };
                    }
                }

                // 5. Mapear datos a actualizar (EXCLUYENDO ROL, INCLUYENDO ESTADO ISACTIVE)
                var userToUpdate = new Users
                {
                    UserId = id,
                    UserName = userDto.UserName.Trim(),
                    UserLastName = userDto.UserLastName.Trim(),
                    Mail = userDto.Mail.Trim(),
                    UserPhone = userDto.UserPhone.Trim(),
                    IsActive = targetIsActive,
                    PasswordHash = !string.IsNullOrWhiteSpace(userDto.Password)
                        ? BCrypt.Net.BCrypt.HashPassword(userDto.Password)
                        : null!
                };

                // 6. Persistir cambios en la base de datos
                var updateResponse = await _userRepository.UpdateAsync(id, userToUpdate);
                if (updateResponse.OperationStatusCode == 50004)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "El usuario especificado no fue encontrado."
                    };
                }
                else if (updateResponse.OperationStatusCode == 50005)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El nombre de usuario ya está en uso."
                    };
                }
                else if (updateResponse.OperationStatusCode == 50006)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El correo electrónico ya está en uso."
                    };
                }
                else if (updateResponse.OperationStatusCode != 0 && updateResponse.OperationStatusCode != -1 && updateResponse.Data == null)
                {
                    return new ServiceResponse<GetUsers>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = updateResponse.Message ?? "Error al actualizar los datos del usuario en la base de datos."
                    };
                }

                // 7. Consultar usuario actualizado y conservar sus roles intactos
                var refreshedUser = await _userRepository.GetByIdAsync(id);
                var dataUser = refreshedUser.Data ?? updateResponse.Data ?? currentUser;

                var resultDto = new GetUsers
                {
                    Id = dataUser.UserId,
                    UserName = dataUser.UserName,
                    UserLastName = dataUser.UserLastName,
                    Mail = dataUser.Mail,
                    UserPhone = dataUser.UserPhone,
                    IsActive = dataUser.IsActive,
                    Roles = (dataUser.Roles ?? currentUser.Roles ?? new List<Roles>()).Select(r => new RolesResponseDto
                    {
                        RolId = r.Id,
                        RolName = r.RolName
                    }).ToList()
                };

                return new ServiceResponse<GetUsers>
                {
                    Data = resultDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Datos y estado de usuario actualizados correctamente (roles preservados intactos)."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<GetUsers>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar los datos del usuario."
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<RolesResponseDto>>> UpdateUserRoleAsync(int targetUserId, int newRoleId, int currentAdminUserId)
        {
            try
            {
                // 1. Validar existencia del usuario objetivo
                var targetUserResponse = await _userRepository.GetByIdAsync(targetUserId);
                if (targetUserResponse?.Data == null || targetUserResponse.Data.UserId == 0)
                {
                    return new ServiceResponse<IEnumerable<RolesResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El usuario con ID {targetUserId} no fue encontrado."
                    };
                }

                var targetUser = targetUserResponse.Data;

                // 2. REGLA DE JERARQUÍA Y SEGURIDAD:
                // Solo se puede actualizar el rol de usuarios de nivel inferior (ej. Vendedor, Empleado).
                // No se permite a un admin modificar el rol de otro Administrador ni degradarse a sí mismo.
                var isTargetAdmin = targetUser.Roles != null && targetUser.Roles.Any(r =>
                    string.Equals(r.RolName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.RolName, "Administrador", StringComparison.OrdinalIgnoreCase));

                if (isTargetAdmin)
                {
                    return new ServiceResponse<IEnumerable<RolesResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Restricción de jerarquía: Solo los usuarios con rol Admin pueden modificar el rol de usuarios de nivel inferior (ej. Vendedor, Empleado). No está permitido modificar el rol de otro Administrador."
                    };
                }

                if (targetUserId == currentAdminUserId)
                {
                    return new ServiceResponse<IEnumerable<RolesResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "No está permitido auto-modificar su propio rol administrativo para evitar pérdida de acceso."
                    };
                }

                // 3. Validar existencia del nuevo rol solicitado
                var newRoleResponse = await _rolesRepository.GetByIdAsync(newRoleId);
                if (newRoleResponse?.Data == null)
                {
                    return new ServiceResponse<IEnumerable<RolesResponseDto>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = $"El rol con ID {newRoleId} no existe en el sistema."
                    };
                }

                // 4. Persistir la actualización de rol en BD
                var repositoryResult = await _userRepository.UpdateUserRoleAsync(targetUserId, newRoleId);

                if (repositoryResult.OperationStatusCode == 0 && repositoryResult.Data != null)
                {
                    var responseRoles = repositoryResult.Data.Select(r => new RolesResponseDto
                    {
                        RolId = r.Id,
                        RolName = r.RolName
                    }).ToList();

                    return new ServiceResponse<IEnumerable<RolesResponseDto>>
                    {
                        Data = responseRoles,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = $"Rol actualizado exitosamente al usuario '{targetUser.UserName}'."
                    };
                }

                return new ServiceResponse<IEnumerable<RolesResponseDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repositoryResult.Message ?? "Error al actualizar el rol en la base de datos."
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<RolesResponseDto>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la actualización del rol."
                };
            }
        }
        public async Task<ServiceResponse<(IEnumerable<GetUsers> Items, int TotalCount)>> GetUsersPagedAsync(int page, int limit)
        {
            try
            {
                var repositoryResult = await _userRepository.GetUsersPagedAsync(page, limit);

                if (repositoryResult.OperationStatusCode != 0 || repositoryResult.Data.Items == null)
                {
                    return new ServiceResponse<(IEnumerable<GetUsers> Items, int TotalCount)>
                    {
                        Data = (Enumerable.Empty<GetUsers>(), 0),
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = repositoryResult.Message ?? "Error al obtener usuarios paginados."
                    };
                }

                var usersDto = repositoryResult.Data.Items.Select(user => new GetUsers
                {
                    Id = user.UserId,
                    UserName = user.UserName,
                    UserLastName = user.UserLastName,
                    Mail = user.Mail,
                    UserPhone = user.UserPhone,
                    IsActive = user.IsActive,
                    Roles = user.Roles.Select(rol => new RolesResponseDto
                    {
                        RolId = rol.Id,
                        RolName = rol.RolName
                    }).ToList()
                }).ToList();

                return new ServiceResponse<(IEnumerable<GetUsers> Items, int TotalCount)>
                {
                    Data = (usersDto, repositoryResult.Data.TotalCount),
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<GetUsers> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<GetUsers>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar usuarios."
                };
            }
        }
    }
}
