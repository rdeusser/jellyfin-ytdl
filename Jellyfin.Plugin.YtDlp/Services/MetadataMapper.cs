using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Maps yt-dlp metadata to Jellyfin NFO format.
/// </summary>
public class MetadataMapper : IMetadataMapper
{
    /// <inheritdoc />
    public async Task WriteNfoFileAsync(VideoMetadata video, string videoFilePath, CancellationToken cancellationToken = default)
    {
        var nfoPath = Path.ChangeExtension(videoFilePath, ".nfo");

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            Async = true
        };

        var stream = new FileStream(nfoPath, FileMode.Create, FileAccess.Write);
        await using (stream.ConfigureAwait(false))
        {
            var writer = XmlWriter.Create(stream, settings);
            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteStartDocumentAsync().ConfigureAwait(false);
                await writer.WriteStartElementAsync(null, "episodedetails", null).ConfigureAwait(false);

                await writer.WriteElementStringAsync(null, "title", null, video.Title).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(video.Description))
                {
                    await writer.WriteElementStringAsync(null, "plot", null, video.Description).ConfigureAwait(false);
                }

                if (video.ParsedUploadDate.HasValue)
                {
                    var dateStr = video.ParsedUploadDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    await writer.WriteElementStringAsync(null, "aired", null, dateStr).ConfigureAwait(false);
                }

                if (video.Duration.HasValue)
                {
                    var minutes = video.Duration.Value / 60;
                    await writer.WriteElementStringAsync(null, "runtime", null, minutes.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(video.Channel))
                {
                    await writer.WriteElementStringAsync(null, "studio", null, video.Channel).ConfigureAwait(false);
                }

                if (video.Tags != null)
                {
                    foreach (var tag in video.Tags)
                    {
                        await writer.WriteElementStringAsync(null, "tag", null, tag).ConfigureAwait(false);
                    }
                }

                if (video.Categories != null)
                {
                    foreach (var category in video.Categories)
                    {
                        await writer.WriteElementStringAsync(null, "genre", null, category).ConfigureAwait(false);
                    }
                }

                await writer.WriteStartElementAsync(null, "uniqueid", null).ConfigureAwait(false);
                await writer.WriteAttributeStringAsync(null, "type", null, "youtube").ConfigureAwait(false);
                await writer.WriteAttributeStringAsync(null, "default", null, "true").ConfigureAwait(false);
                await writer.WriteStringAsync(video.Id).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);
                await writer.WriteEndDocumentAsync().ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async Task WriteShowNfoFileAsync(
        string channelName,
        string channelId,
        string channelPath,
        CancellationToken cancellationToken = default)
    {
        var nfoPath = Path.Combine(channelPath, "tvshow.nfo");

        if (File.Exists(nfoPath))
        {
            return;
        }

        Directory.CreateDirectory(channelPath);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            Async = true
        };

        var stream = new FileStream(nfoPath, FileMode.Create, FileAccess.Write);
        await using (stream.ConfigureAwait(false))
        {
            var writer = XmlWriter.Create(stream, settings);
            await using (writer.ConfigureAwait(false))
            {
                await writer.WriteStartDocumentAsync().ConfigureAwait(false);
                await writer.WriteStartElementAsync(null, "tvshow", null).ConfigureAwait(false);

                await writer.WriteElementStringAsync(null, "title", null, channelName).ConfigureAwait(false);

                await writer.WriteStartElementAsync(null, "uniqueid", null).ConfigureAwait(false);
                await writer.WriteAttributeStringAsync(null, "type", null, "youtube").ConfigureAwait(false);
                await writer.WriteAttributeStringAsync(null, "default", null, "true").ConfigureAwait(false);
                await writer.WriteStringAsync(channelId).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);

                await writer.WriteEndElementAsync().ConfigureAwait(false);
                await writer.WriteEndDocumentAsync().ConfigureAwait(false);
            }
        }
    }
}
