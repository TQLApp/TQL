using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Test.Support;

namespace Tql.Plugins.YouTrack.Test;

[TestFixture]
internal class YouTrackClientFixture
{
    private const string ServiceUrl = "https://issues.example.com/";
    private const string Token = "perm:invented.token.value";

    private FakeHttpMessageHandler _handler = null!;
    private HttpClient _httpClient = null!;
    private List<HttpStatusCode> _observed = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new FakeHttpMessageHandler();
        _httpClient = new HttpClient(_handler);
        _observed = new List<HttpStatusCode>();
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    private YouTrackClient CreateClient(
        string serviceUrl = ServiceUrl,
        int pageSize = YouTrackClient.DefaultPageSize
    ) => new(_httpClient, serviceUrl, Token, _observed.Add) { PageSize = pageSize };

    private IEnumerable<string> RequestedUrls => _handler.Requests.Select(p => p.Url);

    private void AssertEveryRequestCarriesTheToken()
    {
        Assert.That(_handler.Requests, Is.Not.Empty);

        foreach (var request in _handler.Requests)
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get), request.Url);
            Assert.That(request.Authorization?.Scheme, Is.EqualTo("Bearer"), request.Url);
            Assert.That(request.Authorization?.Parameter, Is.EqualTo(Token), request.Url);
        }
    }

    // The service URL check.

    [Test]
    public async Task CheckServiceUrlAcceptsTheServiceUrl()
    {
        _handler.AnswerFixture($"{ServiceUrl}api/config?fields=version", "Config.json");

        var result = await YouTrackClient.CheckServiceUrl(
            _httpClient,
            "https://issues.example.com"
        );

        Assert.That(result.EnteredUrl, Is.EqualTo(ServiceUrl));
        Assert.That(result.ServiceUrl, Is.EqualTo(ServiceUrl));
        Assert.That(result.IsSuggestion, Is.False);
        Assert.That(result.Version, Is.EqualTo("2026.2"));
        Assert.That(_handler.Requests.Single().Authorization, Is.Null);
    }

    [Test]
    public async Task CheckServiceUrlOffersTheUrlWithoutTheCopiedPath()
    {
        _handler
            .AnswerHtml($"{ServiceUrl}issues/api/config?fields=version")
            .AnswerFixture($"{ServiceUrl}api/config?fields=version", "Config.json");

        var result = await YouTrackClient.CheckServiceUrl(
            _httpClient,
            "https://issues.example.com/issues?q=for:%20me"
        );

        Assert.That(result.EnteredUrl, Is.EqualTo($"{ServiceUrl}issues/"));
        Assert.That(result.ServiceUrl, Is.EqualTo(ServiceUrl));
        Assert.That(result.IsSuggestion, Is.True);
        Assert.That(
            RequestedUrls,
            Is.EqualTo(
                new[]
                {
                    $"{ServiceUrl}issues/api/config?fields=version",
                    $"{ServiceUrl}api/config?fields=version"
                }
            )
        );
    }

    [Test]
    public async Task CheckServiceUrlKeepsAPathPrefix()
    {
        _handler
            .AnswerHtml("https://www.example.com/youtrack/issue/DEMO-12/api/config?fields=version")
            .AnswerHtml("https://www.example.com/youtrack/issue/api/config?fields=version")
            .AnswerFixture(
                "https://www.example.com/youtrack/api/config?fields=version",
                "Config.json"
            );

        var result = await YouTrackClient.CheckServiceUrl(
            _httpClient,
            "https://www.example.com/youtrack/issue/DEMO-12"
        );

        Assert.That(result.ServiceUrl, Is.EqualTo("https://www.example.com/youtrack/"));
        Assert.That(result.IsSuggestion, Is.True);
        Assert.That(_handler.Requests, Has.Count.EqualTo(3));
    }

    [Test]
    public void CheckServiceUrlFailsClearlyWithoutYouTrack()
    {
        _handler
            .AnswerHtml("https://www.example.com/wiki/api/config?fields=version")
            .AnswerFixture(
                "https://www.example.com/api/config?fields=version",
                "NotFound.json",
                HttpStatusCode.NotFound
            );

        var ex = Assert.ThrowsAsync<YouTrackNotAnApiException>(
            () => YouTrackClient.CheckServiceUrl(_httpClient, "https://www.example.com/wiki")
        )!;

        Assert.That(
            ex.Message,
            Does.Contain("No YouTrack REST API answers at https://www.example.com/wiki/")
        );
        Assert.That(ex.Message, Does.Contain("text/html"));
        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(_handler.Requests, Has.Count.EqualTo(2));
    }

    [Test]
    public void CheckServiceUrlRejectsWhatIsNotAUrl()
    {
        Assert.ThrowsAsync<ArgumentException>(
            () => YouTrackClient.CheckServiceUrl(_httpClient, "ftp://issues.example.com/")
        );
        Assert.That(_handler.Requests, Is.Empty);
    }

    // The current user and the token.

    [Test]
    public async Task GetCurrentUserSendsTheToken()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/users/me?fields=id,login,fullName,email,banned,guest",
            "Me.json"
        );

        var user = await CreateClient().GetCurrentUser();

        Assert.That(
            user,
            Is.EqualTo(
                new YouTrackUserDto(
                    "2-11",
                    "jdoe",
                    "Jane Doe",
                    "jane.doe@example.com",
                    false,
                    false
                )
            )
        );
        AssertEveryRequestCarriesTheToken();
        Assert.That(_observed, Is.EqualTo(new[] { HttpStatusCode.OK }));
    }

    [TestCase("Unauthorized-InvalidToken.json", "Invalid token")]
    [TestCase("Unauthorized-NotLoggedIn.json", "You are not logged in.")]
    public void UnauthorizedMeansAWrongToken(string fixture, string description)
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/users/me?fields=id,login,fullName,email,banned,guest",
            fixture,
            HttpStatusCode.Unauthorized
        );

        var ex = Assert.ThrowsAsync<YouTrackUnauthorizedException>(
            () => CreateClient().GetCurrentUser()
        )!;

        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(ex.Message, Does.Contain(description));
        Assert.That(_observed, Is.EqualTo(new[] { HttpStatusCode.Unauthorized }));
    }

    [Test]
    public void AnHtmlAnswerIsNotAYouTrackApi()
    {
        var serviceUrl = $"{ServiceUrl}issues/";

        _handler.AnswerHtml(
            $"{serviceUrl}api/users/me?fields=id,login,fullName,email,banned,guest"
        );

        var ex = Assert.ThrowsAsync<YouTrackNotAnApiException>(
            () => CreateClient(serviceUrl).GetCurrentUser()
        )!;

        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(ex.Message, Does.Contain("text/html instead of JSON"));
        Assert.That(
            ex.Message,
            Does.Contain($"{serviceUrl} isn't the address of a YouTrack REST API")
        );
        Assert.That(ex, Is.InstanceOf<HttpRequestException>());
    }

    [Test]
    public void ConstructorNormalizesTheServiceUrl()
    {
        Assert.That(CreateClient("https://issues.example.com").ServiceUrl, Is.EqualTo(ServiceUrl));
        Assert.That(
            CreateClient("https://www.example.com/youtrack").ServiceUrl,
            Is.EqualTo("https://www.example.com/youtrack/")
        );
    }

    // Issues.

    [Test]
    public async Task SearchIssuesSendsTheQueryAsTyped()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/issues?fields=idReadable,summary&query=%23Unresolved%20for%3A%20me&$top=100",
            "Issues.json"
        );

        var issues = await CreateClient().SearchIssues("#Unresolved for: me");

        Assert.That(issues, Has.Length.EqualTo(5));
        Assert.That(
            issues[0],
            Is.EqualTo(new YouTrackIssueDto("DEMO-12", "Search matches word prefixes"))
        );
        AssertEveryRequestCarriesTheToken();
    }

    [Test]
    public async Task SearchIssuesPassesTop()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/issues?fields=idReadable,summary&query=kubecod&$top=10",
            "Issues.json"
        );

        await CreateClient().SearchIssues("kubecod", 10);

        Assert.That(_handler.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public void AQueryYouTrackCannotParseIsAnInvalidQuery()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/issues?fields=idReadable,summary&query=state%3A%20Op&$top=100",
            "Issues-InvalidQuery.json",
            HttpStatusCode.BadRequest
        );

        var ex = Assert.ThrowsAsync<YouTrackInvalidQueryException>(
            () => CreateClient().SearchIssues("state: Op")
        )!;

        Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(ex.Message, Does.Contain("Can't parse search query"));
    }

    [Test]
    public async Task GetIssueFetchesTheIssue()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/issues/demo-12?fields=idReadable,summary",
            "Issue.json"
        );

        var issue = await CreateClient().GetIssue("demo-12");

        Assert.That(
            issue,
            Is.EqualTo(new YouTrackIssueDto("DEMO-12", "Search matches word prefixes"))
        );
        AssertEveryRequestCarriesTheToken();
    }

    [Test]
    public async Task GetIssueOfAnUnknownIdIsNull()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/issues/DEMO-999999?fields=idReadable,summary",
            "Issue-NotFound.json",
            HttpStatusCode.NotFound
        );

        var issue = await CreateClient().GetIssue("DEMO-999999");

        Assert.That(issue, Is.Null);
        Assert.That(_observed, Is.EqualTo(new[] { HttpStatusCode.NotFound }));
    }

    // The cached lists.

    [Test]
    public async Task GetProjectsLeavesOutArchivedProjects()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/admin/projects?fields=id,shortName,name,archived,iconUrl&$skip=0&$top=500",
            "Projects.json"
        );

        var projects = await CreateClient().GetProjects();

        Assert.That(
            projects.Select(p => p.ShortName),
            Is.EqualTo(new[] { "DEMO", "WEB", "OPS", "DOCS", "MOB" })
        );
        Assert.That(
            projects[0],
            Is.EqualTo(new YouTrackProjectDto("0-101", "DEMO", "Demo", false, null))
        );
        AssertEveryRequestCarriesTheToken();
    }

    [TestCase(2, new[] { 0, 2, 4, 6 })]
    [TestCase(4, new[] { 0, 4 })]
    [TestCase(6, new[] { 0, 6 })]
    [TestCase(7, new[] { 0 })]
    public async Task CachedListsPageUntilExhausted(int pageSize, int[] skips)
    {
        var all = JsonNode.Parse(Fixtures.Read("Projects.json"))!.AsArray();

        foreach (var skip in skips)
        {
            var page = new JsonArray(
                all.Skip(skip).Take(pageSize).Select(p => p!.DeepClone()).ToArray()
            );

            _handler.AnswerJson(
                $"{ServiceUrl}api/admin/projects?fields=id,shortName,name,archived,iconUrl&$skip={skip}&$top={pageSize}",
                page.ToJsonString()
            );
        }

        var projects = await CreateClient(pageSize: pageSize).GetProjects();

        Assert.That(projects, Has.Length.EqualTo(5));
        Assert.That(
            RequestedUrls,
            Is.EqualTo(
                skips.Select(p =>
                    $"{ServiceUrl}api/admin/projects?fields=id,shortName,name,archived,iconUrl&$skip={p}&$top={pageSize}"
                )
            )
        );
    }

    [Test]
    public async Task GetProjectsResolvesHostRelativeIcons()
    {
        var serviceUrl = "https://www.example.com/youtrack/";
        var json = JsonNode.Parse(Fixtures.Read("Projects.json"))!.AsArray();

        json[0]!["iconUrl"] = "/hub/api/rest/avatar/0b6f1f62-1f5e-4c43-9b0e-6d5c1b1e2a33?s=48";
        json[1]!["iconUrl"] = "api/files/74-1?sign=invented";

        _handler.AnswerJson(
            $"{serviceUrl}api/admin/projects?fields=id,shortName,name,archived,iconUrl&$skip=0&$top=500",
            json.ToJsonString()
        );

        var projects = await CreateClient(serviceUrl).GetProjects();

        Assert.That(
            projects[0].IconUrl,
            Is.EqualTo(
                "https://www.example.com/hub/api/rest/avatar/0b6f1f62-1f5e-4c43-9b0e-6d5c1b1e2a33?s=48"
            )
        );
        Assert.That(
            projects[1].IconUrl,
            Is.EqualTo("https://www.example.com/youtrack/api/files/74-1?sign=invented")
        );
        Assert.That(projects[2].IconUrl, Is.Null);
    }

    [Test]
    public async Task GetAgileBoards()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/agiles?fields=id,name&$skip=0&$top=500",
            "Agiles.json"
        );

        var boards = await CreateClient().GetAgileBoards();

        Assert.That(
            boards,
            Is.EqualTo(
                new[]
                {
                    new YouTrackAgileBoardDto("201-11", "Sprint Board"),
                    new YouTrackAgileBoardDto("201-12", "Kanban"),
                    new YouTrackAgileBoardDto("201-13", "Support")
                }
            )
        );
        AssertEveryRequestCarriesTheToken();
    }

    [Test]
    public async Task GetSavedQueries()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/savedQueries?fields=id,name,query&$skip=0&$top=500",
            "SavedQueries.json"
        );

        var queries = await CreateClient().GetSavedQueries();

        Assert.That(queries, Has.Length.EqualTo(7));
        Assert.That(
            queries[0],
            Is.EqualTo(new YouTrackSavedQueryDto("11-20", "Assigned to me", "for: me"))
        );
        Assert.That(
            queries[6].Query,
            Is.EqualTo("tag: {Needs review} sort by: {resolved date} desc")
        );
        AssertEveryRequestCarriesTheToken();
    }

    [Test]
    public async Task GetDashboards()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/dashboards?fields=id,name&$skip=0&$top=500",
            "Dashboards.json"
        );

        var dashboards = await CreateClient().GetDashboards();

        Assert.That(
            dashboards,
            Is.EqualTo(
                new[]
                {
                    new YouTrackDashboardDto("173-10", "YouTrack Default Dashboard"),
                    new YouTrackDashboardDto("173-11", "Team overview")
                }
            )
        );
        AssertEveryRequestCarriesTheToken();
    }

    [TestCase(HttpStatusCode.NotFound, "NotFound.json")]
    [TestCase(HttpStatusCode.Forbidden, "NotFound.json")]
    public void AMissingOrForbiddenEndpointIsUnavailable(HttpStatusCode statusCode, string fixture)
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/dashboards?fields=id,name&$skip=0&$top=500",
            fixture,
            statusCode
        );

        var ex = Assert.ThrowsAsync<YouTrackUnavailableException>(
            () => CreateClient().GetDashboards()
        )!;

        Assert.That(ex.StatusCode, Is.EqualTo(statusCode));
    }

    // The people directory.

    [Test]
    public async Task SearchUsersLeavesOutUsersWithoutEmailBannedUsersAndGuests()
    {
        _handler.AnswerFixture(
            $"{ServiceUrl}api/users?fields=id,login,fullName,email,banned,guest&query=jane%20doe&$top=100",
            "Users.json"
        );

        var users = await CreateClient().SearchUsers("jane doe");

        Assert.That(users.Select(p => p.Login), Is.EqualTo(new[] { "bsmith", "jdoe" }));
        AssertEveryRequestCarriesTheToken();
    }

    [Test]
    public void SearchUsersWithoutPermissionIsUnavailable()
    {
        _handler.AnswerJson(
            $"{ServiceUrl}api/users?fields=id,login,fullName,email,banned,guest&query=jane&$top=100",
            """{"error":"Forbidden","error_description":"invented"}""",
            HttpStatusCode.Forbidden
        );

        Assert.ThrowsAsync<YouTrackUnavailableException>(() => CreateClient().SearchUsers("jane"));
    }

    // Icons and avatars.

    [Test]
    public async Task DownloadFileResolvesHostRelativeUrlsAndSendsTheToken()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        _handler.AnswerBytes(
            "https://www.example.com/hub/api/rest/avatar/0b6f1f62-1f5e-4c43-9b0e-6d5c1b1e2a33?s=48",
            bytes,
            "image/png"
        );

        var (mediaType, content) = await CreateClient("https://www.example.com/youtrack/")
            .DownloadFile(
                "/hub/api/rest/avatar/0b6f1f62-1f5e-4c43-9b0e-6d5c1b1e2a33?s=48",
                async p => (p.Headers.ContentType?.MediaType, await p.ReadAsByteArrayAsync())
            );

        Assert.That(mediaType, Is.EqualTo("image/png"));
        Assert.That(content, Is.EqualTo(bytes));
        AssertEveryRequestCarriesTheToken();
        Assert.That(_observed, Is.EqualTo(new[] { HttpStatusCode.OK }));
    }

    [TestCase("https://www.gravatar.com/avatar/5f0e4ea2b5ab4b7f9b3b0f4e2c1d6a77.jpg?d=retro&s=96")]
    [TestCase("http://issues.example.com/hub/api/rest/avatar/0b6f1f62?s=48")]
    [TestCase("https://issues.example.com:8443/hub/api/rest/avatar/0b6f1f62?s=48")]
    [TestCase("https://hub.example.com/hub/api/rest/avatar/0b6f1f62?s=48")]
    public async Task DownloadFileKeepsTheTokenOffOtherHosts(string url)
    {
        _handler.AnswerBytes(url, new byte[] { 1 }, "image/jpeg");

        await CreateClient().DownloadFile(url, p => p.ReadAsByteArrayAsync());

        Assert.That(_handler.Requests.Single().Url, Is.EqualTo(url));
        Assert.That(_handler.Requests.Single().Authorization, Is.Null);
        Assert.That(_observed, Is.Empty);
    }

    [Test]
    public void DownloadFileReportsAFailure()
    {
        var ex = Assert.ThrowsAsync<YouTrackException>(
            () => CreateClient().DownloadFile("/missing.png", p => p.ReadAsByteArrayAsync())
        )!;

        Assert.That(ex.RequestUrl, Is.EqualTo($"{ServiceUrl}missing.png"));
        Assert.That((int?)ex.StatusCode, Is.EqualTo(599));
    }
}
