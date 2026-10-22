using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IProductBatchesRepository
    {
        Task<RepositoryResponse<IEnumerable<ProductBatches>>> GetAllAsync();
        Task<RepositoryResponse<ProductBatches>> GetByIdAsync(int id);
        Task<RepositoryResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>> GetBatchesPagedAsync(int pageNumber, int pageSize);
    }
}
