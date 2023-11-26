using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.QuickStart;

internal partial class QuickStartWindow
{
    private readonly IUI _ui;
    private static readonly QuickStartMarkdownRenderer MarkdownRenderer = new();

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(QuickStartWindow),
        new PropertyMetadata(
            string.Empty,
            (d, e) => ((QuickStartWindow)d).ApplyText((string)e.NewValue)
        )
    );

    public static readonly DependencyProperty CanMoveProperty = DependencyProperty.Register(
        nameof(CanMove),
        typeof(bool),
        typeof(QuickStartWindow),
        new PropertyMetadata(true, (d, e) => ((QuickStartWindow)d).OnCanMoveChanged(e))
    );

    public static readonly DependencyProperty CanGoBackProperty = DependencyProperty.Register(
        nameof(CanGoBack),
        typeof(bool),
        typeof(QuickStartWindow),
        new PropertyMetadata(false)
    );

    private Point? _startWindowPosition;
    private Point? _startMousePosition;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool CanMove
    {
        get => (bool)GetValue(CanMoveProperty);
        set => SetValue(CanMoveProperty, value);
    }

    public bool CanGoBack
    {
        get => (bool)GetValue(CanGoBackProperty);
        set => SetValue(CanGoBackProperty, value);
    }

    public event EventHandler<QuickStartPopupButtonEventArgs>? ChoiceButtonClicked;
    public event EventHandler<QuickStartPopupButtonEventArgs>? ButtonClicked;
    public event EventHandler? BackClicked;
    public event EventHandler? DismissClicked;

    public Visibility ArrowVisibility
    {
        get => _arrow.Visibility;
        set => _arrow.Visibility = value;
    }

    public double ContentWidth
    {
        get => _content.Width;
        set => _content.Width = value;
    }

    public QuickStartWindow(IUI ui)
    {
        _ui = ui;

        InitializeComponent();

        _back.Source = Images.QuickStartArrowLeft;
        _backButton.AttachOnClickHandler(OnBackClicked);
        _backButton.SetPopoverToolTip(Labels.QuickStartWindow_PreviousStep);
        _dismiss.Source = Images.QuickStartDismiss;
        _dismissButton.AttachOnClickHandler(OnDismissClicked);
    }

    private void OnCanMoveChanged(DependencyPropertyChangedEventArgs e)
    {
        UpdateCursor();
    }

    private void ChoiceButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;

        OnChoiceButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;

        OnButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    private void ApplyText(string text)
    {
        _lines.Items.Clear();

        foreach (var textBlock in MarkdownRenderer.Render(this, text, _ui))
        {
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(5);

            _lines.Items.Add(textBlock);
        }
    }

    protected virtual void OnChoiceButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ChoiceButtonClicked?.Invoke(this, e);

    protected virtual void OnButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ButtonClicked?.Invoke(this, e);

    protected virtual void OnBackClicked() => BackClicked?.Invoke(this, EventArgs.Empty);

    protected virtual void OnDismissClicked() => DismissClicked?.Invoke(this, EventArgs.Empty);

    private void BaseWindow_SourceInitialized(object? sender, EventArgs e)
    {
        // Hides the window from the task switcher.
        WindowInterop.AddWindowStyle(
            new WindowInteropHelper(this).Handle,
            WindowInterop.WS_EX_TOOLWINDOW
        );

        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (new WindowInteropHelper(this).Handle == IntPtr.Zero)
            return;

        if (CanMove)
        {
            Cursor = CursorUtils.Create(
                Images.Grab,
                new Point(8, 8),
                new Size(16, 16),
                VisualTreeHelper.GetDpi(this)
            );
        }
        else
        {
            ClearValue(CursorProperty);
        }
    }

    private void BaseWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !CanMove)
            return;

        _startWindowPosition = new Point(Left, Top);
        _startMousePosition = GetCursorLocation(e);

        CaptureMouse();
    }

    private void BaseWindow_MouseMove(object sender, MouseEventArgs e)
    {
        if (!IsMouseCaptured || !_startWindowPosition.HasValue || !_startMousePosition.HasValue)
            return;

        var position = GetCursorLocation(e);

        var deltaX = position.X - _startMousePosition.Value.X;
        var deltaY = position.Y - _startMousePosition.Value.Y;

        Left = _startWindowPosition.Value.X + deltaX;
        Top = _startWindowPosition.Value.Y + deltaY;
    }

    private Point GetCursorLocation(MouseEventArgs e)
    {
        var location = PointToScreen(e.GetPosition(this));

        var dpi = VisualTreeHelper.GetDpi(this);

        return new Point(location.X / dpi.DpiScaleX, location.Y / dpi.DpiScaleY);
    }

    private void BaseWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();

            _startWindowPosition = null;
            _startMousePosition = null;
        }
    }
}
