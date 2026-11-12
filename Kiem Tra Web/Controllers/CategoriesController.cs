using Microsoft.AspNetCore.Mvc;
using Warehouse_23DH111747.Models;

namespace Warehouse_23DH111747.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly WarehouseEntities _db;

        public CategoriesController(WarehouseEntities db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _db.GetCategoriesList();
            var products = _db.QueryProducts().ToList();

            var productCounts = categories.ToDictionary(
                c => c.CategoryID,
                c => products.Count(p => p.Category.Trim().ToLower() == c.CategoryName.Trim().ToLower())
            );

            ViewBag.ProductCounts = productCounts;
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string? CategoryName)
        {
            string trimmed = (CategoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                ViewBag.ErrorMessage = "Tên danh mục không được để trống.";
                return View(new CategoryItem { CategoryName = CategoryName ?? string.Empty });
            }

            bool isDuplicate = _db.GetCategoriesList()
                .Any(c => c.CategoryName.Trim().ToLower() == trimmed.ToLower());

            if (isDuplicate)
            {
                ViewBag.ErrorMessage = "Tên danh mục đã tồn tại.";
                return View(new CategoryItem { CategoryName = trimmed });
            }

            _db.AddCategory(trimmed);
            TempData["SuccessMessage"] = "Thêm danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _db.FindCategory(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int CategoryID, string? CategoryName)
        {
            var existing = _db.FindCategory(CategoryID);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            string trimmed = (CategoryName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                ViewBag.ErrorMessage = "Tên danh mục không được để trống.";
                return View(new CategoryItem { CategoryID = CategoryID, CategoryName = CategoryName ?? string.Empty });
            }

            bool isDuplicate = _db.GetCategoriesList()
                .Any(c => c.CategoryID != CategoryID && c.CategoryName.Trim().ToLower() == trimmed.ToLower());

            if (isDuplicate)
            {
                ViewBag.ErrorMessage = "Tên danh mục đã tồn tại.";
                return View(new CategoryItem { CategoryID = CategoryID, CategoryName = trimmed });
            }

            _db.UpdateCategory(CategoryID, trimmed);
            TempData["SuccessMessage"] = "Cập nhật danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _db.FindCategory(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            int usedCount = _db.QueryProducts()
                .AsEnumerable()
                .Count(p => p.Category.Trim().ToLower() == category.CategoryName.Trim().ToLower());

            ViewBag.UsedCount = usedCount;
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int CategoryID)
        {
            if (!_db.DeleteCategory(CategoryID, out string errorMsg))
            {
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
