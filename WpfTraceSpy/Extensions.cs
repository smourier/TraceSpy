using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TraceSpy
{
    internal static class Extensions
    {
        public static void OpenInDefaultBrowser(this string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var psi = new ProcessStartInfo(url);
            Process.Start(psi);
        }

        public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl control, KeyEventArgs args) => RaiseMenuItemClickOnKeyGesture(control, args, true);
        public static void RaiseMenuItemClickOnKeyGesture(this ItemsControl control, KeyEventArgs args, bool throwOnError)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (control == null)
                return;

            var kgc = new KeyGestureConverter();
            foreach (var item in control.Items.OfType<MenuItem>())
            {
                if (!string.IsNullOrWhiteSpace(item.InputGestureText))
                {
                    KeyGesture gesture = null;
                    if (throwOnError)
                    {
                        gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                    }
                    else
                    {
                        try
                        {
                            gesture = kgc.ConvertFrom(item.InputGestureText) as KeyGesture;
                        }
                        catch
                        {
                        }
                    }

                    if (gesture != null && gesture.Matches(null, args))
                    {
                        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        args.Handled = true;
                        return;
                    }
                }

                RaiseMenuItemClickOnKeyGesture(item, args, throwOnError);
                if (args.Handled)
                    return;
            }
        }

        public static string GetProduct() => Assembly.GetEntryAssembly().GetProduct();
        public static string GetProduct(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var atts = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
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

        public static void ShowError(this Window window, string text) => ShowMessage(window, text, MessageBoxImage.Error);
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
                var count = VisualTreeHelper.GetChildrenCount(obj);
                var list = new List<DependencyObject>(count);
                for (var i = 0; i < count; i++)
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
                var count = VisualTreeHelper.GetChildrenCount(obj);
                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);
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

        public static T GetVisualParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            var parent = VisualTreeHelper.GetParent(obj);
            if (parent == null)
                return null;

            if (typeof(T).IsAssignableFrom(parent.GetType()))
                return (T)parent;

            return parent.GetVisualParent<T>();
        }
    }
}
