using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBff()
    .AddRemoteApis();

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

const string bffCookieScheme = "BFFCookieScheme";
const string bffChallengeScheme = "BFFChallengeScheme";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = bffCookieScheme;
        options.DefaultChallengeScheme = bffChallengeScheme;
    })
    .AddCookie(bffCookieScheme, options =>
    {
        options.Cookie.Name = "__Host-ImageGalleryBff";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events = new CookieAuthenticationEvents
        {
            OnSigningIn = context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("CookieAuth");

                if (context?.Principal == null)
                {
                    logger.LogInformation($"[{DateTimeOffset.Now}] missing context?.Principal");
                    
                    return Task.CompletedTask;
                }

                // Log to console or debug output
                logger.LogInformation($"[{DateTimeOffset.Now}] Cookie will contain these claims:");
                foreach (var claim in context.Principal.Claims)
                {
                    logger.LogInformation($"[{DateTimeOffset.Now}] - claim: {claim.Type}: {claim.Value}");
                }

                return Task.CompletedTask;
            }
        };
    })
    .AddOpenIdConnect(bffChallengeScheme, options =>
    {
        options.SignInScheme = bffCookieScheme;
        options.Authority = "https://localhost:5001/";
        options.ClientId = "imagegallerybff";
        options.ClientSecret = "anothersecret";
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("roles");
        options.Scope.Add("imagegalleryapi.read");
        options.Scope.Add("imagegalleryapi.write");
        options.Scope.Add("country");
        options.Scope.Add("offline_access");
        options.ClaimActions.Add(new JsonKeyClaimAction("role", ClaimValueTypes.String, "role"));
        options.ClaimActions.Add(new JsonKeyClaimAction("country", ClaimValueTypes.String, "country"));
        options.TokenValidationParameters = new()
        {
            NameClaimType = "given_name",
            RoleClaimType = "role",
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseBff();

app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapRemoteBffApiEndpoint("/bff/images", new Uri("https://localhost:7075/api/images"))
    .WithAccessToken(RequiredTokenType.User);

app.MapGet("/bff/logged-user", [Authorize] (HttpContext context) =>
{
    var name = context.User.Identity?.Name ?? "";
    // Or get a specific claim, e.g. given_name:
    // var name = context.User.FindFirst("given_name")?.Value ?? "";
    return Results.Json(new { name });
});

app.MapFallbackToFile("/index.html");

app.Run();
