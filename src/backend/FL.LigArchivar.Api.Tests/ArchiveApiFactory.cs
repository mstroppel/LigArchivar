using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FL.LigArchivar.Api.Services;

namespace FL.LigArchivar.Api.Tests;

/// <summary>
/// Creates a WebApplicationFactory with a MockFileSystem pre-populated with a
/// small test archive under /archive.
/// The ARCHIVE_ROOT configuration is forced to "/archive" so that the service
/// uses the mock filesystem path regardless of any host environment variables.
/// </summary>
public class ArchiveApiFactory : WebApplicationFactory<Program>
{
    private readonly IFileSystem _fileSystem;

    public ArchiveApiFactory() : this(CreateDefaultMockFileSystem()) { }

    public ArchiveApiFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Override ARCHIVE_ROOT so the ArchiveService uses the mock path,
        // regardless of any environment variables set on the host machine.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ARCHIVE_ROOT"] = "/archive",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove previously registered IFileSystem and ArchiveService, then
            // replace them with our mock versions.
            services.RemoveAll<IFileSystem>();
            services.RemoveAll<ArchiveService>();

            services.AddSingleton(_fileSystem);
            services.AddSingleton<ArchiveService>();
        });
    }

    /// <summary>Returns an HttpClient pre-authenticated with the default test credentials.</summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        string username = "admin",
        string password = "changeme")
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>
    /// A minimal archive layout rooted at /archive.
    /// </summary>
    public static IFileSystem CreateDefaultMockFileSystem() =>
        new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.jpg", new MockFileData(string.Empty) },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_002.jpg", new MockFileData(string.Empty) },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_003.jpg", new MockFileData(string.Empty) },
            },
            "/archive");
}
