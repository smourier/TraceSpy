using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class Colorizers : Form
    {
        private readonly Settings _settings;

        public Colorizers(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
            InitializeComponent();
            LoadColorizers();
            LoadColorSets();
        }

        private void LoadColorizers()
        {
            listViewColorizers.Items.Clear();
            foreach (Colorizer colorizer in _settings.Colorizers)
            {
                ListViewItem item = GetColorizerItem(colorizer);
                listViewColorizers.Items.Add(item);
            }
        }

        private void LoadColorSets()
        {
            listViewColorSets.Items.Clear();
            foreach (ColorSet set in _settings.ColorSets)
            {
                ListViewItem item = GetColorSetItem(set);
                listViewColorSets.Items.Add(item);
            }
        }

        private Colorizer GetSelectedColorizer()
        {
            if (listViewColorizers.SelectedItems.Count == 0)
                return null;

            return listViewColorizers.SelectedItems[0].Tag as Colorizer;
        }

        private ColorSet GetSelectedColorSet()
        {
            if (listViewColorSets.SelectedItems.Count == 0)
                return null;

            return listViewColorSets.SelectedItems[0].Tag as ColorSet;
        }

        private static ListViewItem GetColorizerItem(Colorizer colorizer)
        {
            ListViewItem item = new ListViewItem(colorizer.Active.ToString());
            item.SubItems.Add(colorizer.IgnoreCase.ToString());
            item.SubItems.Add(colorizer.Definition);
            item.Tag = colorizer;
            return item;
        }

        private static ListViewItem GetColorSetItem(ColorSet set)
        {
            ListViewItem item = new ListViewItem(set.Name);
            item.SubItems.Add(set.ForeColorName);
            item.SubItems.Add(set.BackColorName);
            item.SubItems.Add(set.Mode.ToString());
            item.SubItems.Add(set.FrameWidth.ToString());
            item.Tag = set;
            return item;
        }

        private IEnumerable<Colorizer> AllColorizers
        {
            get
            {
                foreach (ListViewItem item in listViewColorizers.Items)
                {
                    Colorizer colorizer = item.Tag as Colorizer;
                    if (colorizer != null)
                        yield return colorizer;
                }
            }
        }

        private IEnumerable<ColorSet> AllColorSets
        {
            get
            {
                foreach (ListViewItem item in listViewColorSets.Items)
                {
                    ColorSet set = item.Tag as ColorSet;
                    if (set != null)
                        yield return set;
                }
            }
        }

        private Colorizer GetExistingColorizer(Colorizer colorizer)
        {
            return AllColorizers.FirstOrDefault(c => c.Equals(colorizer));
        }

        private ColorSet GetExistingColorSet(ColorSet set)
        {
            return AllColorSets.FirstOrDefault(c => c.Equals(set));
        }

        private void ButtonAddColorizerClick(object sender, EventArgs e)
        {
            ColorizerEdit edit = new ColorizerEdit(null);
            DialogResult dr = edit.ShowDialog(this);
            listViewColorizers.Focus();
            if (dr != DialogResult.OK)
                return;

            Colorizer existing = GetExistingColorizer(edit.Colorizer);
            if (existing != null)
            {
                GetColorizerItem(existing).Selected = true;
                return;
            }

            listViewColorizers.Items.Add(GetColorizerItem(edit.Colorizer)).Selected = true;
        }

        private void ButtonRemoveColorizerClick(object sender, EventArgs e)
        {
            Colorizer colorizer = GetSelectedColorizer();
            if (colorizer == null)
                return;

            listViewColorizers.SelectedItems[0].Remove();
        }

        private void ButtonModifyColorizerClick(object sender, EventArgs e)
        {
            Colorizer colorizer = GetSelectedColorizer();
            if (colorizer == null)
                return;

            ColorizerEdit edit = new ColorizerEdit(colorizer);
            DialogResult dr = edit.ShowDialog(this);
            listViewColorizers.Focus();
            if (dr != DialogResult.OK)
                return;

            Colorizer existing = GetExistingColorizer(edit.Colorizer);
            if ((existing != null) && (existing != edit.Colorizer))
            {
                Remove(edit.Colorizer);
                return;
            }

            ListViewItem item = listViewColorizers.SelectedItems[0];
            item.Text = edit.Colorizer.Active.ToString();
            item.SubItems[1].Text = edit.Colorizer.IgnoreCase.ToString();
            item.SubItems[2].Text = edit.Colorizer.Definition;
        }

        private void ButtonAddColorSetClick(object sender, EventArgs e)
        {
            ColorSetEdit edit = new ColorSetEdit(null, true);
            DialogResult dr = edit.ShowDialog(this);
            listViewColorizers.Focus();
            if (dr != DialogResult.OK)
                return;

            ColorSet existing = GetExistingColorSet(edit.ColorSet);
            if (existing != null)
            {
                GetColorSetItem(existing).Selected = true;
                return;
            }

            listViewColorSets.Items.Add(GetColorSetItem(edit.ColorSet)).Selected = true;
        }

        private void Remove(IEquatable<Colorizer> colorizer)
        {
            foreach (ListViewItem item in listViewColorizers.Items)
            {
                if (colorizer.Equals(item.Tag as Colorizer))
                {
                    item.Remove();
                    return;
                }
            }
        }

        private void Remove(IEquatable<ColorSet> set)
        {
            foreach (ListViewItem item in listViewColorSets.Items)
            {
                if (set.Equals(item.Tag as ColorSet))
                {
                    item.Remove();
                    return;
                }
            }
        }

        private void ButtonRemoveColorSetClick(object sender, EventArgs e)
        {
            ColorSet set = GetSelectedColorSet();
            if (set == null)
                return;

            listViewColorSets.SelectedItems[0].Remove();
        }

        private void ButtonModifyColorSetClick(object sender, EventArgs e)
        {
            ColorSet set = GetSelectedColorSet();
            if (set == null)
                return;

            ColorSetEdit edit = new ColorSetEdit(set, true);
            DialogResult dr = edit.ShowDialog(this);
            listViewColorSets.Focus();
            if (dr != DialogResult.OK)
                return;

            ColorSet existing = GetExistingColorSet(edit.ColorSet);
            if ((existing != null) && (existing != edit.ColorSet))
            {
                Remove(edit.ColorSet);
                return;
            }

            ListViewItem item = listViewColorSets.SelectedItems[0];
            item.Text = edit.ColorSet.Name;
            item.SubItems[1].Text = edit.ColorSet.ForeColorName;
            item.SubItems[2].Text = edit.ColorSet.BackColorName;
            item.SubItems[3].Text = edit.ColorSet.Mode.ToString();
            item.SubItems[4].Text = edit.ColorSet.FrameWidth.ToString();
        }

        private void ListViewColorizersSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void ListViewColorSetsSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            Colorizer colorizer = GetSelectedColorizer();
            buttonRemoveColorizer.Enabled = colorizer != null;
            buttonModifyColorizer.Enabled = colorizer != null;

            ColorSet set = GetSelectedColorSet();
            buttonRemoveColorSet.Enabled = set != null;
            buttonModifyColorSet.Enabled = set != null;
        }

        private void ListViewColorizersMouseDoubleClick(object sender, MouseEventArgs e)
        {
            ButtonModifyColorizerClick(this, EventArgs.Empty);
        }

        private void ListViewColorSetsMouseDoubleClick(object sender, MouseEventArgs e)
        {
            ButtonModifyColorSetClick(this, EventArgs.Empty);
        }

        private void ColorizersFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.None)
            {
                if (DialogResult == DialogResult.OK)
                {
                    _settings.Colorizers = new List<Colorizer>(AllColorizers).ToArray();
                    _settings.ColorSets = new List<ColorSet>(AllColorSets).ToArray();
                    _settings.SerializeToConfiguration();

                    ((Main)Owner).RefreshListView();
                }
            }
        }
    }
}
