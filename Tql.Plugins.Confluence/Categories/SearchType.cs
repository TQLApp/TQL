using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchType : IMatchType
{
    public Guid Id => TypeIds.Search.Id;

    public IMatch Deserialize(string json)
    {
        return new SearchMatch(JsonSerializer.Deserialize<SearchMatchDto>(json)!);
    }
}
