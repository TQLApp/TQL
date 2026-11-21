using Tql.Plugins.YouTrack.Data;

namespace Tql.Plugins.YouTrack.Support;

/// <summary>
/// Marks a match type whose matches the plugin offers as root items, one per
/// connection. A root match type that lists what an endpoint returns names
/// it, so the plugin can leave the category out for a connection whose
/// endpoint answered 403 or 404.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal class RootMatchTypeAttribute : Attribute
{
    public YouTrackEndpoint? Endpoint { get; }

    public RootMatchTypeAttribute() { }

    public RootMatchTypeAttribute(YouTrackEndpoint endpoint)
    {
        Endpoint = endpoint;
    }
}
