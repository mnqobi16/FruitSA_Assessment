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
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;
using FruitSA_Data.Context;
using OfficeOpenXml;

namespace FruitSA_Assessment.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProduct_Business product_Business;
        private readonly IWebHostEnvironment _hostEnvironment;

        private readonly ApplicationDbContext _context;
        private readonly IMapper _autoMapper;

        public ProductController(IProduct_Business product_, IWebHostEnvironment hostEnvironment , ApplicationDbContext db, IMapper mapper)
        {
            product_Business = product_;
            _hostEnvironment = hostEnvironment;
            _context = db;
            _autoMapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var categories = (await product_Business.GetAll()).ToList();


            var productViewModel = new ProductViewModel
            {
                //CategoryList = categories // Now categories is a List<SelectListItem>, matching CategoryList

                CategoryList = categories.Select(c => new SelectListItem
                {
                    Text = c.CategoryDTO.Name,  // Assuming ProductDTO has CategoryName
                    Value = c.CategoryId.ToString() // Assuming ProductDTO has CategoryId
                }).ToList() // Convert to List<SelectListItem>
            };

            if (id != null)
            {
                productViewModel.Product = await product_Business.Get(Convert.ToInt16(id));
            }
            else
            {
                productViewModel.Product = new ProductDTO();
                productViewModel.Product.ProductCode =await GenerateProductCode(); // Generate product code for new product
            }

            return View(productViewModel);
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
                    await product_Business.Create(obj.Product);
                }
                else // Existing product
                {
                    // Retrieve the existing product from the database to update its values
                    var existingProduct = await product_Business.Get(obj.Product.ProductId);



                    if (existingProduct != null)
                    {
                        // Update existing product properties
                        existingProduct.ProductName = obj.Product.ProductName;
                        existingProduct.Description = obj.Product.Description;
                        existingProduct.Price = obj.Product.Price;
                        existingProduct.ImagePath = obj.Product.ImagePath;
                        existingProduct.UpdateAt = DateTime.Now;
                        // Update the product in the database
                        await product_Business.Update(existingProduct);
                    }
                    else
                    {
                        // Handle the case where the existing product is not found
                        return NotFound();
                    }
                }



                // Save changes to the database
               
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }


        private async Task<string> GenerateProductCode()
        {
            string yearMonth = DateTime.Now.ToString("yyyyMM");

            var latestProduct = await product_Business.GetAll()
            .ContinueWith(task => task.Result
            .Where(p => p.ProductCode.StartsWith(yearMonth))
            .OrderByDescending(p => p.ProductCode)
            .FirstOrDefault());


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
        //public IActionResult DownloadProductsExcel()
        //{
        //    // Set the license context
        //    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        //    // Retrieve data from the database
        //    var products = product_Business.GetAll();

        //    // Project the data and format the dates
        //    var formattedData = products.Select(p => new
        //    {
        //        p.ProductId,
        //        p.ProductCode,
        //        p.Name,
        //        p.Description,
        //        p.CategoryId,
        //        p.Price,
        //        p.ImageUrl,
        //        p.Username,
        //        CreatedDate = p.CreatedDate.ToString("dd MMM yyyy, HH:mm:ss"),
        //        UpdatedAt = p.UpdatedAt != null ? p.UpdatedAt.Value.ToString("dd MMM yyyy, HH:mm:ss") : "N/A"
        //    });

        //    // Create Excel package
        //    using (ExcelPackage excelPackage = new ExcelPackage())
        //    {
        //        // Create worksheet
        //        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Products");

        //        // Load data from collection
        //        worksheet.Cells.LoadFromCollection(formattedData, true);

        //        // Auto-fit columns
        //        worksheet.Cells.AutoFitColumns();

        //        // Convert package to bytes
        //        byte[] fileBytes = excelPackage.GetAsByteArray();

        //        // Return Excel file
        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
        //    }
        //}

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

                       await product_Business.Create(product);
                    }

                    // Save products to the database
                 
                }
            }

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    var productList = product_Business.GetAll(includeProperties: "Category");
        //    return Json(new { data = productList });
        //}
        ////POST
        //[HttpDelete]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var obj = await product_Business.Get(id);
        //    obj.
        //    if (obj == null)
        //    {
        //        return Json(new { success = false, message = "Error while deleting" });
        //    }

        //    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImagePath.TrimStart('\\'));
        //    if (System.IO.File.Exists(oldImagePath))
        //    {
        //        System.IO.File.Delete(oldImagePath);
        //    }

        //    product_Business.Delete(id);
        //    return Json(new { success = true, message = "Product Deleted" });

        //}
        #endregion
    }
}
