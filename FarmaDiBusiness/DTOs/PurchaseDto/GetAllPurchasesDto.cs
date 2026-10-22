using System;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class GetAllPurchasesDto
    {
        public int PurchaseId { get; set; }
        public string? PurchaseNum { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal Total { get; set; }
        public string? Observations { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
