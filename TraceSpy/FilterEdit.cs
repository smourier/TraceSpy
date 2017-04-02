using System;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class FilterEdit : Form
    {
        public FilterEdit(Filter filter)
        {
            if (filter == null)
            {
                filter = new Filter();
            }

            Filter = filter;
            InitializeComponent();

            switch (Filter.FilterType)
            {
                case FilterType.IncludeAll:
                    radioButtonIncludeAll.Checked = true;
                    break;

                case FilterType.Include:
                    radioButtonInclude.Checked = true;
                    break;

                default:
                    radioButtonExclude.Checked = true;
                    break;

            }

            switch (Filter.FilterColumn)
            {
                case FilterColumn.Process:
                    radioButtonFilterProcessName.Checked = true;
                    break;

                //case FilterColumn.Text:
                default:
                    radioButtonFilterText.Checked = true;
                    break;
            }

            textBoxDefinition.Text = filter.Definition;
            checkBoxIgnoreCase.Checked = filter.IgnoreCase;
            checkBoxActive.Checked = filter.Active;
        }

        public Filter Filter { get; private set; }

        private void UpdateControls()
        {
            textBoxDefinition.Enabled = !radioButtonIncludeAll.Checked;
            buttonOK.Enabled = radioButtonIncludeAll.Checked || (textBoxDefinition.Text.Trim().Length > 0);
            checkBoxIgnoreCase.Enabled = textBoxDefinition.Enabled;
        }

        private void RadioButtonIncludeAllCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void RadioButtonExcludeCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void RadioButtonIncludeCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void FilterEditFormClosing(object sender, FormClosingEventArgs e)
        {
            if (((e.CloseReason == CloseReason.UserClosing) || (e.CloseReason == CloseReason.None)) && (DialogResult == DialogResult.OK))
            {
                if (radioButtonIncludeAll.Checked)
                {
                    Filter.FilterType = FilterType.IncludeAll;
                }
                else if (radioButtonInclude.Checked)
                {
                    Filter.FilterType = FilterType.Include;
                }
                else
                {
                    Filter.FilterType = FilterType.Exclude;
                }

                if (Filter.FilterType == FilterType.IncludeAll)
                {
                    Filter.Definition = null;
                }
                else
                {
                    Filter.Definition = textBoxDefinition.Text.Trim();
                }

                if (radioButtonFilterProcessName.Checked)
                {
                    Filter.FilterColumn = FilterColumn.Process;
                }
                else
                {
                    Filter.FilterColumn = FilterColumn.Text;
                }

                Filter.IgnoreCase = checkBoxIgnoreCase.Checked;
                Filter.Active = checkBoxActive.Checked;
            }
        }

        private void TextBoxDefinitionTextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void RadioButtonFilterTextCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void RadioButtonFilterProcessNameCheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }
    }
}
