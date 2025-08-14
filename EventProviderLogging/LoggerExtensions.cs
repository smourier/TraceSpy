using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace EventProviderLogging;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA2254 // Template should be a static expression
public static class LoggerExtensions
{
    private static string? GetMessage(object value, string? methodName)
    {
        var msg = methodName != null ? "(" + methodName + "): " + value : value?.ToString();
        return "[" + Environment.CurrentManagedThreadId + "]:" + msg;
    }

    public static void LogError<T>(this ILoggable<T>? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Error, value, methodName);
    public static void LogError(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Error, value, methodName);
    public static void LogError(this ILogger? logger, Exception value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Error, GetMessage(value, methodName));
    }

    public static void LogError(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Error, GetMessage(value, methodName));
    }

    public static void LogInformation<T>(this ILoggable<T>? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Information, value, methodName);
    public static void LogInformation(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Information, value, methodName);
    public static void LogInformation(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Information, GetMessage(value, methodName));
    }

    public static void LogWarning<T>(this ILoggable<T> loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Warning, value, methodName);
    public static void LogWarning(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Warning, value, methodName);
    public static void LogWarning(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Warning, GetMessage(value, methodName));
    }

    public static void LogDebug<T>(this ILoggable<T>? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Debug, value, methodName);
    public static void LogDebug(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Debug, value, methodName);
    public static void LogDebug(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Debug, GetMessage(value, methodName));
    }

    public static void LogCritical<T>(this ILoggable<T>? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Critical, value, methodName);
    public static void LogCritical(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Critical, value, methodName);
    public static void LogCritical(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Critical, GetMessage(value, methodName));
    }

    public static void LogTrace<T>(this ILoggable<T>? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Trace, value, methodName);
    public static void LogTrace(this ILoggable? loggable, object? value, [CallerMemberName] string? methodName = null) => Log(loggable, LogLevel.Trace, value, methodName);
    public static void LogTrace(this ILogger? logger, object value, [CallerMemberName] string? methodName = null)
    {
        if (logger == null || value == null)
            return;

        logger.Log(LogLevel.Trace, GetMessage(value, methodName));
    }

    public static void Log(this ILoggable? loggable, LogLevel level, object? value, [CallerMemberName] string? methodName = null)
    {
        var logger = loggable?.Logger;
        if (logger == null || value == null)
            return;

        logger.Log(level, GetMessage(value, methodName));
    }

    public static void Log<T>(this ILoggable<T>? loggable, LogLevel level, object? value, [CallerMemberName] string? methodName = null)
    {
        var logger = loggable?.Logger;
        if (logger == null || value == null)
            return;

        logger.Log(level, GetMessage(value, methodName));
    }

    public static void LogDiags(this ILoggable? loggable, TraceLevel level, object? value, [CallerMemberName] string? methodName = null)
        => Log(loggable, FromTraceLevel(level), value, methodName);

    public static void LogDiags<T>(this ILoggable<T>? loggable, TraceLevel level, object? value, [CallerMemberName] string? methodName = null)
        => Log(loggable, FromTraceLevel(level), value, methodName);

    public static LogLevel FromTraceLevel(TraceLevel level) => level switch
    {
        TraceLevel.Off => LogLevel.None,
        TraceLevel.Error => LogLevel.Error,
        TraceLevel.Warning => LogLevel.Warning,
        TraceLevel.Verbose => LogLevel.Trace,
        _ => LogLevel.Information,
    };
}
#pragma warning restore CA2254 // Template should be a static expression
#pragma warning restore IDE0079 // Remove unnecessary suppression
