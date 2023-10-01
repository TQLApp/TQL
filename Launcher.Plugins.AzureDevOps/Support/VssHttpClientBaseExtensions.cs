using Microsoft.VisualStudio.Services.WebApi;
using System.Net.Http;
using System.Reflection;

namespace Launcher.Plugins.AzureDevOps.Support;

internal static class VssHttpClientBaseExtensions
{
    public static HttpClient GetClient(this VssHttpClientBase self)
    {
        return (HttpClient)
            self.GetType()!
                .GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(self);
    }
}
