using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IPresentationRepository
    {
        Task<RepositoryResponse<Presentations>> AddAsync(Presentations presentation);
        Task<RepositoryResponse<IEnumerable<Presentations>>> GetAllAsync();
        Task<RepositoryResponse<Presentations>> GetByIdAsync(int id);
        Task<RepositoryResponse<Presentations>> UpdateAsync(int id, Presentations presentation);
        Task<RepositoryResponse<Presentations>> GetByNameAsync(string name);
        Task<RepositoryResponse<Presentations>> SetStateAsync(int id, bool state);
        Task<RepositoryResponse<(IEnumerable<Presentations> Items, int TotalCount)>> GetPresentationsPagedAsync(int pageNumber, int pageSize);
    }
}
