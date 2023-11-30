using Tql.Abstractions;
using Tql.Plugins.Outlook.Support;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

[RootMatchType]
internal class EmailsType(IMatchFactory<EmailsMatch, RootItemDto> factory)
    : MatchType<EmailsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Emails.Id;
}
