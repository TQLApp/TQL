namespace Tql.App.QuickStart;

internal interface IPlaybook
{
    PlaybookPage this[string id] { get; }

    event EventHandler Updated;
}

internal record PlaybookPage(string Id, string Title, string Text);
