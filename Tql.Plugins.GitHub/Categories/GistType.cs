using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class GistType : IMatchType
{
    public Guid Id => TypeIds.Gist.Id;

    public IMatch Deserialize(string json)
    {
        return new GistMatch(JsonSerializer.Deserialize<GistMatchDto>(json)!);
    }
}
