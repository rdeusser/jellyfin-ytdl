# Jellyfin yt-dlp Plugin

Downloads and organizes YouTube content for Jellyfin using yt-dlp.

## Features

- Download videos from YouTube channels and playlists
- Organize downloads into folders using configurable rules
- Generate Jellyfin-compatible NFO metadata files
- Download thumbnails, subtitles, and channel artwork
- Schedule automatic syncs at configurable intervals
- Skip already-downloaded videos using an archive file
- Override settings per channel (custom names, folders, format strings)

## Installation

1. In Jellyfin, go to Dashboard > Plugins > Repositories
2. Add repository: `https://raw.githubusercontent.com/rdeusser/jellyfin-ytdl/master/manifest.json`
3. Install "yt-dlp" from the plugin catalog
4. Restart Jellyfin

## Configuration

### General Settings

| Setting | Description |
|---------|-------------|
| Download Path | Base directory for downloaded videos |
| yt-dlp Path | Path to yt-dlp binary (auto-managed if empty) |
| Format String | yt-dlp format selection (default: `bestvideo*+bestaudio/best`) |
| Max Concurrent Downloads | Parallel download limit |
| Schedule Enabled | Enable automatic syncs |
| Schedule Interval | Hours between syncs |

### Subtitles

| Setting | Description |
|---------|-------------|
| Subtitle Languages | Language codes to download (e.g., `en`, `es`) |
| Include Auto-Generated | Download auto-generated subtitles |
| Subtitle Format | Output format (`srt`, `vtt`, `ass`) |

### Rules

Organize videos into folders by metadata:

- Match on title, description, tags, or category
- Operators: contains, starts with, ends with, equals, not contains, not equals
- Priority determines evaluation order
- Nested rules create subfolder hierarchies

## Building

Requires .NET 9.0 SDK.

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## API Endpoints

REST endpoints under `/YtDlp`:

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/YtDlp/Status` | Get sync status and statistics |
| POST | `/YtDlp/Sync` | Trigger manual sync |
| POST | `/YtDlp/Sync/Cancel` | Cancel running sync |
| GET | `/YtDlp/Preview` | Preview URL metadata |
| GET | `/YtDlp/Export` | Export configuration |
| POST | `/YtDlp/Import` | Import configuration |

## License

GPLv3. See [LICENSE](LICENSE) for details.
