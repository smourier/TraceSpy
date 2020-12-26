using System;
using System.Windows;

namespace TraceSpy
{
    public partial class ColorSetWindow : Window
    {
        public ColorSetWindow(ColorSet colorSetr)
        {
            if (colorSetr == null)
                throw new ArgumentNullException(nameof(colorSetr));

            InitializeComponent();
            ColorSet = colorSetr;
            DataContext = colorSetr;
        }

        public ColorSet ColorSet { get; }

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
