using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController : Controller
{
    private readonly IAdminUserService _userService;

    public UsersController(IAdminUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();

        return View(users);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRole(
    int id,
    UpdateUserRoleDto dto)
    {
        await _userService.UpdateRoleAsync(id, dto);

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;

            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}