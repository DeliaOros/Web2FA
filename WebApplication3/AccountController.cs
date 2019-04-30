using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        public AccountController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _authService.Login(model.Username, model.Password);
                    if (null != user)
                    {
                        var userClaims = new List<Claim>
                    {
                        new Claim("displayName", user.DisplayName),
                        new Claim("username", user.Username)
                    };
                        if (user.IsAdmin)
                        {
                            userClaims.Add(new Claim(ClaimTypes.Role, "Admins"));
                        }
                        var principal = new ClaimsPrincipal(new ClaimsIdentity(userClaims, _authService.GetType().Name));
                        await HttpContext.Authentication.SignInAsync("app", principal);
                        return Redirect("/");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(model);
        }

        //[Authorize(Roles = UserRoles.Everyone)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("app");
            return Redirect("/");
        }
    }
}

