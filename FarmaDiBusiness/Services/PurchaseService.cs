using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.PurchaseDto;
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
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            ISupplierService supplierService,
            IProductService productService)
        {
            _purchaseRepository = purchaseRepository;
            _supplierService = supplierService;
            _productService = productService;
        }

        public async Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto)
        {
            try
            {
                var existingSupplier = await _supplierService.GetByIdAsync(dto.SupplierId);
                if (existingSupplier?.Data == null)
                {
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró un proveedor con el id proporcionado"
                    };
                }

                foreach (var detailDto in dto.Details)
                {
                    var existingProduct = await _productService.GetByIdAsync(detailDto.ProductId);
                    if (existingProduct?.Data == null)
                    {
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = $"No se encontró el producto con el id {detailDto.ProductId} especificado en el detalle"
                        };
                    }
                }

                var purchase = new Purchase
                {
                    SupplierId = dto.SupplierId,
                    UserId = dto.UserId,
                    Observation = dto.Observation,
                    RegisteredDate = DateTime.Now
                };

                var purchaseDetail = dto.Details.Select(dt => new PurchaseDetails
                {
                    ProductId = dt.ProductId,
                    Quantity = dt.Quantity,
                    UnitPrice = dt.UnitPrice,
                    BatchNumber = dt.BatchNumber,
                    ExpirationDate = dt.ExpirationDate,
                    ManufacturingDate = dt.ManufacturingDate
                }).ToList();

                var repoResponse = await _purchaseRepository.InserAsync(purchase, purchaseDetail);

                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    var dataResponse = new PurchaseResponseDto
                    {
                        Id = repoResponse.Data.Master.PurchaseId,
                        SupplierId = repoResponse.Data.Master.SupplierId,
                        SupplierName = repoResponse.Data.Master.SupplierName,
                        UserId = repoResponse.Data.Master.UserId,
                        UserName = repoResponse.Data.Master.UserName,
                        total = repoResponse.Data.Master.Total,
                        Observation = repoResponse.Data.Master.Observation,
                        PurchaseDate = repoResponse.Data.Master.RegisteredDate,
                        PurchaseNum = repoResponse.Data.Master.PurchaseNum,
                        Details = repoResponse.Data.Details.Select(dt => new PurchaseDetailsResponseDto
                        {
                            Id = dt.Id,
                            PurchaseId = dt.PurchaseId,
                            ProductId = dt.ProductId,
                            ProductGenericName = dt.ProductGenericName,
                            ProductTradeName = dt.ProductTradeName,
                            Quantity = dt.Quantity,
                            UnitPrice = dt.UnitPrice,
                            BatchNumber = dt.BatchNumber,
                            ExpirationDate = dt.ExpirationDate,
                            ManufacturingDate = dt.ManufacturingDate,
                            BatchId = dt.BatchId,
                            TotalPrice = dt.TotalPrice,
                            RegisteredDate = dt.RegisteredDate
                        }).ToList()
                    };

                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Compra registrada exitosamente"
                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 50009:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontraron los recursos relacionales solicitados en la base de datos"
                        };

                    case 50003:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.Conflict,
                            Message = repoResponse.Message ?? "Conflicto de datos o duplicidad al procesar la compra"
                        };

                    default:
                        return new ServiceResponse<PurchaseResponseDto>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Error en el registro de la compra"
                        };
                }
            }
            catch (Exception)
            {
                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al procesar el servicio transaccional de compras."
                };
            }
        }

        public async Task<ServiceResponse<PurchaseResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetByIdAsync(id);

                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data.Master != null)
                {
                    var master = repoResponse.Data.Master;
                    var details = repoResponse.Data.Details;

                    var dto = new PurchaseResponseDto
                    {
                        Id = master.PurchaseId,
                        SupplierId = master.SupplierId,
                        SupplierName = master.SupplierName,
                        UserId = master.UserId,
                        UserName = master.UserName,
                        total = master.Total,
                        Observation = master.Observation,
                        PurchaseDate = master.RegisteredDate,
                        PurchaseNum = master.PurchaseNum,
                        Details = details.Select(dt => new PurchaseDetailsResponseDto
                        {
                            Id = dt.Id,
                            PurchaseId = dt.PurchaseId,
                            ProductId = dt.ProductId,
                            ProductGenericName = dt.ProductGenericName,
                            ProductTradeName = dt.ProductTradeName,
                            Quantity = dt.Quantity,
                            UnitPrice = dt.UnitPrice,
                            BatchNumber = dt.BatchNumber,
                            ExpirationDate = dt.ExpirationDate,
                            ManufacturingDate = dt.ManufacturingDate,
                            BatchId = dt.BatchId,
                            TotalPrice = dt.TotalPrice,
                            RegisteredDate = dt.RegisteredDate
                        }).ToList()
                    };

                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = dto,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                if (repoResponse.OperationStatusCode == 50009 || repoResponse.Data.Master == null)
                {
                    return new ServiceResponse<PurchaseResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = repoResponse.Message ?? $"No se encontró la compra con el id {id}"
                    };
                }

                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Error al obtener la compra"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<PurchaseResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al consultar la compra con sus detalles."
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<GetAllPurchasesDto> Items, int TotalCount)>> GetPurchasesPagedAsync(int page, int limit)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetPurchasesPagedAsync(page, limit);

                if (repoResponse.OperationStatusCode == 0)
                {
                    var items = repoResponse.Data.Items.Select(p => new GetAllPurchasesDto
                    {
                        PurchaseId = p.PurchaseId,
                        PurchaseNum = p.PurchaseNum,
                        SupplierId = p.SupplierId,
                        SupplierName = p.SupplierName,
                        UserId = p.UserId,
                        UserName = p.UserName,
                        Total = p.Total,
                        Observations = p.Observation,
                        RegisteredDate = p.RegisteredDate
                    });

                    return new ServiceResponse<(IEnumerable<GetAllPurchasesDto> Items, int TotalCount)>
                    {
                        Data = (items, repoResponse.Data.TotalCount),
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<GetAllPurchasesDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<GetAllPurchasesDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Error al paginar compras"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<GetAllPurchasesDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<GetAllPurchasesDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener compras paginadas."
                };
            }
        }

        public async Task<ServiceResponse<(IEnumerable<PurchaseResponseDto> Items, int TotalCount)>> GetPurchasesWithDetailsPagedAsync(int page, int limit)
        {
            try
            {
                var repoResponse = await _purchaseRepository.GetPurchasesWithDetailsPagedAsync(page, limit);

                if (repoResponse.OperationStatusCode == 0)
                {
                    var detailsLookup = repoResponse.Data.Details.ToLookup(d => d.PurchaseId);

                    var purchasesWithDetails = repoResponse.Data.Purchases.Select(p => new PurchaseResponseDto
                    {
                        Id = p.PurchaseId,
                        PurchaseNum = p.PurchaseNum,
                        SupplierId = p.SupplierId,
                        SupplierName = p.SupplierName,
                        UserId = p.UserId,
                        UserName = p.UserName,
                        total = p.Total,
                        Observation = p.Observation,
                        PurchaseDate = p.RegisteredDate,
                        Details = detailsLookup[p.PurchaseId].Select(dt => new PurchaseDetailsResponseDto
                        {
                            Id = dt.Id,
                            PurchaseId = dt.PurchaseId,
                            ProductId = dt.ProductId,
                            ProductGenericName = dt.ProductGenericName,
                            ProductTradeName = dt.ProductTradeName,
                            Quantity = dt.Quantity,
                            UnitPrice = dt.UnitPrice,
                            BatchNumber = dt.BatchNumber,
                            ExpirationDate = dt.ExpirationDate,
                            ManufacturingDate = dt.ManufacturingDate,
                            BatchId = dt.BatchId,
                            TotalPrice = dt.TotalPrice,
                            RegisteredDate = dt.RegisteredDate
                        }).ToList()
                    }).ToList();

                    return new ServiceResponse<(IEnumerable<PurchaseResponseDto> Items, int TotalCount)>
                    {
                        Data = (purchasesWithDetails, repoResponse.Data.TotalCount),
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa"
                    };
                }

                return new ServiceResponse<(IEnumerable<PurchaseResponseDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<PurchaseResponseDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Error al obtener compras con detalles"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<(IEnumerable<PurchaseResponseDto> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<PurchaseResponseDto>(), 0),
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrió un error inesperado al obtener compras con detalles."
                };
            }
        }
    }
}
