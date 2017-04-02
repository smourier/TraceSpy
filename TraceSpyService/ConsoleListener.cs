using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TraceSpyService
{
    public sealed class ConsoleListener : TraceListener
    {
        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        public override void Write(string message)
        {
            Write(message, string.Empty);
        }

        public override void Write(string message, string category)
        {
            if (category == null || category.Length == 0)
            {
                Console.Write(message);
            }
            else
            {
                Console.Write(category + ": " + message);
            }
        }

        public static bool EnsureConsole()
        {
            return AllocConsole();
        }

        public static bool AttachParentProcessConsole()
        {
            return AttachConsole(-1);
        }

        public static bool AttachConsole(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            return AttachConsole(process.Id);
        }

        public static bool CloseConsole()
        {
            return FreeConsole();
        }

        [DllImport("kernel32.dll")]
        private extern static bool AllocConsole();

        [DllImport("kernel32.dll")]
        private extern static bool FreeConsole();

        [DllImport("kernel32.dll")]
        private extern static bool AttachConsole(int processId);

        public override void WriteLine(string message, string category)
        {
            Write(message + Environment.NewLine, category);
        }
    }
}
