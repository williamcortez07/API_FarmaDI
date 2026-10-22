using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly string _connectionString;
        private const string StoredProcedureName = "USP_InsertPurchase";
        private const string UdttTypeName = "PurchaseDetailsType";

        public PurchaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<PurchaseTransaction>> InserAsync(Purchase master, IEnumerable<PurchaseDetails> details)
        {
            var purchaseTransaction = new PurchaseTransaction();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand(StoredProcedureName, connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    DateTime purchaseDate = master.RegisteredDate == default ? DateTime.Now : master.RegisteredDate;

                    cmd.Parameters.AddWithValue("@SupplierId", master.SupplierId);
                    cmd.Parameters.AddWithValue("@UserId", master.UserId);
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate);
                    cmd.Parameters.AddWithValue("@Observations", (object?)master.Observation ?? DBNull.Value);

                    var detailsTable = new DataTable();
                    detailsTable.Columns.Add("ProductId", typeof(int));
                    detailsTable.Columns.Add("Quantity", typeof(int));
                    detailsTable.Columns.Add("UnitPrice", typeof(decimal));
                    detailsTable.Columns.Add("BatchNumber", typeof(string));
                    detailsTable.Columns.Add("ManufacturingDate", typeof(DateTime));
                    detailsTable.Columns.Add("ExpirationDate", typeof(DateTime));

                    foreach (var item in details)
                    {
                        detailsTable.Rows.Add(
                            item.ProductId,
                            item.Quantity,
                            item.UnitPrice,
                            item.BatchNumber ?? (object)DBNull.Value,
                            item.ManufacturingDate == default || item.ManufacturingDate == null ? DBNull.Value : item.ManufacturingDate,
                            item.ExpirationDate
                        );
                    }

                    SqlParameter detailParm = cmd.Parameters.AddWithValue("@PurchaseDetails", detailsTable);
                    detailParm.SqlDbType = SqlDbType.Structured;
                    detailParm.TypeName = UdttTypeName;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            purchaseTransaction.Master = new Purchase
                            {
                                PurchaseId = (int)reader["PurchaseId"],
                                SupplierId = (int)reader["SupplierId"],
                                UserId = (int)reader["UserId"],
                                Total = (decimal)reader["Total"],
                                Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                RegisteredDate = (DateTime)reader["RegisteredDate"],
                                PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString()
                            };
                        }

                        await reader.NextResultAsync();

                        var detailsList = new List<PurchaseDetails>();
                        while (await reader.ReadAsync())
                        {
                            detailsList.Add(new PurchaseDetails
                            {
                                Id = (int)reader["PurchaseDetailId"],
                                PurchaseId = (int)reader["PurchaseId"],
                                ProductId = (int)reader["ProductId"],
                                BatchId = (int)reader["BatchId"],
                                Quantity = (int)reader["Quantity"],
                                UnitPrice = (decimal)reader["UnitPrice"],
                                TotalPrice = (decimal)reader["TotalPrice"],
                                RegisteredDate = (DateTime)reader["RegisteredDate"],
                                BatchNumber = reader["BatchNumber"] is DBNull ? null : reader["BatchNumber"].ToString(),
                                ManufacturingDate = reader["ManufacturingDate"] is DBNull ? null : (DateTime?)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"]
                            });
                        }

                        purchaseTransaction.Details = detailsList;
                    }

                    return new RepositoryResponse<PurchaseTransaction>
                    {
                        Data = purchaseTransaction,
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = $"Error de Base de Datos ({ex.Number}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<PurchaseTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = $"Error General en Infraestructura de Compras: {ex.Message}"
                };
            }
        }

        public async Task<RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>> GetByIdAsync(int purchaseId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("USP_GetPurchaseById", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);

                        Purchase? master = null;
                        var details = new List<PurchaseDetails>();

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                master = new Purchase
                                {
                                    PurchaseId = (int)reader["PurchaseId"],
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"] is DBNull ? null : reader["SupplierName"].ToString(),
                                    UserId = (int)reader["UserId"],
                                    UserName = reader["UserName"] is DBNull ? null : reader["UserName"].ToString(),
                                    Total = (decimal)reader["Total"],
                                    Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                    RegisteredDate = (DateTime)reader["RegisteredDate"],
                                    PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString()
                                };
                            }

                            if (master == null)
                            {
                                return new RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>
                                {
                                    Data = (null!, Enumerable.Empty<PurchaseDetails>()),
                                    OperationStatusCode = 50009,
                                    Message = "No se encontró la compra con el ID especificado"
                                };
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    details.Add(new PurchaseDetails
                                    {
                                        Id = (int)reader["PurchaseDetailId"],
                                        PurchaseId = (int)reader["PurchaseId"],
                                        ProductId = (int)reader["ProductId"],
                                        ProductGenericName = reader["ProductGenericName"] is DBNull ? null : reader["ProductGenericName"].ToString(),
                                        ProductTradeName = reader["ProductTradeName"] is DBNull ? null : reader["ProductTradeName"].ToString(),
                                        BatchId = (int)reader["BatchId"],
                                        BatchNumber = reader["BatchNumber"] is DBNull ? null : reader["BatchNumber"].ToString(),
                                        ManufacturingDate = reader["ManufacturingDate"] is DBNull ? null : (DateTime?)reader["ManufacturingDate"],
                                        ExpirationDate = (DateTime)reader["ExpirationDate"],
                                        Quantity = (int)reader["Quantity"],
                                        UnitPrice = (decimal)reader["UnitPrice"],
                                        TotalPrice = (decimal)reader["TotalPrice"],
                                        RegisteredDate = (DateTime)reader["RegisteredDate"]
                                    });
                                }
                            }
                        }

                        return new RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>
                        {
                            Data = (master, details),
                            OperationStatusCode = 0,
                            Message = "Operación exitosa"
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>
                {
                    Data = (null!, Enumerable.Empty<PurchaseDetails>()),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(Purchase Master, IEnumerable<PurchaseDetails> Details)>
                {
                    Data = (null!, Enumerable.Empty<PurchaseDetails>()),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<(IEnumerable<Purchase> Items, int TotalCount)>> GetPurchasesPagedAsync(int pageNumber, int pageSize)
        {
            var list = new List<Purchase>();
            int totalCount = 0;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("USP_GetPurchasesPaged", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);

                        var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(totalRecordsParam);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Purchase
                                {
                                    PurchaseId = (int)reader["PurchaseId"],
                                    PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString(),
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"] is DBNull ? null : reader["SupplierName"].ToString(),
                                    UserId = (int)reader["UserId"],
                                    UserName = reader["UserName"] is DBNull ? null : reader["UserName"].ToString(),
                                    Total = (decimal)reader["Total"],
                                    Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                    RegisteredDate = (DateTime)reader["RegisteredDate"]
                                });
                            }
                        }

                        if (totalRecordsParam.Value != DBNull.Value)
                        {
                            totalCount = Convert.ToInt32(totalRecordsParam.Value);
                        }

                        return new RepositoryResponse<(IEnumerable<Purchase> Items, int TotalCount)>
                        {
                            Data = (list, totalCount),
                            OperationStatusCode = 0,
                            Message = "Operación exitosa"
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Purchase> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Purchase>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Purchase> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Purchase>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<(IEnumerable<Purchase> Purchases, IEnumerable<PurchaseDetails> Details, int TotalCount)>> GetPurchasesWithDetailsPagedAsync(int pageNumber, int pageSize)
        {
            var purchases = new List<Purchase>();
            var details = new List<PurchaseDetails>();
            int totalCount = 0;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("USP_GetPurchasesWithDetailsPaged", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);

                        var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(totalRecordsParam);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                purchases.Add(new Purchase
                                {
                                    PurchaseId = (int)reader["PurchaseId"],
                                    PurchaseNum = reader["PurchaseNum"] is DBNull ? null : reader["PurchaseNum"].ToString(),
                                    SupplierId = (int)reader["SupplierId"],
                                    SupplierName = reader["SupplierName"] is DBNull ? null : reader["SupplierName"].ToString(),
                                    UserId = (int)reader["UserId"],
                                    UserName = reader["UserName"] is DBNull ? null : reader["UserName"].ToString(),
                                    Total = (decimal)reader["Total"],
                                    Observation = reader["Observations"] is DBNull ? null : reader["Observations"].ToString(),
                                    RegisteredDate = (DateTime)reader["RegisteredDate"]
                                });
                            }

                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    details.Add(new PurchaseDetails
                                    {
                                        Id = (int)reader["PurchaseDetailId"],
                                        PurchaseId = (int)reader["PurchaseId"],
                                        ProductId = (int)reader["ProductId"],
                                        ProductGenericName = reader["ProductGenericName"] is DBNull ? null : reader["ProductGenericName"].ToString(),
                                        ProductTradeName = reader["ProductTradeName"] is DBNull ? null : reader["ProductTradeName"].ToString(),
                                        BatchId = (int)reader["BatchId"],
                                        BatchNumber = reader["BatchNumber"] is DBNull ? null : reader["BatchNumber"].ToString(),
                                        ManufacturingDate = reader["ManufacturingDate"] is DBNull ? null : (DateTime?)reader["ManufacturingDate"],
                                        ExpirationDate = (DateTime)reader["ExpirationDate"],
                                        Quantity = (int)reader["Quantity"],
                                        UnitPrice = (decimal)reader["UnitPrice"],
                                        TotalPrice = (decimal)reader["TotalPrice"],
                                        RegisteredDate = (DateTime)reader["RegisteredDate"]
                                    });
                                }
                            }
                        }

                        if (totalRecordsParam.Value != DBNull.Value)
                        {
                            totalCount = Convert.ToInt32(totalRecordsParam.Value);
                        }

                        return new RepositoryResponse<(IEnumerable<Purchase> Purchases, IEnumerable<PurchaseDetails> Details, int TotalCount)>
                        {
                            Data = (purchases, details, totalCount),
                            OperationStatusCode = 0,
                            Message = "Operación exitosa"
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Purchase> Purchases, IEnumerable<PurchaseDetails> Details, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Purchase>(), Enumerable.Empty<PurchaseDetails>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Purchase> Purchases, IEnumerable<PurchaseDetails> Details, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Purchase>(), Enumerable.Empty<PurchaseDetails>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
