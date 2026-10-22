using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal Total { get; set; }
        public string? Observation { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string? PurchaseNum { get; set; }
    }
}
