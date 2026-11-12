using Microsoft.EntityFrameworkCore;

namespace Warehouse_23DH111747.Models
{
    public class WarehouseEntities : DbContext
    {
        public static string DefaultConnectionString { get; set; } =
            "Server=.\\TOILAKHUONG;Database=Warehouse_23DH111747;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";

        public WarehouseEntities()
        {
        }

        public WarehouseEntities(DbContextOptions<WarehouseEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<CategoryItem> Categories { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(DefaultConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductID);
                entity.Property(e => e.ProductID).ValueGeneratedOnAdd();
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Quantity).IsRequired();
            });

            modelBuilder.Entity<CategoryItem>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.CategoryID);
                entity.Property(e => e.CategoryID).ValueGeneratedOnAdd();
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            });
        }

        // ==================== PRODUCTS ====================
        public IQueryable<Product> QueryProducts()
        {
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                return Products;
            }
            return WarehouseDatabaseSeeder.MemoryProducts.AsQueryable();
        }

        public Product? FindProduct(int id)
        {
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                return Products.FirstOrDefault(p => p.ProductID == id);
            }
            return WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == id);
        }

        public bool UpdateProductInfo(int productId, string productName, string category, decimal price)
        {
            EnsureCategoryExists(category);

            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                var existing = Products.FirstOrDefault(p => p.ProductID == productId);
                if (existing == null) return false;

                existing.ProductName = productName;
                existing.Category = category;
                existing.Price = price;
                SaveChanges();
                return true;
            }

            var memExisting = WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == productId);
            if (memExisting == null) return false;
            memExisting.ProductName = productName;
            memExisting.Category = category;
            memExisting.Price = price;
            return true;
        }

        public bool DeleteProduct(int productId)
        {
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                var existing = Products.FirstOrDefault(p => p.ProductID == productId);
                if (existing == null) return false;

                Products.Remove(existing);
                SaveChanges();
                return true;
            }

            var memExisting = WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == productId);
            if (memExisting == null) return false;
            WarehouseDatabaseSeeder.MemoryProducts.Remove(memExisting);
            return true;
        }

        public bool UpdateProductQuantity(int productId, int actualQuantity, out int oldQuantity, out Product? updatedProduct)
        {
            oldQuantity = 0;
            updatedProduct = null;

            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                var existing = Products.FirstOrDefault(p => p.ProductID == productId);
                if (existing == null) return false;

                oldQuantity = existing.Quantity;
                existing.Quantity = actualQuantity;
                SaveChanges();
                updatedProduct = existing;
                return true;
            }

            var memExisting = WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == productId);
            if (memExisting == null) return false;

            oldQuantity = memExisting.Quantity;
            memExisting.Quantity = actualQuantity;
            updatedProduct = memExisting;
            return true;
        }

        public bool UpdateDualProductQuantitiesInTransaction(
            int productId1, int actualQuantity1,
            int productId2, int actualQuantity2,
            out int oldQty1, out int oldQty2,
            out Product? prod1, out Product? prod2,
            out string errorMessage)
        {
            oldQty1 = 0;
            oldQty2 = 0;
            prod1 = null;
            prod2 = null;
            errorMessage = string.Empty;

            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                using var transaction = Database.BeginTransaction();
                try
                {
                    var p1 = Products.FirstOrDefault(p => p.ProductID == productId1);
                    var p2 = Products.FirstOrDefault(p => p.ProductID == productId2);

                    if (p1 == null || p2 == null)
                    {
                        transaction.Rollback();
                        errorMessage = "Mã sản phẩm không tồn tại.";
                        return false;
                    }

                    oldQty1 = p1.Quantity;
                    oldQty2 = p2.Quantity;

                    p1.Quantity = actualQuantity1;
                    p2.Quantity = actualQuantity2;

                    SaveChanges();
                    transaction.Commit();

                    prod1 = p1;
                    prod2 = p2;
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    errorMessage = "Lỗi cập nhật: " + ex.Message;
                    return false;
                }
            }
            else
            {
                var p1 = WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == productId1);
                var p2 = WarehouseDatabaseSeeder.MemoryProducts.FirstOrDefault(p => p.ProductID == productId2);

                if (p1 == null || p2 == null)
                {
                    errorMessage = "Mã sản phẩm không tồn tại.";
                    return false;
                }

                oldQty1 = p1.Quantity;
                oldQty2 = p2.Quantity;

                p1.Quantity = actualQuantity1;
                p2.Quantity = actualQuantity2;

                prod1 = p1;
                prod2 = p2;
                return true;
            }
        }

        public Product AddNewProduct(string productName, string category, decimal price, int quantity)
        {
            EnsureCategoryExists(category);

            var newProduct = new Product
            {
                ProductName = productName,
                Category = category,
                Price = price,
                Quantity = quantity
            };

            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                Products.Add(newProduct);
                SaveChanges();
                return newProduct;
            }

            int nextId = WarehouseDatabaseSeeder.MemoryProducts.Any()
                ? WarehouseDatabaseSeeder.MemoryProducts.Max(p => p.ProductID) + 1
                : 1;
            newProduct.ProductID = nextId;
            WarehouseDatabaseSeeder.MemoryProducts.Add(newProduct);
            return newProduct;
        }

        // ==================== CATEGORIES ====================
        public List<CategoryItem> GetCategoriesList()
        {
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    return Categories.OrderBy(c => c.CategoryID).ToList();
                }
                catch
                {
                    return WarehouseDatabaseSeeder.MemoryCategories.OrderBy(c => c.CategoryID).ToList();
                }
            }
            return WarehouseDatabaseSeeder.MemoryCategories.OrderBy(c => c.CategoryID).ToList();
        }

        public CategoryItem? FindCategory(int id)
        {
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    return Categories.FirstOrDefault(c => c.CategoryID == id);
                }
                catch
                {
                    return WarehouseDatabaseSeeder.MemoryCategories.FirstOrDefault(c => c.CategoryID == id);
                }
            }
            return WarehouseDatabaseSeeder.MemoryCategories.FirstOrDefault(c => c.CategoryID == id);
        }

        public void EnsureCategoryExists(string categoryName)
        {
            string trimmed = (categoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return;

            string lower = trimmed.ToLower();
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    bool exists = Categories.AsEnumerable().Any(c => c.CategoryName.Trim().ToLower() == lower);
                    if (!exists)
                    {
                        Categories.Add(new CategoryItem { CategoryName = trimmed });
                        SaveChanges();
                    }
                    return;
                }
                catch
                {
                    // Fallback memory
                }
            }

            if (!WarehouseDatabaseSeeder.MemoryCategories.Any(c => c.CategoryName.Trim().ToLower() == lower))
            {
                int nextId = WarehouseDatabaseSeeder.MemoryCategories.Any()
                    ? WarehouseDatabaseSeeder.MemoryCategories.Max(c => c.CategoryID) + 1
                    : 1;
                WarehouseDatabaseSeeder.MemoryCategories.Add(new CategoryItem
                {
                    CategoryID = nextId,
                    CategoryName = trimmed
                });
            }
        }

        public CategoryItem AddCategory(string categoryName)
        {
            string trimmed = categoryName.Trim();
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    var item = new CategoryItem { CategoryName = trimmed };
                    Categories.Add(item);
                    SaveChanges();
                    return item;
                }
                catch
                {
                    // Fallback memory
                }
            }

            int nextId = WarehouseDatabaseSeeder.MemoryCategories.Any()
                ? WarehouseDatabaseSeeder.MemoryCategories.Max(c => c.CategoryID) + 1
                : 1;
            var memItem = new CategoryItem { CategoryID = nextId, CategoryName = trimmed };
            WarehouseDatabaseSeeder.MemoryCategories.Add(memItem);
            return memItem;
        }

        public bool UpdateCategory(int id, string newCategoryName)
        {
            string trimmed = newCategoryName.Trim();
            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    var existing = Categories.FirstOrDefault(c => c.CategoryID == id);
                    if (existing == null) return false;

                    string oldName = existing.CategoryName.Trim().ToLower();
                    existing.CategoryName = trimmed;

                    var matchingProducts = Products
                        .AsEnumerable()
                        .Where(p => p.Category.Trim().ToLower() == oldName)
                        .ToList();

                    foreach (var p in matchingProducts)
                    {
                        p.Category = trimmed;
                    }

                    SaveChanges();
                    return true;
                }
                catch
                {
                    // Fallback memory
                }
            }

            var memCat = WarehouseDatabaseSeeder.MemoryCategories.FirstOrDefault(c => c.CategoryID == id);
            if (memCat == null) return false;

            string oldMemName = memCat.CategoryName.Trim().ToLower();
            memCat.CategoryName = trimmed;

            foreach (var p in WarehouseDatabaseSeeder.MemoryProducts.Where(p => p.Category.Trim().ToLower() == oldMemName))
            {
                p.Category = trimmed;
            }
            return true;
        }

        public bool DeleteCategory(int id, out string errorMessage)
        {
            errorMessage = string.Empty;
            var cat = FindCategory(id);
            if (cat == null)
            {
                errorMessage = "Không tìm thấy danh mục.";
                return false;
            }

            string catLower = cat.CategoryName.Trim().ToLower();
            int usedCount = QueryProducts().AsEnumerable().Count(p => p.Category.Trim().ToLower() == catLower);
            if (usedCount > 0)
            {
                errorMessage = $"Không thể xóa danh mục \"{cat.CategoryName}\" vì đang có {usedCount} sản phẩm thuộc danh mục này.";
                return false;
            }

            if (WarehouseDatabaseSeeder.IsSqlActive)
            {
                try
                {
                    var sqlCat = Categories.FirstOrDefault(c => c.CategoryID == id);
                    if (sqlCat != null)
                    {
                        Categories.Remove(sqlCat);
                        SaveChanges();
                        return true;
                    }
                }
                catch
                {
                    // Fallback memory
                }
            }

            WarehouseDatabaseSeeder.MemoryCategories.RemoveAll(c => c.CategoryID == id);
            return true;
        }
    }
}
