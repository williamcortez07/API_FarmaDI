using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IUsersRepository
    {
        Task<RepositoryResponse<RolesUers>> RegisterUserWithRolesAsync(Users user, IEnumerable<Roles> roleIds);
        Task<RepositoryResponse<IEnumerable<Users>>> GetAllAsync();
        Task<RepositoryResponse<Users>> GetByIdAsync(int id);
        Task<RepositoryResponse<Users>> UpdateAsync(int id, Users users);
        Task<RepositoryResponse<Users>> GetByUserNameAsync(string name);
        Task<RepositoryResponse<Users>> GetByEmailAsync(string email);
        Task<RepositoryResponse<Users>> SetStateAsync(int id, bool state);
        Task<RepositoryResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId);
        Task<RepositoryResponse<IEnumerable<Roles>>> UpdateUserRoleAsync(int userId, int newRoleId);
        Task<RepositoryResponse<(IEnumerable<Users> Items, int TotalCount)>> GetUsersPagedAsync(int pageNumber, int pageSize);
    }
}
