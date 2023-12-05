using System.Windows;
using Tql.UITests.Support;

namespace Tql.UITests;

internal class SessionFixture : FixtureBase
{
    private Session? _session;

    protected Session Session =>
        _session ?? throw new InvalidOperationException("Session has not been initialized");

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        ResetApp();

        _session = StartApp();
    }

    [TearDown]
    public virtual void TearDown()
    {
        _session?.CloseOpenWindows();
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        _session?.Dispose();
        _session = null;
    }
}
