using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace TraceSpyService
{
    public static class Extensions
    {
        static Extensions()
        {
            AssemblyDate = File.GetLastWriteTimeUtc(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);
        }

        public static DateTime AssemblyDate { get; private set; }

        public static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass, out TokenElevationType tokenInformation, int tokenInformationLength, out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        private const int TOKEN_QUERY = 8;
        private const int TokenElevationTypeInformation = 18;

        public static TokenElevationType GetTokenElevationType()
        {
            TokenElevationType type = TokenElevationType.Unknown;
            int len = IntPtr.Size;
            IntPtr h;
            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, out h))
                return type;

            try
            {
                int olen;
                GetTokenInformation(h, TokenElevationTypeInformation, out type, len, out olen);
                return type;
            }
            finally
            {
                CloseHandle(h);
            }
        }

        public static T ChangeType<T>(object value, T defaultValue)
        {
            return ChangeType(value, defaultValue, null);
        }

        public static T ChangeType<T>(object value, T defaultValue, IFormatProvider provider)
        {
            return (T)ChangeType(value, typeof(T), defaultValue, provider);
        }

        public static object ChangeType(object value, Type conversionType, object defaultValue, IFormatProvider provider)
        {
            if (conversionType == typeof(Guid))
            {
                Guid g;
                if (!Guid.TryParse(string.Format("{0}", value), out g))
                    return defaultValue;

                return g;
            }

            try
            {
                return Convert.ChangeType(value, conversionType, provider);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string Nullify(this string text)
        {
            return Nullify(text, true);
        }

        public static string Nullify(this string text, bool trim)
        {
            if (text == null)
                return text;

            string strim = text.Trim();
            if (strim.Length == 0)
                return null;

            if (trim)
                return strim;

            return text;
        }

        public static string FormatFileSize(long size)
        {
            return FormatFileSize(size, null, null, null);
        }

        public static string FormatFileSize(long size, string byteName, string numberFormat, IFormatProvider formatProvider)
        {
            if (size < 0)
                throw new ArgumentException(null, "size");

            if (byteName == null)
            {
                byteName = "B";
            }

            if (string.IsNullOrEmpty(numberFormat))
            {
                numberFormat = "N2";
            }

            const decimal K = 1024;
            const decimal M = K * K;
            const decimal G = M * K;
            const decimal T = G * K;
            const decimal P = T * K;
            const decimal E = P * K;

            decimal dsize = size;

            string suffix = null;
            if (dsize >= E)
            {
                dsize /= E;
                suffix = "E";
            }
            else if (dsize >= P)
            {
                dsize /= P;
                suffix = "P";
            }
            else if (dsize >= T)
            {
                dsize /= T;
                suffix = "T";
            }
            else if (dsize >= G)
            {
                dsize /= G;
                suffix = "G";
            }
            else if (dsize >= M)
            {
                dsize /= M;
                suffix = "M";
            }
            else if (dsize >= K)
            {
                dsize /= K;
                suffix = "k";
            }
            if (suffix != null)
            {
                suffix = " " + suffix;
            }
            return string.Format(formatProvider, "{0:" + numberFormat + "}" + suffix + byteName, dsize);
        }
        public static string GetConfiguration(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            object[] atts = assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if (atts != null && atts.Length > 0)
                return ((AssemblyConfigurationAttribute)atts[0]).Configuration;

            return null;
        }

        public static string GetInformationalVersion(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            object[] atts = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (atts != null && atts.Length > 0)
                return ((AssemblyInformationalVersionAttribute)atts[0]).InformationalVersion;

            return null;
        }

        public static void SetCurrentThreadCulture(string name)
        {
            SetCurrentThreadCulture(name, false);
        }

        public static void SetCurrentThreadCulture(string name, bool throwOnError)
        {
            if (name == null)
            {
                name = string.Empty; // invariant culture
            }

            try
            {
                CultureInfo culture;
                int lcid;
                if (int.TryParse(name, out lcid))
                {
                    culture = new CultureInfo(lcid);
                }
                else
                {
                    culture = new CultureInfo(name);
                }

                Thread.CurrentThread.CurrentUICulture = culture;

                if (!culture.IsNeutralCulture)
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                }
                else if (culture.TextInfo != null)
                {
                    culture = new CultureInfo(culture.TextInfo.LCID);
                    if (!culture.IsNeutralCulture)
                    {
                        Thread.CurrentThread.CurrentCulture = culture;
                    }
                }
            }
            catch
            {
                if (throwOnError)
                    throw;
            }
        }
    }
}
