using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TraceSpy
{
    public class TraceEventElement : FrameworkElement
    {
        private static readonly CultureInfo _culture = CultureInfo.GetCultureInfo(1033);

        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register(nameof(Event), typeof(TraceEvent), typeof(TraceEventElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        private ListView _listView;

        public TraceEventElement()
        {
        }

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
            if (Event != null && Event.Text != null && App.Current.Settings.WrapText)
            {
                var formattedText = new FormattedText(
                    Event.Text,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    Brushes.Black);

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

            if (evt.Background != null)
            {
                drawingContext.DrawRectangle(evt.Background, null, new Rect(RenderSize));
            }
            else
            {
                if ((evt.Index % 2) == 1)
                {
                    var altBrush = App.Current.Settings.AlternateBrush;
                    if (altBrush != null)
                    {
                        drawingContext.DrawRectangle(altBrush, null, new Rect(RenderSize));
                    }
                }
            }

            double offset = 0;

            var index = evt.Index.ToString();
            var formattedText = new FormattedText(
                index,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                Brushes.Black);

            formattedText.MaxTextWidth = App.Current.ColumnLayout.IndexColumnWidth;
            drawingContext.DrawText(formattedText, new Point(0, 0));
            offset += App.Current.ColumnLayout.IndexColumnWidth;

            formattedText = new FormattedText(
                evt.TicksText,
                _culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                Brushes.Black);

            formattedText.MaxLineCount = 1;
            formattedText.Trimming = TextTrimming.CharacterEllipsis;
            formattedText.MaxTextWidth = App.Current.ColumnLayout.TicksColumnWidth;
            drawingContext.DrawText(formattedText, new Point(offset, 0));
            offset += App.Current.ColumnLayout.TicksColumnWidth;

            if (evt.ProcessName != null)
            {
                formattedText = new FormattedText(
                    evt.ProcessName,
                    _culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    Brushes.Black);

                formattedText.MaxLineCount = 1;
                formattedText.Trimming = TextTrimming.CharacterEllipsis;
                formattedText.MaxTextWidth = App.Current.ColumnLayout.ProcessColumnWidth;
                drawingContext.DrawText(formattedText, new Point(offset, 0));
            }

            offset += App.Current.ColumnLayout.ProcessColumnWidth;

            if (evt.Text != null)
            {
                var ranges = App.Current.Settings.ComputeColorRanges(evt.Text);
                if (ranges.Count == 0)
                {
                    formattedText = new FormattedText(
                        evt.Text,
                        _culture,
                        FlowDirection.LeftToRight,
                        App.Current.Settings.TypeFace,
                        FontSize,
                        Brushes.Black);

                    if (!App.Current.Settings.WrapText)
                    {
                        formattedText.MaxLineCount = 1;
                    }

                    formattedText.Trimming = TextTrimming.CharacterEllipsis;
                    formattedText.MaxTextWidth = App.Current.ColumnLayout.TextColumnWidth;
                    formattedText.MaxTextHeight = Math.Max(1, _listView.ActualHeight * 2 / 3);
                    drawingContext.DrawText(formattedText, new Point(offset, 0));
                }
                else
                {
                    var charIndex = 0;
                    var x = 0d;
                    var y = 0d;
                    var maxWidth = App.Current.ColumnLayout.TextColumnWidth;
                    for (var i = 0; i < ranges.Count; i++)
                    {
                        if (maxWidth < 0)
                            return;

                        var range = ranges[i];
                        var chunk = evt.Text.Substring(charIndex, range.Length);

                        formattedText = new FormattedText(
                            chunk,
                            _culture,
                            FlowDirection.LeftToRight,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item1 : App.Current.Settings.TypeFace,
                            range.ColorSet != null ? range.ColorSet.Typeface.Item2 : FontSize,
                            range.ColorSet != null ? range.ColorSet.ForeBrush : Brushes.Black);

                        if (!App.Current.Settings.WrapText)
                        {
                            formattedText.MaxLineCount = 1;
                        }

                        formattedText.Trimming = TextTrimming.CharacterEllipsis;
                        formattedText.MaxTextWidth = maxWidth;
                        formattedText.MaxTextHeight = Math.Max(1, _listView.ActualHeight * 2 / 3);

                        if (range.ColorSet != null)
                        {
                            var rc = new Rect(offset + x, y, formattedText.Width, formattedText.Height);
                            drawingContext.DrawRectangle(range.ColorSet.BackBrush, range.ColorSet.BackPen, rc);
                        }

                        drawingContext.DrawText(formattedText, new Point(offset + x, y));
                        x += formattedText.Width;
                        charIndex += range.Length;

                        maxWidth -= formattedText.Width;
                    }
                }
            }
        }
    }
}
