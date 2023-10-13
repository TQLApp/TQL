using Launcher.Abstractions;
using Launcher.App.Search;
using Launcher.App.Support;
using Launcher.Utilities;
using Image = System.Windows.Controls.Image;

namespace Launcher.App;

internal partial class SearchResultUserControl
{
    private static DrawingImage LoadImage(string resourceName)
    {
        using var stream = Application
            .GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.Relative))!
            .Stream;

        return ImageFactory.CreateSvgImage(stream!);
    }

    public static readonly DrawingImage RunImage = LoadImage("Person Running.svg");
    public static readonly DrawingImage StarImage = LoadImage("Star.svg");
    public static readonly DrawingImage DismissImage = LoadImage("Dismiss.svg");
    public static readonly DrawingImage CategoryImage = LoadImage("Apps List.svg");
    public static readonly DrawingImage CopyImage = LoadImage("Copy.svg");

    private ListBoxItem? _listBoxItem;

    private new SearchResult? DataContext => (SearchResult?)base.DataContext;
    private bool IsListBoxItemSelectedOrMouseOver =>
        _listBoxItem != null && (_listBoxItem.IsMouseOver || _listBoxItem.IsSelected);

    public event EventHandler? RemoveHistoryClicked;
    public event EventHandler? CopyClicked;

    public SearchResultUserControl()
    {
        InitializeComponent();
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);

        _listBoxItem = this.FindVisualParent<ListBoxItem>();
        if (_listBoxItem == null)
            return;

        var searchResult = DataContext;

        if (searchResult is { Match: IRunnableMatch or ISearchableMatch })
            _listBoxItem.Cursor = Cursors.Hand;

        _listBoxItem.MouseEnter += (_, _) => IsListBoxItemSelectedOrMouseOverChanged();
        _listBoxItem.MouseLeave += (_, _) => IsListBoxItemSelectedOrMouseOverChanged();
        _listBoxItem.Selected += (_, _) => IsListBoxItemSelectedOrMouseOverChanged();
        _listBoxItem.Unselected += (_, _) => IsListBoxItemSelectedOrMouseOverChanged();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var searchResult = DataContext!;

        _resultPanel.Child = SearchResultUtils.RenderMatch(
            searchResult.Match,
            searchResult.TextMatch,
            searchResult.IsFuzzyMatch,
            wrapTextInMarquee: true
        );

        RenderMatchIcons();
    }

    private void IsListBoxItemSelectedOrMouseOverChanged()
    {
        RenderMatchIcons();

        var marquee = _resultPanel.FindVisualChild<MarqueeControl>()!;

        marquee.IsRunning = IsListBoxItemSelectedOrMouseOver;
    }

    private void RenderMatchIcons()
    {
        var searchResult = DataContext!;

        _iconsPanel.Children.Clear();

        if (IsListBoxItemSelectedOrMouseOver)
        {
            if (searchResult.Match is IRunnableMatch)
                AddIcon(RunImage, "Match can be launched");
            if (searchResult.Match is ICopyableMatch)
                AddIcon(CopyImage, "Copy a link to the match to the clipboard")
                    .AttachOnClickHandler((_, _) => OnCopyClicked());
            if (searchResult.Match is ISearchableMatch)
                AddIcon(CategoryImage, "Match contains sub items");
        }

        if (searchResult.HistoryId.HasValue)
        {
            var star = AddIcon(StarImage);
            var dismiss = AddIcon(DismissImage, "Remove match from the history");

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

            dismiss.Visibility = Visibility.Collapsed;
            dismiss.AttachOnClickHandler((_, _) => OnRemoveHistoryClicked());
        }

        _separator.Visibility =
            _iconsPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        Image AddIcon(DrawingImage icon, string? toolTip = null)
        {
            ToolTip? toolTipControl = null;
            if (toolTip != null)
            {
                toolTipControl = new ToolTip
                {
                    FontSize = SystemFonts.MessageFontSize,
                    Content = toolTip
                };
            }

            var image = new Image
            {
                Source = icon,
                Width = 14,
                Height = 14,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = toolTipControl
            };

            _iconsPanel.Children.Add(image);

            return image;
        }
    }

    protected virtual void OnRemoveHistoryClicked() =>
        RemoveHistoryClicked?.Invoke(this, EventArgs.Empty);

    protected virtual void OnCopyClicked() => CopyClicked?.Invoke(this, EventArgs.Empty);
}
