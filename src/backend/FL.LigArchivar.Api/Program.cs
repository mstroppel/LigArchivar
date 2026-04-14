using System.IO.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using FL.LigArchivar.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Logging format ────────────────────────────────────────────────────────────

if (string.Equals(builder.Configuration["LOG_FORMAT"], "json", StringComparison.OrdinalIgnoreCase))
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole();
}

// ── Required configuration ────────────────────────────────────────────────────

if (string.IsNullOrWhiteSpace(builder.Configuration["AUTH_USERNAME"]))
    throw new InvalidOperationException("AUTH_USERNAME is required and must not be empty.");

if (string.IsNullOrWhiteSpace(builder.Configuration["AUTH_PASSWORD"]))
    throw new InvalidOperationException("AUTH_PASSWORD is required and must not be empty.");

// ── Services ─────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// Cookie-based authentication: HttpOnly, SameSite=Strict.
// Unauthenticated API requests return 401 (not a redirect to a login page).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "LigArchivar.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // Override default redirect behaviour — return 401/403 instead of redirect.
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// Real filesystem — singleton is safe; it carries no state.
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<ArchiveService>();

// ── Pipeline ──────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// SPA fallback: any non-file, non-API request returns index.html.
app.MapFallbackToFile("index.html");

app.Run();

// Expose Program for integration-test factory.
public partial class Program { }
