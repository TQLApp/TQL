namespace Tql.App.Support;

internal interface IProgress
{
    string Title { get; set; }
    bool CanCancel { get; set; }
    CancellationToken CancellationToken { get; }

    void SetProgress(double progress);
    void SetProgress(string? status, double progress);
}
