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
    public class SettingsRepository : ISettingsRepository
    {
        public async Task<SiteSettings?> GetAsync()
        {
            string query = @"SELECT TOP 1 * FROM SiteSettings";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", 1); // fetch the singleton row

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Get column indexes
                            int idIndex = reader.GetOrdinal("SiteSettingsId");
                            int contactEmailIndex = reader.GetOrdinal("ContactEmail");
                            int contactPhoneIndex = reader.GetOrdinal("ContactPhone");
                            int facebookURLIndex = reader.GetOrdinal("FacebookUrl");
                            int instagramURLIndex = reader.GetOrdinal("InstagramUrl");
                            int contactWhatsappIndex = reader.GetOrdinal("ContactWhatsapp");
                            int parallaxImageIndex = reader.GetOrdinal("ParallaxImage");
                            int lastUpdatedIndex = reader.GetOrdinal("LastUpdated");

                            return new SiteSettings
                            {
                                Id = reader.GetInt32(idIndex),
                                ContactEmail = reader.IsDBNull(contactEmailIndex) ? null : reader.GetString(contactEmailIndex),
                                ContactPhone = reader.IsDBNull(contactPhoneIndex) ? null : reader.GetString(contactPhoneIndex),
                                FacebookUrl = reader.IsDBNull(facebookURLIndex) ? null : reader.GetString(facebookURLIndex),
                                InstagramUrl = reader.IsDBNull(instagramURLIndex) ? null : reader.GetString(instagramURLIndex),
                                ContactWhatsapp = reader.IsDBNull(contactWhatsappIndex) ? null : reader.GetString(contactWhatsappIndex),
                                ParallaxImage = reader.IsDBNull(parallaxImageIndex) ? null : reader.GetString(parallaxImageIndex),
                                LastUpdated = reader.GetDateTime(lastUpdatedIndex)
                            };
                        }

                        return null;
                    }
                }
                catch (SqlException)
                {
                    throw new($"Error fetching settings");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> UpdateAsync(SiteSettings settings)
        {
            string query = @"
                UPDATE [dbo].[SiteSettings]
                SET 
                    ContactEmail = @ContactEmail,
                    ContactPhone = @ContactPhone,
                    FacebookUrl = @FacebookUrl,
                    InstagramUrl = @InstagramUrl,
                    ContactWhatsapp = @ContactWhatsapp,
                    ParallaxImage = @ParallaxImage,
                    LastUpdated = @LastUpdated
                WHERE SiteSettingsId = 1";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ContactEmail", (object?)settings.ContactEmail ?? DBNull.Value);
                command.Parameters.AddWithValue("@ContactPhone", (object?)settings.ContactPhone ?? DBNull.Value);
                command.Parameters.AddWithValue("@FacebookUrl", (object?)settings.FacebookUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@InstagramUrl", (object?)settings.InstagramUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@ContactWhatsapp", (object?)settings.ContactWhatsapp ?? DBNull.Value);
                command.Parameters.AddWithValue("@ParallaxImage", (object?)settings.ParallaxImage ?? DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", settings.LastUpdated);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                catch (SqlException)
                {
                    throw new($"Error updating settings");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
