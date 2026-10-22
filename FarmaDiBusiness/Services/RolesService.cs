using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.RolsDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FarmaDiBusiness.Services
{
    public class RolesService : IRolService
    {
        private readonly IRolesRepository _rolRepository;

        public RolesService(IRolesRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<ServiceResponse<Roles>> AddRolAsync(AddRolDto newRol)
        {
            try
            {
                // Validar de forma segura si existe un rol con el mismo nombre
                var existing = await _rolRepository.GetByNameAsync(newRol.RolName);

                if (existing?.Data != null && existing.Data.Id != 0 && !existing.Data.RolName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un rol con el nombre proporcionado"
                    };
                }

                var rol = new Roles
                {
                    RolName = newRol.RolName
                };

                var result = await _rolRepository.AddAsync(rol);

                return new ServiceResponse<Roles>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol registrado correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar el rol",
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Roles>>> GetAllRolsAsync()
        {
            try
            {
                // CORREGIDO: Todo el flujo envuelto en try-catch protector
                var result = await _rolRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Roles>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<IEnumerable<Roles>>
                        {
                            Data = result.Data,
                            IsSuccess = false, // CORREGIDO: Sincronización de estado fallido/vacío
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Roles>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Mapeo semántico en default
                            Message = "Ocurrió un error inesperado"
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
                    Message = "Ocurrió un error inesperado al recuperar los roles"
                };
            }
        }

        public async Task<ServiceResponse<Roles>> GetRolByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Llamada asíncrona movida de forma segura al interior del try
                var result = await _rolRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = result.Message ?? "Operación exitosa"
                    };
                }

                switch (result.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<Roles>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El rol asociado a este Id no existe"
                        };
                    default:
                        return new ServiceResponse<Roles>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = "Ocurrió un error inesperado al obtener el rol"
                        };
                }
            }
            catch (Exception)
            {
                // CORREGIDO: Se eliminó el 'throw' descontrolado para prevenir caídas de la API
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar la consulta del rol"
                };
            }
        }

        public async Task<ServiceResponse<Roles>> UpdateRolAsync(int id, UpdateRolDto rol)
        {
            try
            {
                var existingId = await _rolRepository.GetByIdAsync(id);
                if (existingId?.Data == null || (existingId.Data.Id == 0 && existingId.Data.RolName.IsNullOrEmpty()))
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un rol relacionado al Id proporcionado"
                    };
                }

                var existingName = await _rolRepository.GetByNameAsync(rol.RolName);
                if (existingName?.Data != null && existingName.Data.RolName != null && existingName.Data.Id != id)
                {
                    return new ServiceResponse<Roles>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un rol con el nombre proporcionado"
                    };
                }

                var dataRoles = new Roles
                {
                    RolName = rol.RolName,
                    IsActive = rol.IsActive,
                };

                var result = await _rolRepository.UpdateAsync(id, dataRoles);

                return new ServiceResponse<Roles>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol actualizado correctamente" // CORREGIDO: Comentario de marca saneado
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar el rol"
                };
            }
        }

        public async Task<ServiceResponse<Roles>> GetRolByNameAsync(string name)
        {
            try
            {
                // CORREGIDO: Todo el flujo protegido bajo try-catch
                var result = await _rolRepository.GetByNameAsync(name);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Roles>
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
                        message = "No se encontró un rol que corresponda al nombre proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener el rol.";
                        break;
                }

                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar el rol por su nombre"
                };
            }
        }

        public async Task<ServiceResponse<Roles>> SetRolStateAsync(int id, bool state)
        {
            try
            {
                // CORREGIDO: Todo el flujo protegido bajo try-catch relacional
                var response = new ServiceResponse<Roles>();

                // CORREGIDO: Validación semántica real de existencia sobre la data del repositorio
                var existing = await _rolRepository.GetByIdAsync(id);
                if (existing?.Data == null || existing.Data.Id == 0)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.NotFound; // CORREGIDO: Cambiado a NotFound para alineación de controlador
                    response.Message = "El rol no existe";
                    return response;
                }

                var repoResponse = await _rolRepository.SetStateAsync(id, state);

                if (repoResponse?.Data == null)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.ErrorValidation; // CORREGIDO: Comentario de categoría removido
                    response.Message = "No se pudo actualizar el estado del rol";
                    return response;
                }

                response.Data = repoResponse.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "Rol activado" : "Rol desactivado";

                return response;
            }
            catch (Exception)
            {
                return new ServiceResponse<Roles>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al modificar el estado del rol"
                };
            }
        }
    
        public async Task<ServiceResponse<(IEnumerable<Roles> Items, int TotalCount)>> GetRolesPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _rolRepository.GetRolesPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Roles> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<Roles> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Roles>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar roles"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Roles> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Roles>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar roles."
                };
            }
        }
    }
}
