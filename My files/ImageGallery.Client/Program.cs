using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(configure => 
        configure.JsonSerializerOptions.PropertyNamingPolicy = null);

//By default, ASP.NET Core (and the underlying Microsoft identity libraries)
//map certain standard JWT claim types (like sub, aud, role, etc.) to legacy
//or .NET-specific claim types (like http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier)
//Calling .Clear() on the DefaultInboundClaimTypeMap disables all automatic claim type mapping.
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add token management for API access
builder.Services.AddOpenIdConnectAccessTokenManagement();
//builder.Services.AddAccessTokenManagement();

// create an HttpClient used for accessing the API
builder.Services.AddHttpClient("APIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageGalleryAPIRoot"]);
    client.DefaultRequestHeaders.Clear();
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
}).AddUserAccessTokenHandler();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Authentication/AccessDenied";
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Authority = "https://localhost:5001/"; //idp
    options.ClientId = "imagegalleryclient";
    options.ClientSecret = "ClientSecret123";
    options.ResponseType = "code";
    //options.Scope.Add("openid"); //it is not required, those requested by meadlewear by defult
    //options.Scope.Add("profile");
    //options.CallbackPath = new PathString("signin-oidc"); //required for validation, but this is default value
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SaveTokens = true;

    options.ClaimActions.Remove("aud"); //removes the filter for the "aud" (audience) claim, so it will be included in the user's claims collection.
    options.ClaimActions.DeleteClaim("sid"); //removes the "sid" (session id) claim from the user's claims collection after mapping, keeping the cookie smaller.
    options.ClaimActions.DeleteClaim("idp"); //removes the "idp" (identity provider) claim from the user's claims collection after mapping, keeping the cookie smaller.
    options.Scope.Add("roles");
    options.Scope.Add("imagegalleryapi.fullaccess");
    options.ClaimActions.MapJsonKey("role", "role");
    options.TokenValidationParameters = new()
    {
        NameClaimType = "given_name",
        RoleClaimType = "role",
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}");

app.Run();
