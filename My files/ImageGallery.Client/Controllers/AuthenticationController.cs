using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.Client.Controllers
{
    [Authorize]  // This controller requires the user to be authenticated
    public class AuthenticationController : Controller
    {
        public async Task Logout()
        {
            //Clears the local cookie, this logouts only from the client, in IdP the user is still logged in
            //, so if the user tries to log in again, it will be logged in without entering credentials
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirects to the IdP to clear its own session/cookie
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
