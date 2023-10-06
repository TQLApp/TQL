using Launcher.Abstractions;
using Launcher.App.Search;
using Launcher.App.Services;
using System.Windows.Media.Animation;
using Image = System.Windows.Controls.Image;

namespace Launcher.App;

internal partial class SearchResultUserControl
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
        typeof(SearchResultUserControl),
        new FrameworkPropertyMetadata(
            false,
            (d, e) => ((SearchResultUserControl)d).OnIsSelectedChanged(d, e)
        )
    );

    private new SearchResult? DataContext => (SearchResult?)base.DataContext;

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public event EventHandler? HistoryRemoved;

    public SearchResultUserControl()
    {
        InitializeComponent();
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

    private void IsSelectedOrMouseOverChanged()
    {
        RenderMatchIcons();

        if (IsMouseOver || IsSelected)
        {
            var to = _marqueeBorder.ActualWidth - _marqueeContent.ActualWidth - 4;
            if (to < 0)
            {
                var duration = TimeSpan.FromMilliseconds(-to * 10);
                var delay = TimeSpan.FromSeconds(0.7);
                var wait = TimeSpan.FromSeconds(1);

                var animation = new DoubleAnimationUsingKeyFrames
                {
                    RepeatBehavior = RepeatBehavior.Forever,
                    KeyFrames =
                    {
                        new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)),
                        new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay)),
                        new LinearDoubleKeyFrame(to, KeyTime.FromTimeSpan(delay + duration)),
                        new LinearDoubleKeyFrame(to, KeyTime.FromTimeSpan(delay + duration + wait))
                    }
                };

                _marqueeContent.BeginAnimation(Canvas.LeftProperty, animation);
            }
        }
        else
        {
            _marqueeContent.BeginAnimation(Canvas.LeftProperty, null);

            Canvas.SetLeft(_marqueeContent, 0);
        }
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

    private void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        IsSelectedOrMouseOverChanged();

    private void UserControl_MouseEnter(object sender, MouseEventArgs e) =>
        IsSelectedOrMouseOverChanged();

    private void UserControl_MouseLeave(object sender, MouseEventArgs e) =>
        IsSelectedOrMouseOverChanged();

    protected virtual void OnHistoryRemoved()
    {
        HistoryRemoved?.Invoke(this, EventArgs.Empty);
    }

    private void _marqueeCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        _marqueeCanvas.Height =
            _resultPanel.RenderSize.Height + _resultPanel.Margin.Top + _resultPanel.Margin.Bottom;
    }
}
