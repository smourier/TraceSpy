using System;
using System.Collections.Generic;

namespace TraceSpyService
{
    public static class CommandLineUtilities
    {
        private static readonly Dictionary<string, string> _namedArguments;
        private static readonly Dictionary<int, string> _positionArguments;

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static CommandLineUtilities()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            _namedArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _positionArguments = new Dictionary<int, string>();

            var args = Environment.GetCommandLineArgs();
            if (args == null)
                return;

            for (var i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    continue;

                var arg = args[i].Nullify();
                if (arg == null)
                    continue;

                var upper = arg.ToUpperInvariant();
                if (arg == "/?" || arg == "-?" || upper == "/HELP" || upper == "-HELP")
                {
                    HelpRequested = true;
                }

                var named = false;
                if (arg[0] == '-' || arg[0] == '/')
                {
                    arg = arg.Substring(1);
                    named = true;
                }

                string name;
                string value;
                var pos = arg.IndexOf(':');
                if (pos < 0)
                {
                    name = arg;
                    value = null;
                }
                else
                {
                    name = arg.Substring(0, pos).Trim();
                    value = arg.Substring(pos + 1).Trim();
                }
                _positionArguments[i - 1] = arg;

                if (named)
                {
                    _namedArguments[name] = value;
                }
            }
        }

        public static IDictionary<string, string> NamedArguments => _namedArguments;
        public static IDictionary<int, string> PositionArguments => _positionArguments;
        public static bool HelpRequested { get; private set; }

        public static T GetArgument<T>(IEnumerable<string> arguments, string name, T defaultValue)
        {
            if (arguments == null)
                return defaultValue;

            foreach (var arg in arguments)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    var pos = arg.IndexOfAny(new[] { '=', ':' }, 1);
                    var argName = pos < 0 ? arg.Substring(1) : arg.Substring(1, pos - 1);
                    if (string.Compare(name, argName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var value = pos < 0 ? string.Empty : arg.Substring(pos + 1).Trim();
                        if (value.Length == 0)
                        {
                            if (typeof(T) == typeof(bool)) // special case for bool args: if it's there, return true
                                return (T)(object)true;

                            return defaultValue;
                        }
                        return Extensions.ChangeType(value, defaultValue);
                    }
                }
            }
            return defaultValue;
        }

        public static T GetArgument<T>(int index, T defaultValue, IFormatProvider provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out var s))
                return defaultValue;

            return Extensions.ChangeType(s, defaultValue, provider);
        }

        public static object GetArgument(int index, object defaultValue, Type conversionType, IFormatProvider provider = null)
        {
            if (!_positionArguments.TryGetValue(index, out var s))
                return defaultValue;

            return Extensions.ChangeType(s, conversionType, defaultValue, provider);
        }

        public static T GetArgument<T>(string name, T defaultValue, IFormatProvider provider = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!_namedArguments.TryGetValue(name, out var s))
                return defaultValue;

            if (typeof(T) == typeof(bool) && string.IsNullOrEmpty(s))
                return (T)(object)true;

            return Extensions.ChangeType(s, defaultValue, provider);
        }

        public static bool HasArgument(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return _namedArguments.TryGetValue(name, out _);
        }

        public static object GetArgument(string name, object defaultValue, Type conversionType, IFormatProvider provider = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));

            if (!_namedArguments.TryGetValue(name, out var s))
                return defaultValue;

            if (conversionType == typeof(bool) && string.IsNullOrEmpty(s))
                return true;

            return Extensions.ChangeType(s, conversionType, defaultValue, provider);
        }
    }
}
