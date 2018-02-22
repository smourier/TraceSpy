using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TraceSpy
{
    public partial class EtwProvidersWindow : Window
    {
        private Context _context;

        public EtwProvidersWindow()
        {
            InitializeComponent();
            _context = new Context();
            DataContext = _context;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Modify_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EtwProviderWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private class Context : DictionaryObject
        {
            public Context()
            {
                Providers = new ObservableCollection<EtwProvider>();
                Providers.AddRange(App.Current.Settings.EtwProviders);
            }

            public ObservableCollection<EtwProvider> Providers { get; }
        }
    }
}
