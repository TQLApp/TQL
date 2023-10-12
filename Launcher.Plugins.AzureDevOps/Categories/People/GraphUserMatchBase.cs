using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal abstract class GraphUserMatchBase : ISearchableMatch, ISerializableMatch
{
    private readonly string _url;
    private readonly AzureDevOpsApi _api;

    public abstract string Text { get; }
    public abstract ImageSource Icon { get; }
    public abstract MatchTypeId TypeId { get; }

    protected GraphUserMatchBase(string url, AzureDevOpsApi api)
    {
        _url = url;
        _api = api;
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

        var client = await _api.GetClient<GraphHttpClient>(_url);

        var users = await client.QuerySubjectsAsync(
            new GraphSubjectQuery(text, new[] { "User" }, new SubjectDescriptor()),
            cancellationToken: cancellationToken
        );

        if (users == null)
            return ImmutableArray<IMatch>.Empty;

        var duplicateDisplayNames = users
            .Cast<GraphUser>()
            .GroupBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .Where(p => p.Count() > 1)
            .Select(p => p.Key)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        return users
            .Cast<GraphUser>()
            .Select(p =>
            {
                var displayName = p.DisplayName;
                if (duplicateDisplayNames.Contains(displayName))
                    displayName += $" - {p.MailAddress}";

                return CreateMatch(new GraphUserDto(_url, displayName, p.MailAddress));
            })
            .OrderBy(p => p.Text, StringComparer.CurrentCultureIgnoreCase);
    }

    protected abstract IMatch CreateMatch(GraphUserDto dto);

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}

internal record GraphUserDto(string Url, string DisplayName, string EmailAddress);
