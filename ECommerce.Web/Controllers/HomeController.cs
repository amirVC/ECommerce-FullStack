using System.Diagnostics;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly IWishlistService _wishlistService;
        private readonly IAuthService _authService;

        public HomeController(
            ILogger<HomeController> logger,
            IProductService productService,
            IWishlistService wishlistService,
            IAuthService authService)
        {
            _logger = logger;
            _productService = productService;
            _wishlistService = wishlistService;
            _authService = authService;
        }

        private bool IsLoggedIn => !string.IsNullOrWhiteSpace(_authService.GetToken());

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAsync(page: 1);

            var model = new ProductCatalogViewModel
            {
                Products = products,
                WishlistedProductIds = IsLoggedIn
                    ? await _wishlistService.GetWishlistedProductIdsAsync()
                    : new HashSet<int>()
            };

            return View(model);
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