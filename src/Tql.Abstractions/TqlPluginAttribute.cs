namespace Tql.Abstractions;

/// <summary>
/// Identifies a class as a TQL plugin.
/// </summary>
/// <remarks>
/// Only classes that specify this attribute are candidates for TQL plugins.
/// See <see cref="ITqlPlugin"/> for more information.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class TqlPluginAttribute : Attribute { }
