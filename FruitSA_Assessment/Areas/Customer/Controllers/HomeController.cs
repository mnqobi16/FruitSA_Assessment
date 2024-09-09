using FruitSA_Assessment.Models;
using FruitSA_Common;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FruitSA_Assessment.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProduct_Business _product_Business;

        public HomeController(ILogger<HomeController> logger, IProduct_Business product_Business)
        {
            _logger = logger;
            _product_Business = product_Business;
        }


        public async Task<IActionResult> Index()
        {
            IEnumerable<ProductDTO> productList = await _product_Business.GetAll(includeProperties: "Category");

            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
