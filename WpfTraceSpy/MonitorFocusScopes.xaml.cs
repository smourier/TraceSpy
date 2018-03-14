using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TraceSpy
{
    // for focus diagnostics only
    public partial class MonitorFocusScopes : Window
    {
        private DispatcherTimer _timer;
        private ObservableCollection<FocusScopeData> _focusScopes;

        public MonitorFocusScopes()
        {
            Loaded += MonitorFocusScopes_Loaded;
            Closed += (sender, e) => { _timer.IsEnabled = false; };
            MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            };
            InitializeComponent();
        }

        private void MonitorFocusScopes_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, (s, ev) => CheckKeyboardFocus(), Dispatcher) { IsEnabled = true };
            _focusScopes = new ObservableCollection<FocusScopeData>(
                    from fe in EnumerateVisualTree(Application.Current.MainWindow)
                    where FocusManager.GetIsFocusScope(fe)
                    select new FocusScopeData { FocusScope = fe, FocusedElement = FocusManager.GetFocusedElement(fe) });

            FocusManager.FocusedElementProperty.OverrideMetadata(typeof(FrameworkElement), new PropertyMetadata(OnLogicalFocusChanged));
            DataContext = _focusScopes;
        }

        private void OnLogicalFocusChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Log.Text += Environment.NewLine + "OLD: " + e.OldValue + Environment.NewLine + " NEW: " + e.NewValue;
            var fe = _focusScopes.FirstOrDefault(fsd => fsd.FocusScope == obj);
            if (fe != null)
            {
                fe.FocusedElement = e.NewValue;
            }

            CheckKeyboardFocus();
        }

        private void CheckKeyboardFocus() => _focusScopes.All(fs => { fs.IsKeyboardFocused = fs.FocusedElement == Keyboard.FocusedElement; return true; });

        private static IEnumerable<DependencyObject> EnumerateVisualTree(DependencyObject obj)
        {
            yield return obj;
            foreach (var e in EnumerateChildren(obj))
            {
                yield return e;
            }
        }

        private static IEnumerable<DependencyObject> EnumerateChildren(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null)
                {
                    yield return child;
                    foreach (var grandChild in EnumerateChildren(child))
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        private class FocusScopeData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private DependencyObject _focusScope;
            private object _focusElement;
            private bool _isKeyboardFocused;

            public DependencyObject FocusScope { get => _focusScope; set { _focusScope = value; OnPropertyChanged(nameof(FocusScope)); } }
            public object FocusedElement { get => _focusElement; set { _focusElement = value; OnPropertyChanged(nameof(FocusedElement)); } }
            public bool IsKeyboardFocused { get => _isKeyboardFocused; set { _isKeyboardFocused = value; OnPropertyChanged(nameof(IsKeyboardFocused)); } }

            private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
