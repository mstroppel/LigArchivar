using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FL.LigArchivar.Api.Services;

namespace FL.LigArchivar.Api.Tests;

/// <summary>
/// Creates a WebApplicationFactory with a MockFileSystem pre-populated with a
/// small test archive under /archive (the hardcoded path used by ArchiveService).
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
    /// A minimal archive layout rooted at /archive (the hardcoded path in ArchiveService).
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
