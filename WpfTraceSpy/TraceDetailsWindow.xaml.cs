using System;
using System.Windows;
using System.Windows.Media;

namespace TraceSpy
{
    public partial class TraceDetailsWindow : Window
    {
        public TraceDetailsWindow(TraceEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            InitializeComponent();
            TB.FontFamily = new FontFamily(App.Current.Settings.FontName);
            TB.FontSize = App.Current.Settings.FontSize;
            DataContext = evt;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
