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
    public class ProductBatchesService : IProductBatchesService
    {
        private readonly IProductBatchesRepository _batchRepository;

        public ProductBatchesService(IProductBatchesRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<ServiceResponse<IEnumerable<ProductBatches>>> GetAllAsync()
        {
            try
            {
                var result = await _batchRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<ProductBatches>>
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
                        return new ServiceResponse<IEnumerable<ProductBatches>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<ProductBatches>>
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
                return new ServiceResponse<IEnumerable<ProductBatches>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar los lotes de productos"
                };
            }
        }

        public async Task<ServiceResponse<ProductBatches>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _batchRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<ProductBatches>
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
                        return new ServiceResponse<ProductBatches>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "El lote no existe"
                        };

                    default:
                        return new ServiceResponse<ProductBatches>
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
                return new ServiceResponse<ProductBatches>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar el lote de producto"
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>> GetBatchesPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _batchRepository.GetBatchesPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<ProductBatches>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar lotes"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<ProductBatches>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar los lotes de productos."
                };
            }
        }
    }
}
