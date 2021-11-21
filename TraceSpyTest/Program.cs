using System;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.Threading;
using TraceSpyService;

namespace TraceSpyTest
{
    public class Program
    {
        private static EventProvider _etw;
        private static Timer _timer;
        private static readonly Random _rnd = new Random(Environment.TickCount);

        private static void Main()
        {
            if (Debugger.IsAttached)
            {
                SafeMain();
            }
            else
            {
                try
                {
                    SafeMain();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static void SafeMain()
        {
            var count = 1;
            string t;
            var etwProvider = CommandLineUtilities.GetArgument("etw", Guid.Empty);
            Console.WriteLine("TraceSpy Test.");
            Console.WriteLine();
            Console.WriteLine("Press Q, ESC, or CTRL-C to quit");
            Console.WriteLine();
            Console.WriteLine("OutputDebugString");
            Console.WriteLine(" Press O to send an OutputDebugString trace.");
            Console.WriteLine(" Press C to send an OutputDebugString trace every random interval, C again to stop.");

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
                var key = (int)info.Key;
                var num = GetFinalNumber(info);
                switch (info.Key)
                {
                    case ConsoleKey.Escape:
                        return;

                    case ConsoleKey.Q:
                        if (_etw != null)
                        {
                            _etw.Dispose();
                            _etw = null;
                        }
                        return;

                    case ConsoleKey.O:
                        t = "ODSTrace #" + count + " from TraceSpyTest. Date:" + DateTime.Now;
                        Console.WriteLine("Sending: '" + t + "'");
                        Trace.WriteLine(t);
                        count++;
                        break;

                    case ConsoleKey.C:
                        if (_timer == null)
                        {
                            _timer = new Timer((state) =>
                            {
                                t = "ODS Continuous Trace #" + count + " from TraceSpyTest. Date:" + DateTime.Now + ". " + GetSomeRandomText();
                                Console.WriteLine("Sending: '" + t + "'");
                                Trace.WriteLine(t);
                                count++;
                                _timer.Change(_rnd.Next(100, 2000), 0);
                            });
                            _timer.Change(_rnd.Next(100, 2000), 0);
                        }
                        else
                        {
                            _timer.Dispose();
                            _timer = null;
                        }
                        break;

                    case ConsoleKey.E:
                        if (_etw != null)
                        {
                            t = "ETWTrace #" + count + " from TraceSpyTest. Date:" + DateTime.Now;
                            Console.WriteLine("Sending: '" + t + "'");
                            _etw.WriteMessageEvent(t);
                            count++;
                        }
                        break;

                    default:
                        if (num > 0)
                        {
                            for (var i = 1; i <= num; i++)
                            {
                                t = "Trace #" + count + ", " + i + "/" + num + " from TraceSpyTest. Date:" + DateTime.Now;
                                if (num < 1000 || (i % 1000) == 0)
                                {
                                    Console.WriteLine("Sending: '" + t + "'");
                                }

                                if (_etw != null)
                                {
                                    _etw.WriteMessageEvent(t);
                                }
                                else
                                {
                                    Trace.WriteLine(t);
                                }
                                count++;
                            }
                        }
                        break;
                }
            }
            while (true);
        }

        private static string GetSomeRandomText()
        {
            const string line = "The quick brown fox jumps over the lazy dog";
            string s = null;
            var numLines = _rnd.Next(5);
            for (var i = 0; i < numLines; i++)
            {
                var l = line.Substring(0, line.Length - _rnd.Next(0, line.Length / 2));
                if (s != null)
                {
                    s += Environment.NewLine;
                    for (var j = 0; j < _rnd.Next(3); j++)
                    {
                        s += "\t";
                    }
                }
                s += l;
            }
            return s;
        }

        private static int GetNumber(ConsoleKey key)
        {
            var k = (int)key;
            if (k > (int)ConsoleKey.D0 && k <= (int)ConsoleKey.D9)
                return k - (int)ConsoleKey.D0;

            if (k > (int)ConsoleKey.NumPad0 && k <= (int)ConsoleKey.NumPad9)
                return k - (int)ConsoleKey.NumPad0;

            return -1;
        }

        private static int GetFinalNumber(ConsoleKeyInfo info)
        {
            var k = GetNumber(info.Key);
            if ((info.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
            {
                k *= 10;
            }

            if ((info.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                k *= 100;
            }

            if ((info.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
            {
                k *= 1000;
            }
            return k;
        }
    }
}
