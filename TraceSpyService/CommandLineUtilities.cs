using System;
using System.Collections.Generic;

namespace TraceSpyService
{
    public sealed class CommandLineUtilities
    {
        private static readonly Dictionary<string, string> _namedArguments;
        private static readonly Dictionary<int, string> _positionArguments;
        private static readonly bool _helpRequested;

        static CommandLineUtilities()
        {
            _namedArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _positionArguments = new Dictionary<int, string>();

            string[] args = Environment.GetCommandLineArgs();
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                    continue;

                string arg = args[i].Nullify();
                if (arg == null)
                    continue;

                string upper = arg.ToUpperInvariant();
                if (arg == "/?" || arg == "-?" || upper == "/HELP" || upper == "-HELP")
                {
                    _helpRequested = true;
                }

                bool named = false;
                if (arg[0] == '-' || arg[0] == '/')
                {
                    arg = arg.Substring(1);
                    named = true;
                }

                string name;
                string value;
                int pos = arg.IndexOf(':');
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

        private CommandLineUtilities()
        {
        }

        public static IDictionary<string, string> NamedArguments
        {
            get
            {
                return _namedArguments;
            }
        }

        public static IDictionary<int, string> PositionArguments
        {
            get
            {
                return _positionArguments;
            }
        }

        public static bool HelpRequested
        {
            get
            {
                return _helpRequested;
            }
        }

        public static T GetArgument<T>(IEnumerable<string> arguments, string name, T defaultValue)
        {
            if (arguments == null)
                return defaultValue;

            foreach (string arg in arguments)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    int pos = arg.IndexOfAny(new[] { '=', ':' }, 1);
                    string argName = pos < 0 ? arg.Substring(1) : arg.Substring(1, pos - 1);
                    if (string.Compare(name, argName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        string value = pos < 0 ? string.Empty : arg.Substring(pos + 1).Trim();
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

        public static T GetArgument<T>(int index, T defaultValue)
        {
            return GetArgument(index, defaultValue, null);
        }

        public static T GetArgument<T>(int index, T defaultValue, IFormatProvider provider)
        {
            string s;
            if (!_positionArguments.TryGetValue(index, out s))
                return defaultValue;

            return Extensions.ChangeType(s, defaultValue, provider);
        }

        public static object GetArgument(int index, object defaultValue, Type conversionType)
        {
            return GetArgument(index, defaultValue, conversionType, null);
        }

        public static object GetArgument(int index, object defaultValue, Type conversionType, IFormatProvider provider)
        {
            string s;
            if (!_positionArguments.TryGetValue(index, out s))
                return defaultValue;

            return Extensions.ChangeType(s, conversionType, defaultValue, provider);
        }

        public static T GetArgument<T>(string name, T defaultValue)
        {
            return GetArgument(name, defaultValue, null);
        }

        public static T GetArgument<T>(string name, T defaultValue, IFormatProvider provider)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            string s;
            if (!_namedArguments.TryGetValue(name, out s))
                return defaultValue;

            if (typeof(T) == typeof(bool) && string.IsNullOrEmpty(s))
                return (T)(object)true;

            return Extensions.ChangeType(s, defaultValue, provider);
        }

        public static bool HasArgument(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            string s;
            return _namedArguments.TryGetValue(name, out s);
        }

        public static object GetArgument(string name, object defaultValue, Type conversionType)
        {
            return GetArgument(name, defaultValue, conversionType, null);
        }

        public static object GetArgument(string name, object defaultValue, Type conversionType, IFormatProvider provider)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (conversionType == null)
                throw new ArgumentNullException("conversionType");

            string s;
            if (!_namedArguments.TryGetValue(name, out s))
                return defaultValue;

            if (conversionType == typeof(bool) && string.IsNullOrEmpty(s))
                return true;

            return Extensions.ChangeType(s, conversionType, defaultValue, provider);
        }
    }
}
