namespace Tql.App.Support;

internal static class ProgressExtensions
{
    public static IProgress GetSubProgress(this IProgress self, double start, double end) =>
        new SubProgress(self, start, end);

    private class DelegateProgress(IProgress owner) : IProgress
    {
        public virtual string Title
        {
            get => owner.Title;
            set => owner.Title = value;
        }

        public virtual bool CanCancel
        {
            get => owner.CanCancel;
            set => owner.CanCancel = value;
        }

        public virtual CancellationToken CancellationToken => owner.CancellationToken;

        public virtual void SetProgress(double progress) => SetProgress(null, progress);

        public virtual void SetProgress(string? status, double progress) =>
            owner.SetProgress(status, progress);
    }

    private class SubProgress(IProgress owner, double start, double end) : DelegateProgress(owner)
    {
        public override void SetProgress(string? status, double progress)
        {
            base.SetProgress(status, start + Math.Clamp(progress, 0, 1) * (end - start));
        }
    }
}
