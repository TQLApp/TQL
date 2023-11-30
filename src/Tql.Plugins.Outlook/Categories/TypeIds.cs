using Tql.Abstractions;

namespace Tql.Plugins.Outlook.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), OutlookPlugin.Id);

    public static readonly MatchTypeId Emails = CreateId("83495e78-6fa7-4634-9fe0-02832822f83c");
    public static readonly MatchTypeId Email = CreateId("7b087243-e8ff-48a5-8b7d-e0262111f663");
}
