namespace Tql.UITests.App;

[TestFixture]
internal class BasicsFixture : SessionFixture
{
    [Test]
    public void MainWindowShows()
    {
        Session.OpenMainWindow();
    }

    [Test]
    public void OpenConfigurationWindow()
    {
        Session.OpenConfigurationWindow();
    }
}
