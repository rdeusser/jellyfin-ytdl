using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Manages the yt-dlp binary lifecycle.
/// </summary>
public class YtDlpBinaryManager : IYtDlpBinaryManager
{
    private const string YtDlpReleasesUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/";

    private readonly IApplicationPaths _appPaths;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<YtDlpBinaryManager> _logger;
    private readonly string _binaryPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="YtDlpBinaryManager"/> class.
    /// </summary>
    /// <param name="appPaths">Application paths.</param>
    /// <param name="httpClientFactory">HTTP client factory.</param>
    /// <param name="logger">Logger instance.</param>
    public YtDlpBinaryManager(
        IApplicationPaths appPaths,
        IHttpClientFactory httpClientFactory,
        ILogger<YtDlpBinaryManager> logger)
    {
        _appPaths = appPaths;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _binaryPath = GetDefaultBinaryPath();
    }

    /// <inheritdoc />
    public async Task<string> GetBinaryPathAsync(CancellationToken cancellationToken = default)
    {
        var config = Plugin.Instance?.Configuration;
        if (config != null && !string.IsNullOrEmpty(config.YtDlpPath) && File.Exists(config.YtDlpPath))
        {
            return config.YtDlpPath;
        }

        if (!File.Exists(_binaryPath))
        {
            await UpdateAsync(cancellationToken).ConfigureAwait(false);
        }

        return _binaryPath;
    }

    /// <inheritdoc />
    public bool IsAvailable()
    {
        var config = Plugin.Instance?.Configuration;
        if (config != null && !string.IsNullOrEmpty(config.YtDlpPath))
        {
            return File.Exists(config.YtDlpPath);
        }

        return File.Exists(_binaryPath);
    }

    /// <inheritdoc />
    public async Task<string?> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable())
        {
            return null;
        }

        var binaryPath = await GetBinaryPathAsync(cancellationToken).ConfigureAwait(false);

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = binaryPath,
            Arguments = "--version",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var version = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return version.Trim();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var binaryName = GetBinaryName();
        var downloadUrl = YtDlpReleasesUrl + binaryName;

        _logger.LogInformation("Downloading yt-dlp from {Url}", downloadUrl);

        var directory = Path.GetDirectoryName(_binaryPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var fileStream = new FileStream(_binaryPath, FileMode.Create, FileAccess.Write);
        await using (fileStream.ConfigureAwait(false))
        {
            await response.Content.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(_binaryPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        _logger.LogInformation("yt-dlp downloaded successfully to {Path}", _binaryPath);
    }

    private string GetDefaultBinaryPath()
    {
        var pluginDataPath = Path.Combine(_appPaths.PluginConfigurationsPath, "yt-dlp");
        var binaryName = GetBinaryName();
        return Path.Combine(pluginDataPath, binaryName);
    }

    private static string GetBinaryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "yt-dlp.exe";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "yt-dlp_macos";
        }

        return "yt-dlp_linux";
    }
}
