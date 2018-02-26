using System;
using System.Windows;

namespace TraceSpy
{
    public partial class FilterWindow : Window
    {
        public FilterWindow(Filter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            InitializeComponent();
            Filter = filter;
            DataContext = filter;
        }

        public Filter Filter { get; }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
