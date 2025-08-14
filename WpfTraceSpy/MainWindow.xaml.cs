using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TraceSpy
{
    public partial class MainWindow : Window
    {
        public const string ClearTracesPrefix = "##TraceSpyClear##";

        private const int _odsBufferSize = 4096;
        private readonly EventWaitHandle _bufferReadyEvent;
        private readonly EventWaitHandle _dataReadyEvent;
        private readonly MemoryMappedFile _buffer;
        private readonly MemoryMappedViewStream _bufferStream;
        private readonly Thread _outputDebugStringThread;
        private readonly ObservableCollection<TraceEvent> _dataSource = [];
        private readonly MainWindowState _state;
        private readonly ConcurrentDictionary<int, string> _processes = new ConcurrentDictionary<int, string>();
        private readonly Dictionary<Guid, EventRealtimeListener> _etwListeners = [];
        private readonly ConcurrentLinkedList<TraceEvent> _events = new ConcurrentLinkedList<TraceEvent>();
        private bool _stopOutputDebugStringTraces;
        private FindWindow _findWindow;
        private int _scrollingTo;
        private TraceEvent _scrollTo;

        public MainWindow()
        {
#if !FX4
            _pixelsPerDip = new Lazy<double>(() => VisualTreeHelper.GetDpi(this).PixelsPerDip);
#endif
            _state = new MainWindowState();
            _state.AutoScroll = App.Current.Settings.AutoScroll;
            _state.RemoveEmptyLines = App.Current.Settings.RemoveEmptyLines;
            _state.ResolveProcessName = App.Current.Settings.ResolveProcessName;
            _state.ShowEtwDescription = App.Current.Settings.ShowEtwDescription;
            _state.ShowProcessId = App.Current.Settings.ShowProcessId;
            _state.WrapText = App.Current.Settings.WrapText;
            _state.DontSplitText = App.Current.Settings.DontSplitText;
            _state.IsTopmost = App.Current.Settings.IsTopmost;
            _state.ShowTicksMode = App.Current.Settings.ShowTicksMode;
            _state.ThemeName = App.Current.Settings.ThemeName.Nullify();
            _state.PropertyChanged += OnStatePropertyChanged;

            if (App.Current.Settings.EnableTransparency)
            {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }

            InitializeComponent();

            ApplyTheme();

            if (App.Current.Settings.EnableTransparency)
            {
                CaptionBorder.Fill = SystemColors.ActiveBorderBrush;
                CaptionIconBorder.BorderBrush = CaptionBorder.Fill;
                CaptionIconBackground.Background = MainMenu.Background;
                MainMenu.BorderBrush = CaptionBorder.Fill;
                MainGrid.Background = MainMenu.Background;
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath))
                {
                    var img = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    CaptionIcon.Source = img;
                }
                Opacity = Math.Max(0.1, App.Current.Settings.Opacity);
            }
            else
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(0);
                CaptionIconBorder.Visibility = Visibility.Collapsed;
                CaptionIconBackground.Visibility = Visibility.Collapsed;
                CaptionIcon.Visibility = Visibility.Collapsed;
                MainMenu.Margin = new Thickness();
                MainMenu.BorderThickness = new Thickness(0);
                ButtonsPanel.Visibility = Visibility.Collapsed;
            }

            LoadSettings();
            DataContext = _state;
            _state.EtwStarted = App.Current.Settings.CaptureEtwTraces;
            _state.OdsStarted = App.Current.Settings.CaptureOdsTraces;
            UpdateEtwEvents();

            try
            {
                _bufferReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_BUFFER_READY");
                _dataReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_DATA_READY");
                _buffer = MemoryMappedFile.CreateNew("DBWIN_BUFFER", _odsBufferSize);
                _bufferStream = _buffer.CreateViewStream(0, _odsBufferSize);
            }
            catch
            {
                OdsTrace.IsEnabled = false;
                OdsTrace.Header = "ODS unavailable";
                OdsTrace.ToolTip = "There's already an ODS trace listener running in the machine.";
                _state.OdsStarted = null;
            }

            SetFont();

            IndexColumn.Width = App.Current.ColumnLayout.IndexColumnWidth;
            TicksColumn.Width = App.Current.ColumnLayout.TicksColumnWidth;
            ProcessColumn.Width = App.Current.ColumnLayout.ProcessColumnWidth;
            TextColumn.Width = App.Current.ColumnLayout.TextColumnWidth;
            App.Current.ColumnLayout.PropertyChanged += OnColumnLayoutPropertyChanged;

            LV.ItemsSource = _dataSource;
            foreach (var col in GV.Columns)
            {
                ((INotifyPropertyChanged)col).PropertyChanged += OnGridViewColumnPropertyChanged;
            }

            if (_buffer != null)
            {
                _outputDebugStringThread = new Thread(ReadOutputDebugStringTraces);
                _outputDebugStringThread.IsBackground = true;
                _outputDebugStringThread.Start();
            }


