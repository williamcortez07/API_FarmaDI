using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class InventoryLossRepository : IInventoryLossRepository
    {
        private readonly string _connectionString;

        public InventoryLossRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<InventoryLoss>>> GetAllAsync()
        {
            var lossList = new List<InventoryLoss>(); // CORREGIDO: Nomenclatura clara para evitar colisión visual
            var response = new RepositoryResponse<IEnumerable<InventoryLoss>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllInventoryLoss", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lossList.Add(new InventoryLoss
                            {
                                LowId = (int)reader["LowId"],
                                oBatch = new ProductBatches { Id = (int)reader["BatchId"], BatchNumer = reader["BatchNumber"].ToString()! },
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products { ProductId = (int)reader["ProductId"], GenericName = reader["ProductGenericName"].ToString()!, TradeName = reader["ProductTradeName"].ToString()! },
                                oUser = new Users { UserId = (int)reader["UserId"], UserName = reader["UserName"].ToString()! },
                                Reason = reader["Reason"].ToString()!
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = lossList;
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message; // CORREGIDO: Trazabilidad del mensaje nativo agregada
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<InventoryLoss>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<InventoryLoss>> GetByIdAsync(int id)
        {
            var lossResult = new InventoryLoss();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryLossById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LowId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            lossResult.LowId = (int)reader["LowId"];
                            lossResult.oBatch = new ProductBatches { Id = (int)reader["BatchId"], BatchNumer = reader["BatchNumber"].ToString()! };
                            lossResult.Quantity = (int)reader["Quantity"];
                            lossResult.oProduct = new Products { ProductId = (int)reader["ProductId"], GenericName = reader["ProductGenericName"].ToString()!, TradeName = reader["ProductTradeName"].ToString()! };
                            lossResult.oUser = new Users { UserId = (int)reader["UserId"], UserName = reader["UserName"].ToString()! };
                            lossResult.Reason = reader["Reason"].ToString()!;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<InventoryLoss>
                    {
                        Data = lossResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryLoss>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Control global ante fallos de parseo en el lector
            {
                return new RepositoryResponse<InventoryLoss>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<InventoryLoss>> AddAsync(InventoryLoss inventoryLoss)
        {
            var lossResult = new InventoryLoss();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddInventoryLoss", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BatchId", inventoryLoss.BatchId);
                    cmd.Parameters.AddWithValue("@Quantity", inventoryLoss.Quantity);
                    cmd.Parameters.AddWithValue("@ProductId", inventoryLoss.ProductId);
                    cmd.Parameters.AddWithValue("@UserId", inventoryLoss.UserId);
                    cmd.Parameters.AddWithValue("@Reason", inventoryLoss.Reason);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // CORREGIDO: Mapeo correcto de la columna LowId (Antes leías BatchId por error)
                            lossResult.LowId = (int)reader["LowId"];
                            lossResult.oBatch = new ProductBatches { Id = (int)reader["BatchId"] };
                            lossResult.Quantity = (int)reader["Quantity"];
                            lossResult.oProduct = new Products { ProductId = (int)reader["ProductId"] };
                            lossResult.oUser = new Users { UserId = (int)reader["UserId"] };
                            lossResult.Reason = reader["Reason"].ToString()!;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<InventoryLoss>
                    {
                        Data = lossResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<InventoryLoss>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Bloque de respaldo global implementado
            {
                return new RepositoryResponse<InventoryLoss>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    
        public async Task<RepositoryResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>> GetInventoryLossPagedAsync(int pageNumber, int pageSize)
        {
            var lossList = new List<InventoryLoss>();
            int totalCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetInventoryLossPaged", connection);
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
                            lossList.Add(new InventoryLoss
                            {
                                LowId = (int)reader["LowId"],
                                oBatch = new ProductBatches
                                {
                                    Id = (int)reader["BatchId"],
                                    BatchNumer = reader["BatchNumber"] is DBNull ? string.Empty : reader["BatchNumber"].ToString()!
                                },
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products
                                {
                                    ProductId = (int)reader["ProductId"],
                                    GenericName = reader["ProductGenericName"] is DBNull ? string.Empty : reader["ProductGenericName"].ToString()!,
                                    TradeName = reader["ProductTradeName"] is DBNull ? string.Empty : reader["ProductTradeName"].ToString()!
                                },
                                oUser = new Users
                                {
                                    UserId = (int)reader["UserId"],
                                    UserName = reader["UserName"] is DBNull ? string.Empty : reader["UserName"].ToString()!
                                },
                                Reason = reader["Reason"] is DBNull ? string.Empty : reader["Reason"].ToString()!
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                    {
                        Data = (lossList, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryLoss>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<InventoryLoss> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<InventoryLoss>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
