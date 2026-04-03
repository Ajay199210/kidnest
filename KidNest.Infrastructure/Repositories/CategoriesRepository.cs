using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace KidNest.Infrastructure.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        public async Task<int> AddAsync(Category category)
        {
            string query = @"INSERT INTO [dbo].[Categories] (CategoryName, CategoryDescription) 
                VALUES (@CategoryName, @CategoryDescription);";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CategoryName", category.Name);
                command.Parameters.AddWithValue("@CategoryDescription",
                    string.IsNullOrEmpty(category.Description) ? DBNull.Value : category.Description);

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
            string query = @"DELETE FROM [dbo].[Categories] WHERE [CategoryId] = @id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0; // Return true if at least one row was deleted
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

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var categories = new List<Category>();
            string query = "SELECT * FROM [dbo].[Categories]";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int idIndex = reader.GetOrdinal("CategoryId");
                        int nameIndex = reader.GetOrdinal("CategoryName");
                        int descriptionIndex = reader.GetOrdinal("CategoryDescription");

                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                Id = reader.GetInt32(idIndex),
                                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                                Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
                            });
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

            return categories;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            string query = "SELECT * FROM Categories WHERE CategoryId = @Id";

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
                            return new Category
                            {
                                Id = (int)reader["CategoryId"],
                                Name = reader["CategoryName"].ToString(),
                                Description = reader.IsDBNull("CategoryDescription") ?
                                    null : reader.GetString("CategoryDescription")
                            };
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

        public async Task<bool> UpdateAsync(Category category)
        {
            string query = "UPDATE [dbo].[Categories] " +
                "SET CategoryName = @Name, CategoryDescription = @Description " +
                "WHERE CategoryId = @Id";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", category.Id);
                command.Parameters.AddWithValue("@Name", category.Name);
                command.Parameters.AddWithValue("@Description",
                    category.Description == null ? DBNull.Value : category.Description); // Handle NULL

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0; // Returns true if update succeeded
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

        public async Task<bool> HasProductsAsync(int id)
        {
            string query = @"
                SELECT COUNT(*) AS ProductCount
                FROM Products
                WHERE CategoryId = @CategoryId;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@CategoryId", id);

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

        public async Task<bool> ExistsByNameAsync(string name, int? exclucdedId = null)
        {
            string query = "SELECT COUNT(1) FROM [dbo].[Categories] WHERE CategoryName = @Name";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", name);

                if (exclucdedId != null)
                {
                    command.CommandText += " AND CategoryId <> @Id;";
                    command.Parameters.AddWithValue("@Id", exclucdedId);
                }

                try
                {
                    await connection.OpenAsync();
                    int rowsCount = (int)await command.ExecuteScalarAsync();

                    return rowsCount > 0;
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
    }
}
