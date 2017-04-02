using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class Filters : Form
    {
        private readonly Settings _settings;

        public Filters(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
            InitializeComponent();
            LoadFilters();
        }

        private static ListViewItem GetItem(Filter filter)
        {
            ListViewItem item = new ListViewItem(filter.Active.ToString());
            item.SubItems.Add(filter.FilterColumn.ToString());
            item.SubItems.Add(filter.FilterType.ToString());
            item.SubItems.Add(filter.IgnoreCase.ToString());
            item.SubItems.Add(filter.Definition);
            item.Tag = filter;
            return item;
        }

        private void LoadFilters()
        {
            listView.Items.Clear();
            foreach (Filter filter in _settings.Filters)
            {
                ListViewItem item = GetItem(filter);
                listView.Items.Add(item);
            }
        }

        private Filter GetSelectedFilter()
        {
            if (listView.SelectedItems.Count == 0)
                return null;

            return listView.SelectedItems[0].Tag as Filter;
        }

        private void UpdateControls()
        {
            Filter filter = GetSelectedFilter();
            buttonRemove.Enabled = filter != null;
            buttonModify.Enabled = filter != null;
        }

        private Filter GetExisting(Filter filter)
        {
            return AllFilters.FirstOrDefault(f => f.Equals(filter));
        }

        private IEnumerable<Filter> AllFilters
        {
            get
            {
                foreach (ListViewItem item in listView.Items)
                {
                    Filter filter = item.Tag as Filter;
                    if (filter != null)
                        yield return filter;
                }
            }
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            FilterEdit edit = new FilterEdit(null);
            DialogResult dr = edit.ShowDialog(this);
            listView.Focus();
            if (dr != DialogResult.OK)
                return;

            Filter existing = GetExisting(edit.Filter);
            if (existing != null)
            {
                GetItem(existing).Selected = true;
                return;
            }

            listView.Items.Add(GetItem(edit.Filter)).Selected = true;
        }

        private void Remove(IEquatable<Filter> filter)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (filter.Equals(item.Tag as Filter))
                {
                    item.Remove();
                    return;
                }
            }
        }

        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            Filter filter = GetSelectedFilter();
            if (filter == null)
                return;

            listView.SelectedItems[0].Remove();
        }

        private void ButtonModifyClick(object sender, EventArgs e)
        {
            Filter filter = GetSelectedFilter();
            if (filter == null)
                return;

            FilterEdit edit = new FilterEdit(filter);
            DialogResult dr = edit.ShowDialog(this);
            listView.Focus();
            if (dr != DialogResult.OK)
                return;

            Filter existing = GetExisting(edit.Filter);
            if ((existing != null) && (existing != edit.Filter))
            {
                Remove(edit.Filter);
                return;
            }

            ListViewItem item = listView.SelectedItems[0];
            item.Text = edit.Filter.Active.ToString();
            item.SubItems[1].Text = edit.Filter.FilterColumn.ToString();
            item.SubItems[2].Text = edit.Filter.FilterType.ToString();
            item.SubItems[3].Text = edit.Filter.IgnoreCase.ToString();
            item.SubItems[4].Text = edit.Filter.Definition;
        }

        private void ListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void FiltersFormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) || (e.CloseReason == CloseReason.None))
            {
                if (DialogResult == DialogResult.OK)
                {
                    _settings.Filters = new List<Filter>(AllFilters).ToArray();
                    _settings.SerializeToConfiguration();
                }
            }
        }

        private void ListViewDoubleClick(object sender, EventArgs e)
        {
            ButtonModifyClick(this, EventArgs.Empty);
        }
    }
}
