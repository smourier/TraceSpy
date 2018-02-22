using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TraceSpy
{
    public partial class MainWindow : Window
    {
        private const int _odsBufferSize = 4096;
        private EventWaitHandle _bufferReadyEvent;
        private EventWaitHandle _dataReadyEvent;
        private MemoryMappedFile _buffer;
        private MemoryMappedViewStream _bufferStream;
        private bool _stopOutputDebugStringTraces;
        private Thread _outputDebugStringThread;
        private ObservableCollection<TraceEvent> _dataSource = new ObservableCollection<TraceEvent>();
        private MainWindowState _state;
        private ConcurrentDictionary<int, string> _processes = new ConcurrentDictionary<int, string>();

        public MainWindow()
        {
            _state = new MainWindowState();
            InitializeComponent();
            DataContext = _state;

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

            IndexColumn.Width = App.Current.ColumnLayout.IndexColumnWidth;
            TicksColumn.Width = App.Current.ColumnLayout.TicksColumnWidth;
            ProcessColumn.Width = App.Current.ColumnLayout.ProcessColumnWidth;
            TextColumn.Width = App.Current.ColumnLayout.TextColumnWidth;

            App.Current.ColumnLayout.PropertyChanged += OnColumnLayoutPropertyChanged;

            //var rnd = new Random(Environment.TickCount);
            //for (int i = 0; i < 1000000; i++)
            //{
            //    var te = new TraceEvent();
            //    te.Index = i;
            //    te.Height = 10 + rnd.Next(0, 20);
            //    te.Background = (i % 2) == 0 ? Brushes.White : Brushes.LightGray;
            //    _dataSource.Add(te);
            //}

            LV.ItemsSource = _dataSource;
            foreach (var col in GV.Columns)
            {
                ((INotifyPropertyChanged)col).PropertyChanged += GridViewColumnPropertyChanged;
            }

            if (_buffer != null)
            {
                _outputDebugStringThread = new Thread(ReadOutputDebugStringTraces);
                _outputDebugStringThread.IsBackground = true;
                _outputDebugStringThread.Start();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
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
                item.InvalidateVisual();
            }
        }

        private void GridViewColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Gif_MediaEnded(object sender, RoutedEventArgs e)
        {
            var me = (MediaElement)sender;
            me.Position = new TimeSpan(0, 0, 1);
            me.Play();
        }

        public void AddTrace(TraceEvent evt)
        {
            if (evt == null)
                return;

            _dataSource.Add(evt);
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
                    if (string.IsNullOrWhiteSpace(text) && _state.RemoveEmptyLines)
                        continue;

                    var evt = new TraceEvent();
                    evt.ProcessId = pid;
                    evt.ProcessName = GetProcessName(pid);
                    evt.Text = text;
                    Dispatcher.BeginInvoke(() =>
                    {
                        _dataSource.Add(evt);
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
        }

        private void EtwTrace_Click(object sender, RoutedEventArgs e)
        {
            _state.EtwStarted = !_state.EtwStarted;
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
            return name;
        }

        private void ClearTraces_Click(object sender, RoutedEventArgs e)
        {
            _dataSource.Clear();
        }

        private void ETWProviders_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EtwProviders();
            dlg.Owner = this;
            dlg.ShowDialog();
        }
    }
}
