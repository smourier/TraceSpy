using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace TraceSpy
{
    public partial class ColorizersWindow : Window
    {
        private readonly Context _context;

        public ColorizersWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void LVColorizer_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Modify((e.OriginalSource as FrameworkElement)?.DataContext as Colorizer);
        private void ModifyColorizer_Click(object sender, RoutedEventArgs e) => Modify(LVColorizers.SelectedValue as Colorizer);
        private void Modify(Colorizer provider)
        {
            if (provider == null)
                return;

            var dlg = new ColorizerWindow(provider.Clone());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            App.Current.Settings.AddColorizer(dlg.Colorizer);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void RemoveColorizer_Click(object sender, RoutedEventArgs e)
        {
            if (!(LVColorizers.SelectedValue is Colorizer colorizer))
                return;

            if (this.ShowConfirm("Are you sure you want to remove the '" + colorizer + "' colorizer?") != MessageBoxResult.Yes)
                return;

            _context.Colorizers.Remove(colorizer);
            App.Current.Settings.RemoveColorizer(colorizer);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void AddColorizer_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ColorizerWindow(new Colorizer());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            var added = App.Current.Settings.AddColorizer(dlg.Colorizer);
            App.Current.Settings.SerializeToConfiguration();
            if (added)
            {
                _context.Colorizers.Add(dlg.Colorizer);
            }
        }

        private void LVColorSets_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Modify((e.OriginalSource as FrameworkElement)?.DataContext as ColorSet);
        private void ModifyColorSet_Click(object sender, RoutedEventArgs e) => Modify(LVColorSets.SelectedValue as ColorSet);
        private void Modify(ColorSet set)
        {
            if (set == null)
                return;

            var dlg = new ColorSetWindow(set.Clone());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            App.Current.Settings.AddColorSet(dlg.ColorSet);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void RemoveColorSet_Click(object sender, RoutedEventArgs e)
        {
            if (!(LVColorizers.SelectedValue is ColorSet colorSet))
                return;

            if (this.ShowConfirm("Are you sure you want to remove the '" + colorSet + "' color set?") != MessageBoxResult.Yes)
                return;

            _context.ColorSets.Remove(colorSet);
            App.Current.Settings.RemoveColorSet(colorSet);
            App.Current.Settings.SerializeToConfiguration();
        }

        private void AddColorSet_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ColorSetWindow(new ColorSet());
            dlg.Owner = this;
            if (!dlg.ShowDialog().GetValueOrDefault())
                return;

            var added = App.Current.Settings.AddColorSet(dlg.ColorSet);
            App.Current.Settings.SerializeToConfiguration();
            if (added)
            {
                _context.ColorSets.Add(dlg.ColorSet);
            }
        }

        private class Context : DictionaryObject
        {
            private readonly ColorizersWindow _window;

            public Context(ColorizersWindow window)
            {
                _window = window;
                _window.LVColorizers.SelectionChanged += (sender, e) =>
                {
                    OnPropertyChanged(nameof(ModifyColorizerEnabled));
                    OnPropertyChanged(nameof(RemoveColorizerEnabled));
                };


                _window.LVColorSets.SelectionChanged += (sender, e) =>
                {
                    OnPropertyChanged(nameof(ModifyColorSetEnabled));
                    OnPropertyChanged(nameof(RemoveColorSetEnabled));
                };

                Colorizers = new ObservableCollection<Colorizer>();
                Colorizers.AddRange(App.Current.Settings.Colorizers);

                ColorSets = new ObservableCollection<ColorSet>();
                ColorSets.AddRange(App.Current.Settings.ColorSets);
            }

            public ObservableCollection<Colorizer> Colorizers { get; }
            public ObservableCollection<ColorSet> ColorSets { get; }

            public bool ModifyColorizerEnabled => _window.LVColorizers.SelectedIndex >= 0;
            public bool RemoveColorizerEnabled => _window.LVColorizers.SelectedIndex >= 0;

            public bool ModifyColorSetEnabled => _window.LVColorSets.SelectedIndex >= 0;
            public bool RemoveColorSetEnabled => _window.LVColorSets.SelectedIndex >= 0;
        }
    }
}
