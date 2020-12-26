using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TraceSpy
{
    public partial class NumericUpDownControl : UserControl
    {
        public readonly static DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(NumericUpDownControl),
            new PropertyMetadata()
            {
                CoerceValueCallback = (o, v) => ((NumericUpDownControl)o).Coerce((string)v),
                PropertyChangedCallback = (o, e) => ((NumericUpDownControl)o).Text = (string)e.NewValue,
            });

        public NumericUpDownControl()
        {
            InitializeComponent();
            TB.KeyUp += TB_KeyUp;
            TB.IsReadOnly = true;
            SB.Value = 0;
            SB.Minimum = -1;
            SB.Maximum = 1;
            TB.TextChanged += (s, e) => Text = TB.Text;
            SB.ValueChanged += (s, e) =>
            {
                if (e.NewValue == 0)
                    return;

                if (e.NewValue > e.OldValue)
                {
                    TB.Text = Decrement(Text);
                }
                else
                {
                    TB.Text = Increment(Text);
                }

                // always reset to 0
                SB.Value = 0;
            };
        }

        private void TB_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                TB.Text = Increment(Text);
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                TB.Text = Decrement(Text);
            }
        }

        public string Text { get => (string)GetValue(TextProperty); set => SetCurrentValue(TextProperty, value); }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // rework the scrollbar
            var up = SB.FindVisualChild<RepeatButton>(b => b.Name == "PART_LineUpButton");
            if (up != null)
            {
                up.FontFamily = new FontFamily("Segoe MDL2 Assets");
                up.FontSize = 8;
                up.LayoutTransform = new RotateTransform(-90);
                up.Content = "\uE26B";
            }

            var down = SB.FindVisualChild<RepeatButton>(b => b.Name == "PART_LineDownButton");
            if (down != null)
            {
                down.FontFamily = new FontFamily("Segoe MDL2 Assets");
                down.FontSize = 8;
                down.LayoutTransform = new RotateTransform(90);
                down.Content = "\uE26B";
            }
            return base.ArrangeOverride(arrangeBounds);
        }

        protected virtual string Increment(string text) => text;
        protected virtual string Decrement(string text) => text;
        protected virtual bool TryParse(string text, out object value) { value = text; return true; }
        protected virtual string GetDefaultValue() => string.Empty;
        protected virtual string Coerce(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return GetDefaultValue();

            if (!TryParse(text, out var value))
                return GetDefaultValue();

            if (value == null)
                return GetDefaultValue();

            return value.ToString();
        }
    }

    public class Int32UpDownControl : NumericUpDownControl
    {
        public readonly static DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(int.MinValue));

        public readonly static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(int.MaxValue));

        public int MinValue { get => (int)GetValue(MinValueProperty); set => SetCurrentValue(MinValueProperty, value); }
        public int MaxValue { get => (int)GetValue(MaxValueProperty); set => SetCurrentValue(MaxValueProperty, value); }

        protected override string GetDefaultValue() => 0.ToString();

        protected override bool TryParse(string text, out object value)
        {
            if (!int.TryParse(text, out var i))
            {
                value = 0;
                return false;
            }

            value = i;
            return true;
        }

        protected override string Decrement(string text)
        {
            if (!TryParse(text, out var obj))
                return text;

            var value = (int)obj;
            if (value == int.MinValue || value <= MinValue)
                return value.ToString();

            return (value - 1).ToString();
        }

        protected override string Increment(string text)
        {
            if (!TryParse(text, out var obj))
                return text;

            var value = (int)obj;
            if (value == int.MaxValue || value >= MaxValue)
                return value.ToString();

            return (value + 1).ToString();
        }
    }
}
