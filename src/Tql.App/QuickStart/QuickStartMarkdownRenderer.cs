﻿using System.Diagnostics;
using System.Web;
using Markdig;
using Markdig.Extensions.Emoji;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Tql.Abstractions;
using Tql.App.Support;
using Inline = Markdig.Syntax.Inlines.Inline;

namespace Tql.App.QuickStart;

internal class QuickStartMarkdownRenderer
{
    private readonly MarkdownPipeline _pipeline;

    public QuickStartMarkdownRenderer()
    {
        var builder = new MarkdownPipelineBuilder().UseEmojiAndSmiley().UseAutoLinks();

        builder.Extensions.Add(new KeyExtension());

        _pipeline = builder.Build();
    }

    public IEnumerable<TextBlock> Render(FrameworkElement owner, string markdown, IUI ui)
    {
        var document = MarkdownParser.Parse(markdown, _pipeline);

        var renderer = new TextBlockRenderer(owner, ui);

        renderer.Render(document);

        return renderer.TextBlocks;
    }

    private class TextBlockRenderer : RendererBase
    {
        public FrameworkElement Owner { get; }
        public IUI UI { get; }
        public List<TextBlock> TextBlocks { get; } = new();
        public InlineCollection? CurrentInlines { get; set; }
        public FontStyle? FontStyle { get; set; }
        public FontWeight? FontWeight { get; set; }

