using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IInventoryRepository
    {
        Task<RepositoryResponse<IEnumerable<Inventory>>> GetAllAsync();
        Task<RepositoryResponse<Inventory>> GetByIdAsync(int id);
        Task<RepositoryResponse<InventoryDashboard>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte);
        Task<RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>> GetInventoryWithDetailsByProductIdAsync(int productId);
    }
}
