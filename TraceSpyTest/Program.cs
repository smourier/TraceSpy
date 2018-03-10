using System;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using TraceSpyService;

namespace TraceSpyTest
{
    class Program
    {
        static EventProvider _etw;

        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                SafeMain(args);
            }
            else
            {
                try
                {
                    SafeMain(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static int GetNumber(ConsoleKey key)
        {
            int k = (int)key;
            if (k > (int)ConsoleKey.D0 && k <= (int)ConsoleKey.D9)
                return k - (int)ConsoleKey.D0;

            if (k > (int)ConsoleKey.NumPad0 && k <= (int)ConsoleKey.NumPad9)
                return k - (int)ConsoleKey.NumPad0;

            return -1;
        }

        private static int GetFinalNumber(ConsoleKeyInfo info)
        {
            int k = GetNumber(info.Key);
            if ((info.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            {
                k = k * 10;
            }

            if ((info.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                k = k * 100;
            }

            if ((info.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            {
                k = k * 1000;
            }
            return k;
        }

        static void SafeMain(string[] args)
        {
            int count = 1;
            string t;
            var etwProvider = CommandLineUtilities.GetArgument("etw", Guid.Empty);
            Console.WriteLine("TraceSpy Test.");
            Console.WriteLine();
            Console.WriteLine("Press Q, ESC, or CTRL-C to quit");
            Console.WriteLine();
            Console.WriteLine("OutputDebugString");
            Console.WriteLine(" Press O to send an OutputDebugString trace.");

            if (etwProvider != Guid.Empty)
            {
                _etw = new EventProvider(etwProvider);
                Console.WriteLine();
                Console.WriteLine("ETW");
                Console.WriteLine(" Press E to send an ETW trace to provider '" + etwProvider + "'.");
                Console.WriteLine(" Press 1..9 to send 0..9 ETW traces to provider '" + etwProvider + "'.");
                Console.WriteLine("  Combine with Shift to multiply by 10");
                Console.WriteLine("  Combine with Control to multiply by 100");
                Console.WriteLine("  Combine with Alt to multiply by 1000");
            }

            Console.WriteLine();
            do
            {
                var info = Console.ReadKey(true);
                int key = (int)info.Key;
                int num = GetFinalNumber(info);
                switch (info.Key)
                {
                    case ConsoleKey.Escape:
                        return;

                    case ConsoleKey.Q:
                        if (_etw != null)
                        {
                            _etw.Dispose();
                        }
                        return;

                    case ConsoleKey.O:
                        t = "Trace #" + count + " from TraceSpyTest. Date:" + DateTime.Now;
                        Console.WriteLine("Sending: '" + t + "'");
                        Trace.WriteLine(t);
                        count++;
                        break;

                    case ConsoleKey.E:
                        t = "Trace #" + count + " from TraceSpyTest. Date:" + DateTime.Now;
                        Console.WriteLine("Sending: '" + t + "'");
                        _etw.WriteMessageEvent(t);
                        count++;
                        break;

                    default:
                        if (num > 0)
                        {
                            for (int i = 1; i <= num; i++)
                            {
                                t = "Trace #" + count + ", " + i + "/" + num + " from TraceSpyTest. Date:" + DateTime.Now;
                                if (num < 1000 || (i % 1000) == 0)
                                {
                                    Console.WriteLine("Sending: '" + t + "'");
                                }
                                _etw.WriteMessageEvent(t);
                                count++;
                            }
                        }
                        break;
                }
            }
            while (true);
        }
    }
}
