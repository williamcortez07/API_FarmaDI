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
    public class StockService : IStockService
    {
        private readonly IStockRepository _repository;

        public StockService(IStockRepository repository)
        {
            _repository = repository;
        }

        public async Task<ServiceResponse<IEnumerable<Stock>>> GetAllAsync()
        {
            try
            {
                var result = await _repository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Stock>>
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
                    case 50010:
                        return new ServiceResponse<IEnumerable<Stock>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Stock>>
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
                return new ServiceResponse<IEnumerable<Stock>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar el inventario de stock"
                };
            }
        }

        public async Task<ServiceResponse<Stock>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Stock>
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
                        return new ServiceResponse<Stock>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No encontrado"
                        };

                    default:
                        return new ServiceResponse<Stock>
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
                return new ServiceResponse<Stock>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar el registro de stock"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<Stock> Items, int TotalCount)>> GetStockPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _repository.GetStockPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<Stock> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<Stock> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Stock>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar stock"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<Stock> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Stock>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar stock."
                };
            }
        }
    }
}
