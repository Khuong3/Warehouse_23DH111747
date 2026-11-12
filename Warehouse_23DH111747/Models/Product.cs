using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse_23DH111747.Models
{
    /// <summary>
    /// Thực thể Product ánh xạ bảng dbo.Products trong CSDL Warehouse_23DH111747 (Câu 1)
    /// </summary>
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Mã sản phẩm (ProductID)")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự.")]
        [Display(Name = "Tên sản phẩm (ProductName)")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục không được để trống.")]
        [StringLength(100, ErrorMessage = "Danh mục tối đa 100 ký tự.")]
        [Display(Name = "Danh mục (Category)")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn giá không được để trống.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Đơn giá (Price)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho không được để trống.")]
        [Display(Name = "Số lượng (Quantity)")]
        public int Quantity { get; set; }

        /// <summary>
        /// Câu 2a: Tình trạng tồn kho:
        /// - Hết hàng nếu Quantity = 0
        /// - Sắp hết nếu Quantity từ 1–4
        /// - Đủ hàng nếu Quantity >= 5
        /// </summary>
        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (Quantity <= 0) return "Hết hàng";
                if (Quantity <= 4) return "Sắp hết";
                return "Đủ hàng";
            }
        }

        [NotMapped]
        public string StockBadgeClass
        {
            get
            {
                if (Quantity <= 0) return "bg-danger";
                if (Quantity <= 4) return "bg-warning text-dark";
                return "bg-success";
            }
        }

        [NotMapped]
        public decimal InventoryValue => Price * Quantity;
    }
}
