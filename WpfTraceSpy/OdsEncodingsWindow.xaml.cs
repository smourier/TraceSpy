using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TraceSpy
{
    public partial class OdsEncodingsWindow : Window
    {
        private readonly Context _context;
        private readonly ColumnSortHandler _sortHandler = new ColumnSortHandler();

        public OdsEncodingsWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            ChooseEncoding(LV.SelectedItem as OdsEncoding);
            Close();
        }

        private void LV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChooseEncoding((e.OriginalSource as FrameworkElement)?.DataContext as OdsEncoding);
            Close();
        }

        private void LV_ColumnHeaderClick(object sender, RoutedEventArgs e) => _sortHandler.HandleClick(e.OriginalSource as GridViewColumnHeader, LV.ItemsSource);

        private static void ChooseEncoding(OdsEncoding encoding)
        {
            if (encoding == null)
                return;

            App.Current.Settings.OdsEncodingName = encoding.Encoding.Name;
            App.Current.Settings.SerializeToConfiguration();
        }

        private class Context : DictionaryObject
        {
            private readonly OdsEncodingsWindow _window;

            public Context(OdsEncodingsWindow window)
            {
                _window = window;
                _window.LV.SelectionChanged += (sender, e) =>
                {
                    if (_window.LV.SelectedItem is OdsEncoding select)
                    {
                        _window.Title = "ODS Encoding - " + select.Encoding.DisplayName;
                    }
                    else
                    {
                        _window.Title = "ODS Encodings";
                    }
                };

                Encodings =
                [
                    .. Encoding.GetEncodings()
                        .OrderBy(e => e.DisplayName)
                        .Select(e => new OdsEncoding { Encoding = e, IsSelected = e.CodePage == App.Current.Settings.OdsEncoding.CodePage }),
                ];

                var selected = Encodings.FirstOrDefault(e => e.IsSelected);
                if (selected != null)
                {
                    _window.LV.ScrollIntoView(selected);
                }
            }

            public ObservableCollection<OdsEncoding> Encodings { get; }
        }

        private class OdsEncoding : DictionaryObject
        {
            public EncodingInfo Encoding { get => DictionaryObjectGetPropertyValue<EncodingInfo>(); set => DictionaryObjectSetPropertyValue(value); }
            public bool IsSelected { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

            public override string ToString() => Encoding.DisplayName;
        }
    }
}
