using Microsoft.AspNetCore.Mvc;

namespace EJIntegral.Core.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return LocalRedirect($"/Identity/Account/Login{(string.IsNullOrWhiteSpace(returnUrl) ? string.Empty : $"?returnUrl={Uri.EscapeDataString(returnUrl)}")}");
    }
}
