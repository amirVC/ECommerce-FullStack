using ECommerce.Web.DTOs;
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
public async Task<IActionResult> Create(
    AdminCreateProductViewModel model)
    {
        // ---------------------------------------------------------
        // 1. MVC validation
        // ---------------------------------------------------------

        if (!ModelState.IsValid)
        {
            model.Categories =
                await _categoryService.GetAllAsync();

            return View("Create", model);
        }


        // ---------------------------------------------------------
        // 2. Try to create the product through the API
        // ---------------------------------------------------------

        try
        {
            var createdProduct =
                await _productService.CreateAsync(
                    model.Product,
                    model.ImageFile);


            // -----------------------------------------------------
            // 3. Upload gallery images
            // -----------------------------------------------------

            if (model.GalleryImages != null &&
                model.GalleryImages.Any())
            {
                for (int i = 0;
                     i < model.GalleryImages.Count;
                     i++)
                {
                    var file = model.GalleryImages[i];

                    if (file == null || file.Length <= 0)
                        continue;

                    var altText =
                        model.GalleryAltTexts != null &&
                        i < model.GalleryAltTexts.Count
                            ? model.GalleryAltTexts[i]
                            : string.Empty;

                    if (string.IsNullOrWhiteSpace(altText))
                    {
                        altText =
                            model.Product.Name ??
                            string.Empty;
                    }

                    var galleryImageModel =
                        new AdminCreateProductImageViewModel
                        {
                            ImageFile = file,
                            AltText = altText
                        };

                    await _productService.AddImageAsync(
                        createdProduct.Id,
                        galleryImageModel);
                }
            }


            // -----------------------------------------------------
            // 4. Success
            // -----------------------------------------------------

            TempData["SuccessMessage"] =
                "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            // Friendly error instead of ASP.NET exception page.

            ModelState.AddModelError(
                "Product.SalePrice",
                "The sale price must be lower than the regular price.");

            model.Categories =
                await _categoryService.GetAllAsync();

            return View("Create", model);
        }
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
        // ---------------------------------------------------------
        // 1. Client-side / MVC validation
        // ---------------------------------------------------------

        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            model.Images = await _productService.GetImagesAsync(id);

            // ImageUrl is not posted by the Edit form because the
            // current cover is managed through the gallery.
            // Restore it so the cover does not disappear.
            var existing = await _productService.GetByIdAsync(id);

            if (existing != null)
            {
                model.Product.ImageUrl = existing.ImageUrl;
            }

            return View("Edit", model);
        }


        // ---------------------------------------------------------
        // 2. Try to update the product through the API
        // ---------------------------------------------------------

        try
        {
            await _productService.UpdateAsync(
                id,
                model.Product,
                model.ImageFile);
        }
        catch (HttpRequestException)
        {
            // The API rejected the data.
            // Show a friendly validation message instead of
            // displaying the ASP.NET exception page.

            ModelState.AddModelError(
                "Product.SalePrice",
                "The sale price must be lower than the regular price.");

            model.Categories = await _categoryService.GetAllAsync();
            model.Images = await _productService.GetImagesAsync(id);

            var existing = await _productService.GetByIdAsync(id);

            if (existing != null)
            {
                model.Product.ImageUrl = existing.ImageUrl;
            }

            return View("Edit", model);
        }


        // ---------------------------------------------------------
        // 3. Success
        // ---------------------------------------------------------

        TempData["SuccessMessage"] =
            "Product updated successfully.";

        return RedirectToAction(
            nameof(Edit),
            new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    // Now accepts multiple files, same pattern as the Create action's
    // GalleryImages loop, instead of being limited to a single ImageFile.
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
        // This calls the service method we updated previously
        await _productService.UpdateImageAltTextAsync(imageId, altText ?? string.Empty);

        // This redirects the user back to the edit page after the save
        return RedirectToAction(nameof(Edit), new { id = productId });
    }
}