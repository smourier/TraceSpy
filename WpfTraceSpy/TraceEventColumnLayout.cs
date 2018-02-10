using System;

namespace TraceSpy
{
    public class TraceEventColumnLayout : DictionaryObject
    {
        public TraceEventColumnLayout()
        {
            IndexColumnWidth = 75;
            TicksColumnWidth = 96;
            ProcessColumnWidth = 102;
            TextColumnWidth = 681;
        }

        public double IndexColumnWidth
        {
            get => DictionaryObjectGetPropertyValue<double>();
            set
            {
                if (double.IsNaN(value))
                    return;

                value = Math.Max(10, value);
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(RowWidth));
                }
            }
        }

        public double TicksColumnWidth
        {
            get => DictionaryObjectGetPropertyValue<double>();
            set
            {
                if (double.IsNaN(value))
                    return;

                value = Math.Max(10, value);
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(RowWidth));
                }
            }
        }

        public double ProcessColumnWidth
        {
            get => DictionaryObjectGetPropertyValue<double>();
            set
            {
                if (double.IsNaN(value))
                    return;

                value = Math.Max(10, value);
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(RowWidth));
                }
            }
        }

        public double TextColumnWidth
        {
            get => DictionaryObjectGetPropertyValue<double>();
            set
            {
                if (double.IsNaN(value))
                    return;

                value = Math.Max(10, value);
                if (DictionaryObjectSetPropertyValue(value))
                {
                    OnPropertyChanged(nameof(RowWidth));
                }
            }
        }

        public double RowWidth => IndexColumnWidth + TicksColumnWidth + ProcessColumnWidth + TextColumnWidth;
    }
}
