using FarmaDiBusiness.DTOs;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IPresentationService
    {
        Task<ServiceResponse<Presentations>> AddAsync(AddPresentationDto newpresentation);
        Task<ServiceResponse<IEnumerable<Presentations>>> GetAllAsync();
        Task<ServiceResponse<Presentations>> GetByIdAsync(int id);
        Task<ServiceResponse<Presentations>> UpdateAsync(int id, UpdatePresentationDto presentation);
        Task<ServiceResponse<Presentations>> GetByNameAsync(string name);
        Task<ServiceResponse<Presentations>> SetStateAsync(int id, bool state);
        Task<ServiceResponse<(IEnumerable<Presentations> Items, int TotalCount)>> GetPresentationsPagedAsync(int page, int limit);
    }
}
