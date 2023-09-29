using Microsoft.VisualStudio.Services.WebApi;

namespace Launcher.Plugins.AzureDevOps.Services;

internal interface IAzureDevOpsApi
{
    Task<T> GetClient<T>(string collectionUri)
        where T : IVssHttpClient;
}
