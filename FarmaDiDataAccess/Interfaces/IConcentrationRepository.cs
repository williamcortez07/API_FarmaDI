using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IConcentrationRepository
    {
        Task<RepositoryResponse<IEnumerable<Concentrations>>> GetAllAsync();
        Task<RepositoryResponse<(IEnumerable<Concentrations> Items, int TotalCount)>> GetConcentrationsPagedAsync(int pageNumber, int pageSize);
    }
}
