using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Linq;

namespace KidNest.Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        public async Task<int> AddAsync(Product product)
        {
            string productQuery = @"INSERT INTO [dbo].[Products](
                ProductName, 
                ProductDescription, 
                CategoryId, 
                ProductBarcode,
                ProductPrice, 
                ProductDiscount, 
                ProductQuantity,
                ProductImagePath,
                ProductCreatedDate,
                ProductIsNewRelease,
                ProductNewReleaseUntil)
                OUTPUT INSERTED.ProductId
                VALUES (@ProductName, @ProductDescription, @CategoryId, @ProductBarcode,
                @ProductPrice, @ProductDiscount, @ProductQuantity, @ProductImagePath,
                @ProductCreatedDate, @ProductIsNewRelease, @ProductNewReleaseUntil);
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    int productId;

                    // Insert Product
                    using (var command = new SqlCommand(productQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@ProductBarcode",
                            string.IsNullOrEmpty(product.Barcode) ? DBNull.Value : product.Barcode);

                        command.Parameters.AddWithValue("@ProductName", product.Name);
                        command.Parameters.AddWithValue("@ProductDescription",
                            string.IsNullOrEmpty(product.Description) ? DBNull.Value : product.Description);
                        command.Parameters.AddWithValue("@ProductPrice",
                            product.Price.HasValue ? (object)product.Price.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@ProductDiscount",
                            product.Discount.HasValue ? (object)product.Discount.Value : DBNull.Value);
                        command.Parameters.AddWithValue("@ProductQuantity", product.Quantity);
                        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                        command.Parameters.AddWithValue("@ProductImagePath",
                            product.ImagePath ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProductCreatedDate", product.CreatedDate);
                        command.Parameters.AddWithValue("@ProductIsNewRelease", product.IsNewRelease);
                        command.Parameters.AddWithValue("@ProductNewReleaseUntil",
                            product.NewReleaseUntil ?? (object)DBNull.Value);

                        productId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    // Insert product variants only if there are any
                    if (product.ProductVariants.Count > 0)
                    {
                        foreach (var variant in product.ProductVariants)
                        {
                            string variantQuery = @"INSERT INTO [dbo].[ProductVariants] (
                                ProductVariantProductId,
                                ProductVariantColorId,
                                ProductVariantSizeId,
                                ProductVariantCategoryId,
                                ProductVariantBarcode,
                                ProductVariantQuantity,
                                ProductVariantCreatedDate,
                                ProductVariantModifiedDate,
                                ProductVariantIsActive)
                                VALUES (
                                    @ProductId,
                                    @ColorId,
                                    @SizeId,
                                    @CategoryId,
                                    @Barcode,
                                    @Quantity,
                                    @CreatedDate,
                                    @ModifiedDate,
                                    @IsActive);
                            ";

                            using (var variantCommand = new SqlCommand(variantQuery, connection, transaction))
                            {
                                variantCommand.Parameters.AddWithValue("@ProductId", productId);
                                variantCommand.Parameters.AddWithValue("@ColorId", (object?)variant.ColorId ?? DBNull.Value);
                                variantCommand.Parameters.AddWithValue("@SizeId", (object?)variant.SizeId ?? DBNull.Value);
                                variantCommand.Parameters.AddWithValue("@CategoryId", product.CategoryId); // or variant.CategoryId
                                variantCommand.Parameters.AddWithValue("@Barcode", string.IsNullOrEmpty(variant.Barcode) ? 
                                    DBNull.Value : variant.Barcode);
                                variantCommand.Parameters.AddWithValue("@Quantity", variant.Quantity);
                                variantCommand.Parameters.AddWithValue("@CreatedDate", variant.CreatedDate);
                                variantCommand.Parameters.AddWithValue("@ModifiedDate", variant.ModifiedDate ?? (object)DBNull.Value);
                                variantCommand.Parameters.AddWithValue("@IsActive", variant.IsActive);

                                await variantCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    await transaction.CommitAsync();

                    return productId;
                }
                catch (SqlException)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("An error occurred while adding the product and its variants.");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            string query = @"DELETE FROM [dbo].[Products] WHERE [ProductId] = @id;";

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

        public Task<bool> ExistsByNameAsync(string name, int? excludedId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsBarcodeDuplicateAsync(string barcode, int? excludeId = null)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM [dbo].[Products] AS p
                WHERE p.ProductBarcode = @ProductBarcode AND p.ProductBarcode IS NOT NULL
            ";

            if (excludeId.HasValue)
            {
                query += " AND ProductId != @ProductId;";
            }

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProductBarcode",
                    string.IsNullOrEmpty(barcode) ? DBNull.Value : (object)barcode);

                if (excludeId.HasValue)
                {
                    command.Parameters.AddWithValue("@ProductId", excludeId);
                }

                try
                {
                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();
                    int count = Convert.ToInt32(result);

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

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            string query = @"
                SELECT * FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int productIdIndex = reader.GetOrdinal("ProductId");
                        int nameIndex = reader.GetOrdinal("ProductName");
                        int descriptionIndex = reader.GetOrdinal("ProductDescription");
                        int priceIndex = reader.GetOrdinal("ProductPrice");
                        int discountIndex = reader.GetOrdinal("ProductDiscount");
                        int quantityIndex = reader.GetOrdinal("ProductQuantity");
                        int barcodeIndex = reader.GetOrdinal("ProductBarcode");
                        int categoryIdIndex = reader.GetOrdinal("CategoryId");

                        int categoryNameIndex = reader.GetOrdinal("CategoryName");
                        int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                Id = reader.GetInt32(productIdIndex),
                                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                                Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                                Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                                Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                                Quantity = reader.GetInt32(quantityIndex),
                                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                                CategoryId = reader.GetInt32(categoryIdIndex),  // if CategoryId is NOT nullable,
                                Category = new Category()
                                {
                                    Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                    Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex),
                                }
                            };

                            products.Add(product);
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

            return products;
        }

        public async Task<IEnumerable<ProductVariant>> GetAllVariantsAsync(int productId)
        {
            var productVariants = new List<ProductVariant>();
            string query = @"
                SELECT * FROM [dbo].[ProductVariants] AS pv
                INNER JOIN [dbo].[Products] AS p 
                ON pv.ProductVariantProductId = p.ProductId
                WHERE p.ProductId = @ProductId;
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProductId", productId);

                try
                {
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Variant indexes
                        int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                        int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                        int variantColorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                        int variantSizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                        int variantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                        int variantBarcodeIndex = reader.GetOrdinal("ProductVariantBarcode");
                        int variantIsActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                        while (await reader.ReadAsync())
                        {
                            var variant = new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = reader.GetInt32(variantProductIdIndex),
                                ColorId = reader.IsDBNull(variantColorIdIndex) ? null : reader.GetInt32(variantColorIdIndex),
                                SizeId = reader.IsDBNull(variantSizeIdIndex) ? null : reader.GetInt32(variantSizeIdIndex),
                                Quantity = reader.GetInt32(variantQuantityIndex),
                                Barcode = reader.IsDBNull(variantBarcodeIndex) ? null : reader.GetString(variantBarcodeIndex),
                                IsActive = reader.GetBoolean(variantIsActiveIndex)

                                // Optional: Copy Product-level properties if needed
                                //Name = product.Name,
                                //Description = product.Description,
                                //Price = product.Price,
                                //Discount = product.Discount,
                                //ImagePath = product.ImagePath,
                                //IsNewRelease = product.IsNewRelease,
                                //NewReleaseUntil = product.NewReleaseUntil,
                            };

                            productVariants.Add(variant);
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

            return productVariants;
        }

        public async Task<IEnumerable<Product>> GetAllNewReleasesAsync()
        {
            var products = new Dictionary<int, Product>();
            var productIds = new List<int>();

            string productQuery = @"
                SELECT p.*, c.*
                FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                WHERE p.ProductIsNewRelease = 1;
            ";

            using var connection = DbConnectionFactory.CreateConnection();
            try
            {
                await connection.OpenAsync();

                // 1. Get Products and Categories
                using (var command = new SqlCommand(productQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    int productIdIndex = reader.GetOrdinal("ProductId");
                    int nameIndex = reader.GetOrdinal("ProductName");
                    int descriptionIndex = reader.GetOrdinal("ProductDescription");
                    int priceIndex = reader.GetOrdinal("ProductPrice");
                    int discountIndex = reader.GetOrdinal("ProductDiscount");
                    int quantityIndex = reader.GetOrdinal("ProductQuantity");
                    int barcodeIndex = reader.GetOrdinal("ProductBarcode");
                    int imagePathIndex = reader.GetOrdinal("ProductImagePath");
                    int newReleaseUntilIndex = reader.GetOrdinal("ProductNewReleaseUntil");
                    int categoryIdIndex = reader.GetOrdinal("CategoryId");
                    int categoryNameIndex = reader.GetOrdinal("CategoryName");
                    int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                    while (await reader.ReadAsync())
                    {
                        var product = new Product
                        {
                            Id = reader.GetInt32(productIdIndex),
                            Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                            Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                            Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                            Quantity = reader.GetInt32(quantityIndex),
                            Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                            ImagePath = reader.IsDBNull(imagePathIndex) ? null : reader.GetString(imagePathIndex),
                            NewReleaseUntil = reader.IsDBNull(newReleaseUntilIndex) ? null : reader.GetDateTime(newReleaseUntilIndex),
                            CategoryId = reader.GetInt32(categoryIdIndex),
                            Category = new Category
                            {
                                Id = reader.GetInt32(categoryIdIndex),
                                Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                            },
                            ProductVariants = new List<ProductVariant>()
                        };

                        products[product.Id] = product;
                        productIds.Add(product.Id);
                    }
                }

                if (productIds.Count == 0)
                    return products.Values;

                // 2. Get ProductVariants
                var variantQuery = $@"
                    SELECT pv.ProductVariantId, pv.ProductVariantProductId, 
                           pv.ProductVariantColorId, mc.MdColorName, mc.MdColorHexValue,
                           pv.ProductVariantSizeId, ms.MdSizeCode, pv.ProductVariantCategoryId, 
                           pv.ProductVariantBarcode, pv.ProductVariantQuantity, pv.ProductVariantIsActive
                    FROM [dbo].[ProductVariants] AS pv
                    LEFT JOIN [dbo].[MdColors] AS mc ON pv.ProductVariantColorId = mc.MdColorId
                    LEFT JOIN [dbo].[MdSizes] AS ms ON pv.ProductVariantSizeId = ms.MdSizeId
                    WHERE pv.ProductVariantProductId IN ({string.Join(",", productIds)});
                ";

                using (var variantCommand = new SqlCommand(variantQuery, connection))
                using (var reader = await variantCommand.ExecuteReaderAsync())
                {
                    int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                    int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                    int colorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                    int colorNameIndex = reader.GetOrdinal("MdColorName");
                    int colorHexIndex = reader.GetOrdinal("MdColorHexValue");
                    int sizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                    int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                    int categoryIdIndex = reader.GetOrdinal("ProductVariantCategoryId");
                    int barcodeIndex = reader.GetOrdinal("ProductVariantBarcode");
                    int quantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                    int isActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                    while (await reader.ReadAsync())
                    {
                        var productId = reader.GetInt32(variantProductIdIndex);
                        if (products.TryGetValue(productId, out var product))
                        {
                            product.ProductVariants!.Add(new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = productId,
                                ColorId = reader.IsDBNull(colorIdIndex) ? null : reader.GetInt32(colorIdIndex),
                                ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                                ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                                SizeId = reader.IsDBNull(sizeIdIndex) ? null : reader.GetInt32(sizeIdIndex),
                                SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                                CategoryId = reader.GetInt32(categoryIdIndex),
                                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                                Quantity = reader.GetInt32(quantityIndex),
                                IsActive = !reader.IsDBNull(isActiveIndex) && reader.GetBoolean(isActiveIndex)
                            });
                        }
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

            return products.Values;
        }

        public async Task<IEnumerable<Product>> GetAllHotDealsAsync()
        {
            var products = new Dictionary<int, Product>();
            var productIds = new List<int>();

            string productQuery = @"
                SELECT p.*, c.*
                FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                WHERE p.ProductDiscount > 0;
            ";

            using var connection = DbConnectionFactory.CreateConnection();
            try
            {
                await connection.OpenAsync();

                // 1. Get Products and Categories
                using (var command = new SqlCommand(productQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    int productIdIndex = reader.GetOrdinal("ProductId");
                    int nameIndex = reader.GetOrdinal("ProductName");
                    int descriptionIndex = reader.GetOrdinal("ProductDescription");
                    int priceIndex = reader.GetOrdinal("ProductPrice");
                    int discountIndex = reader.GetOrdinal("ProductDiscount");
                    int quantityIndex = reader.GetOrdinal("ProductQuantity");
                    int barcodeIndex = reader.GetOrdinal("ProductBarcode");
                    int imagePathIndex = reader.GetOrdinal("ProductImagePath");
                    int categoryIdIndex = reader.GetOrdinal("CategoryId");
                    int newReleaseUntilIndex = reader.GetOrdinal("ProductNewReleaseUntil");
                    int categoryNameIndex = reader.GetOrdinal("CategoryName");
                    int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                    while (await reader.ReadAsync())
                    {
                        var product = new Product
                        {
                            Id = reader.GetInt32(productIdIndex),
                            Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                            Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                            Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                            Quantity = reader.GetInt32(quantityIndex),
                            Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                            ImagePath = reader.IsDBNull(imagePathIndex) ? null : reader.GetString(imagePathIndex),
                            NewReleaseUntil = reader.IsDBNull(newReleaseUntilIndex) ? null : reader.GetDateTime(newReleaseUntilIndex),
                            CategoryId = reader.GetInt32(categoryIdIndex),
                            Category = new Category
                            {
                                Id = reader.GetInt32(categoryIdIndex),
                                Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                            },
                            ProductVariants = new List<ProductVariant>()
                        };

                        products[product.Id] = product;
                        productIds.Add(product.Id);
                    }
                }

                if (productIds.Count == 0)
                    return products.Values;

                // 2. Get ProductVariants (combining sizes and colors)
                var variantQuery = $@"
                    SELECT pv.ProductVariantProductId, pv.ProductVariantColorId, mc.MdColorName, mc.MdColorHexValue,
                           pv.ProductVariantSizeId, ms.MdSizeCode, pv.ProductVariantIsActive
                    FROM [dbo].[ProductVariants] AS pv
                    LEFT JOIN [dbo].[MdColors] AS mc ON pv.ProductVariantColorId = mc.MdColorId
                    LEFT JOIN [dbo].[MdSizes] AS ms ON pv.ProductVariantSizeId = ms.MdSizeId
                    WHERE pv.ProductVariantProductId IN ({string.Join(",", productIds)});
                ";

                using (var variantCommand = new SqlCommand(variantQuery, connection))
                using (var reader = await variantCommand.ExecuteReaderAsync())
                {
                    int productIdIndex = reader.GetOrdinal("ProductVariantProductId");
                    int colorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                    int colorNameIndex = reader.GetOrdinal("MdColorName");
                    int colorHexIndex = reader.GetOrdinal("MdColorHexValue");
                    int sizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                    int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                    int isActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                    while (await reader.ReadAsync())
                    {
                        var productId = reader.GetInt32(productIdIndex);
                        if (products.TryGetValue(productId, out var product))
                        {
                            product.ProductVariants!.Add(new ProductVariant
                            {
                                ProductId = productId,
                                ColorId = reader.IsDBNull(colorIdIndex) ? null : reader.GetInt32(colorIdIndex),
                                ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                                ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                                SizeId = reader.IsDBNull(sizeIdIndex) ? null : reader.GetInt32(sizeIdIndex),
                                SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                                IsActive = !reader.IsDBNull(isActiveIndex) && reader.GetBoolean(isActiveIndex)
                            });
                        }
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

            return products.Values;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            string query = @"
                SELECT 
                    p.*, 
                    c.*, 
                    pv.*, 
                    mc.*, 
                    ms.*
                FROM [dbo].[Products] p
                INNER JOIN [dbo].[Categories] c ON p.CategoryId = c.CategoryId
                LEFT JOIN [dbo].[ProductVariants] pv ON pv.ProductVariantProductId = p.ProductId
                LEFT JOIN [dbo].[MdColors] mc ON mc.MdColorId = pv.ProductVariantColorId
                LEFT JOIN [dbo].[MdSizes] ms ON ms.MdSizeId = pv.ProductVariantSizeId
                WHERE p.ProductId = @ProductId;
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProductId", id);

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Product? product = null;

                        // Column indexes for product
                        int productIdIndex = reader.GetOrdinal("ProductId");
                        int categoryIdIndex = reader.GetOrdinal("CategoryId");
                        int productNameIndex = reader.GetOrdinal("ProductName");
                        int productDescriptionIndex = reader.GetOrdinal("ProductDescription");
                        int productBarcodeIndex = reader.GetOrdinal("ProductBarcode");
                        int productPriceIndex = reader.GetOrdinal("ProductPrice");
                        int productDiscountIndex = reader.GetOrdinal("ProductDiscount");
                        int productQuantityIndex = reader.GetOrdinal("ProductQuantity");
                        int productImagePathIndex = reader.GetOrdinal("ProductImagePath");
                        int productIsNewReleaseIndex = reader.GetOrdinal("ProductIsNewRelease");
                        int productNewReleaseUntilIndex = reader.GetOrdinal("productNewReleaseUntil");

                        // Category indexes
                        int categoryNameIndex = reader.GetOrdinal("CategoryName");
                        int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                        // Variant indexes
                        int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                        int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                        int variantColorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                        int variantSizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                        int variantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                        int variantBarcodeIndex = reader.GetOrdinal("ProductVariantBarcode");
                        int variantIsActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                        // Color indexes
                        int colorIdIndex = reader.GetOrdinal("MdColorId");
                        int colorNameIndex = reader.GetOrdinal("MdColorName");
                        int colorHexIndex = reader.GetOrdinal("MdColorHexValue");
                        int colorIsActiveIndex = reader.GetOrdinal("MdColorIsActive");

                        // Size indexes
                        int sizeIdIndex = reader.GetOrdinal("MdSizeId");
                        int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                        int sizeDescriptionIndex = reader.GetOrdinal("MdSizeDescription");
                        int sizeIsActiveIndex = reader.GetOrdinal("MdSizeIsActive");

                        while (await reader.ReadAsync())
                        {
                            if (product == null)
                            {
                                product = new Product
                                {
                                    Id = reader.GetInt32(productIdIndex),
                                    CategoryId = reader.GetInt32(categoryIdIndex),
                                    Name = reader.IsDBNull(productNameIndex) ? null : reader.GetString(productNameIndex),
                                    Description = reader.IsDBNull(productDescriptionIndex) ? null : reader.GetString(productDescriptionIndex),
                                    Barcode = reader.IsDBNull(productBarcodeIndex) ? null : reader.GetString(productBarcodeIndex),
                                    Price = reader.IsDBNull(productPriceIndex) ? null : reader.GetDecimal(productPriceIndex),
                                    Discount = reader.IsDBNull(productDiscountIndex) ? null : reader.GetDecimal(productDiscountIndex),
                                    Quantity = reader.GetInt32(productQuantityIndex),
                                    ImagePath = reader.IsDBNull(productImagePathIndex) ? null : reader.GetString(productImagePathIndex),
                                    IsNewRelease = reader.IsDBNull(productIsNewReleaseIndex) ? null : reader.GetBoolean(productIsNewReleaseIndex),
                                    NewReleaseUntil = reader.IsDBNull(productNewReleaseUntilIndex) ? null : reader.GetDateTime(productNewReleaseUntilIndex),

                                    Category = new Category
                                    {
                                        Id = reader.GetInt32(categoryIdIndex),
                                        Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                        Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                                    },

                                    ProductVariants = new List<ProductVariant>()
                                };
                            }

                            // If ProductVariant exists, add it
                            if (!reader.IsDBNull(variantIdIndex))
                            {
                                int variantId = reader.GetInt32(variantIdIndex);
                                if (!product.ProductVariants.Any(v => v.Id == variantId))
                                {
                                    var variant = new ProductVariant
                                    {
                                        Id = variantId,
                                        ProductId = reader.GetInt32(variantProductIdIndex),
                                        ColorId = reader.IsDBNull(variantColorIdIndex) ? null : reader.GetInt32(variantColorIdIndex),
                                        ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                                        ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                                        SizeId = reader.IsDBNull(variantSizeIdIndex) ? null : reader.GetInt32(variantSizeIdIndex),
                                        SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                                        Quantity = reader.GetInt32(variantQuantityIndex),
                                        Barcode = reader.IsDBNull(variantBarcodeIndex) ? null : reader.GetString(variantBarcodeIndex),
                                        IsActive = reader.GetBoolean(variantIsActiveIndex)

                                        // Optional: Copy Product-level properties if needed
                                        //Name = product.Name,
                                        //Description = product.Description,
                                        //Price = product.Price,
                                        //Discount = product.Discount,
                                        //ImagePath = product.ImagePath,
                                        //IsNewRelease = product.IsNewRelease,
                                        //NewReleaseUntil = product.NewReleaseUntil,
                                    };

                                    product.ProductVariants.Add(variant);
                                }
                            }
                        }

                        return product;
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

        public async Task<ProductVariant?> GetVariantByKeyAsync(string key)
        {
            ProductVariant? productVariant = null;

            string query = @"
                SELECT 
                    ProductVariantId,
                    ProductVariantProductId,
                    ProductVariantColorId,
                    ProductVariantSizeId,
                    ProductVariantQuantity,
                    ProductVariantBarcode,
                    ProductVariantIsActive
                FROM [dbo].[ProductVariants]
                WHERE ProductVariantKey = @Key;
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Key", key);

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Prepare column ordinals
                        int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                        int productIdIndex = reader.GetOrdinal("ProductVariantProductId");
                        int colorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                        int sizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                        int quantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                        int barcodeIndex = reader.GetOrdinal("ProductVariantBarcode");
                        int isActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                        if (await reader.ReadAsync())
                        {
                            productVariant = new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = reader.GetInt32(productIdIndex),
                                ColorId = reader.IsDBNull(colorIdIndex) ? null : reader.GetInt32(colorIdIndex),
                                SizeId = reader.IsDBNull(sizeIdIndex) ? null : reader.GetInt32(sizeIdIndex),
                                Quantity = reader.GetInt32(quantityIndex),
                                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                                IsActive = reader.GetBoolean(isActiveIndex)
                            };
                        }
                    }

                    return productVariant;
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

        public async Task<ProductVariant?> GetVariantByIdAsync(int id)
        {
            ProductVariant? productVariant = null;

            string query = @"
                SELECT 
                    ProductVariantId,
                    ProductVariantProductId,
                    ProductVariantColorId,
                    ProductVariantSizeId,
                    ProductVariantQuantity,
                    ProductVariantBarcode,
                    ProductVariantIsActive
                FROM [dbo].[ProductVariants]
                WHERE ProductVariantId = @Id;
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Prepare column ordinals
                        int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                        int productIdIndex = reader.GetOrdinal("ProductVariantProductId");
                        int colorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                        int sizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                        int quantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                        int barcodeIndex = reader.GetOrdinal("ProductVariantBarcode");
                        int isActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                        if (await reader.ReadAsync())
                        {
                            productVariant = new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = reader.GetInt32(productIdIndex),
                                ColorId = reader.IsDBNull(colorIdIndex) ? null : reader.GetInt32(colorIdIndex),
                                SizeId = reader.IsDBNull(sizeIdIndex) ? null : reader.GetInt32(sizeIdIndex),
                                Quantity = reader.GetInt32(quantityIndex),
                                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                                IsActive = reader.GetBoolean(isActiveIndex)
                            };
                        }
                    }

                    return productVariant;
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

        public async Task<bool> UpdateAsync(Product product)
        {
            string updateProductQuery = @"
                UPDATE [dbo].[Products] 
                SET 
                    ProductName = @ProductName,
                    ProductDescription = @ProductDescription,
                    CategoryId = @CategoryId,
                    ProductPrice = @ProductPrice,
                    ProductDiscount = @ProductDiscount,
                    ProductQuantity = @ProductQuantity,
                    ProductBarcode = @ProductBarcode,
                    ProductImagePath = @ProductImagePath,
                    ProductIsNewRelease = @ProductIsNewRelease,
                    ProductNewReleaseUntil = @ProductNewReleaseUntil
                WHERE ProductId = @ProductId;
    ";

            string deleteVariantsQuery = @"DELETE FROM [dbo].[ProductVariants] WHERE ProductVariantProductId = @ProductId;";

            string insertVariantQuery = @"
                INSERT INTO [dbo].[ProductVariants] (
                    ProductVariantProductId,
                    ProductVariantColorId,
                    ProductVariantSizeId,
                    ProductVariantCategoryId,
                    ProductVariantBarcode,
                    ProductVariantQuantity,
                    ProductVariantIsActive
                )
                VALUES (
                    @ProductId,
                    @ColorId,
                    @SizeId,
                    @CategoryId,
                    @Barcode,
                    @Quantity,
                    @IsActive
                );
            ";

            using (var connection = DbConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

                try
                {
                    // Update product
                    using (var command = new SqlCommand(updateProductQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@ProductId", product.Id);
                        command.Parameters.AddWithValue("@ProductName", product.Name);
                        command.Parameters.AddWithValue("@ProductDescription", product.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                        command.Parameters.AddWithValue("@ProductPrice", product.Price ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProductDiscount", product.Discount ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProductQuantity", product.Quantity);
                        command.Parameters.AddWithValue("@ProductBarcode", string.IsNullOrEmpty(product.Barcode) ? (object)DBNull.Value : product.Barcode);
                        command.Parameters.AddWithValue("@ProductImagePath", product.ImagePath ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ProductIsNewRelease", product.IsNewRelease);
                        command.Parameters.AddWithValue("@ProductNewReleaseUntil", product.NewReleaseUntil ?? (object)DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }

                    // Delete existing variants
                    using (var deleteCommand = new SqlCommand(deleteVariantsQuery, connection, transaction))
                    {
                        deleteCommand.Parameters.AddWithValue("@ProductId", product.Id);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    // Insert updated variants
                    foreach (var variant in product.ProductVariants)
                    {
                        using (var insertCommand = new SqlCommand(insertVariantQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@ProductId", product.Id);
                            insertCommand.Parameters.AddWithValue("@ColorId", variant.ColorId ?? (object)DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@SizeId", variant.SizeId ?? (object)DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@CategoryId", variant.CategoryId);
                            insertCommand.Parameters.AddWithValue("@Barcode", variant.Barcode ?? (object)DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Quantity", variant.Quantity);
                            insertCommand.Parameters.AddWithValue("@IsActive", variant.IsActive);
                            
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (SqlException)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("An error occurred while updating the product.");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, int? count = null)
        {
            var products = new Dictionary<int, Product>();
            var productIds = new List<int>();

            string productsQuery = count.HasValue
                ? @"SELECT TOP (@Count) p.*, c.*
                    FROM [dbo].[Products] AS p
                    INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                    WHERE p.CategoryId = @CategoryId
                    ORDER BY p.ProductName DESC;
                "
                : @"SELECT p.*, c.*
                    FROM [dbo].[Products] AS p
                    INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                    WHERE p.CategoryId = @CategoryId
                    ORDER BY p.ProductName DESC;
                ";

            using var connection = DbConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            // 1. Get Products and Categories
            using (var command = new SqlCommand(productsQuery, connection))
            {
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                if (count.HasValue)
                    command.Parameters.AddWithValue("@Count", count.Value);

                using var reader = await command.ExecuteReaderAsync();

                // Ordinals: Product
                int productIdIndex = reader.GetOrdinal("ProductId");
                int productNameIndex = reader.GetOrdinal("ProductName");
                int productDescriptionIndex = reader.GetOrdinal("ProductDescription");
                int productBarcodeIndex = reader.GetOrdinal("ProductBarcode");
                int productPriceIndex = reader.GetOrdinal("ProductPrice");
                int productDiscountIndex = reader.GetOrdinal("ProductDiscount");
                int productQuantityIndex = reader.GetOrdinal("ProductQuantity");
                int productImagePathIndex = reader.GetOrdinal("ProductImagePath");
                int productIsNewReleaseIndex = reader.GetOrdinal("ProductIsNewRelease");
                int productNewReleaseUntilIndex = reader.GetOrdinal("ProductNewReleaseUntil");
                int productCategoryIdIndex = reader.GetOrdinal("CategoryId");

                // Ordinals: Category
                int categoryNameIndex = reader.GetOrdinal("CategoryName");
                int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                while (await reader.ReadAsync())
                {
                    var productId = reader.GetInt32(productIdIndex);

                    var product = new Product
                    {
                        Id = productId,
                        Name = reader.IsDBNull(productNameIndex) ? null : reader.GetString(productNameIndex),
                        Description = reader.IsDBNull(productDescriptionIndex) ? null : reader.GetString(productDescriptionIndex),
                        Barcode = reader.IsDBNull(productBarcodeIndex) ? null : reader.GetString(productBarcodeIndex),
                        Price = reader.IsDBNull(productPriceIndex) ? null : reader.GetDecimal(productPriceIndex),
                        Discount = reader.IsDBNull(productDiscountIndex) ? null : reader.GetDecimal(productDiscountIndex),
                        Quantity = reader.GetInt32(productQuantityIndex),
                        ImagePath = reader.IsDBNull(productImagePathIndex) ? null : reader.GetString(productImagePathIndex),
                        IsNewRelease = reader.IsDBNull(productIsNewReleaseIndex) ? null : reader.GetBoolean(productIsNewReleaseIndex),
                        NewReleaseUntil = reader.IsDBNull(productNewReleaseUntilIndex) ? null : reader.GetDateTime(productNewReleaseUntilIndex),
                        CategoryId = reader.GetInt32(productCategoryIdIndex),
                        Category = new Category
                        {
                            Id = reader.GetInt32(productCategoryIdIndex),
                            Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                            Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                        },
                        ProductVariants = new List<ProductVariant>()
                    };

                    products[productId] = product;
                    productIds.Add(productId);
                }
                reader.Close();
            }

            if (productIds.Count == 0)
                return products.Values;

            // 2. Get ProductVariants (+ MdColor, MdSize)
            var variantQuery = $@"
                SELECT 
                    pv.*, 
                    mc.MdColorName, mc.MdColorHexValue,
                    ms.MdSizeCode, ms.MdSizeDescription
                FROM [dbo].[ProductVariants] AS pv
                LEFT JOIN [dbo].[MdColors] AS mc ON mc.MdColorId = pv.ProductVariantColorId
                LEFT JOIN [dbo].[MdSizes] AS ms ON ms.MdSizeId = pv.ProductVariantSizeId
                WHERE pv.ProductVariantProductId IN ({string.Join(",", productIds)});
            ";

            using (var command = new SqlCommand(variantQuery, connection))
            {
                using var reader = await command.ExecuteReaderAsync();

                // Ordinals: ProductVariant
                int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                int variantColorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                int variantSizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                int variantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                int variantIsActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                // Ordinals: MdColor
                int colorNameIndex = reader.GetOrdinal("MdColorName");
                int colorHexIndex = reader.GetOrdinal("MdColorHexValue");

                // Ordinals: MdSize
                int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                int sizeDescriptionIndex = reader.GetOrdinal("MdSizeDescription");

                while (await reader.ReadAsync())
                {
                    var productId = reader.GetInt32(variantProductIdIndex);

                    if (products.TryGetValue(productId, out var product))
                    {
                        var variant = new ProductVariant
                        {
                            Id = reader.GetInt32(variantIdIndex),
                            ProductId = productId,
                            ColorId = reader.IsDBNull(variantColorIdIndex) ? null : reader.GetInt32(variantColorIdIndex),
                            SizeId = reader.IsDBNull(variantSizeIdIndex) ? null : reader.GetInt32(variantSizeIdIndex),
                            Quantity = reader.GetInt32(variantQuantityIndex),
                            IsActive = reader.GetBoolean(variantIsActiveIndex),
                            ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                            ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                            SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                        };

                        product.ProductVariants.Add(variant);
                    }
                }

                reader.Close();
            }

            return products.Values;
        }

        public async Task<IEnumerable<Product>> SearchByNameOrDescAsync(string searchQuery)
        {
            var products = new Dictionary<int, Product>();
            var productIds = new List<int>();

            string productQuery = @"
                SELECT p.*, c.*
                FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                WHERE p.ProductName LIKE @searchQuery OR p.ProductDescription LIKE @searchQuery;
            ";

            using var connection = DbConnectionFactory.CreateConnection();
            try
            {
                await connection.OpenAsync();

                // 1. Get Products and Categories
                using (var command = new SqlCommand(productQuery, connection))
                {
                    command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int productIdIndex = reader.GetOrdinal("ProductId");
                        int nameIndex = reader.GetOrdinal("ProductName");
                        int descriptionIndex = reader.GetOrdinal("ProductDescription");
                        int priceIndex = reader.GetOrdinal("ProductPrice");
                        int discountIndex = reader.GetOrdinal("ProductDiscount");
                        int quantityIndex = reader.GetOrdinal("ProductQuantity");
                        int barcodeIndex = reader.GetOrdinal("ProductBarcode");
                        int imagePathIndex = reader.GetOrdinal("ProductImagePath");
                        int categoryIdIndex = reader.GetOrdinal("CategoryId");
                        int newReleaseUntilIndex = reader.GetOrdinal("ProductNewReleaseUntil");
                        int categoryNameIndex = reader.GetOrdinal("CategoryName");
                        int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                        while (await reader.ReadAsync())
                        {
                            var product = new Product
                            {
                                Id = reader.GetInt32(productIdIndex),
                                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                                Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                                Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                                Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                                Quantity = reader.GetInt32(quantityIndex),
                                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                                ImagePath = reader.IsDBNull(imagePathIndex) ? null : reader.GetString(imagePathIndex),
                                NewReleaseUntil = reader.IsDBNull(newReleaseUntilIndex) ? null : reader.GetDateTime(newReleaseUntilIndex),
                                CategoryId = reader.GetInt32(categoryIdIndex),
                                Category = new Category
                                {
                                    Id = reader.GetInt32(categoryIdIndex),
                                    Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                    Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                                },
                                ProductVariants = new List<ProductVariant>()
                            };

                            products[product.Id] = product;
                            productIds.Add(product.Id);
                        }
                    }
                }

                if (productIds.Count == 0)
                    return products.Values;

                // 2. Get ProductVariants (+ MdColor, MdSize)
                var variantQuery = $@"
                    SELECT 
                        pv.*, 
                        mc.MdColorName, mc.MdColorHexValue,
                        ms.MdSizeCode, ms.MdSizeDescription
                    FROM [dbo].[ProductVariants] AS pv
                    LEFT JOIN [dbo].[MdColors] AS mc ON mc.MdColorId = pv.ProductVariantColorId
                    LEFT JOIN [dbo].[MdSizes] AS ms ON ms.MdSizeId = pv.ProductVariantSizeId
                    WHERE pv.ProductVariantProductId IN ({string.Join(",", productIds)});
                ";

                using (var command = new SqlCommand(variantQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Ordinals: ProductVariant
                    int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                    int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                    int variantColorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                    int variantSizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                    int variantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                    int variantIsActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                    // Ordinals: MdColor
                    int colorNameIndex = reader.GetOrdinal("MdColorName");
                    int colorHexIndex = reader.GetOrdinal("MdColorHexValue");

                    // Ordinals: MdSize
                    int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                    int sizeDescriptionIndex = reader.GetOrdinal("MdSizeDescription");

                    while (await reader.ReadAsync())
                    {
                        var productId = reader.GetInt32(variantProductIdIndex);

                        if (products.TryGetValue(productId, out var product))
                        {
                            var variant = new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = productId,
                                ColorId = reader.IsDBNull(variantColorIdIndex) ? null : reader.GetInt32(variantColorIdIndex),
                                SizeId = reader.IsDBNull(variantSizeIdIndex) ? null : reader.GetInt32(variantSizeIdIndex),
                                Quantity = reader.GetInt32(variantQuantityIndex),
                                IsActive = reader.GetBoolean(variantIsActiveIndex),
                                ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                                ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                                SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                            };

                            product.ProductVariants.Add(variant);
                        }
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

            return products.Values;
        }

        public async Task<IEnumerable<Product>> SearchByNameDescAndCategoryAsync(string searchQuery, int categoryId)
        {
            var products = new Dictionary<int, Product>();
            var productIds = new List<int>();

            string productQuery = @"
                SELECT p.*, c.*
                FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId
                WHERE (p.ProductName LIKE @searchQuery OR p.ProductDescription LIKE @searchQuery)
                  AND p.CategoryId = @categoryId;
            ";

            using var connection = DbConnectionFactory.CreateConnection();
            try
            {
                await connection.OpenAsync();

                // 1. Get Products and Categories
                using (var command = new SqlCommand(productQuery, connection))
                {
                    command.Parameters.AddWithValue("@searchQuery", $"%{searchQuery}%");
                    command.Parameters.AddWithValue("@categoryId", categoryId);

                    using var reader = await command.ExecuteReaderAsync();

                    int productIdIndex = reader.GetOrdinal("ProductId");
                    int nameIndex = reader.GetOrdinal("ProductName");
                    int descriptionIndex = reader.GetOrdinal("ProductDescription");
                    int priceIndex = reader.GetOrdinal("ProductPrice");
                    int discountIndex = reader.GetOrdinal("ProductDiscount");
                    int quantityIndex = reader.GetOrdinal("ProductQuantity");
                    int barcodeIndex = reader.GetOrdinal("ProductBarcode");
                    int imagePathIndex = reader.GetOrdinal("ProductImagePath");
                    int categoryIdIndex = reader.GetOrdinal("CategoryId");
                    int newReleaseUntilIndex = reader.GetOrdinal("ProductNewReleaseUntil");
                    int categoryNameIndex = reader.GetOrdinal("CategoryName");
                    int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

                    while (await reader.ReadAsync())
                    {
                        var product = new Product
                        {
                            Id = reader.GetInt32(productIdIndex),
                            Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
                            Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                            Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                            Quantity = reader.GetInt32(quantityIndex),
                            Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                            ImagePath = reader.IsDBNull(imagePathIndex) ? null : reader.GetString(imagePathIndex),
                            NewReleaseUntil = reader.IsDBNull(newReleaseUntilIndex) ? null : reader.GetDateTime(newReleaseUntilIndex),
                            CategoryId = reader.GetInt32(categoryIdIndex),
                            Category = new Category
                            {
                                Id = reader.GetInt32(categoryIdIndex),
                                Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                                Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex)
                            },
                            ProductVariants = new List<ProductVariant>()
                        };

                        products[product.Id] = product;
                        productIds.Add(product.Id);
                    }
                }

                if (productIds.Count == 0)
                    return products.Values;

                // 2. Get ProductVariants (+ MdColor, MdSize)
                var variantQuery = $@"
                    SELECT 
                        pv.*, 
                        mc.MdColorName, mc.MdColorHexValue,
                        ms.MdSizeCode, ms.MdSizeDescription
                    FROM [dbo].[ProductVariants] AS pv
                    LEFT JOIN [dbo].[MdColors] AS mc ON mc.MdColorId = pv.ProductVariantColorId
                    LEFT JOIN [dbo].[MdSizes] AS ms ON ms.MdSizeId = pv.ProductVariantSizeId
                    WHERE pv.ProductVariantProductId IN ({string.Join(",", productIds)});
                ";

                using (var variantCommand = new SqlCommand(variantQuery, connection))
                using (var reader = await variantCommand.ExecuteReaderAsync())
                {
                    int variantIdIndex = reader.GetOrdinal("ProductVariantId");
                    int variantProductIdIndex = reader.GetOrdinal("ProductVariantProductId");
                    int variantColorIdIndex = reader.GetOrdinal("ProductVariantColorId");
                    int variantSizeIdIndex = reader.GetOrdinal("ProductVariantSizeId");
                    int variantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                    int variantIsActiveIndex = reader.GetOrdinal("ProductVariantIsActive");

                    int colorNameIndex = reader.GetOrdinal("MdColorName");
                    int colorHexIndex = reader.GetOrdinal("MdColorHexValue");

                    int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");
                    int sizeDescriptionIndex = reader.GetOrdinal("MdSizeDescription");

                    while (await reader.ReadAsync())
                    {
                        var productId = reader.GetInt32(variantProductIdIndex);

                        if (products.TryGetValue(productId, out var product))
                        {
                            var variant = new ProductVariant
                            {
                                Id = reader.GetInt32(variantIdIndex),
                                ProductId = productId,
                                ColorId = reader.IsDBNull(variantColorIdIndex) ? null : reader.GetInt32(variantColorIdIndex),
                                SizeId = reader.IsDBNull(variantSizeIdIndex) ? null : reader.GetInt32(variantSizeIdIndex),
                                Quantity = reader.GetInt32(variantQuantityIndex),
                                IsActive = reader.GetBoolean(variantIsActiveIndex),
                                ColorName = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                                ColorHex = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                                SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex)
                            };

                            product.ProductVariants!.Add(variant);
                        }
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

            return products.Values;
        }

        // Filter products
        public async Task<(IEnumerable<Product> products, int totalCount)> GetFilteredProductsAsync(
            int start,
            int length,
            string searchValue,
            string sortColumn,
            string sortDirection)
        {
            var products = new List<Product>();
            int totalCount = 0;

            string baseQuery = @"
                SELECT p.*, c.*
                FROM [dbo].[Products] AS p
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId";

            // Add search filtering
            string whereClause = string.Empty;
            if (!string.IsNullOrEmpty(searchValue))
            {
                whereClause = @"
                    WHERE p.ProductName LIKE @searchValue 
                    OR c.CategoryName LIKE @searchValue 
                    OR p.ProductPrice LIKE @searchValue
                    OR p.ProductDiscount LIKE @searchValue
                    OR p.ProductQuantity LIKE @searchValue
                    OR p.ProductBarcode LIKE @searchValue";
            }
          
            string countQuery = $@"
                SELECT COUNT(*) FROM [dbo].[Products]  AS p 
                INNER JOIN [dbo].[Categories] AS c ON p.CategoryId = c.CategoryId {whereClause}";

            // Add sorting
            string orderByClause = string.Empty;
            if (!string.IsNullOrEmpty(sortColumn))
            {
                string columnName = sortColumn switch
                {
                    "name" => "p.ProductName",
                    "categoryName" => "c.CategoryName",
                    "price" => "p.ProductPrice",
                    "discount" => "p.ProductDiscount",
                    "quantity" => "p.ProductQuantity",
                    _ => "p.ProductId"
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
                        // Read products
                        while (await reader.ReadAsync())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
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
                    throw;
                }
            }

            return (products, totalCount);
        }

        private static Product MapProductFromReader(SqlDataReader reader)
        {
            // Get ordinal indexes first
            int productIdIndex = reader.GetOrdinal("ProductId");
            int nameIndex = reader.GetOrdinal("ProductName");
            int priceIndex = reader.GetOrdinal("ProductPrice");
            int discountIndex = reader.GetOrdinal("ProductDiscount");
            int quantityIndex = reader.GetOrdinal("ProductQuantity");
            int barcodeIndex = reader.GetOrdinal("ProductBarcode");
            int categoryIdIndex = reader.GetOrdinal("CategoryId");

            int categoryNameIndex = reader.GetOrdinal("CategoryName");
            int categoryDescriptionIndex = reader.GetOrdinal("CategoryDescription");

            return new Product
            {
                Id = reader.GetInt32(productIdIndex),
                Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
                Price = reader.IsDBNull(priceIndex) ? null : reader.GetDecimal(priceIndex),
                Discount = reader.IsDBNull(discountIndex) ? null : reader.GetDecimal(discountIndex),
                Quantity = reader.GetInt32(quantityIndex),
                Barcode = reader.IsDBNull(barcodeIndex) ? null : reader.GetString(barcodeIndex),
                CategoryId = reader.GetInt32(categoryIdIndex),
                Category = new Category()
                {
                    Name = reader.IsDBNull(categoryNameIndex) ? null : reader.GetString(categoryNameIndex),
                    Description = reader.IsDBNull(categoryDescriptionIndex) ? null : reader.GetString(categoryDescriptionIndex),
                }
            };
        }
    }
}
