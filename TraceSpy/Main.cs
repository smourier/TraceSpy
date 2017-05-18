using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class Main : Form
    {
        private static ReaderWriterLockSlim _lock;
        private static MethodInfo _decodeMessage;
        private const string TextItemName = "Text";

        private Image _shieldImage;
        private IntPtr _bufferReadyEvent;
        private IntPtr _dataReadyEvent;
        private IntPtr _mapping;
        private IntPtr _file;
        private bool _stop;
        private Thread _reader;
        private int _id;
        private readonly Stopwatch _watch;
        private int _index;
        private Settings _settings;
        private readonly System.Windows.Forms.Timer _timer;
        private Find _find;
        private Image _animatedGif;
        private string _saveFilePath;
        private ComponentResourceManager _resources;
        private readonly Dictionary<int, Line> _queue = new Dictionary<int, Line>();
        private readonly Dictionary<int, object> _skipped = new Dictionary<int, object>();
        private readonly Dictionary<int, string> _processes = new Dictionary<int, string>();
        private readonly Dictionary<Guid, EventRealtimeListener> _etwListeners = new Dictionary<Guid, EventRealtimeListener>();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, FileMapAccess dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [Flags]
        private enum FileMapAccess
        {
            FileMapRead = 0x0004,
        }

        [Flags]
        private enum FileMapProtection
        {
            PageReadWrite = 0x04,
        }

#if DEBUG
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
#endif

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, FileMapProtection flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        private const uint WAIT_OBJECT_0 = 0;
        private const int WaitTimeout = 500;
        internal static Main _instance;
        internal static int _processId;
        private ListViewEx listView;
        private ColumnHeader columnHeaderIndex;
        private ColumnHeader columnHeaderTime;
        private ColumnHeader columnHeaderPid;
        private ColumnHeader columnHeaderText;

        public Main()
        {
            columnHeaderIndex = new ColumnHeader();
            columnHeaderIndex.Text = "#";
            columnHeaderIndex.Width = 75;

            columnHeaderTime = new ColumnHeader();
            columnHeaderTime.Text = "Ticks";
            columnHeaderTime.Width = 96;
            
            columnHeaderPid = new ColumnHeader();
            columnHeaderPid.Text = "Process";
            columnHeaderPid.Width = 100;
            
            columnHeaderText = new ColumnHeader();
            columnHeaderText.Text = "Text";
            columnHeaderText.Width = 680;

            listView = new ListViewEx();
            listView.Columns.AddRange(new [] { columnHeaderIndex, columnHeaderTime, columnHeaderPid, columnHeaderText});
            listView.DoubleClick += ListView_DoubleClick;
            Controls.Add(this.listView);
            listView.AutoArrange = false;
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView.HideSelection = false;
            listView.Location = new Point(0, 24);
            listView.Name = "listView";
            listView.OwnerDraw = true;
            listView.Size = new Size(1084, 650);
            listView.TabIndex = 1;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;

            InitializeComponent();

            _instance = this;
            _processId = Process.GetCurrentProcess().Id;
            _animatedGif = captureOnToolStripMenuItem.Image;
            _resources = new ComponentResourceManager(typeof(Main));
            LoadSettings();
            UpdateControls();
            listView.DrawColumnHeader += OnListViewDrawColumnHeader;
            listView.DrawSubItem += OnListViewDrawSubItem;
            dontAnimateCaptureMenuItemToolStripMenuItem.Checked = _settings.DontAnimateCaptureMenuItem;
            captureOnToolStripMenuItem.Image = _settings.DontAnimateCaptureMenuItem ? null : _animatedGif;
            ETWCaptureOnToolStripMenuItem.Image = _settings.DontAnimateCaptureMenuItem ? null : _animatedGif;

#if DEBUG
            AllocConsole();
            Console.WriteLine("Note: this console is shown in DEBUG mode only.");
            debugToolStripMenuItem.Visible = true;
            debugToolStripMenuItem.Click += (sender, e) => SendTestTraces();
#endif

            _bufferReadyEvent = CreateEvent(IntPtr.Zero, false, false, "DBWIN_BUFFER_READY");
            if (_bufferReadyEvent == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            _dataReadyEvent = CreateEvent(IntPtr.Zero, false, false, "DBWIN_DATA_READY");
            if (_dataReadyEvent == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            _mapping = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, FileMapProtection.PageReadWrite, 0, 4096, "DBWIN_BUFFER");
            if (_mapping == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            _file = MapViewOfFile(_mapping, FileMapAccess.FileMapRead, 0, 0, 1024);
            if (_file == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

            removeEmptyLinesToolStripMenuItem.Checked = _settings.RemoveEmptyLines;
            autoScrollToolStripMenuItem.Checked = _settings.AutoScroll;
            showProcessNameToolStripMenuItem.Checked = _settings.ShowProcessName;
            showProcessIdToolStripMenuItem.Enabled = showProcessNameToolStripMenuItem.Checked;
            showProcessIdToolStripMenuItem.Checked = _settings.ShowProcessId;
            showETWDescriptionToolStripMenuItem.Checked = _settings.ShowEtwDescription;
            showTooltipsToolStripMenuItem.Checked = _settings.ShowTooltips;

            captureETWProvidersTracesToolStripMenuItem.Checked = _settings.CaptureEtwTraces;
            captureToolStripMenuItem.Checked = _settings.CaptureOutputDebugString;

            if (_settings.Font != null)
            {
                listView.Font = _settings.Font;
            }

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 100;
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _watch = new Stopwatch();
            _reader = new Thread(Read);
            _reader.Start();

            if (_settings.CaptureEtwTraces)
            {
                UpdateEtwEvents();
            }

            if (UacUtilities.IsAdministrator())
            {
                Text += " - Administrator";
            }

            UpdateControls();
        }

        private class ListViewEx : ListView
        {
            public ListViewEx()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
                SetStyle(ControlStyles.Opaque, true);
                DoubleBuffered = true;
            }

            //protected override void WndProc(ref Message m)
            //{
            //    Log("DecodeMessage: " + DecodeMessage(m));
            //    base.WndProc(ref m);
            //}

            //[StructLayout(LayoutKind.Sequential)]
            //private struct NMHDR
            //{
            //    public IntPtr hwndFrom;
            //    public IntPtr idFrom;
            //    public int code;
            //}
        }

        internal static string DecodeMessage(Message message)
        {
            if (_decodeMessage == null)
            {
                Type type = typeof(Message).Assembly.GetType("System.Windows.Forms.MessageDecoder", true);
                _decodeMessage = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
            }

            string name = null;
            switch (message.Msg)
            {
                case 0x100E:
                    name = "LVM_GETITEMRECT";
                    break;

                case 0x1012:
                    name = "LVM_HITTEST";
                    break;

                case 0x1013:
                    name = "LVM_ENSUREVISIBLE";
                    break;

                case 0x101D:
                    name = "LVM_GETCOLUMNWIDTH";
                    break;

                case 0x101F:
                    name = "LVM_GETHEADER";
                    break;

                case 0x102C:
                    name = "LVM_GETITEMSTATE";
                    break;

                case 0x102F:
                    name = "LVM_SETITEMCOUNT";
                    break;

                case 0x1038:
                    name = "LVM_GETSUBITEMRECT";
                    break;

                case 0x104B:
                    name = "LVM_GETITEMW";
                    break;

                case 0x104D:
                    name = "LVM_INSERTITEMW";
                    break;

                case 0x1053:
                    name = "LVM_FINDITEMW";
                    break;

                case 0x1074:
                    name = "LVM_SETITEMTEXTW";
                    break;

                case 0x10C1:
                    name = "LVM_GETACCVERSION";
                    break;
            }

            if (name == null)
                return (string)_decodeMessage.Invoke(message, null);

            return "msg=0x" + message.Msg.ToString("x") + " (" + name + ") hwnd=0x" + message.HWnd.ToString("x") + " wparam=0x" + message.WParam.ToString("x") + " lparam=0x" + message.LParam.ToString("x") + " result=0x" + message.Result.ToString("x");
        }

        internal void RefreshListView()
        {
            listView.Refresh();
        }

        private void OnListViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            try
            {
                OnListViewDrawSubItem(e);
            }
            catch (Exception ex)
            {
                Log("OnListViewDrawSubItem e:" + ex);
                throw;
            }
        }

        private void OnListViewDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (e.SubItem.Name != TextItemName)
            {
                e.DrawDefault = true;
                return;
            }

            //Log("State:" + e.Item.Selected);
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }

            var format = StringFormat.GenericTypographic;
            format.FormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces;
            format.Trimming = StringTrimming.EllipsisCharacter;
            format.SetTabStops(0, new[] { _settings.TabSize });

            float x = e.Bounds.X;
            float y = e.Bounds.Y;
            int charIndex = 0;
            foreach (ColorRange range in _settings.ComputeColorRanges(e.SubItem.Text))
            {
                //Log("OLVD charIndex:" + charIndex + " range.Length:" + range.Length + " Text:[" + e.SubItem.Text + "]");
                string chunk = e.SubItem.Text.Substring(charIndex, range.Length);
                //Log("Range:" + range + " chunk='" + chunk + "'");

                Font font;
                if (range.ColorSet == null || range.ColorSet.Font == null)
                {
                    font = listView.Font;
                }
                else
                {
                    font = range.ColorSet.Font;
                }

                SizeF size = e.Graphics.MeasureString(chunk, font, new PointF(), format);
                var layout = new RectangleF(x, y + (e.Bounds.Height - size.Height) / 2, e.Bounds.Width - (x - e.Bounds.X), e.Bounds.Height);
                if (range.ColorSet != null)
                {
                    if (x + size.Width < e.Bounds.Right)
                    {
                        //Log(" BackColor:" + range.ColorSet.BackColor + " ForeColor=" + range.ColorSet.ForeColor + " size=" + size);
                        switch (range.ColorSet.Mode)
                        {
                            case ColorSetDrawMode.Frame:
                                e.Graphics.DrawRectangle(range.ColorSet.BackPen, x, y + (e.Bounds.Height - size.Height) / 2, size.Width, size.Height);
                                break;

                            //case ColorizerMode.Fill:
                            default:
                                e.Graphics.FillRectangle(range.ColorSet.BackBrush, x, y, size.Width, e.Bounds.Height);
                                break;
                        }
                    }
                    e.Graphics.DrawString(chunk, font, range.ColorSet.ForeBrush, layout, format);
                }
                else
                {
                    using (var brush = new SolidBrush(listView.ForeColor))
                    {
                        e.Graphics.DrawString(chunk, font, brush, layout, format);
                    }
                }
                x += size.Width;
                charIndex += range.Length;
            }
        }

        private static void OnListViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_lock.TryEnterReadLock(_timer.Interval / 2))
                {
                    try
                    {
                        if (_queue.Count > 0)
                        {
                            bool beginUpdate = false;
                            do
                            {
                                while (_skipped.ContainsKey(_index))
                                {
                                    _index++;
                                }

                                Line line;
                                if (!_queue.TryGetValue(_index, out line))
                                    break;

                                if (!beginUpdate)
                                {
                                    beginUpdate = true;
                                    listView.BeginUpdate();
                                }

                                double seconds = (double)line.Ticks / Stopwatch.Frequency;

                                // split list into multiple
                                string[] lines = line.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                ListViewItem lastItem = null;
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    ListViewItem item;
                                    if (i == 0)
                                    {
                                        item = new ListViewItem(line.Index.ToString());
                                        item.SubItems.Add(seconds.ToString("0.00000000"));

                                        string name;
                                        if (line.ProcessName != null)
                                        {
                                            if (_settings.ShowProcessId)
                                            {
                                                name = line.ProcessName + " ("+ line.Pid + ")";
                                            }
                                            else
                                            {
                                                name = line.ProcessName;
                                            }
                                        }
                                        else
                                        {
                                            name = line.Pid.ToString();
                                        }

                                        if (line.Description != null && _settings.ShowEtwDescription)
                                        {
                                            name += " (" + line.Description + ")";
                                        }

                                        item.SubItems.Add(name);
                                    }
                                    else
                                    {
                                        item = new ListViewItem(string.Empty);
                                        item.SubItems.Add(string.Empty);
                                        item.SubItems.Add(string.Empty);
                                    }
                                    item.SubItems.Add(lines[i]).Name = TextItemName;

                                    // note: we don't use Tag to store line objects as we want to save memory here
                                    listView.Items.Add(item);
                                    lastItem = item;
                                    if (line.Pid == _processId)
                                    {
                                        foreach (ListViewItem.ListViewSubItem si in item.SubItems)
                                        {
                                            si.BackColor = Color.Red;
                                        }
                                    }
                                }

                                _queue.Remove(_index);
                                _index++;

                                if (lastItem != null && autoScrollToolStripMenuItem.Checked)
                                {
                                    lastItem.EnsureVisible();
                                }
                            }
                            while (true);
                            if (beginUpdate)
                            {
                                listView.EndUpdate();
                            }
                        }
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }
            catch (Exception ex)
            {
                Log("OnTimerTick Exception: " + ex);
            }
        }

        [Conditional("DEBUG")]
        internal static void Log(object value)
        {
            // we don't want to use standard traces here...
            Console.WriteLine(value);
        }

        private string GetProcessName(int id)
        {
            if (!showProcessNameToolStripMenuItem.Checked)
                return id.ToString();

            string name;
            if (!_processes.TryGetValue(id, out name))
            {
                try
                {
                    Process process = Process.GetProcessById(id);
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

        private class Line
        {
            public int Index;
            public int Pid;
            public string ProcessName;
            public string Text;
            public long Ticks;
            public string Description;
        }

        private void Read()
        {
            try
            {
                do
                {
                    SetEvent(_bufferReadyEvent);
                    uint wait = WaitForSingleObject(_dataReadyEvent, WaitTimeout);
                    if (_stop)
                        return;

                    if (!captureToolStripMenuItem.Checked)
                        continue;

                    if (wait == WAIT_OBJECT_0) // we don't care about other return values
                    {
                        int pid = Marshal.ReadInt32(_file);
                        string text = Marshal.PtrToStringAnsi(new IntPtr(_file.ToInt32() + Marshal.SizeOf(typeof(int)))).TrimEnd(null);
                        if (string.IsNullOrEmpty(text) && removeEmptyLinesToolStripMenuItem.Checked)
                            continue;

                        Enqueue(pid, text);
                    }
                }
                while (true);
            }
            catch (Exception e)
            {
                Log("Read Exception: " + e);
            }
        }

        internal void Enqueue(int pid, string text)
        {
            Enqueue(pid, text, null);
        }

        internal void Enqueue(int pid, string text, string description)
        {
            var line = new Line();
            line.Index = _id++;
            line.Pid = pid;
            line.Text = text;
            line.Description = description;
            if (!_watch.IsRunning)
            {
                _watch.Start();
                // small hack; we ensure the first has 0 ticks
                line.Ticks = 0;
            }
            else
            {
                line.Ticks = _watch.ElapsedTicks;
            }

            // enqueue in the thread pool so we don't wait too long here and the traced process will not be stuck by us.
            ThreadPool.QueueUserWorkItem(Enqueue, line);
        }

        private void Enqueue(object obj)
        {
            try
            {
                Line line = (Line)obj;
                if (_settings.ShowProcessName)
                {
                    line.ProcessName = GetProcessName(line.Pid);
                }

                if (!_settings.IncludeLine(line.Text, line.ProcessName))
                {
                    _skipped.Add(line.Index, null);
                    //Log("Don't include '" + line.Text + "'");
                    return;
                }

                if (_settings.ExcludeLine(line.Text, line.ProcessName))
                {
                    _skipped.Add(line.Index, null);
                    //Log("Do Exclude '" + line.Text + "'");
                    return;
                }

                _lock.EnterWriteLock();
                try
                {
                    //Log("Enqueue index=" + line.Index + " text=" + line.Text);
                    _queue.Add(line.Index, line);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception e)
            {
                Log("Enqueue Exception: " + e);
            }
        }

        private void DisposeEvents()
        {
            if (_bufferReadyEvent != IntPtr.Zero)
            {
                CloseHandle(_bufferReadyEvent);
                _bufferReadyEvent = IntPtr.Zero;
            }

            if (_dataReadyEvent != IntPtr.Zero)
            {
                CloseHandle(_dataReadyEvent);
                _dataReadyEvent = IntPtr.Zero;
            }

            if (_file != IntPtr.Zero)
            {
                UnmapViewOfFile(_file);
                _file = IntPtr.Zero;
            }

            if (_mapping != IntPtr.Zero)
            {
                CloseHandle(_mapping);
                _mapping = IntPtr.Zero;
            }
        }

        private void LoadSettings()
        {
            _settings = Settings.DeserializeFromConfiguration();
            Left = _settings.Left;
            Top = _settings.Top;
            Width = _settings.Width;
            Height = _settings.Height;

            columnHeaderIndex.Width = _settings.IndexColumnWidth;
            columnHeaderTime.Width = _settings.TicksColumnWidth;
            columnHeaderPid.Width = _settings.ProcessColumnWidth;
            columnHeaderText.Width = _settings.TextColumnWidth;
        }

        private void SaveSettings()
        {
            if (_settings == null)
                return;

            if (WindowState != FormWindowState.Minimized)
            {
                _settings.Left = Left;
                _settings.Top = Top;
                _settings.Width = Width;
                _settings.Height = Height;
            }

            if (_find != null)
            {
                _settings.FindLeft = _find.Location.X - Location.X;
                _settings.FindTop = _find.Location.Y - Location.Y;
            }

            _settings.IndexColumnWidth = columnHeaderIndex.Width;
            _settings.TicksColumnWidth = columnHeaderTime.Width;
            _settings.ProcessColumnWidth = columnHeaderPid.Width;
            _settings.TextColumnWidth = columnHeaderText.Width;

            _settings.SerializeToConfiguration();
        }

        protected override void Dispose(bool disposing)
        {
            _stop = true; // ask thread to stop

            SaveSettings();
            if (disposing && components != null)
            {
                components.Dispose();
            }

            DisposeEvents();
            DisposeEtwEvents();

            // ensure the thread has stopped or kill it (which shouldn't happen)
            if (_reader != null)
            {
                if (_reader.IsAlive)
                {
                    if (!_reader.Join(WaitTimeout * 2))
                    {
                        _reader.Abort();
                    }
                }
                _reader = null;
            }

            if (_shieldImage != null)
            {
                _shieldImage.Dispose();
                _shieldImage = null;
            }

            base.Dispose(disposing);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ClearToolStripMenuItemClick(object sender, EventArgs e)
        {
            _lock.EnterWriteLock();
            try
            {
                _index = 0;
                _skipped.Clear();
                _queue.Clear();
                _processes.Clear();
                listView.Items.Clear();
                _id = 0;
                _watch.Stop();
                _watch.Reset();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

        }

        private void FiltersToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new Filters(_settings);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            var about = new About();
            about.ShowDialog(this);
        }

        private void RemoveEmptyLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            _settings.RemoveEmptyLines = removeEmptyLinesToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
        }

        private void AutoScrollToolStripMenuItemClick(object sender, EventArgs e)
        {
            _settings.AutoScroll = autoScrollToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_find == null)
            {
                _find = new Find(_settings);
                _find.Owner = this;
                _find.Location = new Point(Location.X + _settings.FindLeft, Location.Y + _settings.FindTop);
            }

            if (!_find.Visible)
            {
                _find.Show();
            }
            _find.Activate();
        }

        private void CopyAllLineToolStripMenuItemClick(object sender, EventArgs e)
        {
            var text = new StringBuilder();
            int count = 0;
            foreach (var line in SelectedLines)
            {
                text.Append(line.Index);
                text.Append('\t');
                text.Append(line.Ticks);
                text.Append('\t');
                text.Append(line.ProcessName);
                text.Append('\t');
                text.AppendLine(line.Text);
                count++;
            }
            if (count == 0)
                return;

            Clipboard.SetText(text.ToString(), TextDataFormat.UnicodeText);
        }

        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            var text = new StringBuilder();
            int count = 0;
            foreach (var line in SelectedLines)
            {
                text.AppendLine(line.Text);
                count++;
            }
            if (count == 0)
                return;

            Clipboard.SetText(text.ToString(), TextDataFormat.UnicodeText);
        }

        internal class TextLine
        {
            public string Index;
            public string Ticks;
            public string ProcessName;
            public string Text;
            public ListViewItem Item;

            public static TextLine FromItem(ListViewItem item)
            {
                var line = new TextLine();
                line.Index = item.Text;
                line.Ticks = item.SubItems[1].Text;
                line.ProcessName = item.SubItems[2].Text;
                line.Text = item.SubItems[3].Text;
                line.Item = item;
                return line;
            }
        }

        internal void FindAndSelect(string search, Regex regex)
        {
            foreach (TextLine line in EnumerateLinesFromFocus())
            {
                if (regex.Match(line.Text).Success)
                {
                    // clear selection
                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        item.Selected = false;
                    }
                    line.Item.Selected = true;
                    listView.FocusedItem = line.Item;
                    line.Item.EnsureVisible();
                    return;
                }
            }

            MessageBox.Show(this, @"Cannot find '" + search + @"'", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            _find.Activate();
        }

        private IEnumerable<TextLine> EnumerateLinesFromFocus()
        {
            int start;
            if (listView.FocusedItem == null)
            {
                start = 0;
            }
            else
            {
                start = listView.FocusedItem.Index + 1;
            }

            for (int i = start; i < listView.Items.Count; i++)
            {
                ListViewItem item = listView.Items[i];
                yield return TextLine.FromItem(item);
            }
        }

        private IEnumerable<TextLine> SelectedLines
        {
            get
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    yield return TextLine.FromItem(item);
                }
            }
        }

        private void UpdateShowProcessOptions()
        {
            _settings.ShowProcessName = showProcessNameToolStripMenuItem.Checked;
            showProcessIdToolStripMenuItem.Enabled = showProcessNameToolStripMenuItem.Checked;
            _settings.ShowProcessId = showProcessIdToolStripMenuItem.Checked;
            _settings.ShowEtwDescription = showETWDescriptionToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
        }

        private void ShowProcessNameToolStripMenuItemClick(object sender, EventArgs e)
        {
            UpdateShowProcessOptions();
        }

        private void ShowProcessIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateShowProcessOptions();
        }

        private void ShowETWDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateShowProcessOptions();
        }

        private void ShowTooltipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settings.ShowTooltips = showTooltipsToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
        }

        private void FontToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new FontDialog();
            dlg.Font = listView.Font;
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            listView.Font = dlg.Font;
            _settings.Font = listView.Font;
        }

        private void FindNextToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_find != null)
            {
                _find.FindNext();
            }
        }

        private void EditToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            findNextToolStripMenuItem.Enabled = _find != null && _find.comboBoxFind.Text.Trim().Length > 0;
        }

        private void ColorizersToolStripMenuItemClick(object sender, EventArgs e)
        {
            var dlg = new Colorizers(_settings);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
        }

        private void QuickColorizersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new QuickColorizers(_settings);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
        }

        private void UpdateControls()
        {
            if (_settings.CaptureOutputDebugString)
            {
                captureOnToolStripMenuItem.Image = ((Image)(_resources.GetObject("captureOnToolStripMenuItem.Image")));
                captureOnToolStripMenuItem.Text = "Stop Capture";
            }
            else
            {
                captureOnToolStripMenuItem.Image = null;
                captureOnToolStripMenuItem.Text = "Start Capture";
            }

            if (_settings.CaptureEtwTraces)
            {
                ETWCaptureOnToolStripMenuItem.Image = ((Image)(_resources.GetObject("captureOnToolStripMenuItem.Image")));
                ETWCaptureOnToolStripMenuItem.Text = "Stop ETW Capture";
            }
            else
            {
                ETWCaptureOnToolStripMenuItem.Image = null;
                ETWCaptureOnToolStripMenuItem.Text = "Start ETW Capture";
            }
            captureOnToolStripMenuItem.ForeColor = _settings.CaptureOutputDebugString ? Color.Red : Color.Green;
            ETWCaptureOnToolStripMenuItem.ForeColor = _settings.CaptureEtwTraces ? Color.Red : Color.Green;
        }

        private void CaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settings.CaptureOutputDebugString = captureToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();

            UpdateControls();
        }

        private void CaptureOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            captureToolStripMenuItem.Checked = !captureToolStripMenuItem.Checked;
            _settings.CaptureOutputDebugString = captureToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
            UpdateControls();
        }

        private void StopETWCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            captureETWProvidersTracesToolStripMenuItem.Checked = !captureETWProvidersTracesToolStripMenuItem.Checked;
            _settings.CaptureEtwTraces = captureETWProvidersTracesToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
            UpdateEtwEvents();
            UpdateControls();
        }

        private void DontAnimateCaptureMenuItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settings.DontAnimateCaptureMenuItem = dontAnimateCaptureMenuItemToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();

            captureOnToolStripMenuItem.Image = _settings.DontAnimateCaptureMenuItem ? null : _animatedGif;
            ETWCaptureOnToolStripMenuItem.Image = _settings.DontAnimateCaptureMenuItem ? null : _animatedGif;
        }

        private void ETWProvidersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new EtwProviders(_settings);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            UpdateEtwEvents();
        }

        private void CaptureETWProvidersTracesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _settings.CaptureEtwTraces = captureETWProvidersTracesToolStripMenuItem.Checked;
            _settings.SerializeToConfiguration();
            UpdateEtwEvents();
            UpdateControls();
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

        private void UpdateEtwEvents()
        {
            DisposeEtwEvents();
            if (!_settings.CaptureEtwTraces)
                return;

            foreach (EtwProvider provider in _settings.EtwProviders)
            {
                EventRealtimeListener listener;
                _etwListeners.TryGetValue(provider.ProviderGuid, out listener);
                if (listener == null)
                {
                    if (!provider.Active)
                        continue;

                    // this would be a serialization bug
                    if (provider.ProviderGuid == Guid.Empty)
                        continue;

                    var level = (EtwTraceLevel)provider.TraceLevel;

                    listener = new EventRealtimeListener(provider.ProviderGuid, provider.ProviderGuid.ToString(), level);
                    listener.Description = provider.Description;

                    var t = new Thread(ProcessEtwTrace);
                    t.Start(listener);

                    listener.RealtimeEvent += OnEtwListenerRealtimeEvent;
                    _etwListeners.Add(provider.ProviderGuid, listener);
                }
                else
                {
                    if (!provider.Active)
                    {
                        listener.RealtimeEvent -= OnEtwListenerRealtimeEvent;
                        listener.Dispose();
                        _etwListeners.Remove(provider.ProviderGuid);
                        continue;
                    }
                }
            }
        }

        private void ProcessEtwTrace(object state)
        {
            EventRealtimeListener listener = (EventRealtimeListener)state;
            listener.ProcessTraces();
        }

        private void OnEtwListenerRealtimeEvent(object sender, RealtimeEventArgs e)
        {
            EventRealtimeListener listener = (EventRealtimeListener)sender;
            Enqueue(e.ProcessId, e.Message, listener.Description);
        }

        private Image ShieldImage
        {
            get
            {
                if (_shieldImage == null)
                {
                    _shieldImage = UacUtilities.GetShieldImage(UacUtilities.SHGSI.SHGSI_ICON | UacUtilities.SHGSI.SHGSI_SMALLICON);
                }
                return _shieldImage;
            }
        }

        private void FileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            restartAsAdministratorToolStripMenuItem.Image = ShieldImage;
            bool admin = UacUtilities.IsAdministrator();
            restartAsAdministratorToolStripMenuItem.Enabled = !admin;
        }

        private void RestartAsAdministratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (UacUtilities.RestartAsAdmin(true))
            {
                Close();
            }
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            ListViewItem item = listView.SelectedItems[0];
            int index = item.Index;
            while (item.Text == string.Empty)
            {
                index--;
                if (index < 0)
                    return;

                item = listView.Items[index];
            }

            var dlg = new RecordView(item);
            dlg.ShowDialog();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.AddExtension = true;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.DefaultExt = ".log";
            dlg.Title = "Open TraceSpy Log File...";
            dlg.Filter = "LOG files (*.log)|*.log|All files (*.*)|*.*";
            dlg.Multiselect = false;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            Open(dlg.FileName);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_saveFilePath == null)
            {
                SaveAsToolStripMenuItem_Click(sender, e);
                return;
            }

            Save(_saveFilePath);
        }

        private void Open(string filePath)
        {
            listView.Items.Clear();
            using (var reader = new StreamReader(filePath))
            {
                do
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        return;

                    string[] split = line.Split(new[] { '\t' }, 4);
                    if (split.Length > 0)
                    {
                        var item = new ListViewItem(split[0]);
                        if (split.Length > 1)
                        {
                            item.SubItems.Add(split[1]);
                            if (split.Length > 2)
                            {
                                item.SubItems.Add(split[2]);
                                if (split.Length > 3)
                                {
                                    item.SubItems.Add(split[3]);
                                }
                            }
                        }
                        listView.Items.Add(item);
                    }
                }
                while (true);
            }
        }

        private void Save(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (ListViewItem item in listView.Items)
                {
                    writer.Write(item.Text);
                    foreach (ListViewItem.ListViewSubItem si in item.SubItems)
                    {
                        writer.Write('\t');
                        writer.Write(si.Text);
                    }
                    writer.WriteLine();
                }
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;
            dlg.RestoreDirectory = true;
            dlg.DefaultExt = ".log";
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            dlg.Title = "Save TraceSpy Output to File...";
            dlg.Filter = "LOG files (*.log)|*.log|All files (*.*)|*.*";

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            _saveFilePath = dlg.FileName;
            Save(_saveFilePath);
        }

        private void ViewConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", Settings.ConfigurationFilePath);
        }

        private void OpenConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("\"" + Path.GetDirectoryName(Settings.ConfigurationFilePath) + "\"");
        }

#if DEBUG
        private void SendTestTraces()
        {
            string lorem = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce suscipit mauris ac mollis consequat. Donec porttitor nisi quis diam feugiat, ac consequat lacus commodo. Suspendisse id mollis sem, at varius ipsum. Phasellus ultrices leo id ligula egestas scelerisque. Integer in feugiat ligula. Ut quis urna felis. Fusce nec tempor diam. Praesent non egestas arcu.";
            // for testing purposes
            for (int i = 0; i < 2; i++)
            {
                Trace.WriteLine(i + " " + lorem);
            }
        }
#endif
    }
}