#if DEBUG
            //            new MonitorFocusScopes().Show();
#endif
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            MainMenu.RaiseMenuItemClickOnKeyGesture(e);
        }

        public Theme CurrentTheme { get; private set; }

#if !FX4
        private Lazy<double> _pixelsPerDip;

        public double PixelsPerDip => _pixelsPerDip.Value;

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _pixelsPerDip = new Lazy<double>(() => VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
#endif

        private void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            App.Current.Settings.AutoScroll = _state.AutoScroll;
            App.Current.Settings.RemoveEmptyLines = _state.RemoveEmptyLines;
            App.Current.Settings.ResolveProcessName = _state.ResolveProcessName;
            App.Current.Settings.ShowEtwDescription = _state.ShowEtwDescription;
            App.Current.Settings.ShowProcessId = _state.ShowProcessId;
            App.Current.Settings.WrapText = _state.WrapText;
            App.Current.Settings.DontSplitText = _state.DontSplitText;
            App.Current.Settings.IsTopmost = _state.IsTopmost;
            App.Current.Settings.ShowTicksMode = _state.ShowTicksMode;
            App.Current.Settings.ThemeName = _state.ThemeName.Nullify();
            App.Current.Settings.SerializeToConfiguration();

            if (e.PropertyName == nameof(MainWindowState.WrapText) || e.PropertyName == nameof(MainWindowState.ShowTicksMode))
            {
                OnColumnLayoutPropertyChanged(null, null);
            }

            if (e.PropertyName == nameof(MainWindowState.ThemeName))
            {
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            CurrentTheme = Theme.Load(_state.ThemeName);
            LV.Background = CurrentTheme.ListViewBackColorBrush;
            LVH.Background = CurrentTheme.ListViewBackColorBrush;
            ApplyTheme(MainMenu);
            UpdateTraceVisuals();
            CurrentTheme.Save();
        }

        private void ApplyTheme(Menu menu)
        {
            menu.Background = CurrentTheme.MenuBackColorBrush;
            menu.Foreground = CurrentTheme.MenuTextColorBrush;
            foreach (var child in menu.Items.OfType<MenuItem>())
            {
                ApplyTheme(child);
            }
        }

        private void ApplyTheme(MenuItem menuItem)
        {
            if (menuItem.Name == "OdsTrace" || menuItem.Name == "EtwTrace")
                return;

            menuItem.Background = CurrentTheme.MenuBackColorBrush;
            menuItem.Foreground = CurrentTheme.MenuTextColorBrush;
            foreach (var child in menuItem.Items.OfType<MenuItem>())
            {
                ApplyTheme(child);
            }
        }

        private void LoadSettings()
        {
            Left = App.Current.Settings.Left;
            Top = App.Current.Settings.Top;
            Width = App.Current.Settings.Width;
            Height = App.Current.Settings.Height;

            App.Current.ColumnLayout.IndexColumnWidth = App.Current.Settings.IndexColumnWidth;
            IndexColumn.Width = App.Current.Settings.IndexColumnWidth;

            App.Current.ColumnLayout.TicksColumnWidth = App.Current.Settings.TicksColumnWidth;
            TicksColumn.Width = App.Current.Settings.TicksColumnWidth;

            App.Current.ColumnLayout.ProcessColumnWidth = App.Current.Settings.ProcessColumnWidth;
            ProcessColumn.Width = App.Current.Settings.ProcessColumnWidth;

            App.Current.ColumnLayout.TextColumnWidth = App.Current.Settings.TextColumnWidth;
            TextColumn.Width = App.Current.Settings.TextColumnWidth;
        }

        private void SaveSettings()
        {
            if (WindowState != WindowState.Minimized)
            {
                App.Current.Settings.Left = Left;
                App.Current.Settings.Top = Top;
                App.Current.Settings.Width = Width;
                App.Current.Settings.Height = Height;
            }

            if (_findWindow != null)
            {
                App.Current.Settings.FindLeft = _findWindow.Left - Left;
                App.Current.Settings.FindTop = _findWindow.Top - Top;
            }

            App.Current.Settings.IndexColumnWidth = App.Current.ColumnLayout.IndexColumnWidth;
            App.Current.Settings.TicksColumnWidth = App.Current.ColumnLayout.TicksColumnWidth;
            App.Current.Settings.ProcessColumnWidth = App.Current.ColumnLayout.ProcessColumnWidth;
            App.Current.Settings.TextColumnWidth = App.Current.ColumnLayout.TextColumnWidth;

            App.Current.Settings.SerializeToConfiguration();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => SaveSettings();
        protected override void OnLocationChanged(EventArgs e) => SaveSettings();

        protected override void OnClosed(EventArgs e)
        {
            DisposeEtwEvents();
            _findWindow?.Close();
            _stopOutputDebugStringTraces = true;
            _outputDebugStringThread?.Join(1000);
            _bufferStream?.Dispose();
            _buffer?.Dispose();
            _dataReadyEvent?.Dispose();
            _bufferReadyEvent?.Dispose();
            base.OnClosed(e);
        }

        private void OnColumnLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var panel = LV.FindVisualChild<VirtualizingStackPanel>();
            foreach (var item in panel.EnumerateVisualChildren(true).OfType<TraceEventElement>())
            {
                item.InvalidateMeasure();
                item.InvalidateVisual();
            }
        }

        private void UpdateTraceVisuals()
        {
            var panel = LV.FindVisualChild<VirtualizingStackPanel>();
            foreach (var item in panel.EnumerateVisualChildren(true).OfType<TraceEventElement>())
            {
                item.InvalidateVisual();
            }
        }

        private void OnGridViewColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var col = (GridViewColumn)sender;
            if (col == IndexColumn)
            {
                App.Current.ColumnLayout.IndexColumnWidth = col.Width;
                col.Width = App.Current.ColumnLayout.IndexColumnWidth;
            }
            else if (col == TicksColumn)
            {
                App.Current.ColumnLayout.TicksColumnWidth = col.Width;
                col.Width = App.Current.ColumnLayout.TicksColumnWidth;
            }
            else if (col == ProcessColumn)
            {
                App.Current.ColumnLayout.ProcessColumnWidth = col.Width;
                col.Width = App.Current.ColumnLayout.ProcessColumnWidth;
            }
            else if (col == TextColumn)
            {
                App.Current.ColumnLayout.TextColumnWidth = col.Width;
                col.Width = App.Current.ColumnLayout.TextColumnWidth;
            }

            SaveSettings();
            OnColumnLayoutPropertyChanged(null, null);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private void Gif_MediaEnded(object sender, RoutedEventArgs e)
        {
            var me = (MediaElement)sender;
            me.Position = new TimeSpan(0, 0, 1);
            me.Play();
        }

        private void DequeueTraces()
        {
            var events = _events.GetAndClear();
            foreach (var evt in events)
            {
                AddTrace(evt);
            }
        }

        public void AddTrace(TraceEvent evt, bool allowExclusion = true)
        {
            if (evt == null || evt.ProcessName == null)
                return;

            if (!App.Current.Settings.DontAllowClearingText && evt.Text?.StartsWith(ClearTracesPrefix) == true)
            {
                _dataSource.Clear();
                TraceEvent.ResetIndex();
                var text = evt.Text.Substring(ClearTracesPrefix.Length);
                if (string.IsNullOrEmpty(text))
                    return;

                // send the rest of clear line
                evt.Text = text;
            }

            if (allowExclusion && App.Current.Settings.ExcludeLine(evt.Text, evt.ProcessName))
                return;

            var last = _dataSource.LastOrDefault();
            evt.PreviousTicks = last != null ? last.Ticks : evt.Ticks;
            _dataSource.Add(evt);

            if (_state.AutoScroll)
            {
                // only ask to scroll if nothing is scrolling
                _scrollTo = evt;
                if (Interlocked.CompareExchange(ref _scrollingTo, 1, 0) == 0)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        var st = _scrollTo;
                        if (st != null)
                        {
                            LV.ScrollIntoView(st);
                        }
                        Interlocked.Exchange(ref _scrollingTo, 0);
                    }, DispatcherPriority.SystemIdle);
                }
            }
        }

        private void ReadOutputDebugStringTraces()
        {
            var pidBytes = new byte[4];
            var strBytes = new byte[_odsBufferSize - 4];
            do
            {
                _bufferReadyEvent.Set();
                var result = _dataReadyEvent.WaitOne(500);
                if (_stopOutputDebugStringTraces)
                    return;

                if (!_state.OdsStarted.GetValueOrDefault())
                    continue;

                if (result) // we don't care about other return values
                {
                    _bufferStream.Position = 0;
#if NET
                    _bufferStream.ReadExactly(pidBytes);
#else
                    _bufferStream.Read(pidBytes, 0, pidBytes.Length);
#endif
                    var pid = BitConverter.ToInt32(pidBytes, 0);
#if NET
                    _bufferStream.ReadExactly(strBytes);
#else
                    _bufferStream.Read(strBytes, 0, strBytes.Length);
#endif
                    var text = GetNullTerminatedString(strBytes);
                    if (_state.RemoveEmptyLines && string.IsNullOrWhiteSpace(text))
                        continue;

                    var evt = new TraceEvent();
                    evt.ProcessName = GetProcessName(pid);
                    evt.Text = text;
                    _events.Add(evt);
                    Dispatcher.BeginInvoke(() =>
                    {
                        DequeueTraces();
                    });
                }
            }
            while (true);
        }

        private static string GetNullTerminatedString(byte[] bytes)
        {
            var encoding = App.Current.Settings.OdsEncoding ?? Encoding.Default;
            var terminator = App.Current.Settings.OdsEncodingTerminator;
            int index;
            if (terminator.Length == 1) // ascii, default or UTF8 falls here
            {
                index = Array.IndexOf(bytes, terminator[0]);
                if (index == 0)
                    return string.Empty;

                if (index < 0)
                {
                    bytes[bytes.Length - 1] = 0;
                    return encoding.GetString(bytes);
                }

                return encoding.GetString(bytes, 0, index);
            }

            // there are few chances that once here (multiple zeros for terminator, UTF16, UTF32, etc.) it will work
            // as OutputDebugStringW uses OutputDebugStringA https://learn.microsoft.com/en-us/windows/win32/api/debugapi/nf-debugapi-outputdebugstringw
            // and OutputDebugStringA truncate strings at first 0 byte...
            index = -1;
            for (var i = 0; i < bytes.Length; i += terminator.Length)
            {
                if (bytes.Skip(i).Take(terminator.Length).SequenceEqual(terminator))
                {
                    if (i == 0)
                        return string.Empty;

                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                // hacky, truncate twice so we're sure to put enough zeroes in there, even non-aligned
                Array.Copy(terminator, 0, bytes, bytes.Length - terminator.Length, terminator.Length);
                Array.Copy(terminator, 0, bytes, bytes.Length - terminator.Length * 2, terminator.Length);
                return encoding.GetString(bytes);
            }

            return encoding.GetString(bytes, 0, index);
        }

        private void OdsTrace_Click(object sender, RoutedEventArgs e)
        {
            _state.OdsStarted = !_state.OdsStarted;
            App.Current.Settings.CaptureOdsTraces = _state.OdsStarted.GetValueOrDefault();
            SaveSettings();
        }

        private void EtwTrace_Click(object sender, RoutedEventArgs e)
        {
            _state.EtwStarted = !_state.EtwStarted;
            App.Current.Settings.CaptureEtwTraces = _state.EtwStarted;
            SaveSettings();
            UpdateEtwEvents();
        }

        private string GetProcessName(int id)
        {
            if (!_state.ResolveProcessName)
                return id.ToString();

            if (!_processes.TryGetValue(id, out string name))
            {
                try
                {
                    var process = Process.GetProcessById(id);
                    name = process.ProcessName;
                }
                catch
                {
                }

                name ??= id.ToString();
                _processes[id] = name;
            }

            if (_state.ShowProcessId)
            {
                name += " (" + id + ")";
            }
            return name;
        }

        private void ClearTraces_Click(object sender, RoutedEventArgs e)
        {
            _dataSource.Clear();
            TraceEvent.ResetIndex();
        }

        private void ETWProviders_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EtwProvidersWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
            UpdateEtwEvents();
        }

        private void Filters_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FiltersWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void Colorizers_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ColorizersWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void SetFont()
        {
            LV.FontFamily = new FontFamily(App.Current.Settings.FontName);
            LV.FontSize = App.Current.Settings.FontSize;
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FontDialog())
            {
                dlg.AllowVerticalFonts = false;
                dlg.FontMustExist = true;
                dlg.Font = new System.Drawing.Font(LV.FontFamily.Source, (float)LV.FontSize);
                if (dlg.ShowDialog(System.Windows.Forms.NativeWindow.FromHandle(new WindowInteropHelper(this).Handle)) != System.Windows.Forms.DialogResult.OK)
                    return;

                App.Current.Settings.FontName = dlg.Font.Name;
                App.Current.Settings.FontSize = dlg.Font.Size;
                App.Current.Settings.SerializeToConfiguration();
                SetFont();
            }
        }

        private void SendTestTrace_Click(object sender, RoutedEventArgs e)
        {
            var st = new SendTrace();
            st.Text = App.Current.Settings.TestTraceText.Nullify();
            var dlg = new SendTraceWindow(st);
            dlg.Owner = this;
            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(st.Text))
            {
                App.AddTrace(TraceLevel.Info, st.Text, true);
                App.Current.Settings.TestTraceText = st.Text.Nullify();
                App.Current.Settings.SerializeToConfiguration();
            }
        }

        private void LV_ScrollChanged(object sender, ScrollChangedEventArgs e) => LVH.Margin = new Thickness(-e.HorizontalOffset, 0, 0, 0);

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var evt in LV.SelectedItems.OfType<TraceEvent>().OrderBy(evt => evt.Index))
            {
                sb.AppendLine(evt.Text);
            }

            try
            {
                System.Windows.Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void CopyFullLine_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var evt in LV.SelectedItems.OfType<TraceEvent>().OrderBy(evt => evt.Index))
            {
                sb.AppendLine(evt.FullText);
            }

            try
            {
                System.Windows.Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (_findWindow == null)
            {
                if (LV.Items.Count == 0)
                    return;

                _findWindow = new FindWindow();
                _findWindow.Owner = this;
                _findWindow.Left = Left + App.Current.Settings.FindLeft;
                _findWindow.Top = Top + App.Current.Settings.FindTop;
                _findWindow.FindingNext += (s, e2) => DoFind(true);
                _findWindow.FindingPrev += (s, e2) => DoFind(false);
                _findWindow.LocationChanged += (s, e2) => SaveSettings();
                _findWindow.IsVisibleChanged += (s, e2) =>
                {
                    if (false.Equals(e2.NewValue))
                    {
                        LV.Focus();
                    }
                };
            }

            _findWindow.Show();
            _findWindow.Searches.Focus();
        }

        private void FindNext_Click(object sender, RoutedEventArgs e) => DoFind(true);
        private void FindPrev_Click(object sender, RoutedEventArgs e) => DoFind(false);

        private void DoFind(bool next)
        {
            if (_findWindow == null)
                return;

            var search = _findWindow.Searches.Text;
            if (string.IsNullOrWhiteSpace(search))
                return;

            var focused = FocusManager.GetFocusedElement(_findWindow) is TextBox;

            var start = Math.Max(0, LV.SelectedIndex);
            var sc = _findWindow.CaseMatch ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach (var i in EnumerateItems(start, next))
            {
                var evt = (TraceEvent)LV.Items[i];
                if (evt.Text == null)
                    continue;

                if (evt.Text.IndexOf(search, sc) >= 0)
                {
                    LV.SelectedItem = evt;
                    ((ListViewItem)LV.ItemContainerGenerator.ContainerFromItem(evt))?.Focus();
                    LV.ScrollIntoView(evt);
                    if (focused)
                    {
                        if (next)
                        {
                            _findWindow.FindNext.Focus();
                        }
                        else
                        {
                            _findWindow.FindPrev.Focus();
                        }
                    }
                    _findWindow.Searches.SelectedItem = search;
                    return;
                }
            }

            _findWindow.ShowMessage("Cannot find '" + search + "'.", MessageBoxImage.Information);
        }

        private IEnumerable<int> EnumerateItems(int start, bool next)
        {
            if (next)
            {
                for (var i = start + 1; i < LV.Items.Count; i++)
                {
                    yield return i;
                }
                yield break;
            }

            for (var i = start - 1; i >= 0; i--)
            {
                yield return i;
            }
        }

        private void Edit_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ThemeSubmenu(sender, e);
            CopyFullLine.IsEnabled = LV.SelectedItems.Count > 0;
            if (LV.SelectedItems.Count > 1)
            {
                CopyFullLine.Header = "Copy Full " + LV.SelectedItems.Count + " _Lines";
            }
            else
            {
                CopyFullLine.Header = "Copy Full _Line";
            }

            CopyText.IsEnabled = LV.SelectedItems.Count > 0;
            if (LV.SelectedItems.Count > 1)
            {
                CopyText.Header = "_Copy " + LV.SelectedItems.Count + " Texts";
            }
            else
            {
                CopyText.Header = "_Copy Text";
            }

            Find.IsEnabled = LV.Items.Count > 0;
            FindNext.IsEnabled = _findWindow != null && !string.IsNullOrWhiteSpace(_findWindow.Searches.Text);
            FindPrev.IsEnabled = FindNext.IsEnabled;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void DisposeEtwEvents()
        {
            foreach (var kvp in _etwListeners)
            {
                try
                {
                    kvp.Value.RealtimeEvent -= OnEtwListenerRealtimeEvent;
                    kvp.Value.Dispose();
                }
                catch
                {
                    // do nothing, continue
                }
            }
            _etwListeners.Clear();
        }

        private void ProcessEtwTrace(object state)
        {
            var listener = (EventRealtimeListener)state;
            listener.ProcessTraces();
        }

        private void OnEtwListenerRealtimeEvent(object sender, EventRealtimeEventArgs e)
        {
            var listener = (EventRealtimeListener)sender;

            var evt = new TraceEvent();
            evt.ProcessName = GetProcessName(e.ProcessId);
            if (listener.Description != null && _state.ShowEtwDescription)
            {
                evt.ProcessName += " (" + listener.Description + ")";
            }
            evt.Text = e.Message;
            _events.Add(evt);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    DequeueTraces();
                }, DispatcherPriority.SystemIdle);
            });
        }

        private void UpdateEtwEvents()
        {
            DisposeEtwEvents();
            if (!_state.EtwStarted)
                return;

            foreach (var provider in App.Current.Settings.EtwProviders)
            {
                _etwListeners.TryGetValue(provider.Guid, out EventRealtimeListener listener);
                if (listener == null)
                {
                    if (!provider.IsActive)
                        continue;

                    // this would be a serialization bug
                    if (provider.Guid == Guid.Empty)
                        continue;

                    var level = (EtwTraceLevel)provider.TraceLevel;
                    listener = new EventRealtimeListener(provider.Guid, provider.Guid.ToString(), level);
                    listener.Description = provider.Description;
                    listener.StringMessageMode = provider.StringMessageMode;

                    var t = new Thread(ProcessEtwTrace);
                    t.Start(listener);

                    listener.RealtimeEvent += OnEtwListenerRealtimeEvent;
                    _etwListeners.Add(provider.Guid, listener);
                }
                else
                {
                    if (!provider.IsActive)
                    {
                        listener.RealtimeEvent -= OnEtwListenerRealtimeEvent;
                        listener.Dispose();
                        _etwListeners.Remove(provider.Guid);
                        continue;
                    }
                    else
                    {
                        listener.Description = provider.Description;
                        listener.StringMessageMode = provider.StringMessageMode;
                    }
                }
            }
        }

        private void OpenConfig_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo { FileName = System.IO.Path.GetDirectoryName(WpfSettings.ConfigurationFilePath), UseShellExecute = true });

        private void LV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LV.SelectedIndex < 0)
                return;

            var dlg = new TraceDetailsWindow(LV);
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void ODSEncoding_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OdsEncodingsWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void ClearSearches_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Settings.ClearSearches();
            App.Current.Settings.SerializeToConfiguration();
        }

        private void Transparency_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new TransparencyWindow();
            dlg.Owner = this;
            dlg.ShowDialog();

            if (App.Current.Settings.EnableTransparency != App.Current.MainWindow.AllowsTransparency)
            {
                if (this.ShowConfirm("Transparency settings have changed, a restart is needed for it to take effect. Do you want to restart TraceSpy now?") != MessageBoxResult.Yes)
                    return;

                System.Windows.Forms.Application.Restart();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Caption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = MainMenu.InputHitTest(e.GetPosition(MainMenu));
            if (hit is Border)
            {
                DragMove();
            }
        }

        private void Caption_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hit = MainMenu.InputHitTest(e.GetPosition(MainMenu));
            if (hit is Border)
            {
                if (WindowState == WindowState.Maximized)
                {
                    RestoreButton_Click(sender, e);
                }
                else
                {
                    MaximizeButton_Click(sender, e);
                }
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            RestoreButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            RestoreButton.Visibility = Visibility.Collapsed;
            MaximizeButton.Visibility = Visibility.Visible;
        }

        private void ResetWindow_Click(object sender, RoutedEventArgs e)
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
            var bounds = screen.WorkingArea;
            bounds.Inflate(-100, -100);
            Height = bounds.Height;
            MaxHeight = bounds.Height;
            Width = bounds.Width;
            MaxWidth = bounds.Width;
            Left = bounds.Left + (bounds.Width - ActualWidth) / 2;
            Top = bounds.Top + (bounds.Height - ActualHeight) / 2;
        }

        private void ThemeSubmenu(object sender, RoutedEventArgs e)
        {
            // apply background all the way down
            var popup = (sender as DependencyObject)?.FindVisualChild<Popup>();
            if (popup.Child is Border border)
            {
                border.Background = CurrentTheme.MenuBackColorBrush;
                if (border.Child is ScrollViewer sv)
                {
                    sv.Background = border.Background;
                    if (sv.Content is Grid g)
                    {
                        foreach (var rc in g.EnumerateVisualChildren().OfType<Rectangle>())
                        {
                            rc.Fill = border.Background;
                        }
                    }
                }
            }
        }

        private void LV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var te in e.AddedItems.OfType<TraceEvent>())
            {
                te.IsSelected = true;
            }

            foreach (var te in e.RemovedItems.OfType<TraceEvent>())
            {
                te.IsSelected = false;
            }

            UpdateTraceVisuals();
        }
    }
}
