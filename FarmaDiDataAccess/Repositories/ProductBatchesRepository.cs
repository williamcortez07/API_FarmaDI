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
    public class ProductBatchesRepository : IProductBatchesRepository
    {
        private readonly string _connectionString;

        public ProductBatchesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<ProductBatches>>> GetAllAsync()
        {
            var batchList = new List<ProductBatches>();
            var response = new RepositoryResponse<IEnumerable<ProductBatches>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllProductBatches", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            batchList.Add(new ProductBatches
                            {
                                Id = (int)reader["BatchId"],
                                BatchNumer = reader["BatchNumber"].ToString()!,
                                ManufacturingDate = (DateTime)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"],
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products
                                {
                                    ProductId = (int)reader["ProductId"],
                                    GenericName = reader["ProductGenericName"].ToString()!,
                                    TradeName = reader["ProductTradeName"].ToString()!
                                },
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = batchList;
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
                return new RepositoryResponse<IEnumerable<ProductBatches>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<ProductBatches>> GetByIdAsync(int id)
        {
            var batchResult = new ProductBatches();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductBatchesById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BatchId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            batchResult.Id = (int)reader["BatchId"];
                            batchResult.BatchNumer = reader["BatchNumber"].ToString()!;
                            batchResult.ManufacturingDate = (DateTime)reader["ManufacturingDate"];
                            batchResult.ExpirationDate = (DateTime)reader["ExpirationDate"];
                            batchResult.Quantity = (int)reader["Quantity"];
                            batchResult.oProduct = new Products
                            {
                                ProductId = (int)reader["ProductId"],
                                GenericName = reader["ProductGenericName"].ToString()!,
                                TradeName = reader["ProductTradeName"].ToString()!
                            };
                            batchResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<ProductBatches>
                    {
                        Data = batchResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<ProductBatches>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<ProductBatches>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>> GetBatchesPagedAsync(int pageNumber, int pageSize)
        {
            var batchList = new List<ProductBatches>();
            int totalCount = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetProductBatchesPaged", connection);
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
                            batchList.Add(new ProductBatches
                            {
                                Id = (int)reader["BatchId"],
                                BatchNumer = reader["BatchNumber"].ToString()!,
                                ManufacturingDate = (DateTime)reader["ManufacturingDate"],
                                ExpirationDate = (DateTime)reader["ExpirationDate"],
                                Quantity = (int)reader["Quantity"],
                                oProduct = new Products
                                {
                                    ProductId = (int)reader["ProductId"],
                                    GenericName = string.Empty,
                                    TradeName = reader["ProductTradeName"].ToString()!
                                },
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                    {
                        Data = (batchList, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<ProductBatches>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<ProductBatches> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<ProductBatches>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
