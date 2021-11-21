using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace TraceSpy
{
    public static class UacUtilities
    {
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            return identity != null && new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool RestartAsAdmin(bool force)
        {
            if (!force && IsAdministrator())
                return false;

            var info = new ProcessStartInfo();
            info.FileName = Environment.GetCommandLineArgs()[0];
            info.UseShellExecute = true;
            info.Verb = "runas"; // Provides Run as Administrator

            return Process.Start(info) != null;
        }

        [Flags]
        public enum SHGSI : int
        {
#pragma warning disable CA1712 // Do not prefix enum values with type name
            SHGSI_ICON = 0x100,
            SHGSI_ICONLOCATION = 0,
            SHGSI_LARGEICON = 0,
            SHGSI_LINKOVERLAY = 0x8000,
            SHGSI_SELECTED = 0x10000,
            SHGSI_SHELLICONSIZE = 4,
            SHGSI_SMALLICON = 1,
            SHGSI_SYSICONINDEX = 0x4000,
#pragma warning restore CA1712 // Do not prefix enum values with type name
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public int cbSize;
            public IntPtr hIcon;
            public int iSysIconIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        private static extern int SHGetStockIconInfo(int siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);
    }
}
