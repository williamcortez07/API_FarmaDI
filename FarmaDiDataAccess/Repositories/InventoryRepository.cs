using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<Inventory>>> GetAllAsync()
        {
            var inventoryList = new List<Inventory>();
            var response = new RepositoryResponse<IEnumerable<Inventory>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductPricing", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            inventoryList.Add(new Inventory
                            {
                                InventoryId = (int)reader["Id"],
                                oproduct = new Products
                                {
                                    ProductId = (int)reader["ProductId"],
                                    GenericName = reader["ProductGenericName"].ToString()!
                                },
                                SalePrice = (decimal)reader["SalePrice"],
                                PurchasePrice = (decimal)reader["PurchasePrice"],
                                CriticalStock = (int)reader["CriticalStock"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = inventoryList;
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Inventory>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Inventory>> GetByIdAsync(int id)
        {
            var inventoryResult = new Inventory();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductPricingById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            inventoryResult.InventoryId = (int)reader["Id"];
                            inventoryResult.oproduct = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!
                            };
                            inventoryResult.SalePrice = (decimal)reader["SalePrice"];
                            inventoryResult.PurchasePrice = (decimal)reader["PurchasePrice"];
                            inventoryResult.CriticalStock = (int)reader["CriticalStock"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Inventory>
                    {
                        Data = inventoryResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Inventory>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<InventoryDashboard>> GetDashboardAsync(int page, int limit, int? categoryId, string? estado, int? brandId, int? supplierId, DateTime? fechaCorte)
        {
            var response = new RepositoryResponse<InventoryDashboard>
            {
                Data = new InventoryDashboard()
            };

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventory", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PageNumber", page);
                    cmd.Parameters.AddWithValue("@PageSize", limit);

                    SqlParameter totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(totalRecordsParam);

                    cmd.Parameters.AddWithValue("@CategoryId", categoryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", string.IsNullOrEmpty(estado) ? DBNull.Value : estado);
                    cmd.Parameters.AddWithValue("@BrandId", brandId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId", supplierId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaCorte", fechaCorte ?? (object)DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // RESULT SET 1: Resumen (Tarjetas)
                        if (await reader.ReadAsync())
                        {
                            response.Data.Summary.TotalProductos = reader["TotalProductos"] != DBNull.Value ? Convert.ToInt32(reader["TotalProductos"]) : 0;
                            response.Data.Summary.StockBajo = reader["StockBajo"] != DBNull.Value ? Convert.ToInt32(reader["StockBajo"]) : 0;
                            response.Data.Summary.Agotados = reader["Agotados"] != DBNull.Value ? Convert.ToInt32(reader["Agotados"]) : 0;
                            response.Data.Summary.ValorInventario = reader["ValorInventario"] != DBNull.Value ? Convert.ToDecimal(reader["ValorInventario"]) : 0;
                        }

                        // RESULT SET 2: Lista Principal
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Data.Items.Add(new InventoryItem
                                {
                                    ProductId = (int)reader["ProductId"],
                                    Producto = reader["Producto"] is DBNull ? string.Empty : reader["Producto"].ToString()!,
                                    NombreGenerico = reader["NombreGenerico"] is DBNull ? string.Empty : reader["NombreGenerico"].ToString()!,
                                    CategoryId = (int)reader["CategoryId"],
                                    Categoria = reader["Categoria"] is DBNull ? string.Empty : reader["Categoria"].ToString()!,
                                    PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                    ConcentrationId = reader["ConcentrationId"] != DBNull.Value ? (int)reader["ConcentrationId"] : null,
                                    ConcentrationValue = reader["ConcentrationValue"] != DBNull.Value ? reader["ConcentrationValue"].ToString()! : string.Empty,
                                    SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : null,
                                    BrandId = reader["BrandId"] != DBNull.Value ? (int)reader["BrandId"] : null,
                                    Isactive = (bool)reader["Isactive"],
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                                    PrecioCosto = reader["PrecioCosto"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioCosto"]) : 0,
                                    StockCritico = reader["StockCritico"] != DBNull.Value ? Convert.ToInt32(reader["StockCritico"]) : 0,
                                    Existencia = Convert.ToInt32(reader["Existencia"]),
                                    CantidadVencida = Convert.ToInt32(reader["CantidadVencida"]),
                                    ValorProducto = Convert.ToDecimal(reader["ValorProducto"]),
                                    Estado = reader["Estado"] is DBNull ? string.Empty : reader["Estado"].ToString()!
                                });
                            }
                        }

                        // RESULT SET 3: Detalle de Lotes
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Data.Batches.Add(new InventoryBatchInfo
                                {
                                    BatchId = (int)reader["BatchId"],
                                    NumeroLote = reader["NumeroLote"] is DBNull ? string.Empty : reader["NumeroLote"].ToString()!,
                                    FechaFabricacion = Convert.ToDateTime(reader["FechaFabricacion"]),
                                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                                    CantidadOriginal = Convert.ToInt32(reader["CantidadOriginal"]),
                                    CantidadDisponible = Convert.ToInt32(reader["CantidadDisponible"]),
                                    ProductId = (int)reader["ProductId"],
                                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                                    Activo = (bool)reader["Activo"],
                                    StockId = (int)reader["StockId"],
                                    FechaEntradaStock = Convert.ToDateTime(reader["FechaEntradaStock"]),
                                    EstadoLote = reader["EstadoLote"] is DBNull ? string.Empty : reader["EstadoLote"].ToString()!
                                });
                            }
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        response.Data.TotalRecords = Convert.ToInt32(totalRecordsParam.Value);
                    }
                    else
                    {
                        response.Data.TotalRecords = response.Data.Summary.TotalProductos;
                    }

                    response.OperationStatusCode = 0;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>> GetInventoryWithDetailsByProductIdAsync(int productId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("USP_GetInventoryWithDetailsByProductId", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ProductId", productId);

                        InventoryItem? item = null;
                        var batches = new List<InventoryBatchInfo>();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                item = new InventoryItem
                                {
                                    ProductId = (int)reader["ProductId"],
                                    Producto = reader["Producto"] is DBNull ? string.Empty : reader["Producto"].ToString()!,
                                    NombreGenerico = reader["NombreGenerico"] is DBNull ? string.Empty : reader["NombreGenerico"].ToString()!,
                                    CategoryId = (int)reader["CategoryId"],
                                    Categoria = reader["Categoria"] is DBNull ? string.Empty : reader["Categoria"].ToString()!,
                                    PresentationId = reader["PresentationId"] != DBNull.Value ? (int)reader["PresentationId"] : null,
                                    ConcentrationId = reader["ConcentrationId"] != DBNull.Value ? (int)reader["ConcentrationId"] : null,
                                    ConcentrationValue = reader["ConcentrationValue"] != DBNull.Value ? reader["ConcentrationValue"].ToString()! : string.Empty,
                                    SupplierId = reader["SupplierId"] != DBNull.Value ? (int)reader["SupplierId"] : null,
                                    BrandId = reader["BrandId"] != DBNull.Value ? (int)reader["BrandId"] : null,
                                    Isactive = (bool)reader["Isactive"],
                                    Precio = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                                    PrecioCosto = reader["PrecioCosto"] != DBNull.Value ? Convert.ToDecimal(reader["PrecioCosto"]) : 0,
                                    StockCritico = reader["StockCritico"] != DBNull.Value ? Convert.ToInt32(reader["StockCritico"]) : 0,
                                    Existencia = Convert.ToInt32(reader["Existencia"]),
                                    CantidadVencida = Convert.ToInt32(reader["CantidadVencida"]),
                                    ValorProducto = Convert.ToDecimal(reader["ValorProducto"]),
                                    Estado = reader["Estado"] is DBNull ? string.Empty : reader["Estado"].ToString()!
                                };
                            }

                            if (item == null)
                            {
                                return new RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>
                                {
                                    Data = (null, Enumerable.Empty<InventoryBatchInfo>()),
                                    OperationStatusCode = 50009,
                                    Message = "No se encontró el inventario para el producto indicado"
                                };
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    batches.Add(new InventoryBatchInfo
                                    {
                                        BatchId = (int)reader["BatchId"],
                                        NumeroLote = reader["NumeroLote"] is DBNull ? string.Empty : reader["NumeroLote"].ToString()!,
                                        FechaFabricacion = Convert.ToDateTime(reader["FechaFabricacion"]),
                                        FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                                        CantidadOriginal = Convert.ToInt32(reader["CantidadOriginal"]),
                                        CantidadDisponible = Convert.ToInt32(reader["CantidadDisponible"]),
                                        ProductId = (int)reader["ProductId"],
                                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                                        Activo = (bool)reader["Activo"],
                                        StockId = (int)reader["StockId"],
                                        FechaEntradaStock = Convert.ToDateTime(reader["FechaEntradaStock"]),
                                        EstadoLote = reader["EstadoLote"] is DBNull ? string.Empty : reader["EstadoLote"].ToString()!
                                    });
                                }
                            }
                        }

                        return new RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>
                        {
                            Data = (item, batches),
                            OperationStatusCode = 0,
                            Message = "Operación exitosa"
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>
                {
                    Data = (null, Enumerable.Empty<InventoryBatchInfo>()),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(InventoryItem? Item, IEnumerable<InventoryBatchInfo> Batches)>
                {
                    Data = (null, Enumerable.Empty<InventoryBatchInfo>()),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
