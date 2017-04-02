using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class QuickColorizers : Form
    {
        private readonly Settings _settings;

        public QuickColorizers(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
            InitializeComponent();
            LoadQuickColorizers();
        }

        private void LoadQuickColorizers()
        {
            listViewQuickColorizers.Items.Clear();
            foreach (QuickColorizer colorizer in _settings.QuickColorizers)
            {
                ListViewItem item = GetQuickColorizerItem(colorizer);
                listViewQuickColorizers.Items.Add(item);
            }
        }

        private QuickColorizer GetSelectedQuickColorizer()
        {
            if (listViewQuickColorizers.SelectedItems.Count == 0)
                return null;

            return listViewQuickColorizers.SelectedItems[0].Tag as QuickColorizer;
        }

        private static ListViewItem GetQuickColorizerItem(QuickColorizer colorizer)
        {
            ListViewItem item = new ListViewItem(colorizer.Active.ToString());
            item.SubItems.Add(colorizer.IgnoreCase.ToString());
            item.SubItems.Add(colorizer.WholeText.ToString());
            item.SubItems.Add(colorizer.ColorSet != null ? colorizer.ColorSet.ToString() : string.Empty);
            item.SubItems.Add(colorizer.Text);
            item.Tag = colorizer;
            return item;
        }

        private IEnumerable<QuickColorizer> AllQuickColorizers
        {
            get
            {
                foreach (ListViewItem item in listViewQuickColorizers.Items)
                {
                    QuickColorizer colorizer = item.Tag as QuickColorizer;
                    if (colorizer != null)
                        yield return colorizer;
                }
            }
        }

        private QuickColorizer GetExistingQuickColorizer(QuickColorizer colorizer)
        {
            return AllQuickColorizers.FirstOrDefault(c => c.Equals(colorizer));
        }

        private void Remove(IEquatable<QuickColorizer> colorizer)
        {
            foreach (ListViewItem item in listViewQuickColorizers.Items)
            {
                if (colorizer.Equals(item.Tag as QuickColorizer))
                {
                    item.Remove();
                    return;
                }
            }
        }

        private void ListViewQuickColorizersSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            QuickColorizer colorizer = GetSelectedQuickColorizer();
            buttonRemoveQuickColorizer.Enabled = colorizer != null;
            buttonModifyQuickColorizer.Enabled = colorizer != null;
        }

        private void ListViewQuickColorizersMouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonModifyQuickColorizer_Click(this, EventArgs.Empty);
        }

        private void QuickColorizersFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                if (DialogResult == DialogResult.OK)
                {
                    _settings.QuickColorizers = new List<QuickColorizer>(AllQuickColorizers).ToArray();
                    _settings.SerializeToConfiguration();

                    ((Main)Owner).RefreshListView();
                }
            }
        }

        private void buttonAddQuickColorizer_Click(object sender, EventArgs e)
        {
            QuickColorizerEdit edit = new QuickColorizerEdit(null);
            DialogResult dr = edit.ShowDialog(this);
            listViewQuickColorizers.Focus();
            if (dr != DialogResult.OK)
                return;

            QuickColorizer existing = GetExistingQuickColorizer(edit.QuickColorizer);
            if (existing != null)
            {
                GetQuickColorizerItem(existing).Selected = true;
                return;
            }

            listViewQuickColorizers.Items.Add(GetQuickColorizerItem(edit.QuickColorizer)).Selected = true;
        }

        private void buttonRemoveQuickColorizer_Click(object sender, EventArgs e)
        {
            QuickColorizer colorizer = GetSelectedQuickColorizer();
            if (colorizer == null)
                return;

            listViewQuickColorizers.SelectedItems[0].Remove();
        }

        private void buttonModifyQuickColorizer_Click(object sender, EventArgs e)
        {
            QuickColorizer colorizer = GetSelectedQuickColorizer();
            if (colorizer == null)
                return;

            QuickColorizerEdit edit = new QuickColorizerEdit(colorizer);
            DialogResult dr = edit.ShowDialog(this);
            listViewQuickColorizers.Focus();
            if (dr != DialogResult.OK)
                return;

            QuickColorizer existing = GetExistingQuickColorizer(edit.QuickColorizer);
            if ((existing != null) && (existing != edit.QuickColorizer))
            {
                Remove(edit.QuickColorizer);
                return;
            }

            ListViewItem item = listViewQuickColorizers.SelectedItems[0];
            item.Text = edit.QuickColorizer.Active.ToString();
            item.SubItems[1].Text = edit.QuickColorizer.IgnoreCase.ToString();
            item.SubItems[2].Text = edit.QuickColorizer.WholeText.ToString();
            item.SubItems[3].Text = edit.QuickColorizer.ColorSet != null ? edit.QuickColorizer.ColorSet.ToString() : string.Empty;
            item.SubItems[4].Text = edit.QuickColorizer.Text;
        }
    }
}
