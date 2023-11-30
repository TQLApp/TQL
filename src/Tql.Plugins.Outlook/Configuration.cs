namespace Tql.Plugins.Outlook;

internal record Configuration(NameFormat NameFormat)
{
    public static readonly Configuration Empty = new(default(NameFormat));
}

internal enum NameFormat
{
    None,
    LastNameCommaFirstName
}
