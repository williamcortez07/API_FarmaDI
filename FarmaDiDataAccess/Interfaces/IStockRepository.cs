using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IStockRepository
    {
        Task<RepositoryResponse<IEnumerable<Stock>>> GetAllAsync();
        Task<RepositoryResponse<Stock>> GetByIdAsync(int id);
        Task<RepositoryResponse<(IEnumerable<Stock> Items, int TotalCount)>> GetStockPagedAsync(int pageNumber, int pageSize);
    }
}
