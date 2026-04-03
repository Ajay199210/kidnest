using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Infrastructure.Repositories
{
    public class MdColorsRepository : IMdColorsRepository
    {
        public async Task<int> AddAsync(MdColor mdColor)
        {
            string query = @"
                INSERT INTO [dbo].[MdColors] (MdColorName, MdColorIsActive, MdColorHexValue, MdColorCreatedDate) 
                VALUES (@Name, @IsActive, @HexValue, @CreatedDate);";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", mdColor.Name);
                command.Parameters.AddWithValue("@IsActive", mdColor.IsActive);
                command.Parameters.AddWithValue("@HexValue", mdColor.HexValue);
                command.Parameters.AddWithValue("@CreatedDate", mdColor.CreatedDate);

                try
                {
                    await connection.OpenAsync();
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected;
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
            string query = @"DELETE FROM [dbo].[MdColors] WHERE [MdColorId] = @id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

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

        public async Task<IEnumerable<MdColor>> GetAllAsync()
        {
            var mdColors = new List<MdColor>();
            string query = "SELECT * FROM [dbo].[MdColors]";

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
                            var mdColor = MapMdColorFromReader(reader);
                            mdColors.Add(mdColor);
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

            return mdColors;
        }

        public async Task<IEnumerable<MdColor>> GetAllActiveAsync()
        {
            var mdColors = new List<MdColor>();
            string query = "SELECT * FROM [dbo].[MdColors] WHERE [MdColorIsActive] = 1;";

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
                            var mdColor = MapMdColorFromReader(reader);
                            mdColors.Add(mdColor);
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

            return mdColors;
        }

        public async Task<MdColor?> GetByIdAsync(int id)
        {
            string query = "SELECT * FROM MdColors WHERE [MdColorId] = @Id";

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
                            var mdColor = MapMdColorFromReader(reader);

                            return mdColor;
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

        public async Task<bool> UpdateAsync(MdColor mdColor)
        {
            string query = "UPDATE [dbo].[MdColors] " +
               "SET [MdColorName] = @Name, [MdColorHexValue] = @HexValue, [MdColorIsActive] = @IsActive " +
               "WHERE [MdColorId] = @Id";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", mdColor.Id);
                command.Parameters.AddWithValue("@Name", mdColor.Name);
                command.Parameters.AddWithValue("@HexValue", mdColor.HexValue);
                command.Parameters.AddWithValue("@IsActive", mdColor.IsActive);

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

        private static MdColor MapMdColorFromReader(SqlDataReader reader)
        {
            // Get ordinal indexes first
            int idIndex = reader.GetOrdinal("MdColorId");
            int nameIndex = reader.GetOrdinal("MdColorName");
            int hexValueIndex = reader.GetOrdinal("MdColorHexValue");
            int isActiveIndex = reader.GetOrdinal("MdColorIsActive");
            int createdDateIndex = reader.GetOrdinal("MdColorCreatedDate");

            return new MdColor
            {
                Id = reader.GetInt32(idIndex),
                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                HexValue = reader.IsDBNull(hexValueIndex) ? null : reader.GetString(hexValueIndex),
                IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                CreatedDate = reader.IsDBNull(createdDateIndex) ? null : reader.GetDateTime(createdDateIndex),
            };
        }
    }
}
