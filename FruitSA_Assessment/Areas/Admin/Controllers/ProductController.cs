using FruitSA_Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Assessment.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using FruitSA_Data.Context;
using OfficeOpenXml;
using FruitSA_DataAccess.BusinessLogic;
using System.Linq;
using FruitSA_Common;
using Microsoft.AspNetCore.Authorization;

namespace FruitSA_Assessment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProduct_Business _product_Business;
        private readonly ICategory_Business _category_Business;
        private readonly IWebHostEnvironment _hostEnvironment;

        private readonly ApplicationDbContext _context;
        private readonly IMapper _autoMapper;

        public ProductController(IProduct_Business product_, IWebHostEnvironment hostEnvironment , ApplicationDbContext db, ICategory_Business category_Business, IMapper mapper)
        {
            _product_Business = product_;
            _hostEnvironment = hostEnvironment;
            _context = db;
            _autoMapper = mapper;
            _category_Business = category_Business;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            // Await the task to get the category list
            var categories = (await _category_Business.GetAll())
                                               .Select(c => new SelectListItem
                                               {
                                                   Text = c.Name,
                                                   Value = c.CategoryId.ToString()
                                               })
                                               .ToList();

            // Initialize ProductViewModel and set CategoryList
            var productVM = new ProductViewModel
            {
                CategoryList = categories
            };

            // If 'id' is not null, we are editing a product; else, we create a new one
            if (id.HasValue)
            {
                productVM.Product = await _product_Business.Get(x => x.ProductId == id.Value);
                if (productVM.Product == null)
                {
                    return NotFound(); // Handle case where product does not exist
                }
            }
            else
            {
                productVM.Product = new ProductDTO();
                productVM.Product.ProductCode = await GenerateProductCode(); 
            }

            return View(productVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(obj.Product.ImagePath))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImagePath.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new image
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Update the ImageUrl property of the product
                    obj.Product.ImagePath = @"\images\products\" + fileName + extension;
                }

                if (obj.Product.ProductId == 0) // New product
                {
                    // Generate new product code
                    obj.Product.ProductCode = await GenerateProductCode();

                    // Set the user who created the product
                    var userName = HttpContext.User.Identity.Name;
                    obj.Product.Username = userName;

                    // Add the new product to the database
                   await _product_Business.Create(obj.Product);
                }
                else // Existing product
                {
                    // Retrieve the existing product from the database to update its values
                    var existingProduct = await _product_Business.Get(x => x.ProductId == obj.Product.ProductId);

                    if (existingProduct != null)
                    {
                        // Update existing product properties
                        existingProduct.ProductName = obj.Product.ProductName;
                        existingProduct.Description = obj.Product.Description;
                        existingProduct.Price = obj.Product.Price;
                        existingProduct.ImagePath = obj.Product.ImagePath;
                        existingProduct.UpdateAt = DateTime.Now;
                        // Update the product in the database
                       await _product_Business.Update(existingProduct);
                    }
                    else
                    {
                        // Handle the case where the existing product is not found
                        return NotFound();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }


        private async Task<string> GenerateProductCode()
        {
            string yearMonth = DateTime.Now.ToString("yyyyMM");

            // Await the result of GetAll before applying LINQ
            var products = await _product_Business.GetAll();

            
            var latestProduct = products.Where(p => p.ProductCode.StartsWith(yearMonth))
                                        .OrderByDescending(p => p.ProductCode)
                                        .FirstOrDefault();

            // If no product exists with the given yearMonth prefix, start with 001
            int sequenceNumber = 1;
            if (latestProduct != null)
            {
                // Extract the sequence number and increment it
                string sequenceStr = latestProduct.ProductCode.Substring(7);
                sequenceNumber = int.Parse(sequenceStr) + 1;
            }

            // Format the product code as yyyyMM-###
            string productCode = $"{yearMonth}-{sequenceNumber.ToString("D3")}";
            return productCode;
        }

        public async Task<IActionResult> DownloadProductsExcel()
        {
            // Set the license context
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Retrieve data from the database
            var products =await _product_Business.GetAll(includeProperties: "Category");

            // Project the data and format the dates
            var formattedData = products.Select(p => new
            {
                p.ProductId,
                p.ProductCode,
                p.ProductName,
                p.Description,
                p.CategoryId,
                p.Price,
                p.ImagePath,
                p.Username,
                CreatedDate = p.CreatedAt.ToString("dd MMM yyyy, HH:mm:ss"),
                UpdatedAt = p.UpdateAt != null ? p.UpdateAt.Value.ToString("dd MMM yyyy, HH:mm:ss") : "N/A"
            });

            // Create Excel package
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Create worksheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Products");

                // Load data from collection
                worksheet.Cells.LoadFromCollection(formattedData, true);

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Convert package to bytes
                byte[] fileBytes = excelPackage.GetAsByteArray();

                // Return Excel file
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadProductsExcel(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest("File is empty");
            }



            // Check if the file is an Excel file
            if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
            {
                return BadRequest("Invalid file format. Please upload a valid Excel file.");
            }



            // Read the Excel file
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    var products = new List<ProductDTO>();



                    // Get the username from HttpContext or from the Excel file
                    var userName = HttpContext.User.Identity.Name;



                    // Start from 2 to skip the header row
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var product = new ProductDTO
                        {
                            // Excel columns are in order: ProductId, ProductCode, Name, Description, CategoryId, Price, ImageUrl, Username
                            // Update the index values for each column
                            ProductCode = worksheet.Cells[row, 2].Value?.ToString(),
                            ProductName = worksheet.Cells[row, 3].Value?.ToString(),
                            Description = worksheet.Cells[row, 4].Value?.ToString(),
                            CategoryId = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                            Price = Convert.ToDouble(worksheet.Cells[row, 6].Value),
                            ImagePath = worksheet.Cells[row, 7].Value?.ToString(),
                            Username = !string.IsNullOrEmpty(worksheet.Cells[row, 8].Value?.ToString()) ? worksheet.Cells[row, 8].Value?.ToString() : userName,
                            CreatedAt = DateTime.Now,
                            UpdateAt = null
                        };



                        products.Add(product);

                       await _product_Business.Create(product);
                    }

                    // Save products to the database
                 
                }
            }

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productList = await _product_Business.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }
        //POST
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            // Fetch the product
            var obj = await _product_Business.Get(x => x.ProductId == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }
            if (!string.IsNullOrEmpty(obj.ImagePath))
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImagePath.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            await _product_Business.Delete(id);

            return Json(new { success = true, message = "Product deleted successfully" });
        }

        #endregion
    }
}
