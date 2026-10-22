using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IRolesRepository
    {
        Task<RepositoryResponse<Roles>> AddAsync(Roles roles);
        Task<RepositoryResponse<IEnumerable<Roles>>> GetAllAsync();
        Task<RepositoryResponse<Roles>> GetByIdAsync(int id);
        Task<RepositoryResponse<Roles>> UpdateAsync(int id, Roles roles);
        Task<RepositoryResponse<Roles>> GetByNameAsync(string name);
        Task<RepositoryResponse<Roles>> SetStateAsync(int id, bool state);
        Task<RepositoryResponse<(IEnumerable<Roles> Items, int TotalCount)>> GetRolesPagedAsync(int pageNumber, int pageSize);
    }
}
