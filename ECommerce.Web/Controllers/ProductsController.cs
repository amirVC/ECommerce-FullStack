using ECommerce.Web.DTOs;
using ECommerce.Web.Helpers;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly IWishlistService _wishlistService;
    private readonly IAuthService _authService;
    private readonly IReviewService _reviewService;
    private readonly IRecentlyViewedService _recentlyViewedService;

    public ProductsController(
        IProductService productService,
        IWishlistService wishlistService,
        IAuthService authService,
        IReviewService reviewService,
        IRecentlyViewedService recentlyViewedService)
    {
        _productService = productService;
        _wishlistService = wishlistService;
        _authService = authService;
        _reviewService = reviewService;
        _recentlyViewedService = recentlyViewedService;
    }

    private bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(_authService.GetToken());

    // ============================================================
    // PRODUCT LIST
    // ============================================================

    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        int? categoryId = null,
        string? sortBy = null)
    {
        var result = await _productService.GetProductsAsync(
            page,
            search,
            categoryId,
            sortBy);

        var model = new ProductCatalogViewModel
        {
            Products = result,
            Search = search,
            CategoryId = categoryId,
            SortBy = sortBy,

            WishlistedProductIds = IsLoggedIn
                ? await _wishlistService.GetWishlistedProductIdsAsync()
                : new HashSet<int>()
        };

        return View(model);
    }

    private void SetProductSeoMetadata(ProductViewModel product)
    {
        ViewData["Title"] = product.Name;
        ViewData["Robots"] = "index, follow";

        // Use the product description for search engine snippets.
        var description = product.Description ?? string.Empty;

        description = System.Net.WebUtility.HtmlDecode(
            System.Text.RegularExpressions.Regex.Replace(
                description,
                "<[^>]*>",
                " "));

        description = System.Text.RegularExpressions.Regex.Replace(
            description,
            @"\s+",
            " ").Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            description = $"Discover {product.Name} in our online store.";
        }

        const int maxDescriptionLength = 160;

        if (description.Length > maxDescriptionLength)
        {
            description = description[..157].TrimEnd() + "...";
        }

        ViewData["MetaDescription"] = description;

        // Prefer the SEO-friendly product URL as the canonical URL.
        var canonicalUrl = !string.IsNullOrWhiteSpace(product.Slug)
            ? Url.Action(
                nameof(DetailsBySlug),
                "Products",
                new { slug = product.Slug },
                Request.Scheme)
            : Url.Action(
                nameof(Details),
                "Products",
                new { id = product.Id },
                Request.Scheme);

        ViewData["CanonicalUrl"] = canonicalUrl;
    }


    // ============================================================
    // PRODUCT DETAILS - EXISTING NUMERIC URL
    // Example: /Products/Details/5
    // ============================================================

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound();

        return await BuildDetailsPageAsync(product);
    }

    // ============================================================
    // PRODUCT DETAILS - SEO-FRIENDLY SLUG URL
    // Example: /products/wireless-headphones
    // ============================================================

    [HttpGet("/products/{slug}")]
    public async Task<IActionResult> DetailsBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var product = await _productService.GetProductBySlugAsync(slug);

        if (product == null)
            return NotFound();

        return await BuildDetailsPageAsync(product);
    }


    private async Task<IActionResult> BuildDetailsPageAsync(
        ProductViewModel product)
    {
        var id = product.Id;

        SetProductSeoMetadata(product);

        var reviewsPage = await _reviewService.GetProductReviewsAsync(
            id, page: 1, pageSize: 20);

        var summary = await _reviewService.GetReviewSummaryAsync(id);

        var myReview = IsLoggedIn
            ? await _reviewService.GetMyReviewForProductAsync(id)
            : null;

        // Recently Viewed

        if (IsLoggedIn)
        {
            try
            {
                await _recentlyViewedService.AddAsync(id);
            }
            catch
            {
                // Recently viewed should not prevent the product page
                // from loading if its service is temporarily unavailable.
            }
        }
        else
        {
            GuestRecentlyViewedHelper.Add(
                HttpContext.Session,
                new RecentlyViewedDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    ThumbnailUrl = product.ThumbnailUrl,
                    AverageRating = summary.AverageRating,
                    ReviewCount = summary.TotalReviews,
                    IsOnSale = product.IsOnSale,
                    SalePrice = product.SalePrice,
                    SaleStartDate = product.SaleStartDate,
                    SaleEndDate = product.SaleEndDate,
                    EffectivePrice = product.EffectivePrice,
                    CampaignDiscount = product.CampaignDiscount,
                    HasCampaign = product.HasCampaign,
                    CampaignName = product.CampaignName,
                    FinalPrice = product.FinalPrice,
                    TotalDiscountAmount = product.TotalDiscountAmount
                });
        }

        List<RecentlyViewedDto> recentlyViewed;

        if (IsLoggedIn)
        {
            try
            {
                recentlyViewed = await _recentlyViewedService.GetAsync();
            }
            catch
            {
                recentlyViewed = new List<RecentlyViewedDto>();
            }
        }
        else
        {
            recentlyViewed = GuestRecentlyViewedHelper.Get(
                HttpContext.Session);
        }

        recentlyViewed = recentlyViewed
            .Where(x => x.Id != id)
            .Take(4)
            .ToList();

        // --------------------------------------------------------
        // Related Products
        // --------------------------------------------------------

        var related = await _productService.GetRelatedProductsAsync(id);

        // --------------------------------------------------------
        // Build Product Details ViewModel
        // --------------------------------------------------------

        var model = new ProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = related,
            RecentlyViewed = recentlyViewed,

            ReviewSummary = new ReviewSummaryViewModel
            {
                AverageRating = summary.AverageRating,
                TotalReviews = summary.TotalReviews,
                RatingBreakdown = summary.RatingBreakdown
            },

            Reviews = reviewsPage.Items
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    ImageUrl = r.ImageUrl,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    CreatedAt = r.CreatedAt
                })
                .ToList(),

            CanReview = IsLoggedIn && myReview == null,

            MyReview = myReview == null
                ? null
                : new ReviewViewModel
                {
                    Id = myReview.Id,
                    UserName = myReview.UserName,
                    Rating = myReview.Rating,
                    Title = myReview.Title,
                    Comment = myReview.Comment,
                    ImageUrl = myReview.ImageUrl,
                    IsVerifiedPurchase = myReview.IsVerifiedPurchase,
                    CreatedAt = myReview.CreatedAt
                }
        };

        ViewBag.IsWishlisted =
            IsLoggedIn &&
            (await _wishlistService.GetWishlistedProductIdsAsync())
                .Contains(id);

        return View("Details", model);
    }

    // ============================================================
    // SUBMIT REVIEW
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> SubmitReview(
        CreateReviewViewModel model)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        "Details",
                        new { id = model.ProductId })
                });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] =
                "Please check your review and try again.";

            return RedirectToAction(
                "Details",
                new { id = model.ProductId });
        }

        try
        {
            await _reviewService.CreateReviewAsync(
                new CreateReviewDto
                {
                    ProductId = model.ProductId,
                    Rating = model.Rating,
                    Title = model.Title,
                    Comment = model.Comment
                },
                model.Image);

            TempData["Success"] = "Thanks for your review!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(
            "Details",
            new { id = model.ProductId });
    }

    // ============================================================
    // EDIT REVIEW
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> EditReview(
        int reviewId,
        int productId,
        CreateReviewViewModel model)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        "Details",
                        new { id = productId })
                });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] =
                "Please check your review and try again.";

            return RedirectToAction(
                "Details",
                new { id = productId });
        }

        try
        {
            await _reviewService.UpdateReviewAsync(
                reviewId,
                new CreateReviewDto
                {
                    ProductId = productId,
                    Rating = model.Rating,
                    Title = model.Title,
                    Comment = model.Comment
                },
                model.Image);

            TempData["Success"] = "Your review was updated.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(
            "Details",
            new { id = productId });
    }

    // ============================================================
    // DELETE REVIEW
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> DeleteReview(
        int reviewId,
        int productId)
    {
        if (!IsLoggedIn)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl = Url.Action(
                        "Details",
                        new { id = productId })
                });
        }

        try
        {
            await _reviewService.DeleteReviewAsync(reviewId);

            TempData["Success"] = "Your review was deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(
            "Details",
            new { id = productId });
    }
}