using FarmaDiBusiness.DTOs.InventoryLossDto;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IInventoryLossService
    {
        Task<ServiceResponse<IEnumerable<InventoryLoss>>> GetAllAsync();
        Task<ServiceResponse<InventoryLoss>> GetByIdAsync(int id);
        Task<ServiceResponse<InventoryLoss>> AddAsync(AddInventoryLossDto newInventoryLoss);
        Task<ServiceResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>> GetInventoryLossPagedAsync(int page, int limit);
    }
}
