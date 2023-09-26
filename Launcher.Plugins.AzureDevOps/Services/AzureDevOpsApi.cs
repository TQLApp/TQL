using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;

namespace Launcher.Plugins.AzureDevOps.Services;

internal class AzureDevOpsApi : IAzureDevOpsApi
{
    public AzureDevOpsApi()
    {
        //new VssConnection(new Uri(collectionUri), new VssClientCredentials());
    }
}
