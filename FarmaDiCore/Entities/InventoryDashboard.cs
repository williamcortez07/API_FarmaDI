using System;
using System.Collections.Generic;

namespace FarmaDiCore.Entities
{
    public class InventoryDashboard
    {
        public InventorySummary Summary { get; set; } = new InventorySummary();
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public List<InventoryBatchInfo> Batches { get; set; } = new List<InventoryBatchInfo>();
        public int TotalRecords { get; set; }
    }

    public class InventorySummary
    {
        public int TotalProductos { get; set; }
        public int StockBajo { get; set; }
        public int Agotados { get; set; }
        public decimal ValorInventario { get; set; }
    }

    public class InventoryItem
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
    }

    public class InventoryBatchInfo
    {
        public int BatchId { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime FechaFabricacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int CantidadOriginal { get; set; }
        public int CantidadDisponible { get; set; }
        public int ProductId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int StockId { get; set; }
        public DateTime FechaEntradaStock { get; set; }
        public string EstadoLote { get; set; } = string.Empty;
    }
}
