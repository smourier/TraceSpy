using System;
using System.ComponentModel;
using System.Windows;

namespace TraceSpy
{
    public partial class TransparencyWindow : Window
    {
        private readonly Context _context;

        public TransparencyWindow()
        {
            InitializeComponent();
            _context = new Context(this);
            DataContext = _context;
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Current.Settings.SerializeToConfiguration();
            base.OnClosed(e);
        }

        private class Context : DictionaryObject
        {
            private readonly TransparencyWindow _window;

            public Context(TransparencyWindow window)
            {
                _window = window;
                Opacity = App.Current.Settings.Opacity;
                EnableTransparency = App.Current.Settings.EnableTransparency;
            }

            public bool EnableTransparency { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }
            public double Opacity { get => DictionaryObjectGetPropertyValue<double>(); set => DictionaryObjectSetPropertyValue(value); }

            protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                base.OnPropertyChanged(sender, e);
                if (e.PropertyName == nameof(Opacity))
                {
                    App.Current.MainWindow.Opacity = Opacity;
                    App.Current.Settings.Opacity = Opacity;
                    return;
                }

                if (e.PropertyName == nameof(EnableTransparency))
                {
                    // can't change if different than boot config
                    if (EnableTransparency != App.Current.MainWindow.AllowsTransparency)
                    {
                        _window.OpacityLabel.IsEnabled = false;
                        _window.OpacitySlider.IsEnabled = false;
                    }
                    else
                    {
                        _window.OpacityLabel.IsEnabled = EnableTransparency;
                        _window.OpacitySlider.IsEnabled = EnableTransparency;
                    }

                    App.Current.Settings.EnableTransparency = EnableTransparency;
                    return;
                }
            }
        }
    }
}
