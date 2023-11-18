namespace Tql.Abstractions;

/// <summary>
/// ID of a match type.
/// </summary>
/// <remarks>
/// Match type IDs are used to uniquely identify a match.
/// </remarks>
/// <param name="Id">ID of the match type.</param>
/// <param name="PluginId">ID of the plugin.</param>
public record MatchTypeId(Guid Id, Guid PluginId);
