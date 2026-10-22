using FarmaDiBusiness.DTOs.Inventory;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IInventoryService
    {
        Task<ServiceResponse<IEnumerable<Inventory>>> GetAllAsync();
        Task<ServiceResponse<Inventory>> GetByIdAsync(int id);
        Task<ServiceResponse<InventoryDashboardDto>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte);
        Task<ServiceResponse<(IEnumerable<InventoryWithDetailsDto> Items, int TotalCount)>> GetInventoryWithDetailsAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte);
        Task<ServiceResponse<InventoryWithDetailsDto>> GetByIdWithDetailsAsync(int productId);
        Task<ServiceResponse<(IEnumerable<GetAllInventoryDto> Items, int TotalCount)>> GetInventoryPagedAsync(int page, int limit);
    }
}
