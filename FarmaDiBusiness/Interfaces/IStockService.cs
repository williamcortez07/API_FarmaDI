using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IStockService
    {
        Task<ServiceResponse<IEnumerable<Stock>>> GetAllAsync();
        Task<ServiceResponse<Stock>> GetByIdAsync(int id);
        Task<ServiceResponse<(IEnumerable<Stock> Items, int TotalCount)>> GetStockPagedAsync(int page, int limit);
    }
}
