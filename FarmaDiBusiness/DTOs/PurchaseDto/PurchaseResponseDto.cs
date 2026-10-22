using System;
using System.Collections.Generic;

namespace FarmaDiBusiness.DTOs.PurchaseDto
{
    public class PurchaseResponseDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal total { get; set; }
        public string? Observation { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? PurchaseNum { get; set; }
        public List<PurchaseDetailsResponseDto> Details { get; set; } = new List<PurchaseDetailsResponseDto>();
    }
}
