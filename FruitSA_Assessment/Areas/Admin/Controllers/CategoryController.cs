using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FruitSA_Assessment.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategory_Business _category_Business;



        public CategoryController(ICategory_Business category_Business)
        {
            _category_Business = category_Business;
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
        public IActionResult Create(CategoryDTO obj)
        {
            if (ModelState.IsValid)
            {
                //// Check for uniqueness of the category code
                //if (!IsCategoryCodeUnique(obj.CategoryCode))
                //{
                //    ModelState.AddModelError("CategoryCode", "Category code must be unique.");
                //    return View(obj);
                //}

                // Set the user who created the category
                var userName = HttpContext.User.Identity.Name;
                obj.Username = userName;

                _category_Business.Create(obj);

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //private bool IsCategoryCodeUnique(string categoryCode)
        //{
        //    // Check if there's any existing category with the same category code in the database
        //    return !_category_Business.Get(c => c.CategoryCode == categoryCode);
        //}

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDbFirst = _category_Business.Get(Convert.ToInt16(id));

            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            return View(categoryFromDbFirst);
        }



        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryDTO obj)
        {
            if (ModelState.IsValid)
            {
                // Set the user who updated the category
                var userName = HttpContext.User.Identity.Name;
                obj.Username = userName;

                _category_Business.Update(obj);
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDbFirst = _category_Business.Delete(Convert.ToInt16(id));



            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }



            return View(categoryFromDbFirst);
        }



        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = _category_Business.Delete(Convert.ToInt16(id));
            if (obj == null)
            {
                return NotFound();
            }

            await _category_Business.Delete(Convert.ToInt16(id));
            return RedirectToAction("Index");
        }


    }
}
