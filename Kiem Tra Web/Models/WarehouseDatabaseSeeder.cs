using Microsoft.EntityFrameworkCore;

namespace Warehouse_23DH111747.Models
{
    public static class WarehouseDatabaseSeeder
    {
        public static bool IsSqlActive { get; private set; } = false;
        public static string ActiveServerName { get; private set; } = ".\\TOILAKHUONG (SQL Server)";

        private static readonly object SyncLock = new object();
        public static List<Product> MemoryProducts { get; } = new List<Product>();
        public static List<CategoryItem> MemoryCategories { get; } = new List<CategoryItem>();

        public static void EnsureDatabaseAndSeed()
        {
            SeedMemoryData(forceReset: false);

            string[] candidateServers = new[]
            {
                ".\\TOILAKHUONG",
                ".\\BAOKHUONG",
                ".\\KHUONG01",
                "localhost",
                ".\\SQLEXPRESS",
                "(localdb)\\MSSQLLocalDB"
            };

            var tasks = candidateServers.Select(server => Task.Run(() =>
            {
                string connStr = $"Server={server};Database=Warehouse_23DH111747;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Connect Timeout=1";
                try
                {
                    var options = new DbContextOptionsBuilder<WarehouseEntities>()
                        .UseSqlServer(connStr)
                        .Options;

                    using var db = new WarehouseEntities(options);
                    db.Database.EnsureCreated();
                    EnsureCategoriesTableSql(db);
                    SeedSqlData(db, forceReset: false);
                    return (Success: true, Server: server, ConnStr: connStr);
                }
                catch
                {
                    return (Success: false, Server: server, ConnStr: string.Empty);
                }
            })).ToArray();

            try
            {
                Task.WaitAll(tasks, TimeSpan.FromMilliseconds(1500));
            }
            catch
            {
            }

            var connected = tasks
                .Where(t => t.IsCompletedSuccessfully && t.Result.Success)
                .Select(t => t.Result)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(connected.ConnStr))
            {
                WarehouseEntities.DefaultConnectionString = connected.ConnStr;
                ActiveServerName = $"{connected.Server} -> Warehouse_23DH111747";
                IsSqlActive = true;
            }
            else
            {
                IsSqlActive = false;
                ActiveServerName = "Warehouse_23DH111747";
            }
        }

        private static void EnsureCategoriesTableSql(WarehouseEntities db)
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL
    );
END";
            try
            {
                db.Database.ExecuteSqlRaw(sql);
            }
            catch
            {
            }
        }

        public static void ResetSampleData(WarehouseEntities db)
        {
            SeedMemoryData(forceReset: true);
            if (IsSqlActive)
            {
                try
                {
                    db.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Products;");
                    db.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.Categories;");
                }
                catch
                {
                    var allProds = db.Products.ToList();
                    db.Products.RemoveRange(allProds);
                    var allCats = db.Categories.ToList();
                    db.Categories.RemoveRange(allCats);
                    db.SaveChanges();
                }
                SeedSqlData(db, forceReset: true);
            }
        }

        private static void SeedSqlData(WarehouseEntities db, bool forceReset)
        {
            if (forceReset || !db.Categories.Any())
            {
                var cats = BuildInitialCategories();
                db.Categories.AddRange(cats);
                db.SaveChanges();
            }

            if (forceReset || !db.Products.Any())
            {
                var samples = BuildInitialProducts();
                db.Products.AddRange(samples);
                db.SaveChanges();
            }
        }

        private static void SeedMemoryData(bool forceReset)
        {
            lock (SyncLock)
            {
                if (forceReset || !MemoryCategories.Any())
                {
                    MemoryCategories.Clear();
                    var cats = BuildInitialCategories();
                    for (int i = 0; i < cats.Count; i++)
                    {
                        cats[i].CategoryID = i + 1;
                    }
                    MemoryCategories.AddRange(cats);
                }

                if (forceReset || !MemoryProducts.Any())
                {
                    MemoryProducts.Clear();
                    var samples = BuildInitialProducts();
                    for (int i = 0; i < samples.Count; i++)
                    {
                        samples[i].ProductID = i + 1;
                    }
                    MemoryProducts.AddRange(samples);
                }
            }
        }

        public static List<CategoryItem> BuildInitialCategories()
        {
            return new List<CategoryItem>
            {
                new CategoryItem { CategoryName = "Thiết bị điện tử - Bảo Khương" },
                new CategoryItem { CategoryName = "Phụ kiện âm thanh - Bảo Khương" },
                new CategoryItem { CategoryName = "Nội thất văn phòng - Bảo Khương" }
            };
        }

        public static List<Product> BuildInitialProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    ProductName = "Bàn phím Bảo Khương",
                    Category = "Thiết bị điện tử - Bảo Khương",
                    Price = 1250000.00m,
                    Quantity = 10
                },
                new Product
                {
                    ProductName = "Chuột không dây Bảo Khương",
                    Category = "Thiết bị điện tử - Bảo Khương",
                    Price = 650000.00m,
                    Quantity = 5
                },
                new Product
                {
                    ProductName = "Tai nghe Bluetooth Bảo Khương",
                    Category = "Phụ kiện âm thanh - Bảo Khương",
                    Price = 1890000.00m,
                    Quantity = 3
                },
                new Product
                {
                    ProductName = "Loa di động Bảo Khương",
                    Category = "Phụ kiện âm thanh - Bảo Khương",
                    Price = 950000.00m,
                    Quantity = 0
                },
                new Product
                {
                    ProductName = "Màn hình LED Bảo Khương",
                    Category = "Thiết bị điện tử - Bảo Khương",
                    Price = 3450000.00m,
                    Quantity = 8
                },
                new Product
                {
                    ProductName = "Ghế văn phòng Bảo Khương",
                    Category = "Nội thất văn phòng - Bảo Khương",
                    Price = 2100000.00m,
                    Quantity = 2
                }
            };
        }
    }
}
