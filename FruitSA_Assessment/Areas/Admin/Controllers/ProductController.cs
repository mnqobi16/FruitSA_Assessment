using FruitSA_Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;

namespace FruitSA_Assessment.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProduct_Business product_Business;
        private readonly IWebHostEnvironment _hostEnvironment;



        public ProductController(IProduct_Business unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            product_Business = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }



        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            var categories = product_Business.GetAll().Select(c => new SelectListItem
            {
                Text = c.CategoryName,
                Value = c.CategoryId.ToString()
            });



            var productVM = new ProductVM
            {
                CategoryList = categories // Assign your categories to the CategoryList property
            };



            if (id != null)
            {
                productVM.Product = product_Business.Get(x => x.ProductId == id);
            }
            else
            {
                productVM.Product = new Product();
                productVM.Product.ProductCode = GenerateProductCode(); // Generate product code for new product
            }



            return View(productVM);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
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
                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
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
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }



                if (obj.Product.ProductId == 0) // New product
                {
                    // Generate new product code
                    obj.Product.ProductCode = GenerateProductCode();



                    // Set the user who created the product
                    var userName = HttpContext.User.Identity.Name;
                    obj.Product.Username = userName;



                    // Add the new product to the database
                    product_Business.Product.Add(obj.Product);
                }
                else // Existing product
                {
                    // Retrieve the existing product from the database to update its values
                    var existingProduct = product_Business.Product.GetFirstOrDefault(x => x.ProductId == obj.Product.ProductId);



                    if (existingProduct != null)
                    {
                        // Update existing product properties
                        existingProduct.Name = obj.Product.Name;
                        existingProduct.Description = obj.Product.Description;
                        existingProduct.Price = obj.Product.Price;
                        existingProduct.ImageUrl = obj.Product.ImageUrl;
                        existingProduct.UpdatedAt = DateTime.Now;
                        // Update the product in the database
                        product_Business.Product.Update(existingProduct);
                    }
                    else
                    {
                        // Handle the case where the existing product is not found
                        return NotFound();
                    }
                }



                // Save changes to the database
                product_Business.Save();
                TempData["success"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }





        private string GenerateProductCode()
        {
            string yearMonth = DateTime.Now.ToString("yyyyMM");
            // Fetch the latest product code with the given yearMonth prefix
            var latestProduct = product_Business.Product.GetAll().Where(p => p.ProductCode.StartsWith(yearMonth))
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
        public IActionResult DownloadProductsExcel()
        {
            // Set the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;



            // Retrieve data from the database
            var products = product_Business.Product.GetAll();



            // Project the data and format the dates
            var formattedData = products.Select(p => new
            {
                p.ProductId,
                p.ProductCode,
                p.Name,
                p.Description,
                p.CategoryId,
                p.Price,
                p.ImageUrl,
                p.Username,
                CreatedDate = p.CreatedDate.ToString("dd MMM yyyy, HH:mm:ss"),
                UpdatedAt = p.UpdatedAt != null ? p.UpdatedAt.Value.ToString("dd MMM yyyy, HH:mm:ss") : "N/A"
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
                    var products = new List<Product>();



                    // Get the username from HttpContext or from the Excel file
                    var userName = HttpContext.User.Identity.Name;



                    // Start from 2 to skip the header row
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var product = new Product
                        {
                            // Excel columns are in order: ProductId, ProductCode, Name, Description, CategoryId, Price, ImageUrl, Username
                            // Update the index values for each column
                            ProductCode = worksheet.Cells[row, 2].Value?.ToString(),
                            Name = worksheet.Cells[row, 3].Value?.ToString(),
                            Description = worksheet.Cells[row, 4].Value?.ToString(),
                            CategoryId = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                            Price = Convert.ToDouble(worksheet.Cells[row, 6].Value),
                            ImageUrl = worksheet.Cells[row, 7].Value?.ToString(),
                            Username = !string.IsNullOrEmpty(worksheet.Cells[row, 8].Value?.ToString()) ? worksheet.Cells[row, 8].Value?.ToString() : userName,
                            CreatedDate = DateTime.Now,
                            UpdatedAt = null
                        };



                        products.Add(product);
                    }



                    // Save products to the database
                    product_Business.Product.AddRange(products);
                    product_Business.Save();
                }
            }



            TempData["success"] = "Product uploaded successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
