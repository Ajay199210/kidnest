using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Products;
using KidNest.Services.Interfaces;

namespace KidNest.Services.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IProductsRepository _productsRepo;

        public ProductsService(IProductsRepository productsRepo)
        {
            _productsRepo = productsRepo;
        }

        public async Task<OperationResult> CreateProductAsync(ProductCreateDTO productCreateDTO)
        {
            try
            {
                bool isBarcodeDuplicate = await _productsRepo.IsBarcodeDuplicateAsync(productCreateDTO.Barcode!);

                if (isBarcodeDuplicate)
                {
                    return OperationResult.Fail("Another product has this bardcode. Please choose another one.");
                }

                // Check other validations as well (e.g. barcode, etc.)

                var productToAdd = new Product
                {
                    Name = productCreateDTO.Name,
                    Description = productCreateDTO.Description,
                    Barcode = productCreateDTO.Barcode,
                    CategoryId = productCreateDTO.CategoryId,
                    Price = productCreateDTO.Price,
                    Discount = productCreateDTO.Discount,
                    Quantity = productCreateDTO.Quantity,
                    ImagePath = productCreateDTO.ImagePath,
                    //ProductColors = productCreateDTO.SelectedColorIds.Select(i => new MdColor { Id = i }).ToList(),
                    //ProductSizes = productCreateDTO.SelectedSizeIds.Select(i => new MdSize { Id = i }).ToList(),
                    IsNewRelease = productCreateDTO.IsNewRelease,
                    CreatedDate = DateTime.Now
                };

                // Check new release date
                if (productCreateDTO.IsNewRelease)
                {
                    productToAdd.NewReleaseUntil = DateTime.Now.AddDays(30);
                }

                // Check product variants (color/sizes)
                if (productCreateDTO.VariantCreateDTOs.Count > 0)
                {
                    productToAdd.ProductVariants = productCreateDTO.VariantCreateDTOs
                    .Select(v => new ProductVariant
                    {
                        ColorId = v.ColorId,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity,
                        CategoryId = v.CategoryId,
                        Barcode = $"{productCreateDTO.Barcode}-{v.ColorId}{v.SizeId}", // custom format
                        CreatedDate = DateTime.Now,
                        ModifiedDate = null,
                        IsActive = true,
                    }).ToList();
                }

                // Add product
                int rowsAffected = await _productsRepo.AddAsync(productToAdd);

                if (rowsAffected == 0)
                    return OperationResult.Fail("Failed to create the product. No rows affected.");

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Here you could log the exception
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while creating the product : {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteProductAsync(int productId)
        {
            try
            {
                // Add business logic (e.g., check if product is related to an order)
                bool isDeleted = await _productsRepo.DeleteAsync(productId);

                if (isDeleted)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Product deletion failed. The product may not exist.");

            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while deleting the product: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productsRepo.GetAllAsync();

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name,
                Description = p.Description ?? "N/A",
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
                ImagePath = p.ImagePath
            }).ToList();
        }

        public async Task<IEnumerable<ProductVariantDTO>> GetAllProductVariantsAsync(int productId)
        {
            var productVariants = await _productsRepo.GetAllVariantsAsync(productId);

            return productVariants.Select(p => new ProductVariantDTO
            {
                Id = p.Id,
                ProductId = p.ProductId,
                CategoryId = p.CategoryId,
                ColorId = p.ColorId,
                ColorName = p.ColorName,
                ColorHex = p.ColorHex,
                SizeId = p.SizeId,
                SizeCode = p.SizeCode,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
            }).ToList();
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int productId)
        {
            // Fetch domain model from repository
            var product = await _productsRepo.GetByIdAsync(productId);

            if (product == null)
                return null;  // Or throw NotFoundException

            // Map domain model to ViewModel
            return new ProductDTO
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discount,
                Quantity = product.Quantity,
                CategoryName = product.Category?.Name,
                ImagePath = product.ImagePath,
                Barcode = product.Barcode,
                IsNewRelease = product.IsNewRelease,
                NewReleaseUntil = product.NewReleaseUntil,
                VariantDTOs = product.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    CategoryId = v.CategoryId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Barcode = v.Barcode,
                    Quantity = v.Quantity
                }).ToList()
            };
        }

        public async Task<ProductVariantDTO?> GetProductVariantByKeyAsync(string key)
        {
            var productVariant = await _productsRepo.GetVariantByKeyAsync(key);

            if (productVariant == null)
                return null;  // Or throw NotFoundException

            // Map domain model to DTO
            return new ProductVariantDTO
            {
                Id = productVariant.Id,
                ProductId = productVariant.ProductId,
                CategoryId = productVariant.CategoryId,
                ColorId = productVariant.ColorId,
                ColorName = productVariant.ColorName,
                ColorHex = productVariant.ColorHex,
                SizeId = productVariant.SizeId,
                SizeCode = productVariant.SizeCode,
                Quantity = productVariant.Quantity,
                Barcode = productVariant.Barcode
            };
        }

        public async Task<OperationResult> UpdateProductAsync(ProductDTO productDTO)
        {
            try
            {
                // Possible validations checks here (e.g. name, barcode..)
                bool isBarcodeDuplicate = await _productsRepo
                    .IsBarcodeDuplicateAsync(productDTO.Barcode!, productDTO.Id);

                if (isBarcodeDuplicate)
                {
                    return OperationResult.Fail("Another product has this bardcode. Please choose another one.");
                }

                var productToEdit = new Product
                {
                    Id = productDTO.Id,
                    CategoryId = productDTO.CategoryId,
                    Name = productDTO.Name,
                    Description = productDTO.Description,
                    Price = productDTO.Price,
                    Discount = productDTO.Discount,
                    Quantity = productDTO.Quantity,
                    Barcode = productDTO.Barcode,
                    ImagePath = productDTO.ImagePath,
                    //ProductColors = productDTO.SelectedColorIds.Select(i => new MdColor { Id = i }).ToList(),
                    //ProductSizes = productDTO.SelectedSizeIds.Select(i => new MdSize { Id = i }).ToList(),
                    IsNewRelease = productDTO.IsNewRelease,
                    NewReleaseUntil = productDTO.NewReleaseUntil
                };

                // Check new release date
                if (!Convert.ToBoolean(productDTO.IsNewRelease))
                {
                    productToEdit.NewReleaseUntil = null;
                }
                else
                {
                    if (productToEdit.NewReleaseUntil == null)
                    {
                        productToEdit.NewReleaseUntil = DateTime.Now.AddDays(30);
                    }
                }

                // Check product variants (color/sizes)
                if (productDTO.VariantDTOs.Count > 0)
                {
                    productToEdit.ProductVariants = productDTO.VariantDTOs
                    .Select(v => new ProductVariant
                    {
                        ColorId = v.ColorId,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity,
                        CategoryId = v.CategoryId,
                        //Barcode = $"{productCreateDTO.Barcode}{sizeCode}{colorCode}", // custom format
                        //CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        IsActive = true,
                    }).ToList();
                }

                bool isProductUpdated = await _productsRepo.UpdateAsync(productToEdit);

                if (isProductUpdated)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Category update failed. " +
                    "The category might not exist or no changes were detected.");
            }
            catch
            {
                // Log the exception here (Serilog, NLog, built-in logger, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while updating the product.");
            }
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryIdAsync(int categoryId, int? count = null)
        {
            var products = await _productsRepo.GetByCategoryIdAsync(categoryId, count);

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                ImagePath = p.ImagePath,
                VariantDTOs = p.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    CategoryId = v.CategoryId,
                    ProductId = v.ProductId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsByNameOrDescAsync(string searchQuery)
        {
            var products = await _productsRepo.SearchByNameOrDescAsync(searchQuery);

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
                ImagePath = p.ImagePath,
                VariantDTOs = p.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    CategoryId = v.CategoryId,
                    ProductId = v.ProductId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity,
                }).ToList()
            }).ToList();
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsByCategoryAndQueryAsync(int categoryId, string query)
        {
            var products = await _productsRepo.SearchByNameDescAndCategoryAsync(query, categoryId);

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
                ImagePath = p.ImagePath,
                VariantDTOs = p.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    CategoryId = v.CategoryId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsNewReleasesAsync()
        {
            var products = await _productsRepo.GetAllNewReleasesAsync();

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
                ImagePath = p.ImagePath,
                VariantDTOs = p.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    CategoryId = v.CategoryId,
                    ProductId = v.ProductId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity,
                }).ToList()
            }).ToList();
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsHotDealsAsync()
        {
            var products = await _productsRepo.GetAllHotDealsAsync();

            return products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name,
                Description = p.Description ?? "N/A",
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode,
                ImagePath = p.ImagePath,
                VariantDTOs = p.ProductVariants.Select(v => new ProductVariantDTO
                {
                    Id = v.Id,
                    CategoryId = v.CategoryId,
                    ProductId = v.ProductId,
                    ColorId = v.ColorId,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity,
                }).ToList()
            }).ToList();
        }

        // Filtering, Sorting & Pagination
        public async Task<DataTableResponse<ProductDTO>> GetPaginatedProductsAsync(DataTableRequest request)
        {
            // Get sorting info from first column
            var sortColumn = request.Order.FirstOrDefault();
            var columnName = sortColumn != null ? request.Columns[sortColumn.Column].Data : "Id";

            var (products, totalCount) = await _productsRepo.GetFilteredProductsAsync(
                request.Start,
                request.Length,
                request.Search.Value,
                columnName,
                sortColumn?.Dir ?? "asc");

            var productsDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category?.Name ?? "N/A",
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                Quantity = p.Quantity,
                Barcode = p.Barcode
            }).ToList();

            return new DataTableResponse<ProductDTO>
            {
                Items = productsDTOs,
                TotalCount = totalCount
            };
        }
    }
}
