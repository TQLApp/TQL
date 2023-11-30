namespace Tql.Plugins.GitHub.Test;

[TestFixture]
internal class GitHubFixture : BaseFixture
{
    [Test]
    public async Task FindRepository()
    {
        var results = await Search("github repository", "gitextensions");

        Assert.IsTrue(results.Any(p => p.Text == "gitextensions/gitextensions"));
    }
}
