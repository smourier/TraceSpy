using System;
using System.Windows.Forms;

namespace TraceSpy
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (s, e) => OnException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => OnException(e.ExceptionObject as Exception);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        private static void OnException(Exception e)
        {
            if (e == null)
                return;

            var dlg = new ThreadExceptionDialog(e);
            dlg.ShowDialog();
        }
    }
}
