using System.Windows;

namespace TraceSpy
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => "http://stackoverflow.com/users/403671/simon-mourier?tab=profile".OpenInDefaultBrowser();
    }
}
