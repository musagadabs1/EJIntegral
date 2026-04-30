using EJIntegral.Core.Data;
using EJIntegral.Core.Models.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class Staff_DetailsController(LegacyDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var data = await db.StaffDetails
            .Where(x => x.Status != false)
            .OrderByDescending(x => x.CreatedOn)
            .ToListAsync();
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new StaffDetails());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffDetails model)
    {
        ValidateStaffDetailsModel(model, isCreate: true);
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var normalizedStaffId = model.StaffId.Trim().ToUpperInvariant();
        var exists = await db.StaffDetails.AnyAsync(x => x.StaffId.ToUpper() == normalizedStaffId && x.Status != false);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.StaffId), "Staff with this ID already exists.");
            await PopulateDropdownsAsync();
            return View(model);
        }

        model.StaffId = normalizedStaffId;
        model.CreatedBy = User.Identity?.Name ?? "system";
        model.CreatedOn = DateTime.UtcNow;
        model.StateOfOrigin = string.IsNullOrWhiteSpace(model.StateOfOrigin) ? "Nasawara" : model.StateOfOrigin;
        model.Status = true;

        db.StaffDetails.Add(model);
        await db.SaveChangesAsync();
        HttpContext.Session.SetString("CurrentStaffId", model.StaffId);
        TempData["StaffId"] = model.StaffId;
        return RedirectToAction("Create", "Service_Details");
    }

    [HttpPost]
    public async Task<IActionResult> GetDAList([FromBody] GetDaRequest request)
    {
        var lgaId = request?.lgId ?? 0;
        var areas = await db.DevelopmentAreas
            .Where(x => x.lgaId == lgaId)
            .OrderBy(x => x.development_area_name)
            .Select(x => new { x.Id, x.development_area_name })
            .ToListAsync();
        return Json(areas);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await db.StaffDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.StaffDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        await PopulateDropdownsAsync();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StaffDetails model)
    {
        if (id != model.Id) return BadRequest();
        ValidateStaffDetailsModel(model, isCreate: false);
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var existing = await db.StaffDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (existing is null) return NotFound();

        existing.StaffId = model.StaffId.ToUpperInvariant();
        existing.FirstName = model.FirstName;
        existing.MiddleName = model.MiddleName;
        existing.LastName = model.LastName;
        existing.Gender = model.Gender;
        existing.DateOfBirth = model.DateOfBirth;
        existing.ResidenceAddress = model.ResidenceAddress;
        existing.MaritalStatus = model.MaritalStatus;
        existing.ContactPhone = model.ContactPhone;
        existing.EmailAddress = model.EmailAddress;
        existing.LGA = model.LGA;
        existing.NFullName = model.NFullName;
        existing.NAddress = model.NAddress;
        existing.NRelationship = model.NRelationship;
        existing.NContactNumber = model.NContactNumber;
        existing.DevArea = model.DevArea;
        existing.ModifiedBy = User.Identity?.Name ?? "system";
        existing.ModifiedOn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.StaffDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await db.StaffDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        item.Status = false;
        item.ModifiedBy = User.Identity?.Name ?? "system";
        item.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateStaffDetailsModel(StaffDetails model, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(model.StaffId))
        {
            ModelState.AddModelError(nameof(model.StaffId), "Staff ID is required.");
        }
        else
        {
            var normalized = model.StaffId.Trim();
            if (normalized.Length > 32 || !normalized.All(c => char.IsLetterOrDigit(c) || c is '-' or '_'))
            {
                ModelState.AddModelError(nameof(model.StaffId), "Staff ID must be alphanumeric and may include '-' or '_'.");
            }
        }

        if (string.IsNullOrWhiteSpace(model.FirstName))
            ModelState.AddModelError(nameof(model.FirstName), "First name is required.");
        if (string.IsNullOrWhiteSpace(model.LastName))
            ModelState.AddModelError(nameof(model.LastName), "Last name is required.");
        if (string.IsNullOrWhiteSpace(model.ContactPhone))
            ModelState.AddModelError(nameof(model.ContactPhone), "Contact phone is required.");

        if (!string.IsNullOrWhiteSpace(model.EmailAddress) && !model.EmailAddress.Contains('@'))
            ModelState.AddModelError(nameof(model.EmailAddress), "Email format is invalid.");

        if (isCreate && model.DateOfBirth.HasValue && model.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
            ModelState.AddModelError(nameof(model.DateOfBirth), "Date of birth cannot be in the future.");
    }

    private async Task PopulateDropdownsAsync()
    {
        var lgas = await db.LocalGovernments
            .OrderBy(x => x.LGAName)
            .Select(x => new SelectListItem(x.LGAName!, x.Id.ToString()))
            .ToListAsync();

        ViewBag.LGA = lgas;
        ViewBag.Gender = new List<SelectListItem>
        {
            new("Female", "F"),
            new("Male", "M")
        };
        ViewBag.MaritalStatus = new List<SelectListItem>
        {
            new("Divorce", "Divorce"),
            new("Married", "Married"),
            new("Single", "Single"),
            new("Widow", "Widow"),
            new("Widower", "Widower")
        };
        ViewBag.Relationship = new List<SelectListItem>
        {
            new("Brother", "Brother"),
            new("Father", "Father"),
            new("Sister", "Sister"),
            new("Mother", "Mother"),
            new("Uncle", "Uncle"),
            new("Son", "Son"),
            new("Daughter", "Daughter"),
            new("Husband", "Husband"),
            new("Wife", "Wife")
        };
    }

    public class GetDaRequest
    {
        public int lgId { get; set; }
    }
}
