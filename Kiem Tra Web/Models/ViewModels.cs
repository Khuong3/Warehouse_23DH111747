using System.ComponentModel.DataAnnotations;

namespace Warehouse_23DH111747.Models
{
    /// <summary>
    /// Thông tin sinh viên hiển thị ở Header và Footer mọi trang (Câu 1)
    /// </summary>
    public static class StudentInfoStore
    {
        public static string FullName { get; set; } = "Nguyễn Phan Bảo Khương";
        public static string MSSV { get; set; } = "23DH111747";
        public static string DateOfBirth { get; set; } = "09/03/2005";
    }

    /// <summary>
    /// ViewModel cho Câu 2b: Sửa thông tin sản phẩm (Không sửa ProductID và Quantity)
    /// </summary>
    public class ProductEditViewModel
    {
        public int ProductID { get; set; }

        [Display(Name = "Tên sản phẩm (ProductName)")]
        public string? ProductName { get; set; }

        [Display(Name = "Danh mục (Category)")]
        public string? Category { get; set; }

        [Display(Name = "Đơn giá (Price)")]
        public decimal? Price { get; set; }

        [Display(Name = "Số lượng hiện tại (Không được sửa)")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// ViewModel cho Câu 3: Tra cứu hàng hóa (Tìm theo tên, lọc mức tồn kho, sắp xếp theo Price)
    /// </summary>
    public class ProductSearchViewModel
    {
        public string? Keyword { get; set; }
        public string? StockFilter { get; set; } // "", "out" (0), "low" (1-4), "in" (>=5)
        public string? SortOrder { get; set; }   // "", "price_asc", "price_desc"
        public List<Product> Results { get; set; } = new List<Product>();
        public int TotalMatched => Results.Count;
    }

    /// <summary>
    /// ViewModel cho Câu 4: Cập nhật số lượng theo kiểm kê (1 sản phẩm)
    /// </summary>
    public class SingleInventoryCheckViewModel
    {
        public int? ProductID { get; set; }
        public string? RawActualQuantity { get; set; }
        public int? ActualQuantity { get; set; }

        // Kết quả sau khi kiểm kê
        public bool IsSubmitted { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public int UpdatedProductID { get; set; }
        public string? UpdatedProductName { get; set; }
        public string? UpdatedCategory { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public int DiffAmount => NewQuantity - OldQuantity;

        /// <summary>
        /// "Chênh lệch tăng" / "Chênh lệch giảm" / "Không chênh lệch"
        /// </summary>
        public string DiffStatus { get; set; } = string.Empty;
        public string DiffBadgeClass { get; set; } = "bg-secondary";
        public string NewStockStatus { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// ViewModel cho Câu 5: Kiểm kê đồng thời hai sản phẩm trong cùng một Transaction
    /// </summary>
    public class DualInventoryCheckViewModel
    {
        public int? ProductID1 { get; set; }
        public string? RawQuantity1 { get; set; }

        public int? ProductID2 { get; set; }
        public string? RawQuantity2 { get; set; }

        public bool IsSubmitted { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        // Thông tin kết quả sản phẩm 1
        public string? Product1Name { get; set; }
        public int OldQuantity1 { get; set; }
        public int NewQuantity1 { get; set; }
        public string? DiffStatus1 { get; set; }

        // Thông tin kết quả sản phẩm 2
        public string? Product2Name { get; set; }
        public int OldQuantity2 { get; set; }
        public int NewQuantity2 { get; set; }
        public string? DiffStatus2 { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
    }

    /// <summary>
    /// ViewModel cho Câu 6: Thêm sản phẩm mới có kiểm tra trùng tên
    /// </summary>
    public class ProductCreateViewModel
    {
        [Display(Name = "Tên sản phẩm (ProductName)")]
        public string? ProductName { get; set; }

        [Display(Name = "Danh mục (Category)")]
        public string? Category { get; set; }

        [Display(Name = "Đơn giá (Price)")]
        public string? RawPrice { get; set; }

        [Display(Name = "Số lượng (Quantity)")]
        public string? RawQuantity { get; set; }
    }

    /// <summary>
    /// ViewModel cho Câu 7: Thống kê kết quả kiểm kê bằng LINQ và Entity Framework
    /// </summary>
    public class WarehouseStatisticsViewModel
    {
        /// <summary>
        /// Tổng số lượng sản phẩm (số mặt hàng trong bảng Products)
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Số sản phẩm hết hàng (Quantity == 0)
        /// </summary>
        public int OutOfStockProducts { get; set; }

        /// <summary>
        /// Số sản phẩm sắp hết hàng (Quantity từ 1 đến 4)
        /// </summary>
        public int LowStockProducts { get; set; }

        /// <summary>
        /// Số sản phẩm đủ hàng (Quantity >= 5)
        /// </summary>
        public int InStockProducts { get; set; }

        /// <summary>
        /// Tổng số đơn vị hàng tồn kho = SUM(Quantity)
        /// </summary>
        public int TotalUnitsInStock { get; set; }

        /// <summary>
        /// Tổng giá trị hàng tồn = SUM(Price * Quantity)
        /// </summary>
        public decimal TotalInventoryValue { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
