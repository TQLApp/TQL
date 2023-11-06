using System.ComponentModel;
using System.Windows.Markup;

namespace Tql.App.Support;

// Taken from https://stackoverflow.com/questions/612774.

[DefaultProperty("Content")]
[ContentProperty("Content")]
internal class TextBlockTrimmer : ContentControl
{
    private class TextChangedEventMonitor : IDisposable
    {
        private readonly TextBlockTrimmer _textBlockTrimmer;

        public TextChangedEventMonitor(TextBlockTrimmer textBlockTrimmer)
        {
            _textBlockTrimmer = textBlockTrimmer;
            TextPropertyDescriptor.RemoveValueChanged(
                textBlockTrimmer.Content,
                textBlockTrimmer.TextBlock_TextChanged
            );
        }

        public void Dispose()
        {
            TextPropertyDescriptor.AddValueChanged(
                _textBlockTrimmer.Content,
                _textBlockTrimmer.TextBlock_TextChanged
            );
        }
    }

    private static readonly DependencyPropertyDescriptor TextPropertyDescriptor =
        DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

    private const string Ellipsis = "...";

    public EllipsisPosition EllipsisPosition
    {
        get => (EllipsisPosition)GetValue(EllipsisPositionProperty);
        set => SetValue(EllipsisPositionProperty, value);
    }

    public static readonly DependencyProperty EllipsisPositionProperty =
        DependencyProperty.Register(
            nameof(EllipsisPosition),
            typeof(EllipsisPosition),
            typeof(TextBlockTrimmer),
            new PropertyMetadata(EllipsisPosition.End, OnEllipsisPositionChanged)
        );

    private static void OnEllipsisPositionChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        ((TextBlockTrimmer)d).TrimText();
    }

    private string _originalText = string.Empty;

    private Size _constraint;

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        if (oldContent is TextBlock oldTextBlock)
        {
            TextPropertyDescriptor.RemoveValueChanged(oldTextBlock, TextBlock_TextChanged);
        }

        if (newContent != null && !(newContent is TextBlock))
            // ReSharper disable once LocalizableElement
            throw new ArgumentException(
                "TextBlockTrimmer access only TextBlock content",
                nameof(newContent)
            );

        var newTextBlock = (TextBlock?)newContent;
        if (newTextBlock != null)
        {
            TextPropertyDescriptor.AddValueChanged(newTextBlock, TextBlock_TextChanged);
            _originalText = newTextBlock.Text;
        }
        else
        {
            _originalText = string.Empty;
        }

        base.OnContentChanged(oldContent, newContent);
    }

    private void TextBlock_TextChanged(object sender, EventArgs e)
    {
        _originalText = ((TextBlock)sender).Text;
        TrimText();
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _constraint = constraint;
        return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var result = base.ArrangeOverride(arrangeBounds);
        TrimText();
        return result;
    }

    private IDisposable BlockTextChangedEvent()
    {
        return new TextChangedEventMonitor(this);
    }

    private static double MeasureString(TextBlock textBlock, string text)
    {
        textBlock.Text = text;
        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return textBlock.DesiredSize.Width;
    }

    private void TrimText()
    {
        var textBlock = (TextBlock)Content;
        if (textBlock == null)
            return;

        if (DesignerProperties.GetIsInDesignMode(textBlock))
            return;

        var freeSize =
            _constraint.Width
            - Padding.Left
            - Padding.Right
            - textBlock.Margin.Left
            - textBlock.Margin.Right;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (freeSize <= 0)
            return;

        using (BlockTextChangedEvent())
        {
            // this actually sets textBlock's text back to its original value
            var desiredSize = MeasureString(textBlock, _originalText);

            if (desiredSize <= freeSize)
                return;

            var ellipsisSize = MeasureString(textBlock, Ellipsis);
            freeSize -= ellipsisSize;
            var epsilon = ellipsisSize / 3;

            if (freeSize < epsilon)
            {
                textBlock.Text = _originalText;
                return;
            }

            var segments = new List<string>();

            var builder = new StringBuilder();

            switch (EllipsisPosition)
            {
                case EllipsisPosition.End:
                    TrimText(textBlock, _originalText, freeSize, segments, epsilon, false);
                    foreach (var segment in segments)
                        builder.Append(segment);
                    builder.Append(Ellipsis);
                    break;

                case EllipsisPosition.Start:
                    TrimText(textBlock, _originalText, freeSize, segments, epsilon, true);
                    builder.Append(Ellipsis);
                    foreach (var segment in ((IEnumerable<string>)segments).Reverse())
                        builder.Append(segment);
                    break;

                case EllipsisPosition.Middle:
                    var textLength = _originalText.Length / 2;
                    var firstHalf = _originalText.Substring(0, textLength);
                    var secondHalf = _originalText.Substring(textLength);

                    freeSize /= 2;

                    TrimText(textBlock, firstHalf, freeSize, segments, epsilon, false);
                    foreach (var segment in segments)
                        builder.Append(segment);
                    builder.Append(Ellipsis);

                    segments.Clear();

                    TrimText(textBlock, secondHalf, freeSize, segments, epsilon, true);
                    foreach (var segment in ((IEnumerable<string>)segments).Reverse())
                        builder.Append(segment);
                    break;
                default:
                    throw new NotSupportedException();
            }

            textBlock.Text = builder.ToString();
        }
    }

    private static void TrimText(
        TextBlock textBlock,
        string text,
        double size,
        ICollection<string> segments,
        double epsilon,
        bool reversed
    )
    {
        while (true)
        {
            if (text.Length == 1)
            {
                var textSize = MeasureString(textBlock, text);
                if (textSize <= size)
                    segments.Add(text);

                return;
            }

            var halfLength = Math.Max(1, text.Length / 2);
            var firstHalf = reversed ? text.Substring(halfLength) : text.Substring(0, halfLength);
            var remainingSize = size - MeasureString(textBlock, firstHalf);
            if (remainingSize < 0)
            {
                // only one character and it's still too large for the room, skip it
                if (firstHalf.Length == 1)
                    return;

                text = firstHalf;
                continue;
            }

            segments.Add(firstHalf);

            if (remainingSize > epsilon)
            {
                var secondHalf = reversed
                    ? text.Substring(0, halfLength)
                    : text.Substring(halfLength);
                text = secondHalf;
                size = remainingSize;
                continue;
            }

            break;
        }
    }
}

internal enum EllipsisPosition
{
    Start,
    Middle,
    End
}
