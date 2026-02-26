using IdentityModel.Client;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthenticationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task Logout()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var discoveryDocumentResponse = await idpClient.GetDiscoveryDocumentAsync();
            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(discoveryDocumentResponse.Error);
            }

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var revokeAccessTokenResponse = await idpClient.RevokeTokenAsync(new TokenRevocationRequest
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "ClientSecret123",
                    Token = accessToken
                });

                if (revokeAccessTokenResponse.IsError)
                {
                    throw new Exception(revokeAccessTokenResponse.Error);
                }
            }

            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var revokeRefreshTokenResponse = await idpClient.RevokeTokenAsync(new TokenRevocationRequest
                {
                    Address = discoveryDocumentResponse.RevocationEndpoint,
                    ClientId = "imagegalleryclient",
                    ClientSecret = "ClientSecret123",
                    Token = refreshToken
                });

                if (revokeRefreshTokenResponse.IsError)
                {
                    throw new Exception(revokeRefreshTokenResponse.Error);
                }
            }

            //comented only for revocation test
            //// Clears the local cookie, this logouts only from the client
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //// Redirects to the IdP to clear its own session/cookie
            //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
