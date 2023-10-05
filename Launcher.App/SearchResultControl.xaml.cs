using Launcher.Abstractions;
using Launcher.App.Search;
using Launcher.App.Services;
using Image = System.Windows.Controls.Image;

namespace Launcher.App;

internal partial class SearchResultControl
{
    private static DrawingImage LoadImage(string resourceName, Brush? fill = null)
    {
        using var stream = Application
            .GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.Relative))!
            .Stream;

        return ImageFactory.CreateSvgImage(stream!, fill);
    }

    private static readonly DrawingImage RunImage = LoadImage("Person Running.svg");
    private static readonly DrawingImage StarImage = LoadImage("Star.svg");
    private static readonly DrawingImage DismissImage = LoadImage("Dismiss.svg");
    private static readonly DrawingImage CategoryImage = LoadImage("Apps List.svg");

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected),
        typeof(bool),
        typeof(SearchResultControl),
        new FrameworkPropertyMetadata(
            false,
            (d, e) => ((SearchResultControl)d).OnIsSelectedChanged(d, e)
        )
    );

    private new SearchResult? DataContext => (SearchResult?)base.DataContext;

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public event EventHandler? HistoryRemoved;

    public SearchResultControl()
    {
        InitializeComponent();
    }

    private void RenderMatchIcons()
    {
        var searchResult = DataContext!;

        _iconsPanel.Children.Clear();

        if (IsMouseOver || IsSelected)
        {
            if (searchResult.Match is IRunnableMatch)
                AddIcon(RunImage);
            if (searchResult.Match is ISearchableMatch)
                AddIcon(CategoryImage);
        }

        if (searchResult.HistoryId.HasValue)
        {
            var star = AddIcon(StarImage);
            var dismiss = AddIcon(DismissImage);

            star.MouseEnter += (_, _) =>
            {
                star.Visibility = Visibility.Collapsed;
                dismiss.Visibility = Visibility.Visible;
            };

            dismiss.MouseLeave += (_, _) =>
            {
                dismiss.Visibility = Visibility.Collapsed;
                star.Visibility = Visibility.Visible;
            };

            dismiss.Cursor = Cursors.Hand;
            dismiss.Visibility = Visibility.Collapsed;

            dismiss.MouseDown += (_, _) => dismiss.CaptureMouse();

            dismiss.MouseUp += (_, e) =>
            {
                if (dismiss.IsMouseOver)
                    OnHistoryRemoved();

                dismiss.ReleaseMouseCapture();

                e.Handled = true;
            };
        }

        Image AddIcon(DrawingImage icon)
        {
            var image = new Image
            {
                Source = icon,
                Width = 14,
                Height = 14,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            _iconsPanel.Children.Add(image);

            return image;
        }
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var searchResult = DataContext!;

        SearchResultUtils.RenderMatch(
            _resultPanel.Children,
            searchResult.Match,
            searchResult.TextMatch,
            searchResult.IsFuzzyMatch
        );

        RenderMatchIcons();
    }

    private void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        RenderMatchIcons();

    private void UserControl_MouseEnter(object sender, MouseEventArgs e) => RenderMatchIcons();

    private void UserControl_MouseLeave(object sender, MouseEventArgs e) => RenderMatchIcons();

    protected virtual void OnHistoryRemoved()
    {
        HistoryRemoved?.Invoke(this, EventArgs.Empty);
    }
}
