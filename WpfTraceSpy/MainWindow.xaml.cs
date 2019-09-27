using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace TraceSpy
{
    public partial class MainWindow : Window
    {
        private const int _odsBufferSize = 4096;
        private readonly EventWaitHandle _bufferReadyEvent;
        private readonly EventWaitHandle _dataReadyEvent;
        private readonly MemoryMappedFile _buffer;
        private readonly MemoryMappedViewStream _bufferStream;
        private readonly Thread _outputDebugStringThread;
        private readonly ObservableCollection<TraceEvent> _dataSource = new ObservableCollection<TraceEvent>();
        private readonly MainWindowState _state;
        private readonly ConcurrentDictionary<int, string> _processes = new ConcurrentDictionary<int, string>();
        private readonly Dictionary<Guid, EventRealtimeListener> _etwListeners = new Dictionary<Guid, EventRealtimeListener>();
        private readonly ConcurrentLinkedList<TraceEvent> _events = new ConcurrentLinkedList<TraceEvent>();
        private bool _stopOutputDebugStringTraces;
        private FindWindow _findWindow;
        private int _scrollingTo;
        private TraceEvent _scrollTo;

        public MainWindow()
        {
            _state = new MainWindowState();
            _state.AutoScroll = App.Current.Settings.AutoScroll;
            _state.RemoveEmptyLines = App.Current.Settings.RemoveEmptyLines;
            _state.ResolveProcessName = App.Current.Settings.ResolveProcessName;
            _state.ShowEtwDescription = App.Current.Settings.ShowEtwDescription;
            _state.ShowProcessId = App.Current.Settings.ShowProcessId;
            _state.WrapText = App.Current.Settings.WrapText;
            _state.ShowTicksMode = App.Current.Settings.ShowTicksMode;
            _state.PropertyChanged += OnStatePropertyChanged;

            InitializeComponent();
            LoadSettings();
            DataContext = _state;
            _state.EtwStarted = App.Current.Settings.CaptureEtwTraces;
            _state.OdsStarted = App.Current.Settings.CaptureOdsTraces;
            UpdateEtwEvents();

            _bufferReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_BUFFER_READY");
            _dataReadyEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_DATA_READY");

            try
            {
                _buffer = MemoryMappedFile.CreateNew("DBWIN_BUFFER", _odsBufferSize);
                _bufferStream = _buffer.CreateViewStream(0, _odsBufferSize);
            }
            catch (Exception e)
            {
                const int ERROR_ALREADY_EXISTS = unchecked((int)0x800700b7);
                if (e.HResult != ERROR_ALREADY_EXISTS)
                    throw;

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

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            MainMenu.RaiseMenuItemClickOnKeyGesture(e);
        }

        private void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            App.Current.Settings.AutoScroll = _state.AutoScroll;
            App.Current.Settings.RemoveEmptyLines = _state.RemoveEmptyLines;
            App.Current.Settings.ResolveProcessName = _state.ResolveProcessName;
            App.Current.Settings.ShowEtwDescription = _state.ShowEtwDescription;
            App.Current.Settings.ShowProcessId = _state.ShowProcessId;
            App.Current.Settings.WrapText = _state.WrapText;
            App.Current.Settings.ShowTicksMode = _state.ShowTicksMode;
            App.Current.Settings.SerializeToConfiguration();

            if (e.PropertyName == nameof(MainWindowState.WrapText) ||
                e.PropertyName == nameof(MainWindowState.ShowTicksMode))
            {
                OnColumnLayoutPropertyChanged(null, null);
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

        public void AddTrace(TraceEvent evt)
        {
            if (evt == null || evt.ProcessName == null)
                return;

            if (App.Current.Settings.ExcludeLine(evt.Text, evt.ProcessName))
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
                bool result = _dataReadyEvent.WaitOne(500);
                if (_stopOutputDebugStringTraces)
                    return;

                if (!_state.OdsStarted.GetValueOrDefault())
                    continue;

                if (result) // we don't care about other return values
                {
                    _bufferStream.Position = 0;
                    _bufferStream.Read(pidBytes, 0, pidBytes.Length);
                    int pid = BitConverter.ToInt32(pidBytes, 0);
                    _bufferStream.Read(strBytes, 0, strBytes.Length);
                    string text = GetNullTerminatedString(strBytes).Trim();
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
            var chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                {
                    if (i == 0)
                        return string.Empty;

                    return new string(chars, 0, i);
                }
                chars[i] = (char)bytes[i];
            }

            return Encoding.Default.GetString(bytes);
        }

        private void OdsTrace_Click(object sender, RoutedEventArgs e)
        {
            _state.OdsStarted = !_state.OdsStarted;
            if (_state.OdsStarted.GetValueOrDefault())
            {
                App.Current.Settings.CaptureOdsTraces = true;
            }
            else
            {
                App.Current.Settings.CaptureOdsTraces = false;
            }
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

                if (name == null)
                {
                    name = id.ToString();
                }
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

        private void SetFont()
        {
            LV.FontFamily = new FontFamily(App.Current.Settings.FontName);
            LV.FontSize = App.Current.Settings.FontSize;
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.AllowVerticalFonts = false;
                dlg.FontMustExist = true;
                dlg.Font = new System.Drawing.Font(LV.FontFamily.Source, (float)LV.FontSize);
                if (dlg.ShowDialog(NativeWindow.FromHandle(new WindowInteropHelper(this).Handle)) != System.Windows.Forms.DialogResult.OK)
                    return;


                App.Current.Settings.FontName = dlg.Font.Name;
                App.Current.Settings.FontSize = dlg.Font.Size;
                App.Current.Settings.SerializeToConfiguration();
                SetFont();
            }
        }

        private void SendTestTrace_Click(object sender, RoutedEventArgs e) => App.AddTrace("Test " + DateTime.Now);
        private void LV_ScrollChanged(object sender, ScrollChangedEventArgs e) => LVH.Margin = new Thickness(-e.HorizontalOffset, 0, 0, 0);

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var evt in LV.SelectedItems.OfType<TraceEvent>())
            {
                sb.AppendLine(evt.Text);
            }
            System.Windows.Clipboard.SetText(sb.ToString());
        }

        private void CopyFullLine_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var evt in LV.SelectedItems.OfType<TraceEvent>())
            {
                sb.AppendLine(evt.FullText);
            }
            System.Windows.Clipboard.SetText(sb.ToString());
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

            var focused = FocusManager.GetFocusedElement(_findWindow) is System.Windows.Controls.TextBox;

            int start = Math.Max(0, LV.SelectedIndex);
            var sc = _findWindow.CaseMatch ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            foreach (int i in EnumerateItems(start, next))
            {
                var evt = (TraceEvent)LV.Items[i];
                if (evt.Text == null)
                    continue;

                if (evt.Text.IndexOf(search, sc) > 0)
                {
                    LV.SelectedItem = evt;
                    ((System.Windows.Controls.ListViewItem)LV.ItemContainerGenerator.ContainerFromItem(evt))?.Focus();
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
                for (int i = start + 1; i < LV.Items.Count; i++)
                {
                    yield return i;
                }
                yield break;
            }

            for (int i = start - 1; i >= 0; i--)
            {
                yield return i;
            }
        }

        private void Edit_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            CopyFullLine.IsEnabled = LV.SelectedItems.Count > 0;
            CopyText.IsEnabled = LV.SelectedItems.Count > 0;
            Find.IsEnabled = LV.Items.Count > 0;
            FindNext.IsEnabled = _findWindow != null && !string.IsNullOrWhiteSpace(_findWindow.Searches.Text);
            FindPrev.IsEnabled = FindNext.IsEnabled;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {

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

            foreach (EtwProvider provider in App.Current.Settings.EtwProviders)
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

        private void OpenConfig_Click(object sender, RoutedEventArgs e) => Process.Start("\"" + Path.GetDirectoryName(WpfSettings.ConfigurationFilePath) + "\"");

        private void LV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(ItemsControl.ContainerFromElement((ItemsControl)sender, (DependencyObject)e.OriginalSource) is ListBoxItem item))
                return;

            var evt = (TraceEvent)item.DataContext;
            if (evt == null)
                return;

            var dlg = new TraceDetailsWindow(evt);
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}
