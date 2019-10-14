using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TraceSpy
{
    public partial class TraceDetailsWindow : Window
    {
        private int _index;
        private ListView _listView;

        public TraceDetailsWindow(ListView listView)
        {
            if (listView == null)
                throw new ArgumentNullException(nameof(listView));

            _listView = listView;
            _index = listView.SelectedIndex;
            InitializeComponent();
            Load();
            TB.FontFamily = new FontFamily(App.Current.Settings.FontName);
            TB.FontSize = App.Current.Settings.FontSize;
        }

        private void Load()
        {
            var evt = (TraceEvent)_listView.SelectedItem;
            DataContext = evt;
            PreviousButton.IsEnabled = _index > 0;
            NextButton.IsEnabled = _index < (_listView.Items.Count - 1);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _index = Math.Max(0, _index - 1);
            if (_listView.Items.Count == 0)
            {
                PreviousButton.IsEnabled = false;
                return;
            }

            _listView.SelectedIndex = _index;
            Load();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _index++;
            if (_index >= _listView.Items.Count)
            {
                NextButton.IsEnabled = false;
                return;
            }

            _listView.SelectedIndex = _index;
            Load();
        }
    }
}
