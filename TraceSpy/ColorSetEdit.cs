using System;
using System.Drawing;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class ColorSetEdit : Form
    {
        public ColorSetEdit(ColorSet set, bool nameEditable)
        {
            if (set == null)
            {
                set = new ColorSet();
            }

            ColorSet = set;
            InitializeComponent();

            textBoxName.Text = set.Name;
            textBoxName.ReadOnly = !nameEditable;
            textBoxBackColor.Text = set.BackColorName;
            textBoxForeColor.Text = set.ForeColorName;
            switch (set.Mode)
            {
                case ColorSetDrawMode.Frame:
                    radioButtonFrame.Checked = true;
                    break;

                //case ColorSetDrawMode.Fill:
                default:
                    radioButtonFill.Checked = true;
                    break;
            }
            textBoxFont.Text = set.FontName;
            numericUpDownFrameWidth.Value = (int)set.FrameWidth;
            UpdateControls();
        }

        public ColorSet ColorSet { get; private set; }

        private void UpdateControls()
        {
            labelFrameWidth.Enabled = radioButtonFrame.Checked;
            numericUpDownFrameWidth.Enabled = labelFrameWidth.Enabled;
            buttonOK.Enabled = (!textBoxName.ReadOnly && textBoxName.Text.Trim().Length > 0) ||
                (textBoxName.ReadOnly && (textBoxBackColor.Text.Trim().Length > 0 || textBoxForeColor.Text.Trim().Length > 0));
        }

        private void ColorSetEditFormClosing(object sender, FormClosingEventArgs e)
        {
            if (((e.CloseReason == CloseReason.UserClosing) || (e.CloseReason == CloseReason.None)) && (DialogResult == DialogResult.OK))
            {
                ColorSet.Name = textBoxName.ReadOnly ? null : textBoxName.Text.Trim();
                ColorSet.BackColorName = textBoxBackColor.Text.Trim();
                ColorSet.ForeColorName = textBoxForeColor.Text.Trim();
                ColorSet.Mode = radioButtonFrame.Checked ? ColorSetDrawMode.Frame : ColorSetDrawMode.Fill;
                if (radioButtonFrame.Checked)
                {
                    ColorSet.FrameWidth = (int)numericUpDownFrameWidth.Value;
                }
                ColorSet.FontName = textBoxFont.Text.Trim();
            }
        }

        private void RadioButtonFillCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void RadioButtonFrameCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void ButtonBackColorClick(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.AnyColor = true;
            dialog.Color = ColorSet.ConvertColor(textBoxBackColor.Text, Color.White);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            textBoxBackColor.Text = ColorSet.ConvertColor(dialog.Color);
        }

        private void ButtonForeColorClick(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.AnyColor = true;
            dialog.Color = ColorSet.ConvertColor(textBoxForeColor.Text, Color.Black);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            textBoxForeColor.Text = ColorSet.ConvertColor(dialog.Color);
        }

        private void TextBoxNameTextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void ButtonFontClick(object sender, EventArgs e)
        {
            FontDialog dlg = new FontDialog();
            dlg.Font = ColorSet.Font;
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            textBoxFont.Text = new FontConverter().ConvertToInvariantString(dlg.Font);
        }

        private void textBoxForeColor_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void textBoxBackColor_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }
    }
}
