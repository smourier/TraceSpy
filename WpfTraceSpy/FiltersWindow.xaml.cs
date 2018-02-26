using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace TraceSpy
{
    public partial class FiltersWindow : Window
    {
        private Context _context;

        public FiltersWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void LV_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ModifyFilter((e.OriginalSource as FrameworkElement)?.DataContext as Filter);
        private void Modify_Click(object sender, RoutedEventArgs e) => ModifyFilter(LV.SelectedValue as Filter);

        private void ModifyFilter(Filter filter)
        {
            if (filter == null)
                return;

            var dlg = new FilterWindow(filter.Clone());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            App.Current.Settings.AddFilter(dlg.Filter);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var filter = LV.SelectedValue as Filter;
            if (filter == null)
                return;

            if (this.ShowConfirm("Are you sure you want to remove the '" + filter + "' Filter?") != MessageBoxResult.Yes)
                return;

            _context.Providers.Remove(filter);
            App.Current.Settings.RemoveFilter(filter);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FilterWindow(new Filter());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            bool added = App.Current.Settings.AddFilter(dlg.Filter);
            App.Current.Settings.SerializeToConfiguration();
            if (added)
            {
                _context.Providers.Add(dlg.Filter);
            }
        }

        private class Context : DictionaryObject
        {
            private FiltersWindow _window;

            public Context(FiltersWindow window)
            {
                _window = window;
                _window.LV.SelectionChanged += (sender, e) =>
                {
                    OnPropertyChanged(nameof(ModifyEnabled));
                    OnPropertyChanged(nameof(RemoveEnabled));
                };
                Providers = new ObservableCollection<Filter>();
                Providers.AddRange(App.Current.Settings.Filters);
            }

            public ObservableCollection<Filter> Providers { get; }
            public bool ModifyEnabled => _window.LV.SelectedIndex >= 0;
            public bool RemoveEnabled => _window.LV.SelectedIndex >= 0;
        }
    }
}
