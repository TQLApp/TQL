namespace Tql.App.Support;

internal interface IProgress
{
    bool CanCancel { get; set; }
    CancellationToken CancellationToken { get; }

    void SetProgress(double progress);
    void SetProgress(string? status, double progress);
}
