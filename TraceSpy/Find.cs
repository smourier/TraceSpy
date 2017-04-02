using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TraceSpy
{
    public partial class Find : Form
    {
        private readonly Settings _settings;

        public Find(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
            InitializeComponent();
            comboBoxFind.AutoCompleteCustomSource = _settings.AutoCompleteSearches;
        }

        public new Main Owner
        {
            get
            {
                return (Main)base.Owner;
            }
            set
            {
                base.Owner = value;
            }
        }

        private void FindFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            Hide();
        }

        private Regex BuildRegex()
        {
            string search = comboBoxFind.Text.Trim();
            if (search.Length == 0)
                return null;

            RegexOptions options = RegexOptions.None;
            if (!checkBoxCase.Checked)
            {
                options |= RegexOptions.IgnoreCase;
            }
            return new Regex(search, options);
        }

        internal void FindNext()
        {
            Regex regex = BuildRegex();
            if (regex == null)
                return;

            Owner.FindAndSelect(comboBoxFind.Text.Trim(), regex);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            Regex regex = BuildRegex();
            if (regex == null)
                return;

            string search = comboBoxFind.Text.Trim();
            _settings.AddSearch(search);
            Owner.FindAndSelect(search, regex);
        }

        private void UpdateControls()
        {
            buttonOK.Enabled = comboBoxFind.Text.Trim().Length > 0;
        }

        private void comboBoxFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void comboBoxFind_TextUpdate(object sender, EventArgs e)
        {
            UpdateControls();
        }
    }
}
