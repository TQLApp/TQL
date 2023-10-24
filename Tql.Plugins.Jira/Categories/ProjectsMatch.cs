using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;
    private readonly ILogger<ProjectsMatch> _logger;

    public override string Text { get; }
    public override ImageSource Icon => Images.Projects;
    public override MatchTypeId TypeId => TypeIds.Projects;

    public ProjectsMatch(
        string text,
        string url,
        ICache<JiraData> cache,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager,
        ILogger<ProjectsMatch> logger
    )
        : base(cache)
    {
        _url = url;
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
        _logger = logger;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        // Download the project avatars in the background.

        var projects = data.GetConnection(_url).Projects;

        TaskUtils.RunBackground(async () =>
        {
            var client = _configurationManager.GetClient(_url);

            foreach (var project in projects)
            {
                try
                {
                    await _iconCacheManager.LoadIcon(client, project.AvatarUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to download project avatar '{Url}'",
                        project.AvatarUrl
                    );
                }
            }
        });

        return from project in projects
            select new ProjectMatch(
                new ProjectMatchDto(_url, project.Key, project.Name, project.AvatarUrl),
                _iconCacheManager,
                _configurationManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
