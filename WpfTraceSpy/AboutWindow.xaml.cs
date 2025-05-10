using System;
using System.Reflection;
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
            WB.Source = new BitmapImage(new Uri("https://stackexchange.com/users/flair/174359.png"));
            var asm = Assembly.GetExecutingAssembly();
            var product = asm.GetCustomAttribute<AssemblyProductAttribute>().Product;
            var copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            var informationalVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            version.Text = product + " V" + informationalVersion + Environment.NewLine + copyright;
        }

        private void Button_Click(object sender, RoutedEventArgs e) => "https://stackoverflow.com/users/403671/simon-mourier?tab=profile".OpenInDefaultBrowser();
    }
}
