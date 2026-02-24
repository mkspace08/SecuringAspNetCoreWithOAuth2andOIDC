using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using static System.Net.WebRequestMethods;

namespace MyTestDev.IDP;

/// <summary>
/// Central configuration for IdentityServer resources, scopes, and clients.
/// </summary>
public static class Config
{
    // Identity resources represent information (claims) about the user that can be included in identity tokens.
    // Standard resources: OpenId (subject id), Profile (name, etc.), plus custom ones like roles and country.
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        { 
            // Required for OpenID Connect authentication (subject id claim)
            new IdentityResources.OpenId(),
            // Includes standard profile claims (name, family_name, etc.)
            new IdentityResources.Profile(),
            // Custom identity resource for user roles (adds "role" claim to id token if requested)
            new IdentityResource("roles", "Your roles", new [] { "role" }),
            // Custom identity resource for user's country (adds "country" claim to id token if requested)
            new IdentityResource("country", "The country you're living in", new List<string> { "country" }),
        };

    // API resources represent protected APIs. Each API resource can have associated claims and allowed scopes.
    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            // Defines the "imagegalleryapi" as a protected API.
            // The API expects "role" and "country" claims in access tokens.
            // The API supports the listed scopes for granular access control.
            new ApiResource("imagegalleryapi", "Image Gallery API", new [] { "role", "country" })
            {
                // Scopes define the operations/permissions available for this API.
                Scopes = { "imagegalleryapi.read", "imagegalleryapi.write" },
            }
        };

    // API scopes define specific permissions that clients can request for APIs.
    // Each scope should be explicitly listed here to be available for clients.
    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
            {
                // Allows read-only access to the Image Gallery API.
                new ApiScope("imagegalleryapi.read"),
                // Allows write access to the Image Gallery API.
                new ApiScope("imagegalleryapi.write"),
            };

    // Clients represent applications that can request tokens from IdentityServer.
    public static IEnumerable<Client> Clients =>
        new Client[] 
            {
                new Client()
                {
                    // Human-readable name for the client application.
                    ClientName = "Image Gallery",
                    // Unique identifier for the client.
                    ClientId = "imagegalleryclient",
                    // Authorization flow: Authorization Code (recommended for server-side web apps).
                    AllowedGrantTypes = GrantTypes.Code,
                    // Controls whether user claims are always included in the id token.
                    AlwaysIncludeUserClaimsInIdToken = false,
                    // Where to redirect after successful login.
                    RedirectUris =
                    {
                        "https://localhost:7184/signin-oidc"  // Default OIDC redirect URI
                    },
                    // Where to redirect after logout.
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:7184/signout-callback-oidc",
                        // Add more URIs if needed
                    },
                    // List of scopes the client is allowed to request.
                    // Must match both ApiScopes and IdentityResources as needed.
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,   // Required for OIDC
                        IdentityServerConstants.StandardScopes.Profile,  // Standard profile info
                        "roles",                                        // Custom identity resource
                        "imagegalleryapi.read",                         // API read permission
                        "imagegalleryapi.write",                        // API write permission
                        "country",                                      // Custom identity resource
                    },
                    // Secret used for client authentication (should be stored securely in production).
                    ClientSecrets =
                    {
                        new Secret("ClientSecret123".Sha256())
                    },
                    // If true, users will be prompted for consent when logging in.
                    RequireConsent = true,
                }
            };
}