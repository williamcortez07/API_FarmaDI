using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiBusiness.Interfaces;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FarmaDiBusiness.DTOs.Inventory.InventoryDashboardDto;

namespace FarmaDiBusiness.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Inventory>>> GetAllAsync()
        {
            try
            {
                var result = await _inventoryRepository.GetAllAsync();

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<IEnumerable<Inventory>>
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
                        return new ServiceResponse<IEnumerable<Inventory>>
                        {
                            Data = result.Data,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No se encontraron registros"
                        };

                    default:
                        return new ServiceResponse<IEnumerable<Inventory>>
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
                return new ServiceResponse<IEnumerable<Inventory>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al recuperar el inventario"
                };
            }
        }

        public async Task<ServiceResponse<Inventory>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _inventoryRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Inventory>
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
                        return new ServiceResponse<Inventory>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = "No se encontró ningún registro que coincida con los parámetros de búsqueda"
                        };

                    default:
                        return new ServiceResponse<Inventory>
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
                return new ServiceResponse<Inventory>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar el registro de inventario"
                };
            }
        }

        public async Task<ServiceResponse<InventoryDashboardDto>> GetDashboardAsync(
            int page,
            int limit,
            int? categoryId,
            string? estado,
            int? brandId,
            int? supplierId,
            DateTime? fechaCorte)
        {
            var result = await _inventoryRepository.GetDashboardAsync(page, limit, categoryId, estado, brandId, supplierId, fechaCorte);

            if (result.OperationStatusCode == 0 && result.Data != null)
            {
                var dto = new InventoryDashboardDto
                {
                    Summary = new InventorySummaryDto
                    {
                        TotalProductos = result.Data.Summary.TotalProductos,
                        StockBajo = result.Data.Summary.StockBajo,
                        Agotados = result.Data.Summary.Agotados,
                        ValorInventario = result.Data.Summary.ValorInventario
                    },
                    Items = result.Data.Items.Select(i => new InventoryItemDto
                    {
                        ProductId = i.ProductId,
                        Producto = i.Producto,
                        NombreGenerico = i.NombreGenerico,
                        CategoryId = i.CategoryId,
                        Categoria = i.Categoria,
                        PresentationId = i.PresentationId,
                        ConcentrationId = i.ConcentrationId,
                        ConcentrationValue = i.ConcentrationValue,
                        SupplierId = i.SupplierId,
                        BrandId = i.BrandId,
                        Isactive = i.Isactive,
                        Precio = i.Precio,
                        PrecioCosto = i.PrecioCosto,
                        StockCritico = i.StockCritico,
                        Existencia = i.Existencia,
                        CantidadVencida = i.CantidadVencida,
                        ValorProducto = i.ValorProducto,
                        Estado = i.Estado
                    }).ToList(),
                    Batches = result.Data.Batches.Select(b => new InventoryBatchInfoDto
                    {
                        BatchId = b.BatchId,
                        NumeroLote = b.NumeroLote,
                        FechaFabricacion = b.FechaFabricacion,
                        FechaVencimiento = b.FechaVencimiento,
                        CantidadOriginal = b.CantidadOriginal,
                        CantidadDisponible = b.CantidadDisponible,
                        ProductId = b.ProductId,
                        FechaRegistro = b.FechaRegistro,
                        Activo = b.Activo,
                        StockId = b.StockId,
                        FechaEntradaStock = b.FechaEntradaStock,
                        EstadoLote = b.EstadoLote
                    }).ToList()
                };

                return new ServiceResponse<InventoryDashboardDto>
                {
                    Data = dto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            return new ServiceResponse<InventoryDashboardDto>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Ocurrió un error inesperado al obtener el dashboard"
            };
        }

        public async Task<ServiceResponse<(IEnumerable<InventoryWithDetailsDto> Items, int TotalCount)>> GetInventoryWithDetailsAsync(
            int page,
            int limit,
            int? categoryId,
            string? estado,
            int? brandId,
            int? supplierId,
            DateTime? fechaCorte)
        {
            try
            {
                var result = await _inventoryRepository.GetDashboardAsync(page, limit, categoryId, estado, brandId, supplierId, fechaCorte);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    var batchesLookup = result.Data.Batches.ToLookup(b => b.ProductId);

                    var itemsWithDetails = result.Data.Items.Select(i => new InventoryWithDetailsDto
                    {
                        ProductId = i.ProductId,
                        Producto = i.Producto,
                        NombreGenerico = i.NombreGenerico,
                        CategoryId = i.CategoryId,
                        Categoria = i.Categoria,
                        PresentationId = i.PresentationId,
                        ConcentrationId = i.ConcentrationId,
                        ConcentrationValue = i.ConcentrationValue,
                        SupplierId = i.SupplierId,
                        BrandId = i.BrandId,
                        Isactive = i.Isactive,
                        Precio = i.Precio,
                        PrecioCosto = i.PrecioCosto,
                        StockCritico = i.StockCritico,
                        Existencia = i.Existencia,
                        CantidadVencida = i.CantidadVencida,
                        ValorProducto = i.ValorProducto,
                        Estado = i.Estado,
                        Batches = batchesLookup[i.ProductId].Select(b => new InventoryBatchInfoDto
                        {
                            BatchId = b.BatchId,
                            NumeroLote = b.NumeroLote,
                            FechaFabricacion = b.FechaFabricacion,
                            FechaVencimiento = b.FechaVencimiento,
                            CantidadOriginal = b.CantidadOriginal,
                            CantidadDisponible = b.CantidadDisponible,
                            ProductId = b.ProductId,
                            FechaRegistro = b.FechaRegistro,
                            Activo = b.Activo,
                            StockId = b.StockId,
                            FechaEntradaStock = b.FechaEntradaStock,
                            EstadoLote = b.EstadoLote
                        }).ToList()
                    }).ToList();

                    return new ServiceResponse<(IEnumerable<InventoryWithDetailsDto> Items, int TotalCount)>
                    {
                        Data = (itemsWithDetails, result.Data.TotalRecords),
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<InventoryWithDetailsDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryWithDetailsDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al obtener inventario con detalles"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<InventoryWithDetailsDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryWithDetailsDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el inventario con detalles."
                };
            }
        }

        public async Task<ServiceResponse<InventoryWithDetailsDto>> GetByIdWithDetailsAsync(int productId)
        {
            try
            {
                var repoResponse = await _inventoryRepository.GetInventoryWithDetailsByProductIdAsync(productId);

                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data.Item != null)
                {
                    var item = repoResponse.Data.Item;
                    var dto = new InventoryWithDetailsDto
                    {
                        ProductId = item.ProductId,
                        Producto = item.Producto,
                        NombreGenerico = item.NombreGenerico,
                        CategoryId = item.CategoryId,
                        Categoria = item.Categoria,
                        PresentationId = item.PresentationId,
                        ConcentrationId = item.ConcentrationId,
                        ConcentrationValue = item.ConcentrationValue,
                        SupplierId = item.SupplierId,
                        BrandId = item.BrandId,
                        Isactive = item.Isactive,
                        Precio = item.Precio,
                        PrecioCosto = item.PrecioCosto,
                        StockCritico = item.StockCritico,
                        Existencia = item.Existencia,
                        CantidadVencida = item.CantidadVencida,
                        ValorProducto = item.ValorProducto,
                        Estado = item.Estado,
                        Batches = repoResponse.Data.Batches.Select(b => new InventoryBatchInfoDto
                        {
                            BatchId = b.BatchId,
                            NumeroLote = b.NumeroLote,
                            FechaFabricacion = b.FechaFabricacion,
                            FechaVencimiento = b.FechaVencimiento,
                            CantidadOriginal = b.CantidadOriginal,
                            CantidadDisponible = b.CantidadDisponible,
                            ProductId = b.ProductId,
                            FechaRegistro = b.FechaRegistro,
                            Activo = b.Activo,
                            StockId = b.StockId,
                            FechaEntradaStock = b.FechaEntradaStock,
                            EstadoLote = b.EstadoLote
                        }).ToList()
                    };

                    return new ServiceResponse<InventoryWithDetailsDto>
                    {
                        Data = dto,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                if (repoResponse.OperationStatusCode == 50009 || repoResponse.Data.Item == null)
                {
                    return new ServiceResponse<InventoryWithDetailsDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = repoResponse.Message ?? $"No se encontró registro de inventario para el producto con Id {productId}"
                    };
                }

                return new ServiceResponse<InventoryWithDetailsDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Error al obtener el detalle de inventario del producto"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<InventoryWithDetailsDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar los detalles de inventario del producto."
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<GetAllInventoryDto> Items, int TotalCount)>> GetInventoryPagedAsync(int page, int limit)
        {
            try
            {
                var result = await _inventoryRepository.GetDashboardAsync(page, limit, null, null, null, null, null);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    var items = result.Data.Items.Select(i => new GetAllInventoryDto
                    {
                        InventoryId = i.ProductId,
                        ProductId = i.ProductId,
                        ProductGenericname = i.NombreGenerico,
                        SalePrice = i.Precio,
                        PurchasePrice = i.PrecioCosto,
                        CriticalStock = i.StockCritico
                    });

                    return new ServiceResponse<(IEnumerable<GetAllInventoryDto> Items, int TotalCount)>
                    {
                        Data = (items, result.Data.TotalRecords),
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<GetAllInventoryDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<GetAllInventoryDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error al obtener inventario paginado"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<GetAllInventoryDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<GetAllInventoryDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener inventario paginado."
                };
            }
        }
    }
}
