using Tql.Abstractions;
using Tql.App.Search;
using Tql.App.Support;
using Image = System.Windows.Controls.Image;

namespace Tql.App;

internal partial class SearchResultUserControl
{
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
            {
                AddIcon(
                    Images.Run,
                    "Match can be launched",
                    "This match is for something you can launch. Click on it, or press Enter to launch the match."
                );
            }
            if (searchResult.Match is ICopyableMatch)
            {
                AddIcon(Images.Copy, content: "Copy a link to the match to the clipboard.")
                    .AttachOnClickHandler(OnCopyClicked);
            }
            if (searchResult.Match is ISearchableMatch)
            {
                AddIcon(
                    Images.Category,
                    "Match contains sub items",
                    "This match is a category and can it self be searched. Press Tab to enter the category to search in it."
                );
            }
        }

        if (searchResult.HistoryId.HasValue)
        {
            var star = AddIcon(Images.Star);
            var dismiss = AddIcon(Images.Dismiss, content: "Remove match from the history");

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
            dismiss.AttachOnClickHandler(OnRemoveHistoryClicked);
        }

        _separator.Visibility =
            _iconsPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        Image AddIcon(DrawingImage icon, string? header = null, string? content = null)
        {
            var image = new Image
            {
                Source = icon,
                Width = 14,
                Height = 14,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            image.SetPopoverToolTip(header, content);

            _iconsPanel.Children.Add(image);

            return image;
        }
    }

    protected virtual void OnRemoveHistoryClicked() =>
        RemoveHistoryClicked?.Invoke(this, EventArgs.Empty);

    protected virtual void OnCopyClicked() => CopyClicked?.Invoke(this, EventArgs.Empty);
}
