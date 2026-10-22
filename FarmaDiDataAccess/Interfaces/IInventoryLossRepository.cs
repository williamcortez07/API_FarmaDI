using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IInventoryLossRepository
    {
        Task<RepositoryResponse<InventoryLoss>> AddAsync(InventoryLoss inventoryLoss);
        Task<RepositoryResponse<IEnumerable<InventoryLoss>>> GetAllAsync();
        Task<RepositoryResponse<InventoryLoss>> GetByIdAsync(int id);
        Task<RepositoryResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>> GetInventoryLossPagedAsync(int pageNumber, int pageSize);
    }
}
