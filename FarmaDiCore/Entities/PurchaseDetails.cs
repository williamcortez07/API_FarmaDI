using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaDiCore.Entities
{
    public class PurchaseDetails
    {
        // Campos que se devuelven después de la inserción / consulta
        public int Id { get; set; }         // Corresponde a PurchaseDetailId
        public int PurchaseId { get; set; }

        // Campos de Entrada y Salida
        public int ProductId { get; set; }
        public string? ProductGenericName { get; set; }
        public string? ProductTradeName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Campos requeridos para la creación del lote (INPUT al SP)
        public string? BatchNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }

        // Campos que se devuelven después de la inserción
        public int BatchId { get; set; } // Es el ID del lote recién creado (OUTPUT del SP)
        public decimal TotalPrice { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}
