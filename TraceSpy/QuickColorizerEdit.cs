using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public partial class QuickColorizerEdit : Form
    {
        public QuickColorizerEdit(QuickColorizer colorizer)
        {
            if (colorizer == null)
            {
                colorizer = new QuickColorizer();
            }

            QuickColorizer = colorizer;
            InitializeComponent();
            textBoxDefinition.Text = colorizer.Text;
            textBoxColor.Text = colorizer.ColorSet != null ? colorizer.ColorSet.ToString() : string.Empty;
            textBoxColor.Tag = colorizer.ColorSet;
            checkBoxIgnoreCase.Checked = colorizer.IgnoreCase;
            checkBoxWholeText.Checked = colorizer.WholeText;
            checkBoxActive.Checked = colorizer.Active;
            UpdateControls();
        }

        public QuickColorizer QuickColorizer { get; private set; }

        private void UpdateControls()
        {
            buttonOK.Enabled = textBoxDefinition.Text.Trim().Length > 0 && textBoxColor.Tag is ColorSet;
        }

        private void TextBoxDefinitionTextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void QuickColorizerEditFormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None) && DialogResult == DialogResult.OK)
            {
                QuickColorizer.Text = textBoxDefinition.Text.Trim();
                QuickColorizer.IgnoreCase = checkBoxIgnoreCase.Checked;
                QuickColorizer.WholeText = checkBoxWholeText.Checked;
                QuickColorizer.ColorSet = (ColorSet)textBoxColor.Tag;
                QuickColorizer.Active = checkBoxActive.Checked;
            }
        }

        private void buttonColor_Click(object sender, EventArgs e)
        {
            ColorSetEdit edit = new ColorSetEdit(QuickColorizer.ColorSet, false);
            DialogResult dr = edit.ShowDialog(this);
            if (dr != DialogResult.OK)
                return;

            textBoxColor.Tag = edit.ColorSet;
            textBoxColor.Text = edit.ColorSet.ToString();
        }

        private void textBoxColor_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }
    }
}
