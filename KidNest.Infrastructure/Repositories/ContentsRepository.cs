using KidNest.Core.Entities;
using KidNest.Core.Enums;
using KidNest.Core.Interfaces;
using System;
using System.Data.SqlClient;

namespace KidNest.Infrastructure.Repositories
{
    public class ContentsRepository : IContentsRepository
    {
        public async Task<int> AddAsync(Content content)
        {
            var query = @"INSERT INTO [dbo].[Contents](
                ContentName,
                ContentType,
                ContentPath, 
                IsActive)
                VALUES (@ContentName, @ContentType, @ContentPath, @IsActive);";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ContentName",
                    string.IsNullOrEmpty(content.Name) ? DBNull.Value : content.Name);

                command.Parameters.AddWithValue("@ContentType",
                    string.IsNullOrEmpty(content.Type.ToString()) ? DBNull.Value : content.Type.ToString());

                command.Parameters.AddWithValue("@ContentPath",
                    string.IsNullOrEmpty(content.Path) ? DBNull.Value : content.Path);

                command.Parameters.AddWithValue("@IsActive", content.IsActive);

                try
                {
                    await connection.OpenAsync();
                    var rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected;
                }
                catch (SqlException)
                {
                    throw new($"An error occurred while adding content");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            string query = @"DELETE FROM [dbo].[Contents] WHERE [ContentId] = @id;";

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
                    throw new($"An error occurred while deleting content");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<IEnumerable<Content>> GetAllAsync()
        {
            var contents = new List<Content>();
            string query = "SELECT * FROM [dbo].[Contents];";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int contentIdOrdinal = reader.GetOrdinal("ContentId");
                        int nameOrdinal = reader.GetOrdinal("ContentName");
                        int typeOrdinal = reader.GetOrdinal("ContentType");
                        int pathOrdinal = reader.GetOrdinal("ContentPath");
                        int isActiveOrdinal = reader.GetOrdinal("IsActive");

                        while (await reader.ReadAsync())
                        {
                            var content = new Content
                            {
                                Id = reader.GetInt32(contentIdOrdinal),
                                Name = reader.IsDBNull(nameOrdinal) ? null : reader.GetString(nameOrdinal),
                                Type = reader.IsDBNull(typeOrdinal) ? null :
                                    Enum.Parse<ContentType>(reader.GetString(typeOrdinal)),
                                Path = reader.IsDBNull(pathOrdinal) ? null : reader.GetString(pathOrdinal),
                                IsActive = reader.GetBoolean(isActiveOrdinal)
                            };

                            contents.Add(content);
                        }
                    }
                }
                catch (SqlException)
                {
                    throw new($"An error occurred while getting contents");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return contents;
        }

        public async Task<Content?> GetByIdAsync(int id)
        {
            string query = @"SELECT * FROM [dbo].[Contents] WHERE ContentId = @ContentId";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ContentId", id);

                try
                {
                    connection.Open();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int contentIdOrdinal = reader.GetOrdinal("ContentId");
                            int nameOrdinal = reader.GetOrdinal("ContentName");
                            int typeOrdinal = reader.GetOrdinal("ContentType");
                            int pathOrdinal = reader.GetOrdinal("ContentPath");
                            int isActiveOrdinal = reader.GetOrdinal("IsActive");

                            return new Content
                            {
                                Id = reader.GetInt32(contentIdOrdinal),
                                Name = reader.IsDBNull(nameOrdinal) ? null : reader.GetString(nameOrdinal),
                                Type = reader.IsDBNull(typeOrdinal) ? null :
                                    Enum.Parse<ContentType>(reader.GetString(typeOrdinal)),
                                Path = reader.IsDBNull(pathOrdinal) ? null : reader.GetString(pathOrdinal),
                                IsActive = reader.GetBoolean(isActiveOrdinal)
                            };
                        }

                        return null;
                    }
                }
                catch (SqlException)
                {
                    throw new($"An error occurred while getting content");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> UpdateAsync(Content content)
        {
            string query =
              @"UPDATE [dbo].[Contents] 
                  SET 
                      ContentName = @ContentName,
                      ContentType = @ContentType,
                      ContentPath = @ContentPath,
                      IsActive = @IsActive
                  WHERE ContentId = @Id";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", content.Id);

                command.Parameters.AddWithValue("@ContentName",
                    content.Name == null ? DBNull.Value : content.Name);

                command.Parameters.AddWithValue("@ContentType",
                    content.Type == null ? DBNull.Value : content.Type);

                command.Parameters.AddWithValue("@ContentPath",
                    content.Path == null ? DBNull.Value : content.Path);

                command.Parameters.AddWithValue("@IsActive", content.IsActive);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                catch (SqlException)
                {
                    throw new($"An error occurred while updating content");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
