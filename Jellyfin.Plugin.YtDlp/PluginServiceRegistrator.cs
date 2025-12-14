using Jellyfin.Plugin.YtDlp.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.YtDlp;

/// <summary>
/// Registers plugin services with the DI container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddHttpClient();
        serviceCollection.AddSingleton<IYtDlpBinaryManager, YtDlpBinaryManager>();
        serviceCollection.AddSingleton<IYtDlpWrapper, YtDlpWrapper>();
        serviceCollection.AddSingleton<IRuleEngine, RuleEngine>();
        serviceCollection.AddSingleton<IFileOrganizer, FileOrganizer>();
        serviceCollection.AddSingleton<IMetadataMapper, MetadataMapper>();
        serviceCollection.AddSingleton<IDownloadManager, DownloadManager>();
        serviceCollection.AddSingleton<SyncService>();
        serviceCollection.AddHostedService(provider => provider.GetRequiredService<SyncService>());
    }
}
