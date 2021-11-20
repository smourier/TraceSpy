using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TraceSpy
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
#if FX4
            Title += "FX4";
#endif
#if NET
            Title += "Core";
#endif
            WB.Source = new BitmapImage(new Uri("https://stackoverflow.com/users/flair/403671.png?theme=clean"));
        }

        private void Button_Click(object sender, RoutedEventArgs e) => "https://stackoverflow.com/users/403671/simon-mourier?tab=profile".OpenInDefaultBrowser();
    }
}
