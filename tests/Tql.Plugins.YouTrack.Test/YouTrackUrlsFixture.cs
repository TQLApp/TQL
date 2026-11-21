using System;
using NUnit.Framework;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Test;

[TestFixture]
internal class YouTrackUrlsFixture
{
    [TestCase("https://issues.example.com", "https://issues.example.com/")]
    [TestCase("https://issues.example.com/", "https://issues.example.com/")]
    [TestCase("https://www.example.com/youtrack", "https://www.example.com/youtrack/")]
    [TestCase("https://www.example.com/youtrack//", "https://www.example.com/youtrack/")]
    [TestCase("https://example.youtrack.cloud", "https://example.youtrack.cloud/")]
    [TestCase(
        "https://example.myjetbrains.com/youtrack/",
        "https://example.myjetbrains.com/youtrack/"
    )]
    [TestCase(
        " https://Issues.Example.COM/issues?q=for:%20me#top ",
        "https://issues.example.com/issues/"
    )]
    [TestCase("issues.example.com", "https://issues.example.com/")]
    [TestCase("http://localhost:8080", "http://localhost:8080/")]
    [TestCase("https://issues.example.com:443/", "https://issues.example.com/")]
    public void NormalizeServiceUrl(string url, string expected)
    {
        Assert.That(YouTrackUrls.NormalizeServiceUrl(url), Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("ftp://issues.example.com/")]
    [TestCase("file:///c:/temp")]
    [TestCase("https://")]
    public void NormalizeServiceUrlRejectsWhatIsNotAUrl(string url)
    {
        Assert.Throws<ArgumentException>(() => YouTrackUrls.NormalizeServiceUrl(url));
    }

    [Test]
    public void ServiceUrlCandidatesStripPathSegmentsDownToTheHost()
    {
        Assert.That(
            YouTrackUrls.GetServiceUrlCandidates("https://www.example.com/youtrack/issue/KC-230"),
            Is.EqualTo(
                new[]
                {
                    "https://www.example.com/youtrack/issue/KC-230/",
                    "https://www.example.com/youtrack/issue/",
                    "https://www.example.com/youtrack/",
                    "https://www.example.com/"
                }
            )
        );
        Assert.That(
            YouTrackUrls.GetServiceUrlCandidates("https://issues.example.com/"),
            Is.EqualTo(new[] { "https://issues.example.com/" })
        );
    }

    [TestCase(
        "https://issues.example.com/",
        "/hub/api/rest/avatar/0b6f1f62?s=48",
        "https://issues.example.com/hub/api/rest/avatar/0b6f1f62?s=48"
    )]
    [TestCase(
        "https://www.example.com/youtrack/",
        "/hub/api/rest/avatar/0b6f1f62?s=48",
        "https://www.example.com/hub/api/rest/avatar/0b6f1f62?s=48"
    )]
    [TestCase(
        "https://www.example.com/youtrack",
        "api/files/74-1?sign=x",
        "https://www.example.com/youtrack/api/files/74-1?sign=x"
    )]
    [TestCase(
        "https://www.example.com/youtrack/",
        "https://www.gravatar.com/avatar/5f0e.jpg?d=retro&s=96",
        "https://www.gravatar.com/avatar/5f0e.jpg?d=retro&s=96"
    )]
    public void ResolveFollowsTheUriRules(string serviceUrl, string url, string expected)
    {
        Assert.That(YouTrackUrls.Resolve(serviceUrl, url), Is.EqualTo(expected));
    }

    [TestCase("https://www.example.com/youtrack/hub/api/x", true)]
    [TestCase("https://WWW.EXAMPLE.COM/hub/api/x", true)]
    [TestCase("https://www.example.com:443/hub/api/x", true)]
    [TestCase("http://www.example.com/hub/api/x", false)]
    [TestCase("https://www.example.com:8443/hub/api/x", false)]
    [TestCase("https://hub.example.com/hub/api/x", false)]
    [TestCase("https://www.gravatar.com/avatar/5f0e.jpg", false)]
    [TestCase("/hub/api/x", false)]
    public void IsSameOriginComparesSchemeHostAndPort(string url, bool expected)
    {
        Assert.That(
            YouTrackUrls.IsSameOrigin("https://www.example.com/youtrack/", url),
            Is.EqualTo(expected)
        );
    }

    [TestCase("https://issues.example.com/")]
    [TestCase("https://issues.example.com")]
    public void Links(string serviceUrl)
    {
        Assert.That(
            YouTrackUrls.GetIssueUrl(serviceUrl, "KC-230"),
            Is.EqualTo("https://issues.example.com/issue/KC-230")
        );
        Assert.That(
            YouTrackUrls.GetProjectUrl(serviceUrl, "KC"),
            Is.EqualTo("https://issues.example.com/projects/KC")
        );
        Assert.That(
            YouTrackUrls.GetAgileBoardUrl(serviceUrl, "201-11"),
            Is.EqualTo("https://issues.example.com/agiles/201-11/current")
        );
        Assert.That(
            YouTrackUrls.GetSavedSearchUrl(serviceUrl, "tag: {Needs review} sort by: updated"),
            Is.EqualTo(
                "https://issues.example.com/issues?q=tag%3A%20%7BNeeds%20review%7D%20sort%20by%3A%20updated"
            )
        );
        Assert.That(
            YouTrackUrls.GetDashboardUrl(serviceUrl, "173-10"),
            Is.EqualTo("https://issues.example.com/dashboard?id=173-10")
        );
        Assert.That(
            YouTrackUrls.GetNewIssueUrl(serviceUrl, "KC"),
            Is.EqualTo("https://issues.example.com/newIssue?project=KC")
        );
    }

    [Test]
    public void LinksKeepAPathPrefix()
    {
        Assert.That(
            YouTrackUrls.GetIssueUrl("https://www.example.com/youtrack/", "KC-230"),
            Is.EqualTo("https://www.example.com/youtrack/issue/KC-230")
        );
    }
}
