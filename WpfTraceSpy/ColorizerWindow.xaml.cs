using System;
using System.Diagnostics;
using System.Windows;

namespace TraceSpy
{
    public partial class ColorizerWindow : Window
    {
        public ColorizerWindow(Colorizer colorizer)
        {
            if (colorizer == null)
                throw new ArgumentNullException(nameof(colorizer));

            InitializeComponent();
            Colorizer = colorizer;
            DataContext = colorizer;
        }

        public Colorizer Colorizer { get; }

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

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference") { UseShellExecute = true });
            Definition.Focus();
        }
    }
}
