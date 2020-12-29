using System;
using System.Windows;

namespace TraceSpy
{
    public partial class SendTraceWindow : Window
    {
        public SendTraceWindow(SendTrace sendTrace)
        {
            if (sendTrace == null)
                throw new ArgumentNullException(nameof(sendTrace));

            InitializeComponent();
            DataContext = sendTrace;
        }

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
