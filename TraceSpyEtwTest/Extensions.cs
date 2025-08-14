using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TraceSpyEtwTest;

public static class Extensions
{
    public static bool EqualsIgnoreCase(this string? thisString, string? text, bool trim = false)
    {
        if (trim)
        {
            thisString = thisString.Nullify();
            text = text.Nullify();
        }

        if (thisString == null)
            return text == null;

        if (text == null)
            return false;

        if (thisString.Length != text.Length)
            return false;

        return string.Compare(thisString, text, StringComparison.OrdinalIgnoreCase) == 0;
    }

    public static string? Nullify(this string? text)
    {
        if (text == null)
            return null;

        if (string.IsNullOrWhiteSpace(text))
            return null;

        var t = text.Trim();
        return t.Length == 0 ? null : t;
    }

    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (source != null)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                    return index;

                index++;
            }
        }
        return -1;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?>? source) where T : class
    {
        if (source == null)
            return [];

        return source.Where(item => item != null)!;
    }

    public static int AddRange<T>(this ICollection<T>? collection, IEnumerable<T>? items)
    {
        if (collection == null || items == null)
            return 0;

        var count = 0;
        foreach (var item in items)
        {
            collection.Add(item);
        }
        return count;
    }

    [return: NotNullIfNotNull(nameof(text))]
    public static string? RemoveDiacritics(this string? text)
    {
        if (text == null)
            return text;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        for (var i = 0; i < normalized.Length; i++)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(normalized[i]);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(normalized[i]);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static void UpdateWith<T>(this IList<T>? list, IEnumerable<T>? items, Func<T, T, bool> compare, Action<T, T> update)
    {
        ArgumentNullException.ThrowIfNull(compare);
        ArgumentNullException.ThrowIfNull(update);
        if (list == null || items == null)
            return;

        var removed = list.ToHashSet(); // copy
        foreach (var item in items)
        {
            removed.RemoveWhere(i => compare(i, item));
            var existing = list.FirstOrDefault(i => compare(i, item));
            if (existing != null)
            {
                update(existing, item);
                continue;
            }

            list.Add(item);
        }

        foreach (var item in removed)
        {
            list.Remove(item);
        }
    }
}
