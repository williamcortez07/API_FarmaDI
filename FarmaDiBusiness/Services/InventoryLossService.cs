using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;

namespace FarmaDiBusiness.Services
{
    public class InventoryLossService : IInventoryLossService
    {
        private readonly IInventoryLossRepository _inventoryLossRepository;
        private readonly IProductBatchesRepository _productBatchesRepository;

        public InventoryLossService(IInventoryLossRepository inventoryLossRepository, IProductBatchesRepository productBatchesRepository)
        {
            _inventoryLossRepository = inventoryLossRepository;
            _productBatchesRepository = productBatchesRepository;
        }

        public async Task<ServiceResponse<IEnumerable<InventoryLoss>>> GetAllAsync()
        {
            try
            {
                // CORREGIDO: Envoltorio try-catch para proteger contra caídas de conexión
                var result = await _inventoryLossRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<InventoryLoss>>
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
                        return new ServiceResponse<IEnumerable<InventoryLoss>>
                        {
                            Data = result.Data,
                            IsSuccess = false, // CORREGIDO: Consistencia semántica de estado fallido
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<InventoryLoss>>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase, // CORREGIDO: Antes NoData de forma errónea
                            Message = "Ocurrió un error inesperado"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<IEnumerable<InventoryLoss>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar las bajas de inventario"
                };
            }
        }

        public async Task<ServiceResponse<InventoryLoss>> GetByIdAsync(int id)
        {
            try
            {
                // CORREGIDO: Movido de forma segura al interior del bloque try
                var result = await _inventoryLossRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<InventoryLoss>
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
                        return new ServiceResponse<InventoryLoss>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "La baja no existe"
                        };

                    case 50007:
                        return new ServiceResponse<InventoryLoss>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = "Cantidad inferior a la requerida"
                        };

                    default:
                        return new ServiceResponse<InventoryLoss>
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
                return new ServiceResponse<InventoryLoss>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    // CORREGIDO: Mensaje estático seguro para prevenir NullReferenceException en runtime
                    Message = "Ocurrió un error inesperado al consultar el registro de la baja"
                };
            }
        }

        public async Task<ServiceResponse<InventoryLoss>> AddAsync(AddInventoryLossDto newInventoryLoss)
        {
            try
            {
                // CORREGIDO: Eliminación de código comentado (zombi) de Marcas

                // CORREGIDO: Corrección de bug de lógica invertida y validación real de existencia de Lote
                var existBatch = await _productBatchesRepository.GetByIdAsync(newInventoryLoss.BatchId);
                if (existBatch?.Data == null || existBatch.OperationStatusCode != 0)
                {
                    return new ServiceResponse<InventoryLoss>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró un lote con el id brindado"
                    };
                }

                var inventoryLoss = new InventoryLoss
                {
                    BatchId = newInventoryLoss.BatchId,
                    Quantity = newInventoryLoss.Quantity,
                    ProductId = newInventoryLoss.ProductId,
                    UserId = newInventoryLoss.UserId,
                    Reason = newInventoryLoss.Reason,
                };

                var result = await _inventoryLossRepository.AddAsync(inventoryLoss);

                return new ServiceResponse<InventoryLoss>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Baja registrada correctamente"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryLoss>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al registrar la baja de inventario",
                };
            }
        }
    
        public async Task<ServiceResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>> GetInventoryLossPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _inventoryLossRepository.GetInventoryLossPagedAsync(page, limit);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryLoss>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al paginar bajas de inventario"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryLoss>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al paginar bajas de inventario."
                };
            }
        }
    }
}
