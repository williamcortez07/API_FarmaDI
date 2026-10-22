using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IConcentrationServices
    {
        Task<ServiceResponse<IEnumerable<Concentrations>>> GetAllAsync();
        Task<ServiceResponse<(IEnumerable<Concentrations> Items, int TotalCount)>> GetConcentrationsPagedAsync(int page, int limit);
    }
}
