using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PeopleTypeBase : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;

    public abstract Guid Id { get; }
    protected abstract string Label { get; }

    protected PeopleTypeBase(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
    {
        _configurationManager = configurationManager;
        _peopleDirectoryManager = peopleDirectoryManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var directory = MatchUtils.GetDirectory(
            _configurationManager,
            _peopleDirectoryManager,
            dto.Id
        );
        if (directory == null)
            return null;

        return CreateMatch(
            MatchUtils.GetMatchLabel(Label, _peopleDirectoryManager, dto.Id),
            directory
        );
    }

    protected abstract IMatch CreateMatch(string label, IPeopleDirectory directory);
}
