using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class PresentationService : IPresentationService
    {
        private readonly IPresentationRepository _presentationRepository;

        public PresentationService(IPresentationRepository presentationRepository)
        {
            _presentationRepository = presentationRepository;
        }

        public async Task<ServiceResponse<Presentations>> AddAsync(AddPresentationDto newPresentation)
        {
            try
            {
                var existing = await _presentationRepository.GetByNameAsync(newPresentation.Description);

                if (existing?.Data != null && existing.Data.Id != 0 && !existing.Data.Description.IsNullOrEmpty())
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                var presentation = new Presentations
                {
                    Description = newPresentation.Description,
                    Quantity = newPresentation.Quantity,
                    UnitMeasure = newPresentation.UnitMeasure,
                    IsActive = true
                };

                var result = await _presentationRepository.AddAsync(presentation);

                return new ServiceResponse<Presentations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Presentación registrada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar la presentación"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<Presentations>>> GetAllAsync()
        {
            try
            {
                var result = await _presentationRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Presentations>>
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
                        return new ServiceResponse<IEnumerable<Presentations>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Presentations>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<Presentations>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar las presentaciones"
                };
            }
        }

        public async Task<ServiceResponse<Presentations>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _presentationRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Presentations>
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
                        return new ServiceResponse<Presentations>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La presentación no existe"
                        };
                    default:
                        return new ServiceResponse<Presentations>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = result.Message ?? "Error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar la presentación"
                };
            }
        }

        public async Task<ServiceResponse<Presentations>> UpdateAsync(int id, UpdatePresentationDto presentacion)
        {
            try
            {
                var existingId = await _presentationRepository.GetByIdAsync(id);
                if (existingId?.Data == null || (existingId.Data.Id == 0 && existingId.Data.Description.IsNullOrEmpty()))
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "No existe una presentación asociada al Id proporcionado"
                    };
                }

                var existingName = await _presentationRepository.GetByNameAsync(presentacion.Description);
                if (existingName?.Data != null && existingName.Data.Description != null && existingName.Data.Id != id)
                {
                    return new ServiceResponse<Presentations>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una presentación con el nombre proporcionado"
                    };
                }

                var data = new Presentations
                {
                    Description = presentacion.Description,
                    Quantity = presentacion.Quantity,
                    UnitMeasure = presentacion.UnitMeasure,
                    IsActive = presentacion.IsActive,
                };

                var result = await _presentationRepository.UpdateAsync(id, data);

                return new ServiceResponse<Presentations>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Presentación actualizada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al actualizar la presentación"
                };
            }
        }

        public async Task<ServiceResponse<Presentations>> GetByNameAsync(string name)
        {
            try
            {
                var result = await _presentationRepository.GetByNameAsync(name);
                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Presentations>
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
                    case 5038:
                    case 50009:
                        messageCode = MessageCodes.NotFound;
                        message = "No se encontró una presentación que corresponda al nombre proporcionado";
                        break;

                    default:
                        messageCode = MessageCodes.ErrorDataBase;
                        message = "Error en la base de datos al obtener la presentación.";
                        break;
                }

                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = messageCode,
                    Message = message
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al buscar por descripción"
                };
            }
        }

        public async Task<ServiceResponse<Presentations>> SetStateAsync(int id, bool state)
        {
            try
            {
                var response = new ServiceResponse<Presentations>();

                var existing = await _presentationRepository.GetByIdAsync(id);
                if (existing?.Data == null || existing.Data.Id == 0)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.ErrorValidation;
                    response.Message = "La presentación no existe";
                    return response;
                }

                var repoResponse = await _presentationRepository.SetStateAsync(id, state);

                if (repoResponse?.Data == null)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.MessageCode = MessageCodes.ErrorValidation;
                    response.Message = "No se pudo actualizar el estado de la presentación";
                    return response;
                }

                response.Data = repoResponse.Data;
                response.IsSuccess = true;
                response.MessageCode = MessageCodes.Success;
                response.Message = state ? "Presentación activada" : "Presentación desactivada";

                return response;
            }
            catch (Exception)
            {
                return new ServiceResponse<Presentations>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al modificar el estado de la presentación"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Presentations> Items, int TotalCount)>> GetPresentationsPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _presentationRepository.GetPresentationsPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Presentations> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<Presentations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Presentations>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar presentaciones"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Presentations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Presentations>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar presentaciones paginadas."
                };
            }
        }
    }
}
