using System;
using System.Collections.Generic;
using static FarmaDiBusiness.DTOs.Inventory.InventoryDashboardDto;

namespace FarmaDiBusiness.DTOs.Inventory
{
    public class InventoryWithDetailsDto
    {
        public int ProductId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string NombreGenerico { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int? PresentationId { get; set; }
        public int? ConcentrationId { get; set; }
        public string ConcentrationValue { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public int? BrandId { get; set; }
        public bool Isactive { get; set; }
        public decimal Precio { get; set; }
        public decimal PrecioCosto { get; set; }
        public int StockCritico { get; set; }
        public int Existencia { get; set; }
        public int CantidadVencida { get; set; }
        public decimal ValorProducto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<InventoryBatchInfoDto> Batches { get; set; } = new List<InventoryBatchInfoDto>();
    }
}
