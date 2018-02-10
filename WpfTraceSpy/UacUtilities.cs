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
            SHGSI_ICON = 0x100,
            SHGSI_ICONLOCATION = 0,
            SHGSI_LARGEICON = 0,
            SHGSI_LINKOVERLAY = 0x8000,
            SHGSI_SELECTED = 0x10000,
            SHGSI_SHELLICONSIZE = 4,
            SHGSI_SMALLICON = 1,
            SHGSI_SYSICONINDEX = 0x4000
        }

        private const int SHIELD = 0x4d;

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

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHGetStockIconInfo(int siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        //public static Image GetShieldImage(SHGSI flags)
        //{
        //    using (var icon = GetShieldIcon(flags))
        //    {
        //        if (icon == null)
        //            return null;

        //        return icon.ToBitmap();
        //    }
        //}

        //private static Icon GetStockIcon(int id, SHGSI flags)
        //{
        //    var info = new SHSTOCKICONINFO();
        //    info.cbSize = Marshal.SizeOf(typeof(SHSTOCKICONINFO));
        //    if (SHGetStockIconInfo(id, flags, ref info) == 0)
        //        return Icon.FromHandle(info.hIcon);

        //    return null;
        //}

        //public static Icon GetShieldIcon(SHGSI flags)
        //{
        //    return GetStockIcon(SHIELD, flags);
        //}
    }
}
