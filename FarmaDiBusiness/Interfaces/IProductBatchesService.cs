using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IProductBatchesService
    {
        Task<ServiceResponse<IEnumerable<ProductBatches>>> GetAllAsync();
        Task<ServiceResponse<ProductBatches>> GetByIdAsync(int id);
        Task<ServiceResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>> GetBatchesPagedAsync(int page, int limit);
    }
}
