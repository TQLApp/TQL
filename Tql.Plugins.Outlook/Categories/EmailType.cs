using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailType : IMatchType
{
    public Guid Id => TypeIds.Email.Id;

    public IMatch Deserialize(string json)
    {
        return new EmailMatch(JsonSerializer.Deserialize<PersonDto>(json)!);
    }
}
