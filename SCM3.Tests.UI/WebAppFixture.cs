using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace SCM3.Tests.UI;

// Boots real SCM3.Api + SCM3.Web processes (the prebuilt Debug DLLs) on free local
// ports against a private copy of the seeded scm3.db, then drives them with a real
// headless Chromium via Playwright — true end-to-end, no in-memory test host, matching
// root CLAUDE.md §18's "Playwright | End-to-end Blazor UI tests" intent.
public sealed class WebAppFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(90);

    private Process? _apiProcess;
    private Process? _webProcess;
    private string? _dbCopyPath;
    private IPlaywright? _playwright;

    public string WebBaseUrl { get; private set; } = string.Empty;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var repoRoot = FindRepoRoot();
        _dbCopyPath = Path.Combine(Path.GetTempPath(), $"scm3-ui-tests-{Guid.NewGuid():N}.db");
        File.Copy(Path.Combine(repoRoot, "SCM3.Data", "scm3.db"), _dbCopyPath);
        var connectionString = $"Data Source={_dbCopyPath}";

        var apiPort = GetFreeTcpPort();
        var apiUrl = $"http://127.0.0.1:{apiPort}";
        _apiProcess = StartApp(repoRoot, "SCM3.Api", apiUrl, new()
        {
            ["DataProvider"] = "Sqlite",
            ["ConnectionStrings__SCM3"] = connectionString,
        });
        await WaitUntilRespondingAsync($"{apiUrl}/health");

        var webPort = GetFreeTcpPort();
        var webUrl = $"http://127.0.0.1:{webPort}";
        _webProcess = StartApp(repoRoot, "SCM3.Web", webUrl, new()
        {
            ["DataProvider"] = "Sqlite",
            ["ConnectionStrings__SCM3"] = connectionString,
            ["Api__BaseUrl"] = apiUrl,
        });
        await WaitUntilRespondingAsync($"{webUrl}/login");

        WebBaseUrl = webUrl;

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();

        StopProcess(_webProcess);
        StopProcess(_apiProcess);

        if (_dbCopyPath is not null && File.Exists(_dbCopyPath))
        {
            File.Delete(_dbCopyPath);
        }
    }

    private static Process StartApp(string repoRoot, string projectName, string url, Dictionary<string, string> extraEnv)
    {
        var projectDir = Path.Combine(repoRoot, projectName);
        var dllPath = Path.Combine(projectDir, "bin", "Debug", "net10.0", $"{projectName}.dll");
        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException(
                $"Expected prebuilt {projectName} at '{dllPath}'. Build the solution (e.g. `dotnet build`) before running SCM3.Tests.UI.",
                dllPath);
        }

        var startInfo = new ProcessStartInfo("dotnet", dllPath)
        {
            WorkingDirectory = projectDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        startInfo.Environment["ASPNETCORE_URLS"] = url;
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        foreach (var (key, value) in extraEnv)
        {
            startInfo.Environment[key] = value;
        }

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start {projectName} from '{dllPath}'.");
        }

        // Drain std streams so the child never blocks on a full pipe buffer.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static async Task WaitUntilRespondingAsync(string url)
    {
        using var client = new HttpClient();
        var deadline = DateTime.UtcNow + StartupTimeout;
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync(url);
                // Any response (including the login page's 200, or a redirect) means the
                // host is up and routing requests — that's all we need before driving it.
                if ((int)response.StatusCode < 500)
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or SocketException or TaskCanceledException)
            {
                lastError = ex;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"'{url}' did not respond within {StartupTimeout}.", lastError);
    }

    private static void StopProcess(Process? process)
    {
        if (process is null)
        {
            return;
        }

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            }
        }
        catch (InvalidOperationException)
        {
            // Process already exited between the check and the kill — nothing to do.
        }
        finally
        {
            process.Dispose();
        }
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SCM3.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new DirectoryNotFoundException($"Could not locate SCM3.slnx above '{AppContext.BaseDirectory}'.");
    }
}

[CollectionDefinition(Name)]
public sealed class WebAppCollection : ICollectionFixture<WebAppFixture>
{
    public const string Name = "SCM3 Web App";
}
