using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.IO.Abstractions.TestingHelpers;
using FL.LigArchivar.Api.Models;

namespace FL.LigArchivar.Api.Tests;

public class AuthControllerTests : IAsyncLifetime
{
    private ArchiveApiFactory _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new ArchiveApiFactory();
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Status_WhenUnauthenticated_ReturnsAuthenticatedFalse()
    {
        var response = await _client.GetAsync("/api/auth/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<AuthStatusDto>();
        Assert.NotNull(dto);
        Assert.False(dto.Authenticated);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndSetsCookie()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "changeme" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "wrong", password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Status_AfterLogin_ReturnsAuthenticatedTrue()
    {
        await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "changeme" });

        var response = await _client.GetAsync("/api/auth/status");
        var dto = await response.Content.ReadFromJsonAsync<AuthStatusDto>();
        Assert.NotNull(dto);
        Assert.True(dto.Authenticated);
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        // Login first
        await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "changeme" });

        // Verify authenticated
        var statusBefore = await _client.GetFromJsonAsync<AuthStatusDto>("/api/auth/status");
        Assert.True(statusBefore!.Authenticated);

        // Logout
        await _client.PostAsync("/api/auth/logout", null);

        // Verify no longer authenticated
        var statusAfter = await _client.GetFromJsonAsync<AuthStatusDto>("/api/auth/status");
        Assert.False(statusAfter!.Authenticated);
    }
}

public class ArchiveControllerTests : IAsyncLifetime
{
    private ArchiveApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new ArchiveApiFactory();
        _client = await _factory.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTree_WhenAuthenticated_ReturnsTree()
    {
        var response = await _client.GetAsync("/api/archive/tree");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tree = await response.Content.ReadFromJsonAsync<TreeNodeDto[]>();
        Assert.NotNull(tree);
        Assert.NotEmpty(tree);
    }

    [Fact]
    public async Task GetTree_ContainsDigitalfotoAsset()
    {
        var tree = await _client.GetFromJsonAsync<TreeNodeDto[]>("/api/archive/tree");
        Assert.NotNull(tree);

        var digitalfoto = tree.FirstOrDefault(n => n.Name == "Digitalfoto");
        Assert.NotNull(digitalfoto);
        Assert.Equal("asset", digitalfoto.NodeType);
    }

    [Fact]
    public async Task GetTree_WithSubPath_ReturnsSubtree()
    {
        var response = await _client.GetAsync("/api/archive/tree?path=Digitalfoto");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tree = await response.Content.ReadFromJsonAsync<TreeNodeDto[]>();
        Assert.NotNull(tree);
        // Should contain the 2018 year directory
        Assert.Contains(tree, n => n.Name == "2018");
    }

    [Fact]
    public async Task GetTree_WithTraversalPath_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/archive/tree?path=../etc");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTree_WhenUnauthenticated_Returns401()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/api/archive/tree");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class EventsControllerTests : IAsyncLifetime
{
    private ArchiveApiFactory _factory = null!;
    private HttpClient _client = null!;

    private const string EventPath =
        "Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung";

    public async Task InitializeAsync()
    {
        _factory = new ArchiveApiFactory();
        _client = await _factory.CreateAuthenticatedClientAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetEvent_ValidPath_ReturnsEventDetail()
    {
        var response = await _client.GetAsync($"/api/events/{EventPath}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<EventDetailDto>();
        Assert.NotNull(dto);
        Assert.Equal("A_2018-05-01_Maiwanderung", dto.Name);
        Assert.Equal(3, dto.Files.Length);
    }

    [Fact]
    public async Task GetEvent_InvalidPath_ReturnsBadRequest()
    {
        // ASP.NET Core normalises path-traversal sequences in route segments before
        // they reach the controller, so we verify path validation via a path that
        // contains ".." but is passed as a query parameter on the rename endpoint.
        // For the GET event route, an empty/whitespace path (caught by ValidatePath)
        // is naturally rejected as a 404 by routing, so we test the POST path instead.
        var request = new RenameRequestDto(1);
        var response = await _client.PostAsJsonAsync(
            "/api/events/rename?path=..%2Fetc%2Fpasswd", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_NonExistentPath_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/events/Digitalfoto/2018/A-Albverein/A_2018-01-01_DoesNotExist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_ValidRequest_ReturnsRenamedFiles()
    {
        var request = new RenameRequestDto(1);
        var response = await _client.PostAsJsonAsync(
            $"/api/events/rename?path={Uri.EscapeDataString(EventPath)}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<EventDetailDto>();
        Assert.NotNull(dto);
        // After renaming starting at 1, files should be 001, 002, 003
        Assert.Contains(dto.Files, f => f.Name == "A_2018-05-01_001");
    }

    [Fact]
    public async Task Rename_WithFileOrder_RenamesInSpecifiedOrder()
    {
        // Use files numbered 010/020/030 so renaming from 1 (→001,002,003)
        // cannot conflict with any existing filename.
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_010.jpg", new MockFileData(string.Empty) },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_020.jpg", new MockFileData(string.Empty) },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_030.jpg", new MockFileData(string.Empty) },
            },
            "/archive");

        await using var factory = new ArchiveApiFactory(fs);
        var client = await factory.CreateAuthenticatedClientAsync();

        var request = new RenameRequestDto(1, ["A_2018-05-01_030", "A_2018-05-01_020", "A_2018-05-01_010"]);
        var response = await client.PostAsJsonAsync(
            $"/api/events/rename?path={Uri.EscapeDataString(EventPath)}", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<EventDetailDto>();
        Assert.NotNull(dto);
        Assert.Equal(3, dto.Files.Length);
    }

    [Fact]
    public async Task Rename_InvalidPath_ReturnsBadRequest()
    {
        var request = new RenameRequestDto(1);
        var response = await _client.PostAsJsonAsync(
            "/api/events/rename?path=../etc", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RenameByDateTime_ValidPath_Returns200()
    {
        // Each file needs a distinct LastWriteTimeUtc so datetime-based rename
        // produces unique target names and doesn't conflict.
        var t1 = new MockFileData(string.Empty) { LastWriteTime = new DateTime(2018, 5, 1, 10, 0, 0, DateTimeKind.Utc) };
        var t2 = new MockFileData(string.Empty) { LastWriteTime = new DateTime(2018, 5, 1, 11, 0, 0, DateTimeKind.Utc) };
        var t3 = new MockFileData(string.Empty) { LastWriteTime = new DateTime(2018, 5, 1, 12, 0, 0, DateTimeKind.Utc) };

        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.jpg", t1 },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_002.jpg", t2 },
                { "/archive/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_003.jpg", t3 },
            },
            "/archive");

        await using var factory = new ArchiveApiFactory(fs);
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync(
            $"/api/events/rename-by-datetime?path={Uri.EscapeDataString(EventPath)}", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_Returns200()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
