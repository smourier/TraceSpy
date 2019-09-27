using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace TraceSpy
{
    public partial class EtwProvidersWindow : Window
    {
        private readonly Context _context;

        public EtwProvidersWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void LV_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ModifyProvider((e.OriginalSource as FrameworkElement)?.DataContext as EtwProvider);
        private void Modify_Click(object sender, RoutedEventArgs e) => ModifyProvider(LV.SelectedValue as EtwProvider);

        private void ModifyProvider(EtwProvider provider)
        {
            if (provider == null)
                return;

            var dlg = new EtwProviderWindow(provider.Clone());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            App.Current.Settings.AddEtwProvider(dlg.Provider);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var provider = LV.SelectedValue as EtwProvider;
            if (provider == null)
                return;

            if (this.ShowConfirm("Are you sure you want to remove the '" + provider + "' ETW provider?") != MessageBoxResult.Yes)
                return;

            _context.Providers.Remove(provider);
            App.Current.Settings.RemoveEtwProvider(provider.Guid);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EtwProviderWindow(new EtwProvider());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            bool added = App.Current.Settings.AddEtwProvider(dlg.Provider);
            App.Current.Settings.SerializeToConfiguration();
            if (added)
            {
                _context.Providers.Add(dlg.Provider);
            }
        }

        private class Context : DictionaryObject
        {
            private readonly EtwProvidersWindow _window;

            public Context(EtwProvidersWindow window)
            {
                _window = window;
                _window.LV.SelectionChanged += (sender, e) =>
                {
                    OnPropertyChanged(nameof(ModifyEnabled));
                    OnPropertyChanged(nameof(RemoveEnabled));
                };
                Providers = new ObservableCollection<EtwProvider>();
                Providers.AddRange(App.Current.Settings.EtwProviders);
            }

            public ObservableCollection<EtwProvider> Providers { get; }
            public bool ModifyEnabled => _window.LV.SelectedIndex >= 0;
            public bool RemoveEnabled => _window.LV.SelectedIndex >= 0;
        }
    }
}
