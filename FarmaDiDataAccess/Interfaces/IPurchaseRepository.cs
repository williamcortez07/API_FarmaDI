using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<RepositoryResponse<PurchaseTransaction>> InserAsync(Purchase master, IEnumerable<PurchaseDetails> details);
        Task<RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>> GetByIdAsync(int purchaseId);
        Task<RepositoryResponse<(IEnumerable<Purchase> Items, int TotalCount)>> GetPurchasesPagedAsync(int pageNumber, int pageSize);
        Task<RepositoryResponse<(IEnumerable<Purchase> Purchases, IEnumerable<PurchaseDetails> Details, int TotalCount)>> GetPurchasesWithDetailsPagedAsync(int pageNumber, int pageSize);
    }
}
