using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System.Data.SqlClient;

namespace KidNest.Infrastructure.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        public Task<IEnumerable<Role>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Role?> GetByIdAsync(int roleId)
        {
            string query = "SELECT * FROM [dbo].[Roles] WHERE RoleId = @RoleId";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@RoleId", roleId);

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int idIndex = reader.GetOrdinal("RoleId");
                        int nameIndex = reader.GetOrdinal("RoleName");
                        int descriptionIndex = reader.GetOrdinal("RoleDescription");

                        if (await reader.ReadAsync())
                        {
                            return new Role
                            {
                                Id = reader.GetInt32(idIndex),
                                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                                Description = reader.IsDBNull(descriptionIndex) ? 
                                    null : reader.GetString(descriptionIndex)
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
    }
}
