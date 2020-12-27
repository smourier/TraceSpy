using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace TraceSpy
{
    public partial class ColorSetWindow : Window
    {
        public ColorSetWindow(ColorSet colorSet)
        {
            if (colorSet == null)
                throw new ArgumentNullException(nameof(colorSet));

            InitializeComponent();
            ColorSet = colorSet;
            DataContext = colorSet;
        }

        public ColorSet ColorSet { get; }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void FontButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.AllowVerticalFonts = false;
                dlg.FontMustExist = true;
                if (!string.IsNullOrWhiteSpace(ColorSet.FontText))
                {
                    try
                    {
                        dlg.Font = (Font)new FontConverter().ConvertFromString(ColorSet.FontText);
                    }
                    catch
                    {
                        // do nothing
                    }
                }

                if (dlg.ShowDialog(NativeWindow.FromHandle(new WindowInteropHelper(this).Handle)) != System.Windows.Forms.DialogResult.OK)
                    return;

                ColorSet.FontText = new FontConverter().ConvertToString(dlg.Font);
            }
        }

        private string SetColor(string input)
        {
            using (var dlg = new ColorDialog())
            {
                dlg.FullOpen = true;
                dlg.AnyColor = true;
                if (!string.IsNullOrEmpty(input))
                {
                    try
                    {
                        dlg.Color = (Color)new ColorConverter().ConvertFromString(input);
                    }
                    catch
                    {
                        // do nothing
                    }
                }

                if (dlg.ShowDialog(NativeWindow.FromHandle(new WindowInteropHelper(this).Handle)) != System.Windows.Forms.DialogResult.OK)
                    return null;

                return dlg.Color.ToName();
            }
        }

        private void ForeBrushButton_Click(object sender, RoutedEventArgs e)
        {
            var output = SetColor(ColorSet.ForeBrushText);
            if (output != null)
            {
                ColorSet.ForeBrushText = output;
            }
        }

        private void BackBrushButton_Click(object sender, RoutedEventArgs e)
        {
            var output = SetColor(ColorSet.BackBrushText);
            if (output != null)
            {
                ColorSet.BackBrushText = output;
            }
        }

        private void FrameBrushButton_Click(object sender, RoutedEventArgs e)
        {
            var output = SetColor(ColorSet.FrameBrushText);
            if (output != null)
            {
                ColorSet.FrameBrushText = output;
            }
        }
    }
}
