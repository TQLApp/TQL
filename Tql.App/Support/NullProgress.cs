namespace Tql.App.Support;

internal class NullProgress : IProgress
{
    public static readonly NullProgress Instance = new();

    public static NullProgress FromCancellationToken(CancellationToken cancellationToken) =>
        new NullProgress(cancellationToken);

    public string Title { get; set; } = "";
    public bool CanCancel { get; set; }
    public CancellationToken CancellationToken { get; }

    private NullProgress(CancellationToken cancellationToken = default)
    {
        CancellationToken = cancellationToken;
    }

    public void SetProgress(double progress) { }

    public void SetProgress(string? status, double progress) { }
}
