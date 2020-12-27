using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace TraceSpy
{
    public partial class NumericUpDownControl : UserControl
    {
        public NumericUpDownControl()
        {
            InitializeComponent();
            TB.KeyUp += TB_KeyUp;
            TB.IsReadOnly = true;
            SB.Value = 0;
            SB.Minimum = -1;
            SB.Maximum = 1;
            SB.ValueChanged += (s, e) =>
            {
                if (e.NewValue == 0)
                    return;

                if (e.NewValue > e.OldValue)
                {
                    TB.Text = Decrement(TB.Text);
                }
                else
                {
                    TB.Text = Increment(TB.Text);
                }

                // always reset to 0
                SB.Value = 0;
            };
        }

        private void TB_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                TB.Text = Increment(TB.Text);
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                TB.Text = Decrement(TB.Text);
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // rework the scrollbar's look
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

        protected string Text { get => TB.Text; set => TB.Text = value ?? string.Empty; }

        protected virtual string Increment(string text) => text;
        protected virtual string Decrement(string text) => text;
    }

    public class DoubleUpDownControl : NumericUpDownControl
    {
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(DoubleUpDownControl),
            new PropertyMetadata(0d)
            {
                CoerceValueCallback = (o, v) => ((DoubleUpDownControl)o).EnsureMinMax((double)v),
                PropertyChangedCallback = (o, e) => ((DoubleUpDownControl)o).Text = e.NewValue.ToString(),
            });

        public readonly static DependencyProperty IncrementValueProperty = DependencyProperty.Register(
            nameof(IncrementValue),
            typeof(double),
            typeof(DoubleUpDownControl),
            new PropertyMetadata(0.1d));

        public readonly static DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(double),
            typeof(DoubleUpDownControl),
            new PropertyMetadata(double.MinValue)
            {
                PropertyChangedCallback = (o, e) =>
                {
                    var ud = (DoubleUpDownControl)o;
                    ud.Value = ud.EnsureMinMax(ud.Value);
                }
            });

        public readonly static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(double),
            typeof(DoubleUpDownControl),
            new PropertyMetadata(double.MaxValue)
            {
                PropertyChangedCallback = (o, e) =>
                {
                    var ud = (DoubleUpDownControl)o;
                    ud.Value = ud.EnsureMinMax(ud.Value);
                }
            });

        public double Value { get => (double)GetValue(ValueProperty); set { SetCurrentValue(ValueProperty, value); Text = value.ToString(); } }
        public double IncrementValue { get => (double)GetValue(IncrementValueProperty); set => SetCurrentValue(IncrementValueProperty, value); }
        public double MinValue { get => (double)GetValue(MinValueProperty); set => SetCurrentValue(MinValueProperty, value); }
        public double MaxValue { get => (double)GetValue(MaxValueProperty); set => SetCurrentValue(MaxValueProperty, value); }

        private double EnsureMinMax(double value)
        {
            if (value > MaxValue)
                return MaxValue;

            if (value < MinValue)
                return MinValue;

            return value;
        }

        private double Dec(string text)
        {
            if (!double.TryParse(text, out var value))
            {
                value = Value;
            }

            if (value == double.MinValue || value <= MinValue)
                return value;

            return value - IncrementValue;
        }

        private double Inc(string text)
        {
            if (!double.TryParse(text, out var value))
            {
                value = Value;
            }

            if (value == double.MaxValue || value >= MaxValue)
                return value;

            return value + IncrementValue;
        }

        protected override string Decrement(string text)
        {
            Value = Dec(text);
            return Value.ToString();
        }

        protected override string Increment(string text)
        {
            Value = Inc(text);
            return Value.ToString();
        }
    }

    public class Int32UpDownControl : NumericUpDownControl
    {
        public readonly static DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(0)
            {
                CoerceValueCallback = (o, v) => ((Int32UpDownControl)o).EnsureMinMax((int)v),
                PropertyChangedCallback = (o, e) => ((Int32UpDownControl)o).Text = e.NewValue.ToString(),
            });

        public readonly static DependencyProperty IncrementValueProperty = DependencyProperty.Register(
            nameof(IncrementValue),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(1));

        public readonly static DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(int.MinValue)
            {
                PropertyChangedCallback = (o, e) =>
                {
                    var ud = (Int32UpDownControl)o;
                    ud.Value = ud.EnsureMinMax(ud.Value);
                }
            });

        public readonly static DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(int),
            typeof(Int32UpDownControl),
            new PropertyMetadata(int.MaxValue)
            {
                PropertyChangedCallback = (o, e) =>
                {
                    var ud = (Int32UpDownControl)o;
                    ud.Value = ud.EnsureMinMax(ud.Value);
                }
            });

        public int Value { get => (int)GetValue(ValueProperty); set { SetCurrentValue(ValueProperty, value); Text = value.ToString(); } }
        public int IncrementValue { get => (int)GetValue(IncrementValueProperty); set => SetCurrentValue(IncrementValueProperty, value); }
        public int MinValue { get => (int)GetValue(MinValueProperty); set => SetCurrentValue(MinValueProperty, value); }
        public int MaxValue { get => (int)GetValue(MaxValueProperty); set => SetCurrentValue(MaxValueProperty, value); }

        private int EnsureMinMax(int value)
        {
            if (value > MaxValue)
                return MaxValue;

            if (value < MinValue)
                return MinValue;

            return value;
        }

        private int Dec(string text)
        {
            if (!int.TryParse(text, out var value))
            {
                value = Value;
            }

            if (value == int.MinValue || value <= MinValue)
                return value;

            return value - IncrementValue;
        }

        private int Inc(string text)
        {
            if (!int.TryParse(text, out var value))
            {
                value = Value;
            }

            if (value == int.MaxValue || value >= MaxValue)
                return value;

            return value + IncrementValue;
        }

        protected override string Decrement(string text)
        {
            Value = Dec(text);
            return Value.ToString();
        }

        protected override string Increment(string text)
        {
            Value = Inc(text);
            return Value.ToString();
        }
    }
}
