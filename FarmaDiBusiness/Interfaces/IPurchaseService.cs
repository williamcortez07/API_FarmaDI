using FarmaDiBusiness.DTOs.PurchaseDto;
using FarmaDiCore.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiBusiness.Interfaces
{
    public interface IPurchaseService
    {
        Task<ServiceResponse<PurchaseResponseDto>> InsertAsync(CreatePurchaseDto dto);
        Task<ServiceResponse<PurchaseResponseDto>> GetByIdAsync(int id);
        Task<ServiceResponse<(IEnumerable<GetAllPurchasesDto> Items, int TotalCount)>> GetPurchasesPagedAsync(int page, int limit);
        Task<ServiceResponse<(IEnumerable<PurchaseResponseDto> Items, int TotalCount)>> GetPurchasesWithDetailsPagedAsync(int page, int limit);
    }
}
