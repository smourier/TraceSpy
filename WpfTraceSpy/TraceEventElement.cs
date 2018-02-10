using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TraceSpy
{
    public class TraceEventElement : FrameworkElement
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo(1033);

        public static readonly DependencyProperty EventProperty =
            DependencyProperty.Register(nameof(Event), typeof(TraceEvent), typeof(TraceEventElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public TraceEventElement()
        {
        }

        private static double FontSize => Math.Max(App.Current.Settings.FontSize, 5);

        public TraceEvent Event { get => (TraceEvent)GetValue(EventProperty); set => SetValue(EventProperty, value); }

        protected override Size MeasureOverride(Size availableSize)
        {
            //if (Event != null)
            //{
            //    return new Size(100, 20 + 5 * (Event.Id % 10));
            //}
            return new Size(App.Current.ColumnLayout.RowWidth, FontSize + 2);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var evt = Event;
            if (evt == null)
                return;

            if ((evt.Index % 2) == 1)
            {
                var altBrush = App.Current.Settings.AlternateBrush;
                if (altBrush != null)
                {
                    drawingContext.DrawRectangle(altBrush, null, new Rect(RenderSize));
                }
            }

            double offset = 0;

            string index = evt.Index.ToString();
            var formattedText = new FormattedText(
                index,
                Culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                Brushes.Black);

            formattedText.MaxTextWidth = App.Current.ColumnLayout.IndexColumnWidth;
            drawingContext.DrawText(formattedText, new Point(0, 0));
            offset += App.Current.ColumnLayout.IndexColumnWidth;

            string ticks = evt.Ticks.ToString();
            formattedText = new FormattedText(
                ticks,
                Culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                Brushes.Black);

            formattedText.MaxTextWidth = App.Current.ColumnLayout.TicksColumnWidth;
            drawingContext.DrawText(formattedText, new Point(offset, 0));
            offset += App.Current.ColumnLayout.TicksColumnWidth;

            formattedText = new FormattedText(
                evt.ProcessName,
                Culture,
                FlowDirection.LeftToRight,
                App.Current.Settings.TypeFace,
                FontSize,
                Brushes.Black);

            formattedText.MaxTextWidth = App.Current.ColumnLayout.ProcessColumnWidth;
            drawingContext.DrawText(formattedText, new Point(offset, 0));
            offset += App.Current.ColumnLayout.ProcessColumnWidth;

            if (evt.Text != null)
            {
                formattedText = new FormattedText(
                    evt.Text,
                    Culture,
                    FlowDirection.LeftToRight,
                    App.Current.Settings.TypeFace,
                    FontSize,
                    Brushes.Black);

                formattedText.MaxTextWidth = App.Current.ColumnLayout.TextColumnWidth;
                drawingContext.DrawText(formattedText, new Point(offset, 0));
            }
        }
    }
}
