using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using static System.Net.WebRequestMethods;

namespace MyTestDev.IDP;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "Your roles", new [] { "role" }),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("imagegalleryapi", "Image Gallery API")
            {
                Scopes = { "imagegalleryapi.fullaccess" }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                new ApiScope("imagegalleryapi.fullaccess"),
            };

    public static IEnumerable<Client> Clients =>
        new Client[] 
            {
                new Client()
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    AlwaysIncludeUserClaimsInIdToken = false,
                    RedirectUris =
                    {
                        "https://localhost:7184/signin-oidc"  //signin-oidc - it is default value
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:7184/signout-callback-oidc",
                        // ...any others you want
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "imagegalleryapi.fullaccess",
                    },
                    ClientSecrets =
                    {
                        new Secret("ClientSecret123".Sha256())
                    },
                    RequireConsent = true,
                    
                    
                }
            };
}