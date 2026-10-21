using ECommerce.Web.DTOs;
using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductsController : Controller
{
    private readonly IAdminProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductsController(
        IAdminProductService productService,
        ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(
        string? search,
        int page = 1)
    {
        var result = await _productService.SearchAsync(
            new ProductSearchDto
            {
                Search = search,
                Page = page,
                PageSize = 10
            });

        ViewBag.Search = search;
        ViewBag.CurrentPage = result.CurrentPage;
        ViewBag.TotalPages = result.TotalPages;

        return View(result.Items);
    }

    public async Task<IActionResult> Create()
    {
        var model = new AdminCreateProductViewModel
        {
            Categories = await _categoryService.GetAllAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminCreateProductViewModel model)
    {
        // 1. MVC validation
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            return View("Create", model);
        }

        // 2. Create the product through the API
        AdminProductDto createdProduct;

        try
        {
            createdProduct = await _productService.CreateAsync(
                model.Product,
                model.ImageFile);
        }
        catch (ApiException ex)
        {
            // API message, e.g. "A product with SKU 'X' already exists."
            AddProductError(ex.Message);
            model.Categories = await _categoryService.GetAllAsync();
            return View("Create", model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The product could not be saved. Please check the values and try again.");

            model.Categories = await _categoryService.GetAllAsync();
            return View("Create", model);
        }

        // 3. Upload gallery images.
        // The product already exists at this point, so a failure here must NOT
        // send the user back to the Create form (re-submitting would fail with
        // "SKU already exists"). Send them to Edit instead.
        try
        {
            await UploadGalleryImagesAsync(createdProduct.Id, model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] =
                "Product was created, but some gallery images failed to upload. You can add them here.";

            return RedirectToAction(nameof(Edit), new { id = createdProduct.Id });
        }

        // 4. Success
        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        var model = new AdminEditProductViewModel
        {
            Id = id,
            Product = product,
            Categories = await _categoryService.GetAllAsync(),
            Images = await _productService.GetImagesAsync(id)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int id,
        AdminEditProductViewModel model)
    {
        // 1. MVC validation
        if (!ModelState.IsValid)
        {
            await ReloadEditModelAsync(id, model);
            return View("Edit", model);
        }

        // 2. Update through the API
        try
        {
            await _productService.UpdateAsync(
                id,
                model.Product,
                model.ImageFile);
        }
        catch (ApiException ex)
        {
            AddProductError(ex.Message);
            await ReloadEditModelAsync(id, model);
            return View("Edit", model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The product could not be saved. Please check the values and try again.");

            await ReloadEditModelAsync(id, model);
            return View("Edit", model);
        }

        // 3. Success
        TempData["SuccessMessage"] = "Product updated successfully.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(
        int productId,
        List<IFormFile> imageFiles,
        string? altText)
    {
        if (imageFiles != null && imageFiles.Any())
        {
            foreach (var file in imageFiles)
            {
                if (file.Length > 0)
                {
                    var newImage = new AdminCreateProductImageViewModel
                    {
                        ImageFile = file,
                        AltText = altText ?? string.Empty
                    };

                    await _productService.AddImageAsync(productId, newImage);
                }
            }
        }

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(
        int productId,
        int imageId)
    {
        await _productService.DeleteImageAsync(imageId);

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    public async Task<IActionResult> SetPrimaryImage(
        int productId,
        int imageId)
    {
        await _productService.SetPrimaryImageAsync(imageId);

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImageAltText(
        int productId,
        int imageId,
        string? altText)
    {
        await _productService.UpdateImageAltTextAsync(imageId, altText ?? string.Empty);

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    /// <summary>
    /// Puts an API error message next to the field it belongs to.
    /// </summary>
    private void AddProductError(string message)
    {
        if (message.Contains("SKU", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Product.SKU", message);
        }
        else if (message.Contains("sale price", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Product.SalePrice", message);
        }
        else if (message.Contains("sale end", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Product.SaleEndDate", message);
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
        }
    }

    /// <summary>
    /// Re-populates everything the Edit view needs when it is shown again
    /// after a validation or API error.
    /// </summary>
    private async Task ReloadEditModelAsync(int id, AdminEditProductViewModel model)
    {
        model.Categories = await _categoryService.GetAllAsync();
        model.Images = await _productService.GetImagesAsync(id);

        // ImageUrl is not posted by the Edit form because the current cover
        // is managed through the gallery. Restore it so it does not disappear.
        var existing = await _productService.GetByIdAsync(id);

        if (existing != null)
        {
            model.Product.ImageUrl = existing.ImageUrl;
        }
    }

    private async Task UploadGalleryImagesAsync(
        int productId,
        AdminCreateProductViewModel model)
    {
        if (model.GalleryImages == null || !model.GalleryImages.Any())
            return;

        for (int i = 0; i < model.GalleryImages.Count; i++)
        {
            var file = model.GalleryImages[i];

            if (file == null || file.Length <= 0)
                continue;

            var altText =
                model.GalleryAltTexts != null && i < model.GalleryAltTexts.Count
                    ? model.GalleryAltTexts[i]
                    : string.Empty;

            if (string.IsNullOrWhiteSpace(altText))
            {
                altText = model.Product.Name ?? string.Empty;
            }

            await _productService.AddImageAsync(
                productId,
                new AdminCreateProductImageViewModel
                {
                    ImageFile = file,
                    AltText = altText
                });
        }
    }
}