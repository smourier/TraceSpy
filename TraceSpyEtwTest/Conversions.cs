using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace TraceSpyEtwTest;

public static class Conversions
{
    private static readonly char[] _enumSeparators = [',', ';', '+', '|', ' '];

    public static Version ToVersion(string? version)
    {
        version = version.Nullify();
        if (version == null)
            return new Version(0, 0);

        if (!Version.TryParse(version, out var v))
            return new Version(0, 0);

        return v;
    }

    public static Type? GetEnumeratedType(Type collectionType)
    {
        ArgumentNullException.ThrowIfNull(collectionType);
        var etype = GetEnumeratedItemType(collectionType);
        if (etype != null)
            return etype;

        foreach (var type in collectionType.GetInterfaces())
        {
            etype = GetEnumeratedItemType(type);
            if (etype != null)
                return etype;
        }
        return null;
    }

    private static Type? GetEnumeratedItemType(Type type)
    {
        if (!type.IsGenericType)
            return null;

        if (type.GetGenericArguments().Length != 1)
            return null;

        if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(ICollection<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IList<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(ISet<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IReadOnlySet<>))
            return type.GetGenericArguments()[0];

        if (type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            return type.GetGenericArguments()[0];

        return null;
    }

    public static IList<T> SplitToList<T>(this string? text, char[] separators, IFormatProvider? provider = null, int count = int.MaxValue, StringSplitOptions options = StringSplitOptions.None)
    {
        var list = new List<T>();
        if (!string.IsNullOrEmpty(text))
        {
            foreach (var str in text.Split(separators, count, options))
            {
                if (TryChangeType<T>(str, provider, out var value) && value != null)
                {
                    list.Add(value);
                }
            }
        }
        return list;
    }

    public static string? GetNullifiedValue(this IDictionary<string, object?> dictionary, string key, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (dictionary == null)
            return null;

        if (!dictionary.TryGetValue(key, out var o))
            return null;

        if (o == null || Convert.IsDBNull(o))
            return null;

        return ChangeType<string>(o, null, provider).Nullify();
    }

    public static T? GetValue<T>(this IDictionary<string, object?> dictionary, string key, T? defaultValue = default, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (dictionary == null)
            return defaultValue;

        if (!dictionary.TryGetValue(key, out var o))
            return defaultValue;

        return ChangeType(o, defaultValue, provider);
    }

    public static string UrlCombine(params string[]? urls)
    {
        ArgumentNullException.ThrowIfNull(urls);
        if (urls.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url))
                continue;

            if (sb.Length > 0)
            {
                if (sb[^1] == '/')
                {
                    if (url[0] == '/')
                    {
                        sb.Append(url.AsSpan(1));
                    }
                    else
                    {
                        sb.Append(url);
                    }
                }
                else
                {
                    if (url[0] != '/')
                    {
                        sb.Append('/');
                    }
                    sb.Append(url);
                }
            }
            else
            {
                sb.Append(url);
            }
        }
        return sb.ToString();
    }

    public static string? JoinExceptionMessagesWithDots(this IEnumerable<string> messages)
    {
        if (messages == null)
            return null;

        return string.Join(".", messages).Nullify()?.Replace(".. ", ". ");
    }

    [return: NotNullIfNotNull(nameof(exception))]
    public static string? GetAllMessagesWithDots(this Exception? exception) => exception.GetAllMessages(".")?.Replace(".. ", ". ");

    [return: NotNullIfNotNull(nameof(exception))]
    public static string? GetAllMessages(this Exception? exception) => exception.GetAllMessages(Environment.NewLine);

    [return: NotNullIfNotNull(nameof(exception))]
    public static string? GetAllMessages(this Exception? exception, string? separator)
    {
        if (exception == null)
            return null;

        separator ??= Environment.NewLine;
        return string.Join(separator, EnumerateAllExceptionsMessages(exception).Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public static IEnumerable<Exception> EnumerateAllExceptions(this Exception? exception)
    {
        if (exception == null)
            yield break;

        if (exception is AggregateException agg)
        {
            foreach (var ae in agg.InnerExceptions)
            {
                foreach (var child in EnumerateAllExceptions(ae))
                {
                    yield return child;
                }
            }
        }
        else
        {
            if (exception is not TargetInvocationException) // useless
                yield return exception;

            foreach (var child in EnumerateAllExceptions(exception.InnerException))
            {
                yield return child;
            }
        }
    }

    public static IEnumerable<string> EnumerateAllExceptionsMessages(this Exception? exception)
    {
        foreach (var e in EnumerateAllExceptions(exception))
        {
            var msg = GetExceptionMessage(e);
            if (msg != null)
                yield return msg;
        }
    }

    public static IEnumerable<string> GetExceptionsMessages(this IEnumerable<Exception> exceptions)
    {
        ArgumentNullException.ThrowIfNull(exceptions);
        foreach (var exception in exceptions)
        {
            var msg = GetExceptionMessage(exception);
            if (msg != null)
                yield return msg;
        }
    }

    public static string? GetExceptionMessage(this Exception? exception)
    {
        if (exception == null)
            return null;

        var typeName = GetExceptionTypeName(exception);
        string? message;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            message = norm(exception.Message);
        }
        else
        {
            message = norm(exception.Message);
            if (message == null)
                return null;

            message = typeName + ": " + message;
        }
        return message;

        static string? norm(string? msg)
        {
            msg = msg.Nullify();
            if (msg == null)
                return null;

            if (!msg.EndsWith('.'))
            {
                msg += ".";
            }
            return msg;
        }
    }

    private static string? GetExceptionTypeName(Exception exception)
    {
        if (exception == null)
            return null;

        var type = exception.GetType();
        if (type == null || string.IsNullOrWhiteSpace(type.FullName))
            return null;

        if (type.FullName.StartsWith("System.") ||
            type.FullName.StartsWith("Microsoft."))
            return null;

        return type.FullName;
    }

    public static string? ToHexa(this byte[] bytes) => ToHexa(bytes, 0, (bytes?.Length).GetValueOrDefault());
    public static string? ToHexa(this byte[] bytes, int count) => ToHexa(bytes, 0, count);
    public static string? ToHexa(this byte[] bytes, int offset, int count)
    {
        if (bytes == null)
            return null;

        if (offset < 0)
            throw new ArgumentException(null, nameof(offset));

        if (count < 0)
            throw new ArgumentException(null, nameof(count));

        if (offset > bytes.Length)
            throw new ArgumentException(null, nameof(offset));

        count = Math.Min(count, bytes.Length - offset);
        var sb = new StringBuilder(count * 2);
        for (var i = offset; i < (offset + count); i++)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", bytes[i]);
        }
        return sb.ToString();
    }

    public static bool TryParseEnum(Type type, object input, out object? value)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!type.IsEnum)
            throw new ArgumentException(null, nameof(type));

        if (input == null || Convert.IsDBNull(input))
        {
            value = Activator.CreateInstance(type);
            return false;
        }

        var stringInput = string.Format(CultureInfo.InvariantCulture, "{0}", input);
        stringInput = stringInput.Nullify();
        if (stringInput == null)
        {
            value = Activator.CreateInstance(type);
            return false;
        }

        if (stringInput.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(stringInput[2..], NumberStyles.HexNumber, null, out ulong ulx))
        {
            value = ToEnum(ulx.ToString(CultureInfo.InvariantCulture), type);
            return true;
        }

        var names = Enum.GetNames(type);
        if (names.Length == 0)
        {
            value = Activator.CreateInstance(type);
            return false;
        }

        var values = Enum.GetValues(type);
        // some enums like System.CodeDom.MemberAttributes *are* flags but are not declared with Flags...
        if (!type.IsDefined(typeof(FlagsAttribute), true) && stringInput.IndexOfAny(_enumSeparators) < 0)
            return StringToEnum(type, names, values, stringInput, out value);

        // multi value enum
        var tokens = stringInput.Split(_enumSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            value = Activator.CreateInstance(type);
            return false;
        }

        ulong ul = 0;
        foreach (var tok in tokens)
        {
            var token = tok.Nullify(); // NOTE: we don't consider empty tokens as errors
            if (token == null)
                continue;

            if (!StringToEnum(type, names, values, token, out var tokenValue))
            {
                value = Activator.CreateInstance(type);
                return false;
            }

            var tokenUl = Convert.GetTypeCode(tokenValue) switch
            {
                TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.SByte => (ulong)Convert.ToInt64(tokenValue, CultureInfo.InvariantCulture),
                _ => Convert.ToUInt64(tokenValue, CultureInfo.InvariantCulture),
            };
            ul |= tokenUl;
        }
        value = Enum.ToObject(type, ul);
        return true;
    }

    public static object? ToEnum(string text, Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);
        TryParseEnum(enumType, text, out var value);
        return value;
    }

    private static bool StringToEnum(Type type, string[] names, Array values, string input, out object? value)
    {
        for (var i = 0; i < names.Length; i++)
        {
            if (names[i].EqualsIgnoreCase(input))
            {
                value = values.GetValue(i);
                return true;
            }
        }

        for (var i = 0; i < values.GetLength(0); i++)
        {
            var valuei = values.GetValue(i)!;
            if (input.Length > 0 && input[0] == '-')
            {
                var ul = (long)EnumToUInt64(valuei);
                if (ul.ToString().EqualsIgnoreCase(input))
                {
                    value = valuei;
                    return true;
                }
            }
            else
            {
                var ul = EnumToUInt64(valuei);
                if (ul.ToString().EqualsIgnoreCase(input))
                {
                    value = valuei;
                    return true;
                }
            }
        }

        if (char.IsDigit(input[0]) || input[0] == '-' || input[0] == '+')
        {
            var obj = EnumToObject(type, input);
            if (obj == null)
            {
                value = Activator.CreateInstance(type);
                return false;
            }
            value = obj;
            return true;
        }

        value = Activator.CreateInstance(type);
        return false;
    }

    public static object EnumToObject(Type enumType, object value)
    {
        ArgumentNullException.ThrowIfNull(enumType);
        ArgumentNullException.ThrowIfNull(value);
        if (!enumType.IsEnum)
            throw new ArgumentException(null, nameof(enumType));

        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (underlyingType == typeof(long))
            return Enum.ToObject(enumType, ChangeType<long>(value));

        if (underlyingType == typeof(ulong))
            return Enum.ToObject(enumType, ChangeType<ulong>(value));

        if (underlyingType == typeof(int))
            return Enum.ToObject(enumType, ChangeType<int>(value));

        if ((underlyingType == typeof(uint)))
            return Enum.ToObject(enumType, ChangeType<uint>(value));

        if (underlyingType == typeof(short))
            return Enum.ToObject(enumType, ChangeType<short>(value));

        if (underlyingType == typeof(ushort))
            return Enum.ToObject(enumType, ChangeType<ushort>(value));

        if (underlyingType == typeof(byte))
            return Enum.ToObject(enumType, ChangeType<byte>(value));

        if (underlyingType == typeof(sbyte))
            return Enum.ToObject(enumType, ChangeType<sbyte>(value));

        throw new ArgumentException(null, nameof(enumType));
    }

    public static ulong EnumToUInt64(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var typeCode = Convert.GetTypeCode(value);
        return typeCode switch
        {
            TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture),
            TypeCode.Byte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            _ => ChangeType<ulong>(value, 0, CultureInfo.InvariantCulture),
        };
    }

    public static object? ChangeType(object? input, Type conversionType, object? defaultValue = null, IFormatProvider? provider = null)
    {
        if (!TryChangeType(input, conversionType, provider, out object? value))
            return defaultValue;

        return value;
    }

    public static T? ChangeType<T>(object? input, T? defaultValue = default, IFormatProvider? provider = null)
    {
        if (!TryChangeType(input, provider, out T? value))
            return defaultValue;

        return value;
    }

    public static bool TryChangeType<T>(object? input, out T? value) => TryChangeType(input, null, out value);
    public static bool TryChangeType<T>(object? input, IFormatProvider? provider, out T? value)
    {
        if (!TryChangeType(input, typeof(T), provider, out object? tvalue))
        {
            value = default;
            return false;
        }

        value = (T)tvalue!;
        return true;
    }

    public static bool TryChangeType(object? input, Type conversionType, out object? value) => TryChangeType(input, conversionType, null, out value);
    public static bool TryChangeType(object? input, Type conversionType, IFormatProvider? provider, out object? value)
    {
        ArgumentNullException.ThrowIfNull(conversionType);
        if (conversionType == typeof(object))
        {
            value = input;
            return true;
        }

        var nullable = conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>);
        value = conversionType.IsValueType ? Activator.CreateInstance(conversionType) : null;
        if (input == null || Convert.IsDBNull(input))
            return !conversionType.IsValueType || nullable;

        var inputType = input.GetType();
        if (conversionType.IsAssignableFrom(inputType))
        {
            value = input;
            return true;
        }

        if (input is JsonElement element)
        {
            if (TryConvertToObject(element, out var jvalue))
                return TryChangeType(jvalue, conversionType, provider, out value);

            return false;
        }

        if (conversionType.IsEnum)
            return TryParseEnum(conversionType, input, out value);

        if (conversionType == typeof(Guid))
        {
            var svalue = string.Format(provider, "{0}", input).Nullify();
            if (svalue != null && Guid.TryParse(svalue, out Guid guid))
            {
                value = guid;
                return true;
            }
            return false;
        }

        if (conversionType == typeof(Type))
        {
            var typeName = string.Format(provider, "{0}", input).Nullify();
            if (typeName == null)
                return false;

            var type = Type.GetType(typeName, false);
            if (type == null)
                return false;

            value = type;
            return true;
        }

        if (conversionType == typeof(IntPtr))
        {
            if (IntPtr.Size == 8 && TryChangeType(input, provider, out long l))
            {
                value = new IntPtr(l);
                return true;
            }
            return false;
        }

        if (conversionType == typeof(int))
        {
            if (inputType == typeof(uint))
            {
                value = unchecked((int)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = unchecked((int)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = unchecked((int)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = unchecked((int)(byte)input);
                return true;
            }
        }

        if (conversionType == typeof(long))
        {
            if (inputType == typeof(uint))
            {
                value = unchecked((long)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = unchecked((long)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = unchecked((long)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = unchecked((long)(byte)input);
                return true;
            }
        }

        if (conversionType == typeof(short))
        {
            if (inputType == typeof(uint))
            {
                value = unchecked((short)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = unchecked((short)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = unchecked((short)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = unchecked((short)(byte)input);
                return true;
            }
        }

        if (conversionType == typeof(sbyte))
        {
            if (inputType == typeof(uint))
            {
                value = unchecked((sbyte)(uint)input);
                return true;
            }

            if (inputType == typeof(ulong))
            {
                value = unchecked((sbyte)(ulong)input);
                return true;
            }

            if (inputType == typeof(ushort))
            {
                value = unchecked((sbyte)(ushort)input);
                return true;
            }

            if (inputType == typeof(byte))
            {
                value = unchecked((sbyte)(byte)input);
                return true;
            }
        }

        if (conversionType == typeof(uint))
        {
            if (inputType == typeof(int))
            {
                value = unchecked((uint)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = unchecked((uint)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = unchecked((uint)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = unchecked((uint)(sbyte)input);
                return true;
            }
        }

        if (conversionType == typeof(ulong))
        {
            if (inputType == typeof(int))
            {
                value = unchecked((ulong)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = unchecked((ulong)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = unchecked((ulong)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = unchecked((ulong)(sbyte)input);
                return true;
            }
        }

        if (conversionType == typeof(ushort))
        {
            if (inputType == typeof(int))
            {
                value = unchecked((ushort)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = unchecked((ushort)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = unchecked((ushort)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = unchecked((ushort)(sbyte)input);
                return true;
            }
        }

        if (conversionType == typeof(byte))
        {
            if (inputType == typeof(int))
            {
                value = unchecked((byte)(int)input);
                return true;
            }

            if (inputType == typeof(long))
            {
                value = unchecked((byte)(long)input);
                return true;
            }

            if (inputType == typeof(short))
            {
                value = unchecked((byte)(short)input);
                return true;
            }

            if (inputType == typeof(sbyte))
            {
                value = unchecked((byte)(sbyte)input);
                return true;
            }
        }

        if (conversionType == typeof(bool))
        {
            if (inputType == typeof(string))
            {
                var sinput = (string)input;
                if (string.IsNullOrWhiteSpace(sinput) || sinput.EqualsIgnoreCase("false"))
                {
                    value = false;
                    return true;
                }

                if (sinput.EqualsIgnoreCase("true"))
                {
                    value = true;
                    return true;
                }

                if (TryChangeType<long>(input, out var l))
                {
                    value = l != 0;
                    return true;
                }
            }
        }

        if (conversionType == typeof(DateTime))
        {
            if (input is DateTimeOffset dto)
            {
                value = dto.DateTime;
                return true;
            }

            if (input is double dbl)
            {
                try
                {
                    value = DateTime.FromOADate(dbl);
                    return true;
                }
                catch
                {
                    value = DateTime.MinValue;
                    return false;
                }
            }
        }

        if (conversionType == typeof(DateTimeOffset))
        {
            if (input is DateTime dta)
            {
                value = new DateTimeOffset(dta);
                return true;
            }

            if (input is double dbl2)
            {
                try
                {
                    value = new DateTimeOffset(DateTime.FromOADate(dbl2));
                    return true;
                }
                catch
                {
                    value = DateTimeOffset.MinValue;
                    return false;
                }
            }
        }

        if (conversionType == typeof(TimeSpan))
        {
            if (TryChangeType<long>(input, out var l))
            {
                try
                {
                    value = TimeSpan.FromTicks(l);
                    return true;
                }
                catch
                {
                    // do nothing
                }
            }
            else if (TryChangeType<string>(input, out var str) && !string.IsNullOrEmpty(str) && TimeSpan.TryParse(str, provider, out var ts))
            {
                value = ts;
                return true;
            }
            value = TimeSpan.Zero;
            return false;
        }

        if (nullable)
        {
            if (input == null || Convert.IsDBNull(input) || string.Empty.Equals(input))
            {
                value = null;
                return true;
            }

            var type = conversionType.GetGenericArguments()[0];
            if (TryChangeType(input, type, provider, out var vtValue))
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(type);
                value = Activator.CreateInstance(nullableType, vtValue);
                return true;
            }

            value = null;
            return false;
        }

        if (input is IConvertible convertible)
        {
            try
            {
                value = convertible.ToType(conversionType, provider);
                return true;
            }
            catch
            {
                return false;
            }
        }

        if (conversionType == typeof(byte[]))
        {
            if (input is int i)
            {
                value = BitConverter.GetBytes(i);
                return true;
            }

            if (input is long l)
            {
                value = BitConverter.GetBytes(l);
                return true;
            }

            if (input is short s)
            {
                value = BitConverter.GetBytes(s);
                return true;
            }

            if (input is uint ui)
            {
                value = BitConverter.GetBytes(ui);
                return true;
            }

            if (input is ulong ul)
            {
                value = BitConverter.GetBytes(ul);
                return true;
            }

            if (input is ushort us)
            {
                value = BitConverter.GetBytes(us);
                return true;
            }

            if (input is bool b)
            {
                value = BitConverter.GetBytes(b);
                return true;
            }

            if (input is string str)
            {
                try
                {
                    value = Convert.FromBase64String(str);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (input is Guid g)
            {
                value = g.ToByteArray();
                return true;
            }

            if (input is byte bb)
            {
                value = new byte[] { bb };
                return true;
            }

            if (input is sbyte sb)
            {
                value = new byte[] { (byte)sb };
                return true;
            }

            if (input is char c)
            {
                value = BitConverter.GetBytes(c);
                return true;
            }

            if (input is double dbl)
            {
                value = BitConverter.GetBytes(dbl);
                return true;
            }

            if (input is float flt)
            {
                value = BitConverter.GetBytes(flt);
                return true;
            }

            return false;
        }

        if (conversionType == typeof(string))
        {
            if (input is byte[] bytes)
            {
                value = ToHexa(bytes);
                return true;
            }

            value = string.Format(provider, "{0}", input);
            return true;
        }

        return false;
    }

    public static T? ConvertToObject<T>(this JsonElement element, T? defaultValue = default)
    {
        if (!TryConvertToObject<T>(element, out var value))
            return defaultValue;

        return value;
    }

    public static object? ConvertToObject(this JsonElement element, object? defaultValue)
    {
        if (!TryConvertToObject(element, out var value))
            return defaultValue;

        return value;
    }

    public static bool TryConvertToObject<T>(this JsonElement element, out T? value)
    {
        if (!TryConvertToObject(element, out var cvalue))
        {
            value = default;
            return false;
        }

        return TryChangeType(cvalue, out value);
    }

    public static bool TryConvertToObject(this JsonElement element, out object? value)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                value = null;
                return true;

            case JsonValueKind.Object:
                var dic = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var child in element.EnumerateObject())
                {
                    if (!TryConvertToObject(child.Value, out var childValue))
                    {
                        value = null;
                        return false;
                    }

                    dic[child.Name] = childValue;
                }

                value = dic;
                return true;

            case JsonValueKind.Array:
                var objects = new object?[element.GetArrayLength()];
                var i = 0;
                foreach (var child in element.EnumerateArray())
                {
                    if (!TryConvertToObject(child, out var childValue))
                    {
                        value = null;
                        return false;
                    }

                    objects[i++] = childValue;
                }

                value = objects;
                return true;

            case JsonValueKind.String:
                var str = element.ToString();
                if (DateTime.TryParseExact(str, ["o", "r", "s"], null, DateTimeStyles.None, out var dt))
                {
                    value = dt;
                    return true;
                }

                value = str;
                return true;

            case JsonValueKind.Number:
                if (element.TryGetInt32(out var i32))
                {
                    value = i32;
                    return true;
                }

                if (element.TryGetInt32(out var i64))
                {
                    value = i64;
                    return true;
                }

                if (element.TryGetDecimal(out var dec))
                {
                    value = dec;
                    return true;
                }

                if (element.TryGetDouble(out var dbl))
                {
                    value = dbl;
                    return true;
                }
                break;

            case JsonValueKind.True:
                value = true;
                return true;

            case JsonValueKind.False:
                value = false;
                return true;
        }

        value = null;
        return false;
    }
}
