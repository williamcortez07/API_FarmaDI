using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly string _connectionString;

        public RolesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Roles>> AddAsync(Roles roles)
        {
            var rolResult = new Roles(); // CORREGIDO: Renombrado semántico para evitar confusión visual
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AddRol", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@RolName", roles.RolName);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rolResult.Id = (int)reader["RolId"];
                            rolResult.RolName = reader["RolName"].ToString()!;
                            rolResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Roles>
                    {
                        Data = rolResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Protección global contra fallos de parseo/casteo en runtime
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<Roles>>> GetAllAsync()
        {
            var rolList = new List<Roles>(); // CORREGIDO: Convención camelCase limpia
            var response = new RepositoryResponse<IEnumerable<Roles>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("UspGetAllRoles", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rolList.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["IsActive"]
                            });
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = rolList;
                    response.OperationStatusCode = returnedValue;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message; // CORREGIDO: Añadido mapeo de mensaje nativo ausente
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
            return response;
        }

        public async Task<RepositoryResponse<Roles>> GetByIdAsync(int id)
        {
            var rolResult = new Roles();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("Usp_GetRoleById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rolResult.Id = (int)reader["RolId"];
                            rolResult.RolName = reader["RolName"].ToString()!;
                            rolResult.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Roles>
                    {
                        Data = rolResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Agregado bloque catch genérico de respaldo
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Roles>> UpdateAsync(int id, Roles roles)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.AddWithValue("@RolName", roles.RolName);
                    cmd.Parameters.AddWithValue("@IsActive", roles.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    Roles rolUpdateResult = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rolUpdateResult = new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Roles>
                    {
                        Data = rolUpdateResult,
                        OperationStatusCode = returnedValue // CORREGIDO: Antes forzaba '0' ignorando la respuesta del SP
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Agregado bloque catch genérico de respaldo
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Roles>> GetByNameAsync(string name)
        {
            var rolResult = new Roles();
            var repositoryResponse = new RepositoryResponse<Roles>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetRolByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            rolResult.Id = (int)reader["RolId"];
                            rolResult.RolName = reader["RolName"].ToString()!;
                            rolResult.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    repositoryResponse.Data = rolResult;
                    repositoryResponse.OperationStatusCode = returnedValue;
                    return repositoryResponse;
                }
            }
            catch (SqlException ex)
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = ex.Number;
                repositoryResponse.Message = ex.Message;
                return repositoryResponse;
            }
            catch (Exception ex) // CORREGIDO: Agregado bloque catch genérico de respaldo
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = -1;
                repositoryResponse.Message = ex.Message;
                return repositoryResponse;
            }
        }

        public async Task<RepositoryResponse<Roles>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_DeactivateRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RolId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Roles updatedRol = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedRol = new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["IsActive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Roles>
                    {
                        Data = updatedRol,
                        OperationStatusCode = updatedRol != null ? 0 : 1
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex) // CORREGIDO: Agregado bloque catch genérico de respaldo
            {
                return new RepositoryResponse<Roles>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    
        public async Task<RepositoryResponse<(IEnumerable<Roles> Items, int TotalCount)>> GetRolesPagedAsync(int pageNumber, int pageSize)
        {
            var roleList = new List<Roles>();
            int totalCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetRolesPaged", connection);
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
                            roleList.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!,
                                IsActive = (bool)reader["IsActive"]
                            });
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<Roles> Items, int TotalCount)>
                    {
                        Data = (roleList, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operación exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Roles> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Roles>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Roles> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Roles>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
