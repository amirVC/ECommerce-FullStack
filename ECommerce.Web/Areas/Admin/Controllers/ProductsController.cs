using ECommerce.Web.DTOs;
using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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

        var effectivePageSize = result.PageSize > 0 ? result.PageSize : 10;
        ViewBag.RowOffset = (Math.Max(result.CurrentPage, 1) - 1) * effectivePageSize;

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
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            return View("Create", model);
        }

        AdminProductDto createdProduct;

        try
        {
            createdProduct = await _productService.CreateAsync(
                model.Product,
                model.ImageFile);
        }
        catch (ApiException ex)
        {
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

        try
        {
            await UploadGalleryImagesAsync(createdProduct.Id, model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] =
                "Product was created, but some gallery images failed to upload. You can add them here.";

            return string.IsNullOrWhiteSpace(createdProduct.Slug)
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(Edit), new { slug = createdProduct.Slug });
        }

        TempData["SuccessMessage"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Admin/Products/Edit/{slug}")]
    public async Task<IActionResult> Edit(string slug)
    {
        var product = await _productService.GetBySlugAsync(slug);

        if (product == null)
            return NotFound();

        var model = new AdminEditProductViewModel
        {
            Id = product.Id,
            OriginalSlug = slug,
            Product = product,
            Categories = await _categoryService.GetAllAsync(),
            Images = await _productService.GetImagesAsync(product.Id)
        };

        return View(model);
    }

    [HttpPost("Admin/Products/Edit/{slug}")]
    public async Task<IActionResult> Edit(
        string slug,
        AdminEditProductViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReloadEditViewAsync(slug, model);
        }

        AdminProductDto updated;

        try
        {
            updated = await _productService.UpdateAsync(
                slug,
                model.Product,
                model.ImageFile);
        }
        catch (ApiException ex)
        {
            AddProductError(ex.Message);
            return await ReloadEditViewAsync(slug, model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The product could not be saved. Please check the values and try again.");

            return await ReloadEditViewAsync(slug, model);
        }

        TempData["SuccessMessage"] = "Product updated successfully.";

        var currentSlug = string.IsNullOrWhiteSpace(updated.Slug) ? slug : updated.Slug;

        return RedirectToAction(nameof(Edit), new { slug = currentSlug });
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

        return await RedirectToEditAsync(productId);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(
        int productId,
        int imageId)
    {
        await _productService.DeleteImageAsync(imageId);

        return await RedirectToEditAsync(productId);
    }

    [HttpPost]
    public async Task<IActionResult> SetPrimaryImage(
        int productId,
        int imageId)
    {
        await _productService.SetPrimaryImageAsync(imageId);

        return await RedirectToEditAsync(productId);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImageAltText(
        int productId,
        int imageId,
        string? altText)
    {
        await _productService.UpdateImageAltTextAsync(imageId, altText ?? string.Empty);

        return await RedirectToEditAsync(productId);
    }

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
        else if (message.Contains("slug", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Product.Slug", message);
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
        }
    }

    private async Task<IActionResult> ReloadEditViewAsync(
        string slug,
        AdminEditProductViewModel model)
    {
        var existing = await _productService.GetBySlugAsync(slug);

        if (existing == null)
            return NotFound();

        model.Id = existing.Id;
        model.OriginalSlug = slug;
        model.Categories = await _categoryService.GetAllAsync();
        model.Images = await _productService.GetImagesAsync(existing.Id);
        model.Product.ImageUrl = existing.ImageUrl;

        return View("Edit", model);
    }

    private async Task<IActionResult> RedirectToEditAsync(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);

        return string.IsNullOrWhiteSpace(product?.Slug)
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Edit), new { slug = product.Slug });
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