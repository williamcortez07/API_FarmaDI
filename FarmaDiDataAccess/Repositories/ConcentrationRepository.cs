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
    public class ConcentrationRepository : IConcentrationRepository
    {
        private readonly string _connectionString;

        public ConcentrationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<Concentrations>>> GetAllAsync()
        {
            var list = new List<Concentrations>();
            var response = new RepositoryResponse<IEnumerable<Concentrations>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetConcentration", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Concentrations
                            {
                                ConcentrationId = (int)reader["ConcentrationId"],
                                Volume = reader["Volume"].ToString()!,
                                porcentage = reader["Porcentage"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = list;
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
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<(IEnumerable<Concentrations> Items, int TotalCount)>> GetConcentrationsPagedAsync(int pageNumber, int pageSize)
        {
            var list = new List<Concentrations>();
            int totalCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetConcentrationPage", connection);
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
                            list.Add(new Concentrations
                            {
                                ConcentrationId = (int)reader["ConcentrationId"],
                                Volume = reader["Volume"].ToString()!,
                                porcentage = reader["Porcentage"].ToString(),
                                IsActive = (bool)reader["Isactive"]
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                    {
                        Data = (list, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Concentrations>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Concentrations> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Concentrations>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