        public TextBlockRenderer(FrameworkElement owner, IUI ui)
        {
            Owner = owner;
            UI = ui;

            // Block renderers
            ObjectRenderers.Add(new ParagraphRenderer());

            // Inline renderers
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());
        }

        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return this;
        }

        public void WriteLeafInline(LeafBlock leafBlock)
        {
            var inline = (Inline?)leafBlock.Inline;

            while (inline != null)
            {
                Write(inline);
                inline = inline.NextSibling;
            }
        }

        public Run CreateRun(string? text = null)
        {
            var run = new Run(text);
            if (FontStyle.HasValue)
                run.FontStyle = FontStyle.Value;
            if (FontWeight.HasValue)
                run.FontWeight = FontWeight.Value;
            return run;
        }
    }

    private abstract class TextBlockObjectRenderer<TObject>
        : MarkdownObjectRenderer<TextBlockRenderer, TObject>
        where TObject : MarkdownObject;

    private class ParagraphRenderer : TextBlockObjectRenderer<ParagraphBlock>
    {
        protected override void Write(TextBlockRenderer renderer, ParagraphBlock obj)
        {
            var textBlock = new TextBlock();

            renderer.TextBlocks.Add(textBlock);
            renderer.CurrentInlines = textBlock.Inlines;

            renderer.WriteLeafInline(obj);

            renderer.CurrentInlines = null;
        }
    }

    private class EmphasisInlineRenderer : TextBlockObjectRenderer<EmphasisInline>
    {
        protected override void Write(TextBlockRenderer renderer, EmphasisInline obj)
        {
            var oldFontStyle = renderer.FontStyle;
            var oldFontWeight = renderer.FontWeight;

            try
            {
                if (obj.DelimiterChar is '*' or '_')
                {
                    Debug.Assert(obj.DelimiterCount <= 2);

                    if (obj.DelimiterCount == 1)
                        renderer.FontStyle = FontStyles.Italic;
                    else
                        renderer.FontWeight = FontWeights.Bold;
                }

                renderer.WriteChildren(obj);
            }
            finally
            {
                renderer.FontStyle = oldFontStyle;
                renderer.FontWeight = oldFontWeight;
            }
        }
    }

    private class LineBreakInlineRenderer : TextBlockObjectRenderer<LineBreakInline>
    {
        protected override void Write(TextBlockRenderer renderer, LineBreakInline obj)
        {
            renderer.CurrentInlines!.Add(renderer.CreateRun(" "));
        }
    }

    private class LinkInlineRenderer : TextBlockObjectRenderer<LinkInline>
    {
        protected override void Write(TextBlockRenderer renderer, LinkInline obj)
        {
            if (obj.IsImage)
                WriteImage(renderer, obj);
            else
                WriteLink(renderer, obj);
        }

        private void WriteImage(TextBlockRenderer renderer, LinkInline obj)
        {
            Debug.Assert(obj.Url != null);

            var color = default(Color?);

            var url = obj.Url!;
            var pos = url.IndexOf('?');
            if (pos != -1)
            {
                var queryString = HttpUtility.ParseQueryString(url.Substring(pos));

                if (queryString["color"] == "true")
                {
                    var brush = (SolidColorBrush)renderer.Owner.FindResource("Foreground");
                    color = brush.Color;
                }

                url = url.Substring(0, pos);
            }

            var image = Images.GetImage(url, color);

            renderer.CurrentInlines!.Add(
                new InlineUIContainer
                {
                    BaselineAlignment = BaselineAlignment.Center,
                    Child = new Image
                    {
                        Source = image,
                        Height = 15,
                        Width = 15,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            );
        }

        private static void WriteLink(TextBlockRenderer renderer, LinkInline obj)
        {
            Debug.Assert(obj.Url != null);

            var hyperlink = new Hyperlink();

            hyperlink.Click += (_, _) => renderer.UI.OpenUrl(obj.Url!);

            renderer.CurrentInlines!.Add(hyperlink);

            var oldCurrentInlines = renderer.CurrentInlines;
            renderer.CurrentInlines = hyperlink.Inlines;

            renderer.WriteChildren(obj);

            renderer.CurrentInlines = oldCurrentInlines;
        }
    }

    private class LiteralInlineRenderer : TextBlockObjectRenderer<LiteralInline>
    {
        protected override void Write(TextBlockRenderer renderer, LiteralInline obj)
        {
            if (obj is KeyInline)
            {
                foreach (var item in obj.Content.ToString()!.Split('+'))
                {
                    renderer.CurrentInlines!.Add(
                        new InlineUIContainer
                        {
                            BaselineAlignment = BaselineAlignment.Center,
                            Child = new KeyboardKey
                            {
                                Content = new TextBlock(renderer.CreateRun(item)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    );
                }
            }
            else if (obj is EmojiInline)
            {
                renderer.CurrentInlines!.Add(
                    new Emoji.Wpf.EmojiInline
                    {
                        Text = obj.Content.ToString(),
                        FontSize = WpfUtils.PointsToPixels(13),
                        Foreground = Brushes.Black
                    }
                );
            }
            else
            {
                renderer.CurrentInlines!.Add(renderer.CreateRun(obj.Content.ToString()));
            }
        }
    }

    private class KeyExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Insert the parser before any other parsers
            pipeline.InlineParsers.Insert(0, new KeyParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
    }

    private class KeyParser : InlineParser
    {
        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            // Previous char must be a space
            if (!slice.PeekCharExtra(-1).IsWhiteSpaceOrZero())
                return false;

            // Try to match the start ::
            var end = -1;

            if (slice.CurrentChar == ':' && slice.PeekChar() == ':')
            {
                for (var i = 2; i < slice.Length - 2; i++)
                {
                    if (slice.PeekChar(i) == ':' && slice.PeekChar(i + 1) == ':')
                    {
                        end = i;
                        break;
                    }
                }
            }

            if (end == -1)
                return false;

            // Push the KeyInline
            processor.Inline = new KeyInline(slice.Text.Substring(slice.Start + 2, end - 2))
            {
                Span =
                {
                    Start = processor.GetSourcePosition(slice.Start, out int line, out int column),
                },
                Line = line,
                Column = column
            };
            processor.Inline.Span.End = processor.Inline.Span.Start + end + 1;

            // Move the cursor to the character after the matched string
            slice.Start += end + 2;

            return true;
        }
    }

    private class KeyInline(string text) : LiteralInline(text);
}
