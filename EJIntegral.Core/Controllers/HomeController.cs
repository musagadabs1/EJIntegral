using System.Diagnostics;
using EJIntegral.Core.Data;
using EJIntegral.Core.Models.Legacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EJIntegral.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LegacyDbContext _legacyDb;

    public HomeController(ILogger<HomeController> logger, LegacyDbContext legacyDb)
    {
        _logger = logger;
        _legacyDb = legacyDb;
    }

    public async Task<IActionResult> Index(string? staffId)
    {
        var normalizedStaffId = string.IsNullOrWhiteSpace(staffId) ? null : staffId.Trim();
        var data = await _legacyDb.EmployeeInfoWithPhotos
            .FromSqlInterpolated($"EXEC GetEmployeeInfoWithPhoto {normalizedStaffId}")
            .ToListAsync();
        return View(data);
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
