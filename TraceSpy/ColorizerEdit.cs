using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public partial class ColorizerEdit : Form
    {
        private readonly ToolTip _checkTip;

        public ColorizerEdit(Colorizer colorizer)
        {
            if (colorizer == null)
            {
                colorizer = new Colorizer();
            }

            Colorizer = colorizer;
            InitializeComponent();
            _checkTip = new ToolTip();
            textBoxDefinition.Text = colorizer.Definition;
            checkBoxIgnoreCase.Checked = colorizer.IgnoreCase;
            checkBoxActive.Checked = colorizer.Active;
        }

        public Colorizer Colorizer { get; private set; }

        private string CheckRegex()
        {
            try
            {
                RegexOptions options = RegexOptions.None;
                if (checkBoxIgnoreCase.Checked)
                {
                    options |= RegexOptions.IgnoreCase;
                }

                new Regex(textBoxDefinition.Text.Trim(), options);
                return null;
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        private void UpdateControls()
        {
            buttonOK.Enabled = textBoxDefinition.Text.Trim().Length > 0 && string.IsNullOrEmpty(CheckRegex());
        }

        private void TextBoxDefinitionTextChanged(object sender, EventArgs e)
        {
            UpdateControls();
            _checkTip.SetToolTip(textBoxDefinition, CheckRegex());
        }

        private void ColorizerEditFormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None) && DialogResult == DialogResult.OK)
            {
                Colorizer.Definition = textBoxDefinition.Text.Trim();
                Colorizer.IgnoreCase = checkBoxIgnoreCase.Checked;
                Colorizer.Active = checkBoxActive.Checked;
            }
        }
    }
}
