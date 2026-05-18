using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    [HttpGet("Login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("Logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return SignOut(new AuthenticationProperties
        {
            RedirectUri = "/"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("AccessDenied")]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return Content("Access denied. You do not have permission to access this resource.");
    }

    [HttpGet("UserInfo")]
    [Authorize]
    public IActionResult UserInfo()
    {
        var claims = User.Claims
            .Select(claim => new
            {
                claim.Type,
                claim.Value
            })
            .ToList();

        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            Name = User.Identity?.Name,
            Claims = claims
        });
    }
}