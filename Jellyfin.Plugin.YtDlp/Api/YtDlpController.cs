using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;
using Jellyfin.Plugin.YtDlp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.YtDlp.Api;

/// <summary>
/// API controller for yt-dlp plugin operations.
/// </summary>
[ApiController]
[Route("Plugins/YtDlp")]
[Authorize(Policy = "RequiresElevation")]
public class YtDlpController : ControllerBase
{
    private readonly SyncService _syncService;
    private readonly IYtDlpBinaryManager _binaryManager;
    private readonly IYtDlpWrapper _ytDlpWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="YtDlpController"/> class.
    /// </summary>
    /// <param name="syncService">The sync service.</param>
    /// <param name="binaryManager">The binary manager.</param>
    /// <param name="ytDlpWrapper">The yt-dlp wrapper.</param>
    public YtDlpController(
        SyncService syncService,
        IYtDlpBinaryManager binaryManager,
        IYtDlpWrapper ytDlpWrapper)
    {
        _syncService = syncService;
        _binaryManager = binaryManager;
        _ytDlpWrapper = ytDlpWrapper;
    }

    /// <summary>
    /// Gets the current plugin status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The plugin status.</returns>
    [HttpGet("Status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PluginStatus>> GetStatus(CancellationToken cancellationToken)
    {
        var version = await _binaryManager.GetVersionAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new PluginStatus
        {
            YtDlpAvailable = _binaryManager.IsAvailable(),
            YtDlpVersion = version,
            IsSyncing = _syncService.IsSyncing,
            LastSyncTime = _syncService.LastSyncTime,
            BinaryPath = _binaryManager.GetExpectedBinaryPath()
        });
    }

    /// <summary>
    /// Triggers a sync of all enabled sources.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Action result.</returns>
    [HttpPost("Sync")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult TriggerSync(CancellationToken cancellationToken)
    {
        if (_syncService.IsSyncing)
        {
            return Conflict(new { message = "Sync already in progress" });
        }

        _ = _syncService.SyncAllAsync(cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Updates yt-dlp to the latest version.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Action result.</returns>
    [HttpPost("UpdateYtDlp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    #pragma warning disable CA3003 // Path comes from internal config, not user input
    public async Task<ActionResult> UpdateYtDlp(CancellationToken cancellationToken)
    {
        try
        {
            await _binaryManager.UpdateAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Download failed: {ex.Message}" });
        }

        var binaryPath = await _binaryManager.GetBinaryPathAsync(cancellationToken).ConfigureAwait(false);

        if (!System.IO.File.Exists(binaryPath))
        {
            return StatusCode(500, new { error = $"Binary not found at {binaryPath}" });
        }

        try
        {
            var version = await _binaryManager.GetVersionAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(version))
            {
                return StatusCode(500, new { error = $"Binary exists at {binaryPath} but returned empty version." });
            }

            return Ok(new { version });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Binary exists at {binaryPath} but failed to execute: {ex.Message}" });
        }
    }
    #pragma warning restore CA3003

    /// <summary>
    /// Fetches metadata for a URL without downloading.
    /// </summary>
    /// <param name="url">The URL to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The metadata.</returns>
    [HttpGet("Preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlaylistMetadata>> PreviewUrl([Required] string url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest(new { message = "URL is required" });
        }

        var metadata = await _ytDlpWrapper.FetchMetadataAsync(url, cancellationToken).ConfigureAwait(false);

        if (metadata == null)
        {
            return BadRequest(new { message = "Failed to fetch metadata" });
        }

        return Ok(metadata);
    }

    /// <summary>
    /// Exports the current configuration.
    /// </summary>
    /// <returns>The configuration as JSON.</returns>
    [HttpGet("Export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<PluginConfigExport> ExportConfig()
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return Ok(new PluginConfigExport());
        }

        return Ok(new PluginConfigExport
        {
            Sources = config.Sources,
            ChannelOverrides = config.ChannelOverrides,
            GlobalRules = config.GlobalRules
        });
    }

    /// <summary>
    /// Imports configuration from JSON.
    /// </summary>
    /// <param name="import">The configuration to import.</param>
    /// <returns>Action result.</returns>
    [HttpPost("Import")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult ImportConfig([FromBody] PluginConfigExport import)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null)
        {
            return BadRequest();
        }

        if (import.Sources != null)
        {
            config.Sources = import.Sources;
        }

        if (import.ChannelOverrides != null)
        {
            config.ChannelOverrides = import.ChannelOverrides;
        }

        if (import.GlobalRules != null)
        {
            config.GlobalRules = import.GlobalRules;
        }

        Plugin.Instance?.SaveConfiguration();

        return Ok();
    }
}
