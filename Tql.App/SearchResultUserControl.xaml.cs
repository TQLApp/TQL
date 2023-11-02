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
    private bool IsListBoxItemMouseOver => _listBoxItem is { IsMouseOver: true };

    public event EventHandler? RemoveHistoryClicked;
    public event EventHandler? CopyClicked;
    public event EventHandler? PinClicked;
    public event EventHandler? UnpinClicked;

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
                    Labels.SearchResultControl_MatchCanBeLaunched,
                    Labels.SearchResultControl_MatchCanBeLaunchedHelpText
                );
            }
            if (searchResult.Match is ICopyableMatch)
            {
                AddIcon(Images.Copy, content: Labels.SearchResultControl_CopyMatchLinkHelpText)
                    .AttachOnClickHandler(OnCopyClicked);
            }
            if (searchResult.Match is ISearchableMatch)
            {
                AddIcon(
                    Images.Category,
                    Labels.SearchResultControl_MatchContainsSubItems,
                    Labels.SearchResultControl_MatchContainsSubItemsHelpText
                );
            }
        }

        if (
            searchResult.Match is ISerializableMatch
            && (searchResult.IsPinned || IsListBoxItemMouseOver)
        )
        {
            var pin = AddIcon(
                Images.Pin,
                Labels.SearchResultControl_PinSearchResult,
                Labels.SearchResultControl_PinSearchResultHelpText
            );
            var pinOff = AddIcon(
                Images.PinOff,
                content: Labels.SearchResultControl_UnpinSearchResultHelpText
            );

            if (searchResult.IsPinned)
            {
                pin.Visibility = Visibility.Visible;
                pinOff.Visibility = Visibility.Collapsed;

                pin.MouseEnter += (_, _) =>
                {
                    pin.Visibility = Visibility.Collapsed;
                    pinOff.Visibility = Visibility.Visible;
                };

                pinOff.MouseLeave += (_, _) =>
                {
                    pinOff.Visibility = Visibility.Collapsed;
                    pin.Visibility = Visibility.Visible;
                };

                pinOff.AttachOnClickHandler(OnUnpinClicked);
            }
            else if (IsListBoxItemMouseOver)
            {
                pinOff.Visibility = Visibility.Collapsed;

                pin.AttachOnClickHandler(OnPinClicked);
            }
        }

        if (searchResult.HistoryId.HasValue)
        {
            var star = AddIcon(Images.Star);
            var dismiss = AddIcon(
                Images.Dismiss,
                content: Labels.SearchResultControl_RemoveMatchFromHistoryHelpText
            );

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

    protected virtual void OnPinClicked() => PinClicked?.Invoke(this, EventArgs.Empty);

    protected virtual void OnUnpinClicked() => UnpinClicked?.Invoke(this, EventArgs.Empty);
}
