using EJIntegral.Core.Data;
using EJIntegral.Core.Models.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class DocumentsController(LegacyDbContext db, IWebHostEnvironment env) : Controller
{
    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"
    };
    private const long MaxDocumentBytes = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await db.Documents
            .OrderByDescending(x => x.CreatedOn)
            .Take(200)
            .ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Document());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document model, List<IFormFile> fileBase)
    {
        var staffId = HttpContext.Session.GetString("CurrentStaffId");
        if (string.IsNullOrWhiteSpace(staffId))
        {
            ModelState.AddModelError(string.Empty, "No active staff session. Please restart registration.");
            return View(model);
        }

        if (fileBase.Count == 0)
        {
            ModelState.AddModelError(nameof(model.DocPath), "Please attach at least one document.");
            return View(model);
        }

        if (!ValidateDocumentFiles(fileBase))
        {
            return View(model);
        }

        var docsDir = Path.Combine(env.WebRootPath, "Content", "Docs");
        Directory.CreateDirectory(docsDir);

        foreach (var file in fileBase.Where(f => f.Length > 0))
        {
            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(docsDir, safeName);
            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            db.Documents.Add(new Document
            {
                Description = model.Description,
                DocPath = safeName,
                StaffId = staffId,
                CreatedBy = User.Identity?.Name ?? "system",
                CreatedOn = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return RedirectToAction("Create", "StaffImages");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Document model, IFormFile? fileBase)
    {
        if (id != model.Id) return BadRequest();
        var item = await db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.Description = model.Description;
        item.ModifiedBy = User.Identity?.Name ?? "system";
        item.ModifiedOn = DateTime.UtcNow;

        if (fileBase is not null && fileBase.Length > 0)
        {
            if (!ValidateDocumentFiles(new List<IFormFile> { fileBase }))
            {
                return View(model);
            }

            var docsDir = Path.Combine(env.WebRootPath, "Content", "Docs");
            Directory.CreateDirectory(docsDir);
            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileBase.FileName)}";
            var fullPath = Path.Combine(docsDir, safeName);
            await using var stream = System.IO.File.Create(fullPath);
            await fileBase.CopyToAsync(stream);
            item.DocPath = safeName;
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await db.Documents.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        db.Documents.Remove(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ValidateDocumentFiles(IEnumerable<IFormFile> files)
    {
        var validFiles = files.Where(f => f is { Length: > 0 }).ToList();
        if (validFiles.Count == 0)
        {
            ModelState.AddModelError(nameof(Document.DocPath), "Please attach at least one valid document.");
            return false;
        }

        foreach (var file in validFiles)
        {
            if (file.Length > MaxDocumentBytes)
            {
                ModelState.AddModelError(nameof(Document.DocPath), $"File '{file.FileName}' exceeds 10MB limit.");
                return false;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedDocumentExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(Document.DocPath), $"File type '{extension}' is not allowed.");
                return false;
            }
        }

        return true;
    }
}
