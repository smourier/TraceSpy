using System;
using System.Runtime.InteropServices;

namespace TraceSpyService
{
	public sealed class ConsoleControl : IDisposable
	{
		private delegate void ControlEventHandler(ConsoleEventType eventType);

		public event EventHandler<ConsoleControlEventArgs> Event;

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool FreeConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ConsoleCharacterAttributes wAttributes);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		private const int STD_OUTPUT_HANDLE = -11;

		[DllImport("user32.dll")]
		private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, int index);

        public const int ApplicationIcon = 32512;

		[StructLayout(LayoutKind.Sequential)]
		private struct FLASHWINFO
		{
			public int cbSize;
			public IntPtr hwnd;
			public FLASHFLAGS dwFlags;
			public int uCount;
			public int dwTimeout;
		}

		[Flags]
		private enum FLASHFLAGS
		{
			//FLASHW_STOP = 0,
			FLASHW_CAPTION = 1,
			FLASHW_TRAY = 2,
			//FLASHW_TIMER = 4,
			FLASHW_TIMERNOFG = 12
		}

		public ConsoleControl()
		{
			SetConsoleCtrlHandler(OnEvent, true);
		}

		public void Dispose()
		{
            SetConsoleCtrlHandler(OnEvent, false);
		}

        public static bool SetConsoleIcon(int index)
        {
            // since this is undocumented...
            try
            {
                if (index <= 0)
                    return SetConsoleIcon(IntPtr.Zero);

                IntPtr hIcon = LoadIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress, index);
                // Zero means reset icon, which is cool

                return SetConsoleIcon(hIcon);
            }
            catch
            {
                return false;
            }
        }

		public static bool FlashConsoleWindow()
		{
			IntPtr hwnd = GetConsoleWindow();
			if (hwnd == IntPtr.Zero)
				return false;

			FLASHWINFO fw = new FLASHWINFO();
			fw.cbSize = Marshal.SizeOf(fw);
			fw.hwnd = hwnd;
			fw.dwFlags = FLASHFLAGS.FLASHW_CAPTION | FLASHFLAGS.FLASHW_TRAY | FLASHFLAGS.FLASHW_TIMERNOFG;
			fw.uCount = -1;
			fw.dwTimeout = 0;
			return FlashWindowEx(ref fw);
		}

		public static void SetStandardOutputCharacterAttributes(ConsoleCharacterAttributes attributes)
		{
			SetConsoleTextAttribute(GetStdHandle(STD_OUTPUT_HANDLE), attributes);
		}

		private void OnEvent(ConsoleEventType consoleEvent)
		{
            EventHandler<ConsoleControlEventArgs> handler = Event;
            if (handler != null)
			{
                handler(this, new ConsoleControlEventArgs(consoleEvent));
			}
		}
	}
}
