using System;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class PurchaseDetailsResponseDto
    {
        public int Id { get; set; }         
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string? ProductGenericName { get; set; }
        public string? ProductTradeName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; } 
        public int BatchId { get; set; } 
        public decimal TotalPrice { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
