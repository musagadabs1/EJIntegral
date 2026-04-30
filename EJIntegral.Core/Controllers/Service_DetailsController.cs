using EJIntegral.Core.Data;
using EJIntegral.Core.Models.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class Service_DetailsController(LegacyDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var data = await db.ServiceDetails
            .OrderByDescending(x => x.CreatedOn)
            .Take(300)
            .ToListAsync();
        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new ServiceDetails { DateOfFirstAppt = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceDetails model)
    {
        var staffId = HttpContext.Session.GetString("CurrentStaffId") ?? TempData.Peek("StaffId")?.ToString();
        if (string.IsNullOrWhiteSpace(staffId))
        {
            ModelState.AddModelError(string.Empty, "Staff session expired. Please create staff record again.");
        }
        ValidateServiceDetailsModel(model);

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        model.StaffId = staffId;
        model.CreatedBy = User.Identity?.Name ?? "system";
        model.CreatedOn = DateTime.UtcNow;
        db.ServiceDetails.Add(model);
        await db.SaveChangesAsync();

        return RedirectToAction("Create", "Documents");
    }

    [HttpPost]
    public async Task<IActionResult> GetDesignationList([FromBody] DepartmentRequest request)
    {
        var data = await db.Designations
            .Where(x => x.DeparmentId == request.deptId)
            .OrderBy(x => x.DesignationName)
            .Select(x => new { x.Id, x.DesignationName })
            .ToListAsync();
        return Json(data);
    }

    [HttpPost]
    public async Task<IActionResult> GetSalary([FromBody] SalaryRequest request)
    {
        var salary = await db.EmpSalaries
            .Where(x => x.Grade == request.gradeId && x.Step == request.stepId)
            .Select(x => x.Salary)
            .FirstOrDefaultAsync();
        return Json(salary ?? 0);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var item = await db.ServiceDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.ServiceDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        await PopulateDropdownsAsync();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceDetails model)
    {
        if (id != model.Id) return BadRequest();
        ValidateServiceDetailsModel(model);
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var item = await db.ServiceDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.FileNo = model.FileNo;
        item.Entry_Rank = model.Entry_Rank;
        item.DateOfFirstAppt = model.DateOfFirstAppt;
        item.Entry_GradeLevel = model.Entry_GradeLevel;
        item.Deparment = model.Deparment;
        item.Designation = model.Designation;
        item.BankName = model.BankName;
        item.AccountNumber = model.AccountNumber;
        item.BVN = model.BVN;
        item.ConsolidatedSalary = model.ConsolidatedSalary;
        item.PlaceOfPosting = model.PlaceOfPosting;
        item.ConfirmationDate = model.ConfirmationDate;
        item.YearOfLastPromotion = model.YearOfLastPromotion;
        item.Qualification = model.Qualification;
        item.Step = model.Step;
        item.Allowances = model.Allowances;
        item.CurrentRank = model.CurrentRank;
        item.CurrentQualification = model.CurrentQualification;
        item.CurrentGradeLevel = model.CurrentGradeLevel;
        item.CurrentStep = model.CurrentStep;
        item.SignatoryToApptLetter = model.SignatoryToApptLetter;
        item.CurrentConsolidatedSalary = model.CurrentConsolidatedSalary;
        item.CurrentDesignation = model.CurrentDesignation;
        item.ModifiedBy = User.Identity?.Name ?? "system";
        item.ModifiedOn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.ServiceDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await db.ServiceDetails.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        var hasRelatedDocuments = await db.Documents.AnyAsync(x => x.StaffId == item.StaffId);
        var hasRelatedImages = await db.StaffImages.AnyAsync(x => x.StaffId == item.StaffId);
        if (hasRelatedDocuments || hasRelatedImages)
        {
            TempData["Error"] = "Cannot delete service details while related documents or images exist.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        db.ServiceDetails.Remove(item);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateServiceDetailsModel(ServiceDetails model)
    {
        if (string.IsNullOrWhiteSpace(model.FileNo))
            ModelState.AddModelError(nameof(model.FileNo), "File number is required.");

        if (model.DateOfFirstAppt > DateTime.UtcNow.Date.AddDays(1))
            ModelState.AddModelError(nameof(model.DateOfFirstAppt), "Date of first appointment cannot be in the future.");

        if (model.YearOfLastPromotion.HasValue)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (model.YearOfLastPromotion < 1950 || model.YearOfLastPromotion > currentYear)
                ModelState.AddModelError(nameof(model.YearOfLastPromotion), "Year of last promotion is out of valid range.");
        }

        if (!string.IsNullOrWhiteSpace(model.BVN))
        {
            var bvnDigits = new string(model.BVN.Where(char.IsDigit).ToArray());
            if (bvnDigits.Length != 11)
                ModelState.AddModelError(nameof(model.BVN), "BVN must be 11 digits.");
        }
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Departments = await db.Departments
            .OrderBy(x => x.DepartmentName)
            .Select(x => new SelectListItem(x.DepartmentName!, x.Id.ToString()))
            .ToListAsync();

        ViewBag.GradeLevel = Enumerable.Range(1, 16)
            .Select(x => new SelectListItem($"GL{x:00}", x.ToString()))
            .ToList();

        ViewBag.Step = Enumerable.Range(1, 15)
            .Select(x => new SelectListItem($"STEP{x:00}", x.ToString()))
            .ToList();

        ViewBag.Qualification = new List<SelectListItem>
        {
            new("FSLC", "FSLC"),
            new("SSCE", "SSCE"),
            new("DIPLOMA", "DIPLOMA"),
            new("NATIONAL DIPLOMA", "ND"),
            new("HND", "HND"),
            new("BSC", "Bsc"),
            new("PGD", "PGD"),
            new("MSC", "Msc"),
            new("Phd", "Phd")
        };
    }

    public class DepartmentRequest
    {
        public int deptId { get; set; }
    }

    public class SalaryRequest
    {
        public int gradeId { get; set; }
        public int stepId { get; set; }
    }
}
