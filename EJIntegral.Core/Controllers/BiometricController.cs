using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EJIntegral.Core.Controllers;

[Authorize]
public class BiometricController : Controller
{
    [HttpGet]
    public IActionResult Index(string? staffId)
    {
        ViewData["StaffId"] = staffId;
        return View();
    }

    [HttpGet]
    public IActionResult ImageCapture(string? staffId)
    {
        ViewData["StaffId"] = staffId;
        return View();
    }
}
