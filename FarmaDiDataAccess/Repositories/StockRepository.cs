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
    public class StockRepository : IStockRepository
    {
        private readonly string _connectionString;

        public StockRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<Stock>>> GetAllAsync()
        {
            var stock = new List<Stock>();
            var response = new RepositoryResponse<IEnumerable<Stock>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllStock", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            stock.Add(new Stock
                            {
                                Id = (int)reader["StockId"],
                                BatchId = new ProductBatches
                                {
                                    Id = (int)reader["BatchId"],
                                    BatchNumer = reader["BatchNumber"].ToString()!
                                },
                                ProductId = (int)reader["ProductId"],
                                AvailableQuantity = (int)reader["AvailableQuantity"],
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = stock;
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
                return new RepositoryResponse<IEnumerable<Stock>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Stock>> GetByIdAsync(int id)
        {
            var response = new Stock();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetStockById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@StockId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.Id = (int)reader["StockId"];
                            response.BatchId = new ProductBatches
                            {
                                Id = (int)reader["BatchId"],
                                BatchNumer = reader["BatchNumber"].ToString()!
                            };
                            response.ProductId = (int)reader["ProductId"];
                            response.AvailableQuantity = (int)reader["AvailableQuantity"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                    return new RepositoryResponse<Stock>
                    {
                        Data = response,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Stock>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Stock>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<(IEnumerable<Stock> Items, int TotalCount)>> GetStockPagedAsync(int pageNumber, int pageSize)
        {
            var stockList = new List<Stock>();
            int totalCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetStockPaged", connection);
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
                            stockList.Add(new Stock
                            {
                                Id = (int)reader["StockId"],
                                BatchId = new ProductBatches
                                {
                                    Id = (int)reader["BatchId"],
                                    BatchNumer = reader["BatchNumber"] is DBNull ? string.Empty : reader["BatchNumber"].ToString()!
                                },
                                ProductId = (int)reader["ProductId"],
                                AvailableQuantity = (int)reader["AvailableQuantity"]
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<Stock> Items, int TotalCount)>
                    {
                        Data = (stockList, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Stock> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Stock>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Stock> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Stock>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
