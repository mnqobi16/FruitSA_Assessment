using FruitSA_Common;
using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FruitSA_Assessment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategory_Business _category_Business;
        private readonly ApplicationDbContext _context;


        public CategoryController(ICategory_Business category_Business, ApplicationDbContext context)
        {
            _category_Business = category_Business;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<CategoryDTO> objCategoryList = await _category_Business.GetAll();
            return View(objCategoryList);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDTO obj)
        {
            if (ModelState.IsValid)
            {
                // Check if Category Code already exists
                var existingCategory = await _category_Business.GetByCategoryCode(obj.CategoryCode);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("CategoryCode", "A category with this code already exists.");
                    return View(obj);
                }

                // Set the user who created the category
                var userName = HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    obj.Username = userName;
                }

                await _category_Business.Create(obj);

                return RedirectToAction("Index");
            }
            return View(obj);
        }


        //GET
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }
            var categoryFromDbFirst = await _category_Business.Get(id);

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            return View(categoryFromDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDTO obj)
        {
            if (ModelState.IsValid)
            {

                var userName = HttpContext.User.Identity.Name;
                obj.Username = userName;
                await _category_Business.Update(obj);
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }
            var categoryFromDbFirst =  await _category_Business.Delete(id);

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            return View(categoryFromDbFirst);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int id)
        {
            var obj = await _category_Business.Delete(id);
            if (obj == null)
            {
                return NotFound();
            }

            await _category_Business.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
