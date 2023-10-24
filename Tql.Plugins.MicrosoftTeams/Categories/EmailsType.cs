using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class EmailsType : PeopleTypeBase
{
    public override Guid Id => TypeIds.Emails.Id;
    protected override string Label => "Email";

    public EmailsType(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
        : base(configurationManager, peopleDirectoryManager) { }

    protected override IMatch CreateMatch(string label, IPeopleDirectory directory)
    {
        return new EmailsMatch(label, directory);
    }
}
