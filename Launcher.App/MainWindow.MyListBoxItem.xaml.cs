namespace Launcher.App;

partial class MainWindow
{
    private class MyListBoxItem : ListBoxItem
    {
        public event EventHandler? IsMouseOverOrSelectedChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsSelectedProperty || e.Property == IsMouseOverProperty)
                OnIsMouseOverOrSelectedChanged();
        }

        protected virtual void OnIsMouseOverOrSelectedChanged() =>
            IsMouseOverOrSelectedChanged?.Invoke(this, EventArgs.Empty);
    }
}
