using EJIntegral.Core.Data;
using EJIntegral.Core.Models.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class StaffImagesController(LegacyDbContext db, IWebHostEnvironment env) : Controller
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".bmp"
    };
    private const long MaxImageBytes = 5 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await db.StaffImages
            .OrderByDescending(x => x.Id)
            .Take(200)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StaffImage());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffImage model, IFormFile? file)
    {
        var staffId = HttpContext.Session.GetString("CurrentStaffId");
        if (string.IsNullOrWhiteSpace(staffId))
        {
            ModelState.AddModelError(string.Empty, "No active staff session. Please restart registration.");
            return View(model);
        }

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(model.ImgPath), "Please select an image.");
            return View(model);
        }
        if (!ValidateImage(file))
        {
            return View(model);
        }

        var capturesDir = Path.Combine(env.WebRootPath, "Content", "Captures");
        Directory.CreateDirectory(capturesDir);

        var safeName = $"{staffId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(capturesDir, safeName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        db.StaffImages.Add(new StaffImage
        {
            StaffId = staffId,
            ImgPath = safeName
        });
        await db.SaveChangesAsync();

        return RedirectToAction("Index", "Biometric", new { staffId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await db.StaffImages.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.StaffImages.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StaffImage model, IFormFile? file)
    {
        if (id != model.Id) return BadRequest();
        var item = await db.StaffImages.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        if (file is not null && file.Length > 0)
        {
            if (!ValidateImage(file))
            {
                return View(model);
            }

            var capturesDir = Path.Combine(env.WebRootPath, "Content", "Captures");
            Directory.CreateDirectory(capturesDir);
            var safeName = $"{item.StaffId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(capturesDir, safeName);
            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);
            item.ImgPath = safeName;
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.StaffImages.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await db.StaffImages.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        db.StaffImages.Remove(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ValidateImage(IFormFile file)
    {
        if (file.Length > MaxImageBytes)
        {
            ModelState.AddModelError(nameof(StaffImage.ImgPath), "Image exceeds 5MB limit.");
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
        {
            ModelState.AddModelError(nameof(StaffImage.ImgPath), $"Image type '{extension}' is not allowed.");
            return false;
        }

        return true;
    }
}
