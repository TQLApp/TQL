using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailType(IMatchFactory<EmailMatch, PersonDto> factory)
    : MatchType<EmailMatch, PersonDto>(factory)
{
    public override Guid Id => TypeIds.Email.Id;
}
