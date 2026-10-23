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
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<Users>> RegisterAsync(Users user)
        {
            var userResult = new Users();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_RegisterUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@UserLastName", user.UserLastName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Mail", user.Mail);
                    cmd.Parameters.AddWithValue("@UserPhone", user.UserPhone);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userResult.UserId = (int)reader["UserId"];
                            userResult.UserName = reader["UserName"].ToString()!;
                            userResult.UserLastName = reader["UserLastName"].ToString()!;
                            userResult.PasswordHash = reader["UserPassword"].ToString()!;
                            userResult.Mail = reader["Mail"].ToString()!;
                            userResult.UserPhone = reader["UserPhone"].ToString()!;
                            userResult.IsActive = (bool)reader["Isactive"];
                        }
                    }

                    // Asignación automática del rol Admin (Id 7) al usuario recién registrado
                    if (userResult.UserId > 0)
                    {
                        try
                        {
                            using (SqlCommand assignCmd = new SqlCommand("USP_AsigneRolToUser", connection))
                            {
                                assignCmd.CommandType = CommandType.StoredProcedure;
                                assignCmd.Parameters.AddWithValue("@UserId", userResult.UserId);
                                assignCmd.Parameters.AddWithValue("@RolId", 7);
                                await assignCmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] No se pudo asignar el rol inicial al usuario {userResult.UserId}: {ex.Message}");
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Users>
                    {
                        Data = userResult,
                        OperationStatusCode = returnedValue
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Users>> GetByEmailAsync(string mail)
        {
            var userResult = new Users();
            var repositoryResponse = new RepositoryResponse<Users>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserByEmail", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Mail", mail);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userResult.UserId = (int)reader["UserId"];
                            userResult.UserName = reader["UserName"].ToString()!;
                            userResult.UserLastName = reader["UserLastName"].ToString()!;
                            userResult.PasswordHash = reader["PasswordHash"].ToString()!;
                            userResult.Mail = reader["Mail"].ToString()!;
                            userResult.UserPhone = reader["UserPhone"].ToString()!;
                            userResult.IsActive = (bool)reader["IsActive"];
                        }
                        else
                        {
                            userResult = null;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    repositoryResponse.Data = userResult;
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
            catch (Exception ex)
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = -1;
                repositoryResponse.Message = ex.Message;
                return repositoryResponse;
            }
        }

        public async Task<RepositoryResponse<Users>> GetByUserNameAsync(string name)
        {
            var userResult = new Users();
            var repositoryResponse = new RepositoryResponse<Users>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserByName", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UserName", name);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userResult.UserId = (int)reader["UserId"];
                            userResult.UserName = reader["UserName"].ToString()!;
                            userResult.UserLastName = reader["UserLastName"].ToString()!;
                            userResult.Mail = reader["Mail"].ToString()!;
                            userResult.PasswordHash = reader["UserPassword"].ToString()!;
                            userResult.UserPhone = reader["UserPhone"].ToString()!;
                            userResult.IsActive = (bool)reader["IsActive"];
                        }
                        else
                        {
                            userResult = null;
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    repositoryResponse.Data = userResult;
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
            catch (Exception ex)
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = -1;
                repositoryResponse.Message = ex.Message;
                return repositoryResponse;
            }
        }

        public async Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId)
        {
            var rolesList = new List<string>();
            var repositoryResponse = new RepositoryResponse<IEnumerable<string>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserRolesByUserId", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rolesList.Add(reader["RolName"].ToString()!);
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    repositoryResponse.Data = rolesList;
                    repositoryResponse.OperationStatusCode = returnedValue;
                    repositoryResponse.Message = "Operación exitosa";
                }
            }
            catch (SqlException ex)
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = ex.Number;
                repositoryResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                repositoryResponse.Data = null;
                repositoryResponse.OperationStatusCode = -1;
                repositoryResponse.Message = ex.Message;
            }

            return repositoryResponse;
        }
    }
}
