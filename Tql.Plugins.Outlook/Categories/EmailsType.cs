using Tql.Abstractions;
using Tql.Plugins.Outlook.Support;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

[RootMatchType]
internal class EmailsType : MatchType<EmailsMatch, RootItemDto>
{
    public override Guid Id => TypeIds.Emails.Id;

    public EmailsType(IMatchFactory<EmailsMatch, RootItemDto> factory)
        : base(factory) { }
}
