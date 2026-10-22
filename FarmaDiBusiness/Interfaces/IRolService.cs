using FarmaDiBusiness.DTOs.Roles;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IRolService
    {
        Task<ServiceResponse<Roles>> AddRolAsync(AddRolDto rol);
        Task<ServiceResponse<IEnumerable<Roles>>> GetAllRolsAsync();
        Task<ServiceResponse<Roles>> GetRolByIdAsync(int id);
        Task<ServiceResponse<Roles>> UpdateRolAsync(int id, UpdateRolDto rol);
        Task<ServiceResponse<Roles>> GetRolByNameAsync(string name);
        Task<ServiceResponse<Roles>> SetRolStateAsync(int id, bool state);
        Task<ServiceResponse<(IEnumerable<Roles> Items, int TotalCount)>> GetRolesPagedAsync(int page, int limit);
    }
}
