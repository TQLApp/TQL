using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailType : MatchType<EmailMatch, PersonDto>
{
    public override Guid Id => TypeIds.Email.Id;

    public EmailType(IMatchFactory<EmailMatch, PersonDto> factory)
        : base(factory) { }
}
