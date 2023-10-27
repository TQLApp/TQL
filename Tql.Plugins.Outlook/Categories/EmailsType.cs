using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;
using Tql.Plugins.Outlook.Support;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

[RootMatchType]
internal class EmailsType : IMatchType
{
    private readonly OutlookPeopleDirectory _outlookPeopleDirectory;

    public Guid Id => TypeIds.Emails.Id;

    public EmailsType(OutlookPeopleDirectory outlookPeopleDirectory)
    {
        _outlookPeopleDirectory = outlookPeopleDirectory;
    }

    public IMatch? Deserialize(string json)
    {
        return new EmailsMatch(_outlookPeopleDirectory);
    }
}
