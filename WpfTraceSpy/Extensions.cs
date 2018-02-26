using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace TraceSpy
{
    internal static class Extensions
    {
        public static string GetProduct() => Assembly.GetEntryAssembly().GetProduct();
        public static string GetProduct(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            object[] atts = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (atts != null && atts.Length > 0)
                return ((AssemblyProductAttribute)atts[0]).Product;

            return null;
        }

        public static Window GetActiveWindow()
        {
            var app = Application.Current;
            if (app == null)
                return null;

            return app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }

        public static void ShowMessage(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            MessageBox.Show(window, text, GetProduct(), MessageBoxButton.OK);
        }

        public static void ShowError(this Window window, string text)
        {
            ShowMessage(window, text, MessageBoxImage.Error);
        }

        public static void ShowMessage(this Window window, string text, MessageBoxImage image)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            MessageBox.Show(window, text, GetProduct(), MessageBoxButton.OK, image);
        }

        public static MessageBoxResult ShowConfirm(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            return MessageBox.Show(window, text, GetProduct(), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
        }

        public static MessageBoxResult ShowConfirmCancel(this Window window, string text)
        {
            if (window == null)
            {
                window = GetActiveWindow();
            }
            return MessageBox.Show(window, text, GetProduct(), MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            if (collection == null)
                throw new ArgumentException(nameof(collection));

            if (enumerable == null)
                return;

            foreach (var item in enumerable)
            {
                collection.Add(item);
            }
        }

        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj) => obj.EnumerateVisualChildren(true);
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj, bool recursive) => obj.EnumerateVisualChildren(recursive, true);
        public static IEnumerable<DependencyObject> EnumerateVisualChildren(this DependencyObject obj, bool recursive, bool sameLevelFirst)
        {
            if (obj == null)
                yield break;

            if (sameLevelFirst)
            {
                int count = VisualTreeHelper.GetChildrenCount(obj);
                var list = new List<DependencyObject>(count);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child == null)
                        continue;

                    yield return child;
                    if (recursive)
                    {
                        list.Add(child);
                    }
                }

                foreach (var child in list)
                {
                    foreach (DependencyObject grandChild in child.EnumerateVisualChildren(recursive, sameLevelFirst))
                    {
                        yield return grandChild;
                    }
                }
            }
            else
            {
                int count = VisualTreeHelper.GetChildrenCount(obj);
                for (int i = 0; i < count; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                    if (child == null)
                        continue;

                    yield return child;
                    if (recursive)
                    {
                        foreach (var dp in child.EnumerateVisualChildren(recursive, sameLevelFirst))
                        {
                            yield return dp;
                        }
                    }
                }
            }
        }

        public static T FindVisualChild<T>(this DependencyObject obj) where T : FrameworkElement => FindVisualChild<T>(obj, t => true);
        public static T FindVisualChild<T>(this DependencyObject obj, Func<T, bool> where) where T : FrameworkElement
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            foreach (T item in obj.EnumerateVisualChildren(true, true).OfType<T>())
            {
                if (where(item))
                    return item;
            }
            return null;
        }
    }
}
