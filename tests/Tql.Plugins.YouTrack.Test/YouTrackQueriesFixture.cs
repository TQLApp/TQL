using NUnit.Framework;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Test;

[TestFixture]
internal class YouTrackQueriesFixture
{
    [TestCase("KC-230", true)]
    [TestCase("kc-230", true)]
    [TestCase(" KC-230 ", true)]
    [TestCase("MY_PROJ2-7", true)]
    [TestCase("A-1", true)]
    [TestCase("3-6943", false)]
    [TestCase("KC-", false)]
    [TestCase("KC 230", false)]
    [TestCase("KC-230x", false)]
    [TestCase("kubecod", false)]
    [TestCase("#Unresolved for: me", false)]
    public void IsIssueId(string text, bool expected)
    {
        Assert.That(YouTrackQueries.IsIssueId(text), Is.EqualTo(expected));
    }

    [TestCase("KC", null, "project: KC sort by: updated desc")]
    [TestCase("KC", "  ", "project: KC sort by: updated desc")]
    [TestCase("KC", " kubecod ", "project: KC kubecod")]
    [TestCase("KC", "#Unresolved for: me", "project: KC #Unresolved for: me")]
    [TestCase("MY_PROJ2", "x", "project: MY_PROJ2 x")]
    [TestCase("MY-PROJ", "x", "project: {MY-PROJ} x")]
    public void Project(string shortName, string? text, string expected)
    {
        Assert.That(YouTrackQueries.Project(shortName, text), Is.EqualTo(expected));
    }

    [TestCase("for: me", null, "for: me")]
    [TestCase("for: me", " ", "for: me")]
    [TestCase("for: me", " kubecod ", "(for: me) and (kubecod)")]
    [TestCase(
        "tag: {Needs review} sort by: {resolved date} desc",
        "kubecod",
        "(tag: {Needs review} sort by: {resolved date} desc) and (kubecod)"
    )]
    [TestCase("State: -Done Type: Bug ", "for: me", "(State: -Done Type: Bug) and (for: me)")]
    [TestCase("", "kubecod", "kubecod")]
    public void SavedSearch(string savedQuery, string? text, string expected)
    {
        Assert.That(YouTrackQueries.SavedSearch(savedQuery, text), Is.EqualTo(expected));
    }

    [TestCase("KC", "KC")]
    [TestCase("Needs review", "{Needs review}")]
    [TestCase("in-progress", "{in-progress}")]
    public void Value(string value, string expected)
    {
        Assert.That(YouTrackQueries.Value(value), Is.EqualTo(expected));
    }
}
