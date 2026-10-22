using FarmaDiBusiness.DTOs.Roles;
using FarmaDiBusiness.DTOs.UsersDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IUsersService
    {
        Task<ServiceResponse<RolesUers>> RegisterUserWithRolesAsync(RegisterUserRolesDto userDto);
        Task<ServiceResponse<IEnumerable<GetUsers>>> GetAllAsync();
        Task<ServiceResponse<GetUsers>> GetByIdAsync(int id);
        Task<ServiceResponse<Users>> GetUSerByNameAsync(string name);
        Task<ServiceResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId);
        Task<ServiceResponse<(IEnumerable<GetUsers> Items, int TotalCount)>> GetUsersPagedAsync(int page, int limit);
    }
}
