using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace KidNest.Infrastructure.Repositories
{
    public class UsersRepository : IUsersRespository
    {
        public async Task<int> AddAsync(AppUser user)
        {
            const string query = @"
                INSERT INTO [dbo].[AppUsers] 
                ([AppUserFullName], [AppUserPhoneNumber], [AppUserEmail], 
                 [AppUserPassword], [AppUserAddress], [AppUserCreatedDate]) 
                OUTPUT INSERTED.AppUserId
                VALUES (@AppUserFullName, @AppUserPhoneNumber, 
                @AppUserEmail, @AppUserPassword, @AppUserAddress, @AppUserCreatedDate);";

            await using var connection = DbConnectionFactory.CreateConnection();
            SqlTransaction transaction = null!;

            try
            {
                await connection.OpenAsync();
                transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                // To fetch user ID for stored procedure
                int userId;

                // Insert user and fetch ID
                await using (var cmd = new SqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@AppUserFullName", user.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppUserPhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppUserEmail", user.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppUserAddress", user.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppUserPassword", user.Password ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppUserCreatedDate", user.CreatedDate ?? DateTime.Now);

                    userId = (int)await cmd.ExecuteScalarAsync();
                }

                // Assign role via stored procedure
                await using (var cmd = new SqlCommand("sp_AssignUserRole", connection, transaction))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppUserId", userId);
                    cmd.Parameters.AddWithValue("@RoleName", "User"); // Default role: User

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();

                return userId;
            }
            catch (SqlException)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                await connection.CloseAsync(); // can be removed safetly
            }
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            string query = @"DELETE FROM [dbo].[AppUsers] WHERE AppUserId = @UserId";

            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            try
            {
                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (SqlException)
            {
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            var appUsers = new List<AppUser>();

            var query = "SELECT * FROM [dbo].[AppUsers]";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int appUserIdIndex = reader.GetOrdinal("AppUserId");
                        int fullNameIndex = reader.GetOrdinal("AppUserFullName");
                        int phoneNumberIndex = reader.GetOrdinal("AppUserPhoneNumber");
                        int emailIndex = reader.GetOrdinal("AppUserEmail");
                        int dateOfBirthIndex = reader.GetOrdinal("AppUserDOB");
                        int codeIndex = reader.GetOrdinal("AppUserCode");
                        int passwordIndex = reader.GetOrdinal("AppUserPassword");
                        int lastLoginDateIndex = reader.GetOrdinal("AppUserLastLoginDate");
                        int lastLogInPCNameIndex = reader.GetOrdinal("AppUserLastLogInPCName");
                        int isActiveIndex = reader.GetOrdinal("AppUserIsActive");
                        //int rowVersionIndex = reader.GetOrdinal("AppUserRowVersion");
                        //int updatedByIndex = reader.GetOrdinal("AppUserUpdatedBy");
                        //int lastUpdatedIndex = reader.GetOrdinal("AppUserLastUpdated");
                        //int userCreatedByIndex = reader.GetOrdinal("AppUserUserCreatedBy");
                        //int createdDateIndex = reader.GetOrdinal("AppUserCreatedDate");
                        //int tTimeStampIndex = reader.GetOrdinal("tTimeStamp");

                        while (await reader.ReadAsync())
                        {
                            var user = new AppUser
                            {
                                Id = reader.GetInt32(appUserIdIndex),
                                FullName = reader.IsDBNull(fullNameIndex) ? null : reader.GetString(fullNameIndex),
                                PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? null : reader.GetString(phoneNumberIndex),
                                Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                                DOB = reader.IsDBNull(dateOfBirthIndex) ? null : reader.GetDateTime(dateOfBirthIndex),
                                Code = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                                Password = reader.IsDBNull(passwordIndex) ? null : reader.GetString(passwordIndex),
                                LastLoginDate = reader.IsDBNull(lastLoginDateIndex) ? null : reader.GetDateTime(lastLoginDateIndex),
                                LastLogInPCName = reader.IsDBNull(lastLogInPCNameIndex) ? null : reader.GetString(lastLogInPCNameIndex),
                                IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                                //RowVersion = reader.GetInt32(rowVersionIndex),
                                //UserUpdatedBy = reader.IsDBNull(updatedByIndex) ? null : reader.GetString(updatedByIndex),
                                //LastUpdated = reader.IsDBNull(lastUpdatedIndex) ? null : reader.GetDateTime(lastUpdatedIndex),
                                //UserCreatedBy = reader.IsDBNull(userCreatedByIndex) ? null : reader.GetString(userCreatedByIndex),
                                //CreatedDate = reader.IsDBNull(createdDateIndex) ? null : reader.GetDateTime(createdDateIndex),
                            };

                            appUsers.Add(user);
                        }
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return appUsers;
        }

        public async Task<bool> UpdateAsync(AppUser user)
        {
            string query =
                @"UPDATE [dbo].[AppUsers] 
                  SET 
                    AppUserFullName = @FullName,
                    AppUserPhoneNumber = @PhoneNumber,
                    AppUserEmail = @Email,
                    AppUserAddress = @Address,
                    AppUserDOB = @DOB,
                    AppUserCode = @Code,
                    AppUserPassword = @Password,
                    AppUserLastLoginDate = @LastLoginDate,
                    AppUserLastLogInPCName = @LastLogInPCName,
                    AppUserIsActive = @IsActive,
                    AppUserRowVersion = @RowVersion,
                    AppUserUserUpdatedBy = @UserUpdatedBy,
                    AppUserLastUpdated = @LastUpdated,
                    AppUserUserCreatedBy = @UserCreatedBy,
                    AppUserCreatedDate = @CreatedDate
                WHERE AppUserId = @Id";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@FullName", (object?)user.FullName ?? DBNull.Value);
                command.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address", (object?)user.Address?? DBNull.Value);
                command.Parameters.AddWithValue("@DOB", user.DOB.HasValue ? (object)user.DOB.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Code", (object?)user.Code ?? DBNull.Value);
                command.Parameters.AddWithValue("@Password", (object?)user.Password ?? DBNull.Value);
                command.Parameters.AddWithValue("@LastLoginDate", user.LastLoginDate.HasValue ? (object)user.LastLoginDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@LastLogInPCName", (object?)user.LastLogInPCName ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", user.IsActive.HasValue ? user.IsActive.Value : DBNull.Value);
                command.Parameters.AddWithValue("@RowVersion", user.RowVersion);
                command.Parameters.AddWithValue("@UserUpdatedBy", (object?)user.UserUpdatedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", user.LastUpdated.HasValue ? (object)user.LastUpdated.Value : DBNull.Value);
                command.Parameters.AddWithValue("@UserCreatedBy", (object?)user.UserCreatedBy ?? DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate.HasValue ? (object)user.CreatedDate.Value : DBNull.Value);
                //command.Parameters.AddWithValue("@tTimeStamp", user.TimeStamp.HasValue ? (object)user.TimeStamp.Value : DBNull.Value);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<AppUser?> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            AppUser? appUser = null;

            var query = @"
                SELECT
                    [AppUserId], 
                    [AppUserFullName], 
                    [AppUserPhoneNumber], 
                    [AppUserEmail], 
                    [AppUserDOB], 
                    [AppUserCode], 
                    [AppUserPassword], 
                    [AppUserLastLoginDate], 
                    [AppUserLastLogInPCName], 
                    [AppUserIsActive], 
                    [AppUserRowVersion], 
                    [AppUserUserUpdatedBy], 
                    [AppUserLastUpdated], 
                    [AppUserUserCreatedBy], 
                    [AppUserCreatedDate],
                    [tTimeStamp]
                FROM [dbo].[AppUsers]
                WHERE [AppUserEmail] = @EmailOrPhone OR [AppUserPhoneNumber] = @EmailOrPhone;";

            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.CommandType = CommandType.Text;
            command.Parameters.AddWithValue("@EmailOrPhone", emailOrPhone);

            try
            {
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Assign column indexes to variables for readability
                    int idIndex = reader.GetOrdinal("AppUserId");
                    int fullNameIndex = reader.GetOrdinal("AppUserFullName");
                    int phoneNumberIndex = reader.GetOrdinal("AppUserPhoneNumber");
                    int emailIndex = reader.GetOrdinal("AppUserEmail");
                    int dobIndex = reader.GetOrdinal("AppUserDOB");
                    int codeIndex = reader.GetOrdinal("AppUserCode");
                    int passwordIndex = reader.GetOrdinal("AppUserPassword");
                    int lastLoginDateIndex = reader.GetOrdinal("AppUserLastLoginDate");
                    int lastLogInPCNameIndex = reader.GetOrdinal("AppUserLastLogInPCName");
                    int isActiveIndex = reader.GetOrdinal("AppUserIsActive");
                    int rowVersionIndex = reader.GetOrdinal("AppUserRowVersion");
                    int userUpdatedByIndex = reader.GetOrdinal("AppUserUserUpdatedBy");
                    int lastUpdatedIndex = reader.GetOrdinal("AppUserLastUpdated");
                    int userCreatedByIndex = reader.GetOrdinal("AppUserUserCreatedBy");
                    int createdDateIndex = reader.GetOrdinal("AppUserCreatedDate");
                    int tTimeStampIndex = reader.GetOrdinal("tTimeStamp");

                    appUser = new AppUser
                    {
                        Id = reader.GetInt32(idIndex),
                        FullName = reader.IsDBNull(fullNameIndex) ? null : reader.GetString(fullNameIndex),
                        PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? null : reader.GetString(phoneNumberIndex),
                        Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                        DOB = reader.IsDBNull(dobIndex) ? (DateTime?)null : reader.GetDateTime(dobIndex),
                        Code = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                        Password = reader.IsDBNull(passwordIndex) ? null : reader.GetString(passwordIndex),
                        LastLoginDate = reader.IsDBNull(lastLoginDateIndex) ? (DateTime?)null : reader.GetDateTime(lastLoginDateIndex),
                        LastLogInPCName = reader.IsDBNull(lastLogInPCNameIndex) ? null : reader.GetString(lastLogInPCNameIndex),
                        IsActive = reader.IsDBNull(isActiveIndex) ? (bool?)null : reader.GetBoolean(isActiveIndex),
                        RowVersion = reader.IsDBNull(rowVersionIndex) ? 0 : reader.GetInt32(rowVersionIndex),
                        UserUpdatedBy = reader.IsDBNull(userUpdatedByIndex) ? null : reader.GetString(userUpdatedByIndex),
                        LastUpdated = reader.IsDBNull(lastUpdatedIndex) ? (DateTime?)null : reader.GetDateTime(lastUpdatedIndex),
                        UserCreatedBy = reader.IsDBNull(userCreatedByIndex) ? null : reader.GetString(userCreatedByIndex),
                        CreatedDate = reader.IsDBNull(createdDateIndex) ? (DateTime?)null : reader.GetDateTime(createdDateIndex),
                        // If TTimeStamp is a complex type, you may need to adjust accordingly
                        //tTimeStamp = reader.IsDBNull(tTimeStampIndex) ? null : reader.GetValue(tTimeStampIndex), 
                    };
                }
            }
            catch (SqlException)
            {
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }

            return appUser;
        }

        public async Task<AppUser?> GetByIdAsync(int userId)
        {
            string query = @"
                SELECT *
                FROM [dbo].[AppUsers]
                WHERE [AppUserId] = @UserId";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    await connection.OpenAsync();

                    using var reader = await command.ExecuteReaderAsync();

                    int appUserIdIndex = reader.GetOrdinal("AppUserId");
                    int fullNameIndex = reader.GetOrdinal("AppUserFullName");
                    int phoneNumberIndex = reader.GetOrdinal("AppUserPhoneNumber");
                    int emailIndex = reader.GetOrdinal("AppUserEmail");
                    int addressIndex = reader.GetOrdinal("AppUserAddress");
                    int dateOfBirthIndex = reader.GetOrdinal("AppUserDOB");
                    int codeIndex = reader.GetOrdinal("AppUserCode");
                    int passwordIndex = reader.GetOrdinal("AppUserPassword");
                    int lastLoginDateIndex = reader.GetOrdinal("AppUserLastLoginDate");
                    int lastLogInPCNameIndex = reader.GetOrdinal("AppUserLastLogInPCName");
                    int isActiveIndex = reader.GetOrdinal("AppUserIsActive");
                    int rowVersionIndex = reader.GetOrdinal("AppUserRowVersion");
                    int userUpdatedByIndex = reader.GetOrdinal("AppUserUserUpdatedBy");
                    int lastUpdatedIndex = reader.GetOrdinal("AppUserLastUpdated");
                    int userCreatedByIndex = reader.GetOrdinal("AppUserUserCreatedBy");
                    int createdDateIndex = reader.GetOrdinal("AppUserCreatedDate");
                    int tTimeStampIndex = reader.GetOrdinal("tTimeStamp");

                    if (await reader.ReadAsync())
                    {
                        return new AppUser
                        {
                            Id = reader.GetInt32(appUserIdIndex),
                            FullName = reader.IsDBNull(fullNameIndex) ? null : reader.GetString(fullNameIndex),
                            PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? null : reader.GetString(phoneNumberIndex),
                            Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                            Address = reader.IsDBNull(addressIndex) ? null : reader.GetString(addressIndex),
                            DOB = reader.IsDBNull(dateOfBirthIndex) ? null : reader.GetDateTime(dateOfBirthIndex),
                            Code = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                            Password = reader.IsDBNull(passwordIndex) ? null : reader.GetString(passwordIndex),
                            LastLoginDate = reader.IsDBNull(lastLoginDateIndex) ? null : reader.GetDateTime(lastLoginDateIndex),
                            LastLogInPCName = reader.IsDBNull(lastLogInPCNameIndex) ? null : reader.GetString(lastLogInPCNameIndex),
                            IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                            RowVersion = reader.IsDBNull(rowVersionIndex) ? 0 : reader.GetInt32(rowVersionIndex),
                            UserUpdatedBy = reader.IsDBNull(userUpdatedByIndex) ? null : reader.GetString(userUpdatedByIndex),
                            LastUpdated = reader.IsDBNull(lastUpdatedIndex) ? (DateTime?)null : reader.GetDateTime(lastUpdatedIndex),
                            UserCreatedBy = reader.IsDBNull(userCreatedByIndex) ? null : reader.GetString(userCreatedByIndex),
                            CreatedDate = reader.IsDBNull(createdDateIndex) ? (DateTime?)null : reader.GetDateTime(createdDateIndex),
                            // If TTimeStamp is a complex type, you may need to adjust accordingly
                            //tTimeStamp = reader.IsDBNull(tTimeStampIndex) ? null : reader.GetValue(tTimeStampIndex), 
                        };
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return null;
        }

        public async Task<bool> IsEmailOrPhoneExistsAsync(string emailOrPhone, int? excludedId = null)
        {
            string query = @"
                SELECT COUNT(1) FROM [dbo].[AppUsers] " +
                "WHERE (AppUserEmail = @EmailOrPhone OR AppUserPhoneNumber = @EmailOrPhone)";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EmailOrPhone", emailOrPhone);

                if (excludedId.HasValue)
                {
                    query += " AND AppUserId <> @Id;";
                    command.Parameters.AddWithValue("@Id", emailOrPhone);
                }

                try
                {
                    await connection.OpenAsync();
                    int count = (int)await command.ExecuteScalarAsync();

                    return count > 0;
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<string> GetUserRole(int userId)
        {
            string query = @"
                SELECT 
	                [RoleName]
                FROM 
	                [dbo].[Roles] AS r
                INNER JOIN [dbo].[AppUsersRoles] AS ur ON r.RoleId = ur.RoleId
                WHERE ur.AppUserId = @UserId";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                try
                {
                    await connection.OpenAsync();

                    using var reader = await command.ExecuteReaderAsync();

                    int userRoleNameIndex = reader.GetOrdinal("RoleName");

                    if (await reader.ReadAsync())
                    {
                        string userRoleName = reader.GetString(userRoleNameIndex);

                        return userRoleName;
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return null;
        }

        // Filter users
        public async Task<(IEnumerable<AppUser> users, int totalCount)> GetFilteredUsersAsync(
           int start,
           int length,
           string searchValue,
           string sortColumn,
           string sortDirection)
        {
            var users = new List<AppUser>();
            int totalCount = 0;

            string baseQuery = @"
                SELECT *
                FROM [dbo].[AppUsers] AS u";

            string countQuery = "SELECT COUNT(*) FROM [dbo].[AppUsers]";

            // Add search filtering
            string whereClause = string.Empty;
            if (!string.IsNullOrEmpty(searchValue))
            {
                whereClause = @"
                    WHERE u.AppUserId LIKE @searchValue 
                    OR u.AppUserFullName LIKE @searchValue
                    OR u.AppUserPhoneNumber LIKE @searchValue 
                    OR u.AppUserEmail LIKE @searchValue
                    OR u.AppUserIsActive LIKE @searchvalue";
            }

            // Add sorting
            string orderByClause = string.Empty;
            if (!string.IsNullOrEmpty(sortColumn))
            {
                string columnName = sortColumn switch
                {
                    "fullName" => "u.AppUserFullName",
                    "phoneNumber" => "u.AppUserPhoneNumber",
                    "email" => "u.AppUserEmail",
                    "isActive" => "u.AppUserIsActive",
                    _ => "u.AppUserId"
                };
                orderByClause = $"ORDER BY {columnName} {(sortDirection == "desc" ? "DESC" : "ASC")}";
            }

            // Add pagination
            string pagingClause = "OFFSET @start ROWS FETCH NEXT @length ROWS ONLY";

            string finalQuery = $@"{baseQuery} {whereClause} {orderByClause} {pagingClause}; {countQuery};";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(finalQuery, connection))
            {
                command.Parameters.AddWithValue("@searchValue", $"%{searchValue}%");
                command.Parameters.AddWithValue("@start", start);
                command.Parameters.AddWithValue("@length", length);

                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Read users
                        while (await reader.ReadAsync())
                        {
                            var user = MapUserFromReader(reader);
                            users.Add(user);
                        }

                        // Read total count
                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            totalCount = reader.GetInt32(0);
                        }
                    }
                }
                catch (SqlException)
                {
                    // Maybe log error
                    throw;
                }

                return (users, totalCount);
            }
        }

        private static AppUser MapUserFromReader(SqlDataReader reader)
        {
            // Get ordinal indexes first
            int appUserIdIndex = reader.GetOrdinal("AppUserId");
            int fullNameIndex = reader.GetOrdinal("AppUserFullName");
            int phoneNumberIndex = reader.GetOrdinal("AppUserPhoneNumber");
            int emailIndex = reader.GetOrdinal("AppUserEmail");
            int dateOfBirthIndex = reader.GetOrdinal("AppUserDOB");
            int codeIndex = reader.GetOrdinal("AppUserCode");
            int passwordIndex = reader.GetOrdinal("AppUserPassword");
            int lastLoginDateIndex = reader.GetOrdinal("AppUserLastLoginDate");
            int lastLogInPCNameIndex = reader.GetOrdinal("AppUserLastLogInPCName");
            int isActiveIndex = reader.GetOrdinal("AppUserIsActive");
            int rowVersionIndex = reader.GetOrdinal("AppUserRowVersion");
            int userUpdatedByIndex = reader.GetOrdinal("AppUserUserUpdatedBy");
            int lastUpdatedIndex = reader.GetOrdinal("AppUserLastUpdated");
            int userCreatedByIndex = reader.GetOrdinal("AppUserUserCreatedBy");
            int createdDateIndex = reader.GetOrdinal("AppUserCreatedDate");
            //int tTimeStampIndex = reader.GetOrdinal("tTimeStamp");

            return new AppUser
            {
                Id = reader.GetInt32(appUserIdIndex),
                FullName = reader.IsDBNull(fullNameIndex) ? null : reader.GetString(fullNameIndex),
                PhoneNumber = reader.IsDBNull(phoneNumberIndex) ? null : reader.GetString(phoneNumberIndex),
                Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                DOB = reader.IsDBNull(dateOfBirthIndex) ? null : reader.GetDateTime(dateOfBirthIndex),
                Code = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                Password = reader.IsDBNull(passwordIndex) ? null : reader.GetString(passwordIndex),
                LastLoginDate = reader.IsDBNull(lastLoginDateIndex) ? null : reader.GetDateTime(lastLoginDateIndex),
                LastLogInPCName = reader.IsDBNull(lastLogInPCNameIndex) ? null : reader.GetString(lastLogInPCNameIndex),
                IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                RowVersion = reader.IsDBNull(rowVersionIndex) ? 0 : reader.GetInt32(rowVersionIndex),
                UserUpdatedBy = reader.IsDBNull(userUpdatedByIndex) ? null : reader.GetString(userUpdatedByIndex),
                LastUpdated = reader.IsDBNull(lastUpdatedIndex) ? null : reader.GetDateTime(lastUpdatedIndex),
                UserCreatedBy = reader.IsDBNull(userCreatedByIndex) ? null : reader.GetString(userCreatedByIndex),
                CreatedDate = reader.IsDBNull(createdDateIndex) ? null : reader.GetDateTime(createdDateIndex),
                // If TTimeStamp is a complex type, you may need to adjust accordingly
                //tTimeStamp = reader.IsDBNull(tTimeStampIndex) ? null : reader.GetValue(tTimeStampIndex), 
            };
        }
    }
}
