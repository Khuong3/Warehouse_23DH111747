using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Warehouse_23DH111747.Models;

namespace Warehouse_23DH111747.Controllers
{
    public class ProductsController : Controller
    {
        private readonly WarehouseEntities _db;

        public ProductsController(WarehouseEntities db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(string? category)
        {
            ViewBag.Categories = _db.GetCategoriesList();
            ViewBag.SelectedCategory = category;

            var query = _db.QueryProducts();
            if (!string.IsNullOrWhiteSpace(category))
            {
                string catLower = category.Trim().ToLower();
                query = query.Where(p => p.Category.ToLower() == catLower);
            }

            var products = query
                .OrderBy(p => p.ProductID)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            var allProducts = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            ViewBag.AllProducts = allProducts;
            ViewBag.Categories = _db.GetCategoriesList();

            Product? product = null;
            if (id.HasValue)
            {
                product = _db.FindProduct(id.Value);
            }
            else if (allProducts.Any())
            {
                product = allProducts.First();
            }

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ProductEditViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Category = product.Category,
                Price = product.Price,
                Quantity = product.Quantity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int ProductID, string? ProductName, string? Category, string? Price)
        {
            var allProducts = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            ViewBag.AllProducts = allProducts;
            ViewBag.Categories = _db.GetCategoriesList();

            var existingProduct = _db.FindProduct(ProductID);
            if (existingProduct == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            string trimmedName = (ProductName ?? string.Empty).Trim();
            string trimmedCategory = (Category ?? string.Empty).Trim();

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                errors.Add("Tên sản phẩm không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(trimmedCategory))
            {
                errors.Add("Vui lòng chọn danh mục.");
            }

            decimal parsedPrice = 0;
            bool isPriceValid = TryParseDecimalFlexible(Price, out parsedPrice) && parsedPrice > 0;
            if (!isPriceValid)
            {
                errors.Add("Đơn giá phải lớn hơn 0.");
            }

            var viewModel = new ProductEditViewModel
            {
                ProductID = existingProduct.ProductID,
                ProductName = ProductName,
                Category = Category,
                Price = isPriceValid ? parsedPrice : existingProduct.Price,
                Quantity = existingProduct.Quantity
            };

            if (errors.Any())
            {
                ViewBag.ErrorList = errors;
                ViewBag.RawPrice = Price;
                return View(viewModel);
            }

            _db.UpdateProductInfo(existingProduct.ProductID, trimmedName, trimmedCategory, parsedPrice);

            TempData["SuccessMessage"] = "Cập nhật thông tin sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _db.FindProduct(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ProductID)
        {
            if (!_db.DeleteProduct(ProductID))
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Search(string? keyword, string? stockFilter, string? sortOrder)
        {
            IQueryable<Product> query = _db.QueryProducts();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(stockFilter))
            {
                switch (stockFilter.Trim().ToLower())
                {
                    case "out":
                        query = query.Where(p => p.Quantity == 0);
                        break;
                    case "low":
                        query = query.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
                        break;
                    case "in":
                        query = query.Where(p => p.Quantity >= 5);
                        break;
                }
            }

            switch ((sortOrder ?? string.Empty).Trim().ToLower())
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.ProductID);
                    break;
            }

            var model = new ProductSearchViewModel
            {
                Keyword = keyword,
                StockFilter = stockFilter,
                SortOrder = sortOrder,
                Results = query.ToList()
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult InventoryCheck(int? id)
        {
            var products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            int? selectedId = id ?? products.FirstOrDefault()?.ProductID;
            var selectedProduct = selectedId.HasValue ? products.FirstOrDefault(p => p.ProductID == selectedId.Value) : null;

            var model = new SingleInventoryCheckViewModel
            {
                ProductID = selectedId,
                RawActualQuantity = selectedProduct?.Quantity.ToString(),
                Products = products
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InventoryCheck(int? ProductID, string? RawActualQuantity)
        {
            var products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            var model = new SingleInventoryCheckViewModel
            {
                ProductID = ProductID,
                RawActualQuantity = RawActualQuantity,
                IsSubmitted = true,
                Products = products
            };

            if (!ProductID.HasValue)
            {
                model.IsSuccess = false;
                model.ErrorMessage = "Vui lòng chọn sản phẩm.";
                return View(model);
            }

            var targetProduct = _db.FindProduct(ProductID.Value);
            if (targetProduct == null)
            {
                model.IsSuccess = false;
                model.ErrorMessage = "Sản phẩm không tồn tại.";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(RawActualQuantity) ||
                !int.TryParse(RawActualQuantity.Trim(), out int actualQty) ||
                actualQty < 0)
            {
                model.IsSuccess = false;
                model.ErrorMessage = "Số lượng thực tế phải là số nguyên không âm.";
                return View(model);
            }

            if (!_db.UpdateProductQuantity(ProductID.Value, actualQty, out int oldQty, out Product? updatedProduct) || updatedProduct == null)
            {
                model.IsSuccess = false;
                model.ErrorMessage = "Không thể cập nhật số lượng.";
                return View(model);
            }

            string diffStatus;
            string diffBadge;
            if (actualQty > oldQty)
            {
                diffStatus = $"Chênh lệch tăng {actualQty - oldQty}";
                diffBadge = "bg-success";
            }
            else if (actualQty < oldQty)
            {
                diffStatus = $"Chênh lệch giảm {oldQty - actualQty}";
                diffBadge = "bg-warning text-dark";
            }
            else
            {
                diffStatus = "Không chênh lệch";
                diffBadge = "bg-info text-dark";
            }

            model.IsSuccess = true;
            model.ActualQuantity = actualQty;
            model.UpdatedProductID = updatedProduct.ProductID;
            model.UpdatedProductName = updatedProduct.ProductName;
            model.UpdatedCategory = updatedProduct.Category;
            model.OldQuantity = oldQty;
            model.NewQuantity = actualQty;
            model.DiffStatus = diffStatus;
            model.DiffBadgeClass = diffBadge;
            model.NewStockStatus = updatedProduct.StockStatus;
            model.Products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult DualInventoryCheck()
        {
            var products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            var model = new DualInventoryCheckViewModel
            {
                ProductID1 = products.ElementAtOrDefault(0)?.ProductID,
                RawQuantity1 = products.ElementAtOrDefault(0)?.Quantity.ToString(),
                ProductID2 = products.ElementAtOrDefault(1)?.ProductID,
                RawQuantity2 = products.ElementAtOrDefault(1)?.Quantity.ToString(),
                Products = products
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DualInventoryCheck(int? ProductID1, string? RawQuantity1, int? ProductID2, string? RawQuantity2)
        {
            var products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            var model = new DualInventoryCheckViewModel
            {
                ProductID1 = ProductID1,
                RawQuantity1 = RawQuantity1,
                ProductID2 = ProductID2,
                RawQuantity2 = RawQuantity2,
                IsSubmitted = true,
                Products = products
            };

            var validationErrors = new List<string>();

            if (!ProductID1.HasValue)
            {
                validationErrors.Add("Vui lòng chọn mã sản phẩm thứ nhất.");
            }
            if (!ProductID2.HasValue)
            {
                validationErrors.Add("Vui lòng chọn mã sản phẩm thứ hai.");
            }
            if (ProductID1.HasValue && ProductID2.HasValue && ProductID1.Value == ProductID2.Value)
            {
                validationErrors.Add("Hai sản phẩm phải khác nhau.");
            }

            Product? checkP1 = ProductID1.HasValue ? _db.FindProduct(ProductID1.Value) : null;
            Product? checkP2 = ProductID2.HasValue ? _db.FindProduct(ProductID2.Value) : null;

            if (ProductID1.HasValue && checkP1 == null)
            {
                validationErrors.Add($"Mã sản phẩm {ProductID1.Value} không tồn tại.");
            }
            if (ProductID2.HasValue && checkP2 == null)
            {
                validationErrors.Add($"Mã sản phẩm {ProductID2.Value} không tồn tại.");
            }

            int qty1 = -1;
            int qty2 = -1;
            bool validQty1 = !string.IsNullOrWhiteSpace(RawQuantity1) && int.TryParse(RawQuantity1.Trim(), out qty1) && qty1 >= 0;
            bool validQty2 = !string.IsNullOrWhiteSpace(RawQuantity2) && int.TryParse(RawQuantity2.Trim(), out qty2) && qty2 >= 0;

            if (!validQty1)
            {
                validationErrors.Add("Số lượng thực tế của sản phẩm thứ nhất phải là số nguyên không âm.");
            }
            if (!validQty2)
            {
                validationErrors.Add("Số lượng thực tế của sản phẩm thứ hai phải là số nguyên không âm.");
            }

            if (validationErrors.Any())
            {
                model.IsSuccess = false;
                model.ErrorMessage = string.Join(" ", validationErrors);
                model.Products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
                return View(model);
            }

            bool txSuccess = _db.UpdateDualProductQuantitiesInTransaction(
                ProductID1!.Value, qty1,
                ProductID2!.Value, qty2,
                out int oldQty1, out int oldQty2,
                out Product? updatedP1, out Product? updatedP2,
                out string txError);

            if (!txSuccess || updatedP1 == null || updatedP2 == null)
            {
                model.IsSuccess = false;
                model.ErrorMessage = txError;
                model.Products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
                return View(model);
            }

            model.IsSuccess = true;
            model.Product1Name = updatedP1.ProductName;
            model.OldQuantity1 = oldQty1;
            model.NewQuantity1 = qty1;
            model.DiffStatus1 = qty1 > oldQty1 ? "Chênh lệch tăng" : (qty1 < oldQty1 ? "Chênh lệch giảm" : "Không chênh lệch");

            model.Product2Name = updatedP2.ProductName;
            model.OldQuantity2 = oldQty2;
            model.NewQuantity2 = qty2;
            model.DiffStatus2 = qty2 > oldQty2 ? "Chênh lệch tăng" : (qty2 < oldQty2 ? "Chênh lệch giảm" : "Không chênh lệch");

            model.Products = _db.QueryProducts().OrderBy(p => p.ProductID).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _db.GetCategoriesList();
            return View(new ProductCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCreateViewModel model)
        {
            ViewBag.Categories = _db.GetCategoriesList();

            string trimmedName = (model.ProductName ?? string.Empty).Trim();
            string trimmedCategory = (model.Category ?? string.Empty).Trim();

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                errors.Add("Tên sản phẩm không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(trimmedCategory))
            {
                errors.Add("Vui lòng chọn danh mục.");
            }

            decimal parsedPrice = 0;
            if (!TryParseDecimalFlexible(model.RawPrice, out parsedPrice) || parsedPrice <= 0)
            {
                errors.Add("Đơn giá phải lớn hơn 0.");
            }

            int parsedQty = -1;
            if (string.IsNullOrWhiteSpace(model.RawQuantity) ||
                !int.TryParse(model.RawQuantity.Trim(), out parsedQty) ||
                parsedQty < 0)
            {
                errors.Add("Số lượng phải là số nguyên không âm.");
            }

            if (!string.IsNullOrWhiteSpace(trimmedName))
            {
                string normalizedInputName = trimmedName.ToLower();
                var duplicateProduct = _db.QueryProducts()
                    .AsEnumerable()
                    .FirstOrDefault(p => p.ProductName.Trim().ToLower() == normalizedInputName);

                if (duplicateProduct != null)
                {
                    errors.Add("Tên sản phẩm đã tồn tại trong kho.");
                }
            }

            if (errors.Any())
            {
                ViewBag.ErrorList = errors;
                return View(model);
            }

            _db.AddNewProduct(trimmedName, trimmedCategory, parsedPrice, parsedQty);

            TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Statistics()
        {
            IQueryable<Product> productsQuery = _db.QueryProducts();

            int totalProducts = productsQuery.Count();
            int outOfStockProducts = productsQuery.Count(p => p.Quantity == 0);
            int lowStockProducts = productsQuery.Count(p => p.Quantity >= 1 && p.Quantity <= 4);
            int inStockProducts = productsQuery.Count(p => p.Quantity >= 5);
            int totalUnits = productsQuery.Any() ? productsQuery.Sum(p => p.Quantity) : 0;
            decimal totalInventoryValue = productsQuery.Any()
                ? productsQuery.Sum(p => p.Price * p.Quantity)
                : 0m;

            var model = new WarehouseStatisticsViewModel
            {
                TotalProducts = totalProducts,
                OutOfStockProducts = outOfStockProducts,
                LowStockProducts = lowStockProducts,
                InStockProducts = inStockProducts,
                TotalUnitsInStock = totalUnits,
                TotalInventoryValue = totalInventoryValue,
                Products = productsQuery.OrderBy(p => p.ProductID).ToList()
            };

            return View(model);
        }

        private static bool TryParseDecimalFlexible(string? raw, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            string cleaned = raw.Trim().Replace(" ", "");
            if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                return true;
            if (decimal.TryParse(cleaned, NumberStyles.Number, new CultureInfo("vi-VN"), out value))
                return true;
            return false;
        }
    }
}
