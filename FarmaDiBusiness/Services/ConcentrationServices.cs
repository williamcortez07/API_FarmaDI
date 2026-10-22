using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Services
{
    public class ConcentrationServices : IConcentrationServices
    {
        private readonly IConcentrationRepository _concentrationRepository;

        public ConcentrationServices(IConcentrationRepository concentrationRepository)
        {
            _concentrationRepository = concentrationRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Concentrations>>> GetAllAsync()
        {
            try
            {
                var result = await _concentrationRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Concentrations>>
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
                        return new ServiceResponse<IEnumerable<Concentrations>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Concentrations>>
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
                return new ServiceResponse<IEnumerable<Concentrations>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar los datos"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Concentrations> Items, int TotalCount)>> GetConcentrationsPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _concentrationRepository.GetConcentrationsPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Concentrations>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar concentraciones"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Concentrations>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar concentraciones."
                };
            }
        }
    }
}
