using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TraceSpy
{
    public partial class FindWindow : Window
    {
        private Context _context;

        public event EventHandler FindingNext;
        public event EventHandler FindingPrev;

        public FindWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        public bool CaseMatch => _context.MatchCase;

        private void Close_Click(object sender, RoutedEventArgs e) => Hide();
        private void FindNext_Click(object sender, RoutedEventArgs e) => DoFind(true);
        private void FindPrev_Click(object sender, RoutedEventArgs e) => DoFind(false);

        private void DoFind(bool next)
        {
            App.Current.Settings.AddSearch(Searches.Text);
            App.Current.Settings.SerializeToConfiguration();
            _context.Searches.AddRange(App.Current.Settings.Searches);
            if (next)
            {
                FindingNext?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                FindingPrev?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            string search = Searches.Text;
            _context.Searches.Clear();
            _context.Searches.AddRange(App.Current.Settings.Searches);
            Searches.Text = search;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Searches_TextChanged(object sender, TextChangedEventArgs e)
        {
            _context.UpdateValid();
        }

        private class Context : DictionaryObject
        {
            private FindWindow _window;

            public Context(FindWindow window)
            {
                _window = window;
                Searches = new ObservableCollection<string>();
                Searches.AddRange(App.Current.Settings.Searches);
                _window.Searches.ItemsSource = Searches;
            }

            public void UpdateValid()
            {
                OnPropertyChanged(nameof(IsValid));
            }

            public ObservableCollection<string> Searches { get; }
            public bool FindEnabled => !string.IsNullOrWhiteSpace(_window.Searches.Text);
            public bool MatchCase { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

            protected override IEnumerable DictionaryObjectGetErrors(string propertyName)
            {
                if (propertyName == null)
                {
                    if (string.IsNullOrWhiteSpace(_window.Searches.Text))
                        yield return "Text cannot be empty nor whitespaces only.";
                }
            }
        }
    }
}
