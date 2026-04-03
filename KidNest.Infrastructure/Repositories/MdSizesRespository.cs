using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Infrastructure.Repositories
{
    public class MdSizesRepository : IMdSizesRepository
    {
        public async Task<int> AddAsync(MdSize mdSize)
        {
            string query = @"
            INSERT INTO [dbo].[MdSizes] (MdSizeDescription, MdSizeCode, MdSizeIsActive, MdSizeCreatedDate)
            VALUES (@Description, @Code, @IsActive, @CreatedDate);";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Description", mdSize.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Code", mdSize.SizeCode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", mdSize.IsActive ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedDate", mdSize.CreatedDate ?? DateTime.UtcNow);

                try
                {
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
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

        public async Task<bool> DeleteAsync(int id)
        {
            string query = @"DELETE FROM [dbo].[MdSizes] WHERE [MdSizeId] = @Id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

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

        public async Task<IEnumerable<MdSize>> GetAllAsync()
        {
            var sizes = new List<MdSize>();
            string query = "SELECT * FROM [dbo].[MdSizes];";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sizes.Add(MapMdSizeFromReader(reader));
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

            return sizes;
        }

        public async Task<IEnumerable<MdSize>> GetAllActiveAsync()
        {
            var sizes = new List<MdSize>();
            string query = "SELECT * FROM [dbo].[MdSizes] WHERE MdSizeIsActive = 1;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sizes.Add(MapMdSizeFromReader(reader));
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

            return sizes;
        }

        public async Task<MdSize?> GetByIdAsync(int id)
        {
            string query = "SELECT * FROM [dbo].[MdSizes] WHERE MdSizeId = @Id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapMdSizeFromReader(reader);
                        }

                        return null;
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
        }

        public async Task<bool> UpdateAsync(MdSize mdSize)
        {
            string query = @"
            UPDATE [dbo].[MdSizes]
            SET MdSizeDescription = @Description, MdSizeCode = @Code, MdSizeIsActive = @IsActive
            WHERE MdSizeId = @Id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", mdSize.Id);
                command.Parameters.AddWithValue("@Description", mdSize.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Code", mdSize.SizeCode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", mdSize.IsActive ?? (object)DBNull.Value);

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

        private static MdSize MapMdSizeFromReader(SqlDataReader reader)
        {
            int idIndex = reader.GetOrdinal("MdSizeId");
            int descIndex = reader.GetOrdinal("MdSizeDescription");
            int codeIndex = reader.GetOrdinal("MdSizeCode");
            int isActiveIndex = reader.GetOrdinal("MdSizeIsActive");
            int createdDateIndex = reader.GetOrdinal("MdSizeCreatedDate");

            return new MdSize
            {
                Id = reader.GetInt32(idIndex),
                Description = reader.IsDBNull(descIndex) ? null : reader.GetString(descIndex),
                SizeCode = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                CreatedDate = reader.IsDBNull(createdDateIndex) ? null : reader.GetDateTime(createdDateIndex),
            };
        }
    }
}
