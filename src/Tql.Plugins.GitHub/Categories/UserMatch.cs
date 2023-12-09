using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserMatch(
    UserMatchDto dto,
    GitHubApi api,
    IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => dto.Login;
    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.User;
    public string SearchHint => Labels.UserMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.Url);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient(dto.ConnectionId);

        var response = await client
            .Search
            .SearchRepo(new SearchRepositoriesRequest(text) { User = dto.Login });

        cancellationToken.ThrowIfCancellationRequested();

        return response
            .Items
            .Select(
                p =>
                    factory.Create(
                        new RepositoryMatchDto(dto.ConnectionId, p.Owner.Login, p.Name, p.HtmlUrl)
                    )
            );
    }
}

internal record UserMatchDto(Guid ConnectionId, string Login, string Url);
