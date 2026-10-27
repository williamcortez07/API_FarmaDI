using FarmaDiCore.Common;
using FarmaDiCore.Entities.OperationalMetrics;
using FarmaDiDataAccess.Interfaces;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Repositories
{
    /// <summary>
    /// Repositorio encargado de consultar cubos multidimensionales de SSAS
    /// mediante ADOMD.NET para calcular las métricas y KPIs operacionales.
    /// </summary>
    public class OlapMetricsRepository : IOlapMetricsRepository
    {
        private readonly string _connectionString;

        public OlapMetricsRepository(IConfiguration configuration)
        {
            // Lee la cadena configurada para Analysis Services o establece la instancia por defecto
            _connectionString = configuration.GetConnectionString("AnalysisServicesConnection")
                                ?? "Data Source=laptopdewilliam;Catalog=Cubo_Farma_Di;";
        }

        /// <summary>
        /// Obtiene los 3 KPIs de Compras desde Cubo_Purchase:
        /// 1. Participación de compras por proveedor (%)
        /// 2. Variación del gasto en compras por período (%)
        /// 3. Margen/costo por producto y proveedor
        /// </summary>
        public async Task<RepositoryResponse<PurchasesKpiModel>> GetPurchasesKpisAsync(int? year = null, int? month = null)
        {
            var response = new RepositoryResponse<PurchasesKpiModel>();
            var model = new PurchasesKpiModel();

            try
            {
                await Task.Run(() =>
                {
                    using var connection = new AdomdConnection(_connectionString);
                    connection.Open();

                    // Construye el filtro de año para el slicer (WHERE clause).
                    // Solo se aplica en las queries que no usan la jerarquía de tiempo en ROWS,
                    // para evitar el error "hierarchy already appears in the axis".
                    string yearSlicerCompras = year.HasValue
                        ? $"WHERE ([Dim Time Purchase].[Calendario de Compras].[Year Number].&[{year.Value}])"
                        : string.Empty;

                    // --- KPI 1.1: Participación de compras por proveedor ---
                    // Itera sobre proveedor en ROWS; el slicer de año no colisiona con esta jerarquía.
                    // Se usa NULL (en lugar de 0) en SharePercentage para que NONEMPTY() filtre proveedores
                    // sin compras reales; NON EMPTY no filtra filas donde el miembro calculado vale 0.
                    string mdxSupplierShare = $@"
                        WITH 
                        MEMBER [Measures].[TotalGeneralCompras] AS 
                            ([Dim Supplier Purchase].[Supplier Name].[All], [Measures].[Total])
                        MEMBER [Measures].[SharePercentage] AS 
                            IIF(IsEmpty([Measures].[TotalGeneralCompras]) OR [Measures].[TotalGeneralCompras] = 0,
                                NULL,
                                ([Measures].[Total] / [Measures].[TotalGeneralCompras]) * 100)
                        SELECT
                            {{[Measures].[Total], [Measures].[Quantity], [Measures].[SharePercentage]}} ON COLUMNS,
                            NONEMPTY(
                                ORDER(
                                    [Dim Supplier Purchase].[Supplier Name].[Supplier Name].MEMBERS,
                                    [Measures].[Total],
                                    BDESC
                                ),
                                [Measures].[Total]
                            ) ON ROWS
                        FROM [Cubo_Purchase]
                        {yearSlicerCompras}";

                    using (var cmd = new AdomdCommand(mdxSupplierShare, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = nombre del miembro (proveedor), Cols 1..N = medidas
                            string supplierName = reader.GetString(0);
                            decimal totalSpend  = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                            int     quantity    = reader.IsDBNull(2) ? 0  : Convert.ToInt32(reader.GetValue(2));
                            decimal sharePct    = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3));

                            model.SupplierShares.Add(new SupplierPurchaseShare
                            {
                                SupplierName    = supplierName,
                                TotalSpend      = Math.Round(totalSpend, 2),
                                TotalQuantity   = quantity,
                                SharePercentage = Math.Round(sharePct, 2)
                            });
                        }
                    }

                    // --- KPI 1.2: Variación del gasto en compras por período (%) ---
                    // ParallelPeriod sin 3er argumento: SSAS usa implícitamente el miembro actual del
                    // contexto de celda, que es el correcto cuando la jerarquía está en ROWS.
                    // Si se pasa año, se filtran los meses con DESCENDANTS sobre el año seleccionado.
                    string rowsSetCompras = year.HasValue
                        ? $"DESCENDANTS([Dim Time Purchase].[Calendario de Compras].[Year Number].&[{year.Value}], [Dim Time Purchase].[Calendario de Compras].[Month Number])"
                        : "[Dim Time Purchase].[Calendario de Compras].[Month Number].MEMBERS";

                    string mdxSpendVariation = $@"
                        WITH
                        MEMBER [Measures].[GastoAnterior] AS
                            (ParallelPeriod(
                                [Dim Time Purchase].[Calendario de Compras].[Month Number],
                                1
                             ), [Measures].[Total])
                        MEMBER [Measures].[VariacionGastoPct] AS
                            IIF(IsEmpty([Measures].[GastoAnterior]) OR [Measures].[GastoAnterior] = 0,
                                NULL,
                                (([Measures].[Total] - [Measures].[GastoAnterior]) / [Measures].[GastoAnterior]) * 100)
                        SELECT
                            {{[Measures].[Total], [Measures].[GastoAnterior], [Measures].[VariacionGastoPct]}} ON COLUMNS,
                            NON EMPTY {rowsSetCompras} ON ROWS
                        FROM [Cubo_Purchase]";

                    using (var cmd = new AdomdCommand(mdxSpendVariation, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = número de mes (miembro del eje ROWS), Cols 1..N = medidas
                            string  period       = reader.GetValue(0)?.ToString() ?? string.Empty;
                            decimal currentSpend = reader.IsDBNull(1) ? 0m  : Convert.ToDecimal(reader.GetValue(1));
                            decimal? prevSpend   = reader.IsDBNull(2) ? null : Convert.ToDecimal(reader.GetValue(2));
                            decimal? varPct      = reader.IsDBNull(3) ? null : Convert.ToDecimal(reader.GetValue(3));

                            model.SpendVariations.Add(new PurchaseSpendVariation
                            {
                                Period              = period,
                                CurrentSpend        = Math.Round(currentSpend, 2),
                                PreviousSpend       = prevSpend.HasValue ? Math.Round(prevSpend.Value, 2) : null,
                                VariationPercentage = varPct.HasValue    ? Math.Round(varPct.Value, 2)    : null
                            });
                        }
                    }

                    // --- KPI 1.3: Ganancia/Margen y Costo por producto y proveedor ---
                    // NONEMPTY filtra solo las combinaciones producto × proveedor con compras reales.
                    string mdxProductSupplier = @"
                        SELECT
                            {[Measures].[Total], [Measures].[Quantity]} ON COLUMNS,
                            NONEMPTY(
                                [Dim Products Purchase].[Product Trade Name].[Product Trade Name].MEMBERS *
                                [Dim Supplier Purchase].[Supplier Name].[Supplier Name].MEMBERS,
                                [Measures].[Total]
                            ) ON ROWS
                        FROM [Cubo_Purchase]";

                    using (var cmd = new AdomdCommand(mdxProductSupplier, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Crossjoin produce 2 columnas de miembros antes de las medidas:
                            // Col 0 = Product Trade Name, Col 1 = Supplier Name, Cols 2..3 = medidas
                            string  productName  = reader.GetValue(0)?.ToString() ?? string.Empty;
                            string  supplierName = reader.FieldCount > 3 ? reader.GetValue(1)?.ToString() ?? string.Empty : string.Empty;
                            int     valIndex     = reader.FieldCount > 3 ? 2 : 1;

                            decimal totalAmount = reader.IsDBNull(valIndex)     ? 0m : Convert.ToDecimal(reader.GetValue(valIndex));
                            int     quantity    = reader.IsDBNull(valIndex + 1) ? 0  : Convert.ToInt32(reader.GetValue(valIndex + 1));
                            decimal unitCost    = quantity > 0 ? totalAmount / quantity : 0m;

                            model.ProductSupplierMargins.Add(new ProductSupplierMargin
                            {
                                ProductName              = productName,
                                SupplierName             = supplierName,
                                TotalPurchasedAmount     = Math.Round(totalAmount, 2),
                                QuantityPurchased        = quantity,
                                AverageUnitCost          = Math.Round(unitCost, 2),
                                EstimatedSalesPrice      = Math.Round(unitCost * 1.30m, 2),
                                EstimatedMarginPercentage = 30.00m
                            });
                        }
                    }

                    model.TotalOverallSpend   = model.SupplierShares.Sum(s => s.TotalSpend);
                    model.TotalItemsPurchased = model.SupplierShares.Sum(s => s.TotalQuantity);

                    // Recalcula el porcentaje de participación en memoria con base en el gasto total obtenido
                    if (model.TotalOverallSpend > 0)
                    {
                        foreach (var share in model.SupplierShares)
                        {
                            if (share.SharePercentage == 0 && share.TotalSpend > 0)
                            {
                                share.SharePercentage = Math.Round((share.TotalSpend / model.TotalOverallSpend) * 100m, 2);
                            }
                        }
                    }
                });

                response.Data               = model;
                response.OperationStatusCode = 200;
                response.Message            = "KPIs de compras obtenidos exitosamente desde el Cubo OLAP.";
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = 500;
                response.Message = $"Error al consultar el Cubo_Purchase en Analysis Services: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Obtiene los 3 KPIs de Ventas desde Cubo_Sales y Cubo_Inventory:
        /// 1. Variación de las ventas por período (%)
        /// 2. Margen bruto de ventas (%)
        /// 3. Participación de ventas por categoría (%)
        /// </summary>
        public async Task<RepositoryResponse<SalesKpiModel>> GetSalesKpisAsync(int? year = null, int? month = null)
        {
            var response = new RepositoryResponse<SalesKpiModel>();
            var model = new SalesKpiModel();

            try
            {
                await Task.Run(() =>
                {
                    using var connection = new AdomdConnection(_connectionString);
                    connection.Open();

                    // --- KPI 2.1: Variación de las ventas por período (%) ---
                    // ParallelPeriod sin 3er argumento: SSAS infiere el miembro actual por contexto.
                    // Pasar [Dim Time].[Calendario].CurrentMember como 3er arg causa el error
                    // "CURRENTMEMBER expects a hierarchy expression, a member expression was used".
                    // Si se filtra por año se usa DESCENDANTS() sobre el nodo del año para obtener
                    // solo los meses de ese año; FILTER con CurrentMember.MemberValue no funciona
                    // porque CurrentMember evalúa a nivel de jerarquía completa, no al año padre.
                    string rowsSetVentas = year.HasValue
                        ? $"DESCENDANTS([Dim Time].[Calendario].[Year Number].&[{year.Value}], [Dim Time].[Calendario].[Month Number])"
                        : "[Dim Time].[Calendario].[Month Number].MEMBERS";

                    string mdxSalesVariation = $@"
                        WITH
                        MEMBER [Measures].[VentasAnteriores] AS
                            (ParallelPeriod(
                                [Dim Time].[Calendario].[Month Number],
                                1
                             ), [Measures].[Total Revenue])
                        MEMBER [Measures].[VariacionVentasPct] AS
                            IIF(IsEmpty([Measures].[VentasAnteriores]) OR [Measures].[VentasAnteriores] = 0,
                                NULL,
                                (([Measures].[Total Revenue] - [Measures].[VentasAnteriores]) / [Measures].[VentasAnteriores]) * 100)
                        SELECT
                            {{[Measures].[Total Revenue], [Measures].[VentasAnteriores], [Measures].[VariacionVentasPct]}} ON COLUMNS,
                            NON EMPTY {rowsSetVentas} ON ROWS
                        FROM [Cubo_Sales]";

                    using (var cmd = new AdomdCommand(mdxSalesVariation, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = número de mes (label del miembro en ROWS), Cols 1..N = medidas
                            string  period       = reader.GetValue(0)?.ToString() ?? string.Empty;
                            decimal currentSales = reader.IsDBNull(1) ? 0m  : Convert.ToDecimal(reader.GetValue(1));
                            decimal? prevSales   = reader.IsDBNull(2) ? null : Convert.ToDecimal(reader.GetValue(2));
                            decimal? varPct      = reader.IsDBNull(3) ? null : Convert.ToDecimal(reader.GetValue(3));

                            model.SalesVariations.Add(new SalesVariation
                            {
                                Period              = period,
                                CurrentSales        = Math.Round(currentSales, 2),
                                PreviousSales       = prevSales.HasValue ? Math.Round(prevSales.Value, 2) : null,
                                VariationPercentage = varPct.HasValue    ? Math.Round(varPct.Value, 2)    : null
                            });
                        }
                    }

                    // --- KPI 2.2: Margen bruto de ventas (%) desde Cubo_Inventory ---
                    // Se agrega a nivel de producto para dar contexto por SKU.
                    // Potential Sale Value = precio de venta estimado; Inventory Cost Value = costo.
                    string mdxGrossMargin = @"
                        WITH
                        MEMBER [Measures].[MargenBrutoMonto] AS
                            [Measures].[Potential Sale Value] - [Measures].[Inventory Cost Value]
                        MEMBER [Measures].[MargenBrutoPct] AS
                            IIF(IsEmpty([Measures].[Potential Sale Value]) OR [Measures].[Potential Sale Value] = 0,
                                0,
                                ([Measures].[MargenBrutoMonto] / [Measures].[Potential Sale Value]) * 100)
                        SELECT
                            {[Measures].[Potential Sale Value], [Measures].[Inventory Cost Value], [Measures].[MargenBrutoMonto], [Measures].[MargenBrutoPct]} ON COLUMNS,
                            NON EMPTY [Dim Products Inventory].[Product Trade Name].[Product Trade Name].MEMBERS ON ROWS
                        FROM [Cubo_Inventory]";

                    using (var cmd = new AdomdCommand(mdxGrossMargin, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = nombre del producto, Cols 1..4 = medidas
                            string  productName    = reader.GetValue(0)?.ToString() ?? string.Empty;
                            decimal potentialRev   = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                            decimal inventoryCost  = reader.IsDBNull(2) ? 0m : Convert.ToDecimal(reader.GetValue(2));
                            decimal marginValue    = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3));
                            decimal marginPct      = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4));

                            model.GrossMargins.Add(new GrossSalesMargin
                            {
                                Period               = productName,
                                TotalRevenue         = Math.Round(potentialRev, 2),
                                TotalCost            = Math.Round(inventoryCost, 2),
                                GrossMargin          = Math.Round(marginValue, 2),
                                GrossMarginPercentage = Math.Round(marginPct, 2)
                            });
                        }
                    }

                    // --- KPI 2.3: Participación de ventas por categoría (%) ---
                    // El slicer de año se aplica aquí porque la jerarquía de tiempo NO está en ROWS.
                    string yearSlicerVentas = year.HasValue
                        ? $"WHERE ([Dim Time].[Calendario].[Year Number].&[{year.Value}])"
                        : string.Empty;

                    string mdxCategoryShare = $@"
                        WITH
                        MEMBER [Measures].[TotalGeneralVentas] AS 
                            ([Dim Category].[Category Name].[All], [Measures].[Total Revenue])
                        MEMBER [Measures].[ParticipacionCategoriaPct] AS 
                            IIF(IsEmpty([Measures].[TotalGeneralVentas]) OR [Measures].[TotalGeneralVentas] = 0,
                                NULL,
                                ([Measures].[Total Revenue] / [Measures].[TotalGeneralVentas]) * 100)
                        SELECT
                            {{[Measures].[Total Revenue], [Measures].[Quantity], [Measures].[ParticipacionCategoriaPct]}} ON COLUMNS,
                            NONEMPTY(
                                ORDER(
                                    [Dim Category].[Category Name].[Category Name].MEMBERS,
                                    [Measures].[Total Revenue],
                                    BDESC
                                ),
                                [Measures].[Total Revenue]
                            ) ON ROWS
                        FROM [Cubo_Sales]
                        {yearSlicerVentas}";

                    using (var cmd = new AdomdCommand(mdxCategoryShare, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = nombre de categoría, Cols 1..3 = medidas
                            string  categoryName = reader.GetValue(0)?.ToString() ?? string.Empty;
                            decimal revenue      = reader.IsDBNull(1) ? 0m : Convert.ToDecimal(reader.GetValue(1));
                            int     quantity     = reader.IsDBNull(2) ? 0  : Convert.ToInt32(reader.GetValue(2));
                            decimal sharePct     = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3));

                            model.CategoryShares.Add(new CategorySalesShare
                            {
                                CategoryName    = categoryName,
                                TotalRevenue    = Math.Round(revenue, 2),
                                TotalQuantity   = quantity,
                                SharePercentage = Math.Round(sharePct, 2)
                            });
                        }
                    }

                    model.TotalOverallRevenue = model.CategoryShares.Sum(c => c.TotalRevenue);
                    model.TotalItemsSold      = model.CategoryShares.Sum(c => c.TotalQuantity);

                    // Recalcula el porcentaje de participación en memoria con base en las ventas totales
                    if (model.TotalOverallRevenue > 0)
                    {
                        foreach (var cat in model.CategoryShares)
                        {
                            if (cat.SharePercentage == 0 && cat.TotalRevenue > 0)
                            {
                                cat.SharePercentage = Math.Round((cat.TotalRevenue / model.TotalOverallRevenue) * 100m, 2);
                            }
                        }
                    }
                });

                response.Data               = model;
                response.OperationStatusCode = 200;
                response.Message            = "KPIs de ventas obtenidos exitosamente desde el Cubo OLAP.";
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = 500;
                response.Message = $"Error al consultar el Cubo_Sales en Analysis Services: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Obtiene los 3 KPIs de Inventario desde Cubo_Inventory:
        /// 1. Índice de rotación de inventario por producto
        /// 2. Porcentaje de inventario próximo a vencer (%)
        /// 3. Tasa de disponibilidad de productos (%)
        /// </summary>
        public async Task<RepositoryResponse<InventoryKpiModel>> GetInventoryKpisAsync(int? daysToExpire = null)
        {
            var response = new RepositoryResponse<InventoryKpiModel>();
            var model = new InventoryKpiModel();
            int daysThreshold = daysToExpire ?? 90;

            try
            {
                await Task.Run(() =>
                {
                    using var connection = new AdomdConnection(_connectionString);
                    connection.Open();

                    // --- KPI 3.1: Índice de rotación de inventario por producto ---
                    // Rotación = Unidades despachadas / Inventario promedio.
                    // Todas las medidas pertenecen a Cubo_Inventory, sin jerarquías de tiempo en ROWS.
                    string mdxTurnover = @"
                        WITH
                        MEMBER [Measures].[UnidadesDespachadas] AS
                            [Measures].[Batch Initial Quantity] - [Measures].[Available Quantity]
                        MEMBER [Measures].[InventarioPromedio] AS
                            ([Measures].[Batch Initial Quantity] + [Measures].[Available Quantity]) / 2
                        MEMBER [Measures].[IndiceRotacion] AS
                            IIF(IsEmpty([Measures].[InventarioPromedio]) OR [Measures].[InventarioPromedio] = 0,
                                0,
                                [Measures].[UnidadesDespachadas] / [Measures].[InventarioPromedio])
                        SELECT
                            {[Measures].[Available Quantity], [Measures].[Batch Initial Quantity], [Measures].[UnidadesDespachadas], [Measures].[IndiceRotacion]} ON COLUMNS,
                            NON EMPTY ORDER(
                                [Dim Products Inventory].[Product Trade Name].[Product Trade Name].MEMBERS,
                                [Measures].[IndiceRotacion],
                                BDESC
                            ) ON ROWS
                        FROM [Cubo_Inventory]";

                    using (var cmd = new AdomdCommand(mdxTurnover, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Col 0 = nombre del producto, Cols 1..4 = medidas
                            string  productName   = reader.GetValue(0)?.ToString() ?? string.Empty;
                            int     availableQty  = reader.IsDBNull(1) ? 0  : Convert.ToInt32(reader.GetValue(1));
                            int     initialQty    = reader.IsDBNull(2) ? 0  : Convert.ToInt32(reader.GetValue(2));
                            int     dispatchedQty = reader.IsDBNull(3) ? 0  : Convert.ToInt32(reader.GetValue(3));
                            decimal turnoverRatio = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4));

                            model.ProductTurnovers.Add(new ProductInventoryTurnover
                            {
                                ProductName          = productName,
                                AvailableQuantity    = availableQty,
                                BatchInitialQuantity = initialQty,
                                DispatchedQuantity   = dispatchedQty,
                                TurnoverRatio        = Math.Round(turnoverRatio, 2)
                            });
                        }
                    }

                    // --- KPI 3.2: Porcentaje de inventario próximo a vencer (%) ---
                    // El crossjoin Batch × Producto expone la fecha de vencimiento por lote.
                    // Expiration Date en Dim Batch viene como tipo fecha desde SSAS.
                    string mdxExpiring = @"
                        SELECT
                            {[Measures].[Available Quantity], [Measures].[Batch Initial Quantity]} ON COLUMNS,
                            NON EMPTY (
                                [Dim Batch].[Batch Number].[Batch Number].MEMBERS *
                                [Dim Batch].[Expiration Date].[Expiration Date].MEMBERS *
                                [Dim Products Inventory].[Product Trade Name].[Product Trade Name].MEMBERS
                            ) ON ROWS
                        FROM [Cubo_Inventory]";

                    int totalAvailableGlobal  = 0;
                    int totalExpiringAvailable = 0;

                    using (var cmd = new AdomdCommand(mdxExpiring, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        DateTime today = DateTime.Today;

                        while (reader.Read())
                        {
                            // Crossjoin de 3 dimensiones: Col 0 = BatchNumber, Col 1 = ExpirationDate, Col 2 = ProductName
                            // Cols 3..4 = medidas
                            string batchNumber   = reader.GetValue(0)?.ToString() ?? string.Empty;
                            object rawExpDate    = reader.IsDBNull(1) ? (object)string.Empty : reader.GetValue(1);
                            string productName   = reader.GetValue(2)?.ToString() ?? string.Empty;

                            int availableQty = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                            int initialQty   = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4));

                            totalAvailableGlobal += availableQty;

                            // SSAS puede devolver la fecha como DateTime o como string; se maneja ambos casos.
                            DateTime expirationDate;
                            if (rawExpDate is DateTime dt)
                            {
                                expirationDate = dt;
                            }
                            else if (!DateTime.TryParse(rawExpDate?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out expirationDate))
                            {
                                expirationDate = today.AddDays(180);
                            }

                            int daysUntilExpiration = (int)(expirationDate - today).TotalDays;

                            string status;
                            if (daysUntilExpiration <= 0)
                            {
                                status = "Vencido";
                                totalExpiringAvailable += availableQty;
                            }
                            else if (daysUntilExpiration <= daysThreshold)
                            {
                                status = "Próximo a vencer";
                                totalExpiringAvailable += availableQty;
                            }
                            else
                            {
                                status = "En regla";
                            }

                            decimal nearExpPct = initialQty > 0
                                ? Math.Round(((decimal)availableQty / initialQty) * 100, 2)
                                : 0m;

                            if (daysUntilExpiration <= daysThreshold)
                            {
                                model.ExpiringBatches.Add(new ExpiringInventory
                                {
                                    BatchNumber            = batchNumber,
                                    ProductName            = productName,
                                    ExpirationDate         = expirationDate,
                                    DaysUntilExpiration    = daysUntilExpiration,
                                    AvailableQuantity      = availableQty,
                                    BatchInitialQuantity   = initialQty,
                                    NearExpirationPercentage = nearExpPct,
                                    Status                 = status
                                });
                            }
                        }
                    }

                    model.OverallExpiringPercentage = totalAvailableGlobal > 0
                        ? Math.Round(((decimal)totalExpiringAvailable / totalAvailableGlobal) * 100, 2)
                        : 0m;

                    // --- KPI 3.3: Tasa de disponibilidad de productos (%) ---
                    // Producto disponible = Available Quantity > 0. Producto crítico = entre 1 y Critical Stock.
                    string mdxAvailability = @"
                        SELECT
                            {[Measures].[Available Quantity], [Measures].[Critical Stock]} ON COLUMNS,
                            NON EMPTY [Dim Products Inventory].[Product Trade Name].[Product Trade Name].MEMBERS ON ROWS
                        FROM [Cubo_Inventory]";

                    int totalProducts    = 0;
                    int inStockCount     = 0;
                    int outOfStockCount  = 0;
                    int criticalStockCount = 0;

                    using (var cmd = new AdomdCommand(mdxAvailability, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            totalProducts++;
                            // Col 0 = nombre del producto, Col 1 = Available Quantity, Col 2 = Critical Stock
                            int availableQty  = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                            int criticalStock = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));

                            if (availableQty <= 0)
                            {
                                outOfStockCount++;
                            }
                            else if (availableQty <= criticalStock)
                            {
                                criticalStockCount++;
                                inStockCount++;
                            }
                            else
                            {
                                inStockCount++;
                            }
                        }
                    }

                    decimal availabilityRate = totalProducts > 0
                        ? Math.Round(((decimal)inStockCount / totalProducts) * 100, 2)
                        : 0m;

                    model.Availability = new ProductAvailability
                    {
                        TotalProducts          = totalProducts,
                        InStockProducts        = inStockCount,
                        OutOfStockProducts     = outOfStockCount,
                        CriticalStockProducts  = criticalStockCount,
                        AvailabilityRatePercentage = availabilityRate
                    };

                    model.TotalInventoryItems = totalProducts;
                });

                response.Data               = model;
                response.OperationStatusCode = 200;
                response.Message            = "KPIs de inventario obtenidos exitosamente desde el Cubo OLAP.";
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = 500;
                response.Message = $"Error al consultar el Cubo_Inventory en Analysis Services: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Obtiene un resumen integral de todos los KPIs operacionales de Compras, Ventas e Inventario.
        /// </summary>
        public async Task<RepositoryResponse<OperationalMetricsSummaryModel>> GetOperationalMetricsSummaryAsync(int? year = null, int? month = null)
        {
            var summaryResponse = new RepositoryResponse<OperationalMetricsSummaryModel>();
            var summaryModel = new OperationalMetricsSummaryModel();

            var purchasesTask = GetPurchasesKpisAsync(year, month);
            var salesTask = GetSalesKpisAsync(year, month);
            var inventoryTask = GetInventoryKpisAsync();

            await Task.WhenAll(purchasesTask, salesTask, inventoryTask);

            var purchasesRes = await purchasesTask;
            var salesRes = await salesTask;
            var inventoryRes = await inventoryTask;

            if (purchasesRes.Data != null) summaryModel.Purchases = purchasesRes.Data;
            if (salesRes.Data != null) summaryModel.Sales = salesRes.Data;
            if (inventoryRes.Data != null) summaryModel.Inventory = inventoryRes.Data;

            summaryResponse.Data = summaryModel;
            summaryResponse.OperationStatusCode = 200;
            summaryResponse.Message = "Resumen operacional consolidado obtenido correctamente.";

            return summaryResponse;
        }
    }
}
