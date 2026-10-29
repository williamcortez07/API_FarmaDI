using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FarmaDiDataAccess.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly string _connectionString;

        public UsersRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<RolesUers>> RegisterUserWithRolesAsync(Users user, IEnumerable<Roles> roleIds)
        {
            var userWithRolesResult = new RolesUers(); // CORREGIDO: Variable sem�ntica
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_RegisterUserWithRoles", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", user.UserName);
                    cmd.Parameters.AddWithValue("@UserLastName", user.UserLastName);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Mail", user.Mail);
                    cmd.Parameters.AddWithValue("@UserPhone", user.UserPhone);

                    var roleIdsTable = new DataTable();
                    roleIdsTable.Columns.Add("RollId", typeof(int));

                    foreach (var item in roleIds)
                    {
                        roleIdsTable.Rows.Add(item.Id);
                    }

                    SqlParameter rolParam = cmd.Parameters.AddWithValue("@Roles", roleIdsTable);
                    rolParam.SqlDbType = SqlDbType.Structured;
                    rolParam.TypeName = "TipoListaRoles";

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userWithRolesResult.Users = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }

                        await reader.NextResultAsync();

                        var rolesList = new List<Roles>();
                        while (await reader.ReadAsync())
                        {
                            rolesList.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!
                            });
                        }

                        userWithRolesResult.Roles = rolesList;
                    }

                    return new RepositoryResponse<RolesUers>
                    {
                        Data = userWithRolesResult,
                        OperationStatusCode = 0,
                        Message = "Operaci�n exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<RolesUers>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<RolesUers>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<Users>>> GetAllAsync()
        {
            var userList = new List<Users>(); // CORREGIDO: Nomenclatura camelCase
            var response = new RepositoryResponse<IEnumerable<Users>>();
            var userDictionary = new Dictionary<int, Users>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetAllUsers", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"],
                                Roles = new List<Roles>()
                            };
                            userList.Add(user);

                            if (!userDictionary.ContainsKey(user.UserId))
                            {
                                userDictionary.Add(user.UserId, user);
                            }
                        }

                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            var userIdOwner = (int)reader["UserId"];

                            if (userDictionary.TryGetValue(userIdOwner, out var userOwner))
                            {
                                userOwner.Roles.Add(new Roles
                                {
                                    Id = (int)reader["RolId"],
                                    RolName = reader["RolName"].ToString()!
                                });
                            }
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = userList;
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
                return new RepositoryResponse<IEnumerable<Users>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }

            return response;
        }

        public async Task<RepositoryResponse<Users>> GetByIdAsync(int id)
        {
            var userResult = new Users();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserById", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userResult.UserId = (int)reader["UserId"];
                            userResult.UserName = reader["UserName"].ToString()!;
                            userResult.UserLastName = reader["UserLastName"].ToString()!;
                            userResult.Mail = reader["Mail"].ToString()!;
                            userResult.UserPhone = reader["UserPhone"].ToString()!;
                            userResult.IsActive = (bool)reader["Isactive"];
                        }

                        if (await reader.NextResultAsync())
                        {
                            var rolesList = new List<Roles>();
                            while (await reader.ReadAsync())
                            {
                                rolesList.Add(new Roles
                                {
                                    Id = (int)reader["RolId"],
                                    RolName = reader["RolName"].ToString()!
                                });
                            }
                            userResult.Roles = rolesList;
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
            catch (Exception ex) // CORREGIDO: Bloque catch gen�rico ausente
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Users>> UpdateAsync(int id, Users users)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@UserName", users.UserName);
                    cmd.Parameters.AddWithValue("@UserLastName", users.UserLastName);
                    cmd.Parameters.AddWithValue("@Mail", users.Mail);
                    cmd.Parameters.AddWithValue("@UserPhone", users.UserPhone);
                    cmd.Parameters.AddWithValue("@IsActive", users.IsActive);
                    cmd.Parameters.AddWithValue("@PasswordHash", (object?)users.PasswordHash ?? DBNull.Value);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    Users userUpdateResult = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userUpdateResult = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    return new RepositoryResponse<Users>
                    {
                        Data = userUpdateResult,
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
            catch (Exception ex) // CORREGIDO: Bloque catch gen�rico ausente
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Users>> GetByUserNameAsync(string name)
        {
            var userResult = new Users();
            var response = new RepositoryResponse<Users>();

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
                    response.Data = userResult;
                    response.OperationStatusCode = returnedValue;
                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
                return response;
            }
            catch (Exception ex) // CORREGIDO: Bloque catch gen�rico unificado en la ra�z del m�todo
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<RepositoryResponse<Users>> GetByEmailAsync(string email)
        {
            var response = new RepositoryResponse<Users>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserByEmail", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Mail", email);
                    cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                    Users userResult = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userResult = new Users(); // CORREGIDO: Instanciaci�n obligatoria para mitigar NullReferenceException
                            userResult.UserId = (int)reader["UserId"];
                            userResult.UserName = reader["UserName"].ToString()!;
                            userResult.UserLastName = reader["UserLastName"].ToString()!;
                            userResult.PasswordHash = reader["PasswordHash"].ToString()!;
                            userResult.Mail = reader["Mail"].ToString()!;
                            userResult.UserPhone = reader["UserPhone"].ToString()!;
                            userResult.IsActive = (bool)reader["IsActive"];
                        }
                    }

                    var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                    response.Data = userResult;
                    response.OperationStatusCode = returnedValue;
                    return response;
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
                return response;
            }
            catch (Exception ex) // CORREGIDO: A�adido soporte global de fallos de infraestructura
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<RepositoryResponse<Users>> SetStateAsync(int id, bool state)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // CORREGIDO: Redirecci�n al SP correcto de usuarios (Antes USP_UpdateBrandStatus)
                    SqlCommand cmd = new SqlCommand("USP_UpdateUserStatus", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);
                    cmd.Parameters.AddWithValue("@IsActive", state);

                    Users updatedUser = null;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            updatedUser = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"]
                            };
                        }
                    }

                    return new RepositoryResponse<Users>
                    {
                        Data = updatedUser,
                        OperationStatusCode = updatedUser != null ? 0 : 1
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
            catch (Exception ex) // CORREGIDO: Bloque catch gen�rico ausente
            {
                return new RepositoryResponse<Users>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<Roles>>> AssignRoleToUserAsync(int userId, int roleId)
        {
            var updatedRoles = new List<Roles>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_AsigneRolToUser", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@RolId", roleId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            updatedRoles.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!
                            });
                        }
                    }
                }

                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = updatedRoles,
                    OperationStatusCode = 0,
                    Message = "Operaci�n exitosa"
                };
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number, // CORREGIDO: Mapeo din�mico del c�digo SQL nativo para capturar el error 50003
                    Message = ex.Message
                };
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
        }
    
        
        public async Task<RepositoryResponse<IEnumerable<Roles>>> UpdateUserRoleAsync(int userId, int newRoleId)
        {
            var updatedRoles = new List<Roles>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_UpdateUserRole", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@NewRolId", newRoleId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            updatedRoles.Add(new Roles
                            {
                                Id = (int)reader["RolId"],
                                RolName = reader["RolName"].ToString()!
                            });
                        }
                    }
                }

                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = updatedRoles,
                    OperationStatusCode = 0,
                    Message = "Rol actualizado correctamente"
                };
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<IEnumerable<Roles>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
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
        }
        public async Task<RepositoryResponse<(IEnumerable<Users> Items, int TotalCount)>> GetUsersPagedAsync(int pageNumber, int pageSize)
        {
            var userList = new List<Users>();
            var userDictionary = new Dictionary<int, Users>();
            int totalCount = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUserPage", connection);
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
                            var user = new Users
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["UserName"].ToString()!,
                                UserLastName = reader["UserLastName"].ToString()!,
                                Mail = reader["Mail"].ToString()!,
                                UserPhone = reader["UserPhone"].ToString()!,
                                IsActive = (bool)reader["Isactive"],
                                Roles = new List<Roles>()
                            };
                            userList.Add(user);
                            userDictionary[user.UserId] = user;
                        }

                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var userIdOwner = (int)reader["UserId"];
                                if (userDictionary.TryGetValue(userIdOwner, out var userOwner))
                                {
                                    userOwner.Roles.Add(new Roles
                                    {
                                        Id = (int)reader["RolId"],
                                        RolName = reader["RolName"].ToString()!
                                    });
                                }
                            }
                        }
                    }

                    if (totalRecordsParam.Value != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(totalRecordsParam.Value);
                    }

                    return new RepositoryResponse<(IEnumerable<Users> Items, int TotalCount)>
                    {
                        Data = (userList, totalCount),
                        OperationStatusCode = 0,
                        Message = "Operaci�n exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<(IEnumerable<Users> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Users>(), 0),
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<(IEnumerable<Users> Items, int TotalCount)>
                {
                    Data = (Enumerable.Empty<Users>(), 0),
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
