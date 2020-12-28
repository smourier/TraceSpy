using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace TraceSpy
{
    public partial class FiltersWindow : Window
    {
        private readonly Context _context;

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

            App.Current.Settings.AddFilter(filter, dlg.Filter);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (!(LV.SelectedValue is Filter filter))
                return;

            if (this.ShowConfirm("Are you sure you want to remove the '" + filter + "' Filter?") != MessageBoxResult.Yes)
                return;

            _context.Filters.Remove(filter);
            App.Current.Settings.RemoveFilter(filter);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FilterWindow(new Filter());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            var added = App.Current.Settings.AddFilter(null, dlg.Filter);
            App.Current.Settings.SerializeToConfiguration();
            if (added)
            {
                _context.Filters.Add(dlg.Filter);
            }
        }

        private class Context : DictionaryObject
        {
            private readonly FiltersWindow _window;

            public Context(FiltersWindow window)
            {
                _window = window;
                _window.LV.SelectionChanged += (sender, e) =>
                {
                    OnPropertyChanged(nameof(ModifyEnabled));
                    OnPropertyChanged(nameof(RemoveEnabled));
                };
                Filters = new ObservableCollection<Filter>();
                Filters.AddRange(App.Current.Settings.Filters);
                DisableAll = App.Current.Settings.DisableAllFilters;
            }

            public ObservableCollection<Filter> Filters { get; }
            public bool ModifyEnabled => _window.LV.SelectedIndex >= 0;
            public bool RemoveEnabled => _window.LV.SelectedIndex >= 0;

            public bool DisableAll
            {
                get => DictionaryObjectGetPropertyValue<bool>();
                set
                {
                    if (DictionaryObjectSetPropertyValue(value))
                    {
                        App.Current.Settings.DisableAllFilters = DisableAll;
                    }
                }
            }
        }
    }
}
