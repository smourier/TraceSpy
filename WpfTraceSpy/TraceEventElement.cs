﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TraceSpy
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class TraceEventElement : FrameworkElement
    {
        private static readonly CultureInfo _culture = CultureInfo.GetCultureInfo(1033);

        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register(nameof(Event), typeof(TraceEvent), typeof(TraceEventElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        private ListView _listView;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            _listView = this.GetVisualParent<ListView>();
        }

        private static double FontSize => Math.Max(App.Current.Settings.FontSize, 5);

        public TraceEvent Event { get => (TraceEvent)GetValue(EventProperty); set => SetValue(EventProperty, value); }

        protected override Size MeasureOverride(Size availableSize)
        {
            double height = 0;
            if (Event != null && Event.Text != null)
            {
#if FX4
                var formattedText = new FormattedText(
                    Event.Text,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    Event.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TextColorBrush);
#else
                var formattedText = new FormattedText(
                    Event.Text,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    Event.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TextColorBrush,
                    App.PixelsPerDip);
#endif

                // if colorizers, we don't know how to wrap
                var wrap = App.Current.Settings.WrapText;
                if (Event.Ranges.Count > 1)
                {
                    wrap = false;
                }

                // no wrap => 1 line only
                if (!wrap && Event.Texts.Length <= 1)
                {
                    formattedText.MaxLineCount = 1;
                }

                // if more than one colorized line, we don't know how to wrap
                if (Event.Ranges.Count > 1 && Event.Texts.Length > 1)
                {
                    formattedText.MaxLineCount = Event.Texts.Length;
                }

                formattedText.Trimming = TextTrimming.CharacterEllipsis;
                formattedText.MaxTextWidth = App.Current.ColumnLayout.TextColumnWidth;
                height = formattedText.Height;
            }

            height = Math.Min(_listView.ActualHeight * 2 / 3, height);
            return new Size(App.Current.ColumnLayout.RowWidth, Math.Max(FontSize + 2, height));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var evt = Event;
            if (evt == null)
                return;

            if (evt.BackgroundBrush != null)
            {
                drawingContext.DrawRectangle(evt.BackgroundBrush, null, new Rect(RenderSize));
            }

            if ((evt.Index % 2) == 1)
            {
                var altBrush = App.Current.Settings.AlternateBrush;
                if (altBrush != null)
                {
                    drawingContext.DrawRectangle(altBrush, null, new Rect(RenderSize));
                }
            }

            double offset = 0;
            var index = evt.Index.ToString();

#if FX4
            var formattedText = new FormattedText(
                index,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.IndexColorBrush);
#else
            var ppd = App.PixelsPerDip;
            var formattedText = new FormattedText(
                index,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.IndexColorBrush,
                ppd);
#endif


            formattedText.MaxTextWidth = App.Current.ColumnLayout.IndexColumnWidth;
            drawingContext.DrawText(formattedText, new Point(0, 0));
            offset += App.Current.ColumnLayout.IndexColumnWidth;

#if FX4
            formattedText = new FormattedText(
                evt.TicksText,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TicksColorBrush);
#else
            formattedText = new FormattedText(
                evt.TicksText,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TicksColorBrush,
                ppd);
#endif

            formattedText.MaxLineCount = 1;
            formattedText.Trimming = TextTrimming.CharacterEllipsis;
            formattedText.MaxTextWidth = App.Current.ColumnLayout.TicksColumnWidth;
            drawingContext.DrawText(formattedText, new Point(offset, 0));
            offset += App.Current.ColumnLayout.TicksColumnWidth;

            if (evt.ProcessName != null)
            {
#if FX4
                formattedText = new FormattedText(
                    evt.ProcessName,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.ProcessColorBrush);
#else
                formattedText = new FormattedText(
                    evt.ProcessName,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.ProcessColorBrush,
                    ppd);
#endif

                formattedText.MaxLineCount = 1;
                formattedText.Trimming = TextTrimming.CharacterEllipsis;
                formattedText.MaxTextWidth = App.Current.ColumnLayout.ProcessColumnWidth;
                drawingContext.DrawText(formattedText, new Point(offset, 0));
            }

            offset += App.Current.ColumnLayout.ProcessColumnWidth;

            if (evt.Text != null)
            {
                var ranges = evt.Ranges;
                if (evt.DontColorize || ranges.Count == 0)
                {
#if FX4
                    formattedText = new FormattedText(
                        evt.Text,
                        _culture,
                        FlowDirection.LeftToRight,
                        App.Current.Settings.TypeFace,
                        FontSize,
                        evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TextColorBrush);
#else
                    formattedText = new FormattedText(
                        evt.Text,
                        _culture,
                        FlowDirection.LeftToRight,
                        App.Current.Settings.TypeFace,
                        FontSize,
                        evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.TextColorBrush,
                        ppd);
#endif

                    // no wrap => 1 line only
                    if (!App.Current.Settings.WrapText && Event.Texts.Length <= 1)
                    {
                        formattedText.MaxLineCount = 1;
                    }

                    // if more than one colorized line, we don't know how to wrap
                    if (Event.Ranges.Count > 1 && Event.Texts.Length > 1)
                    {
                        formattedText.MaxLineCount = Event.Texts.Length;
                    }

                    formattedText.Trimming = TextTrimming.CharacterEllipsis;
                    formattedText.MaxTextWidth = App.Current.ColumnLayout.TextColumnWidth;
                    formattedText.MaxTextHeight = Math.Max(1, _listView.ActualHeight * 2 / 3);
                    drawingContext.DrawText(formattedText, new Point(offset, 0));
                }
                else
                {
                    var x = 0d;
                    var y = 0d;
                    var lastLineIndex = 0;
                    var maxWidth = App.Current.ColumnLayout.TextColumnWidth;
                    var rangesOffset = 0;
                    for (var i = 0; i < ranges.Count; i++)
                    {
                        var range = ranges[i];
                        if (range.Length == 0)
                            continue;

                        if (range.TextsIndex != lastLineIndex)
                        {
                            maxWidth = App.Current.ColumnLayout.TextColumnWidth;
                            x = 0;
                            y += formattedText.Height;
                            lastLineIndex = range.TextsIndex;
                            rangesOffset = 0;
                        }

                        if (maxWidth < 0 || range.StartIndex < rangesOffset)
                        {
                            rangesOffset = range.StartIndex + range.Length;
                            continue;
                        }

                        var chunk = evt.Texts[range.TextsIndex].Substring(range.StartIndex, range.Length);

#if FX4
                        formattedText = new FormattedText(
                            chunk,
                            _culture,
                            FlowDirection.LeftToRight,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item1 : App.Current.Settings.TypeFace,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item2 : FontSize,
                            range.ColorSet != null ? range.ColorSet.ForeBrush : evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.RangeTextColorBrush);
#else
                        formattedText = new FormattedText(
                            chunk,
                            _culture,
                            FlowDirection.LeftToRight,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item1 : App.Current.Settings.TypeFace,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item2 : FontSize,
                            range.ColorSet != null ? range.ColorSet.ForeBrush : evt.IsSelected ? App.CurrentTheme.SelectedTextColorBrush : App.CurrentTheme.RangeTextColorBrush,
                            ppd);
#endif

                        formattedText.MaxLineCount = 1;
                        formattedText.Trimming = TextTrimming.CharacterEllipsis;
                        formattedText.MaxTextWidth = maxWidth;
                        formattedText.MaxTextHeight = Math.Max(1, _listView.ActualHeight * 2 / 3);

                        if (range.ColorSet != null)
                        {
                            var rc = new Rect(offset + x, y, formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
                            drawingContext.DrawRectangle(range.ColorSet.BackBrush, range.ColorSet.BackPen, rc);
                        }

                        drawingContext.DrawText(formattedText, new Point(offset + x, y));
                        x += formattedText.WidthIncludingTrailingWhitespace;

                        maxWidth -= formattedText.WidthIncludingTrailingWhitespace;
                        rangesOffset = range.StartIndex + range.Length;
                    }
                }
            }
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
