using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class EtwProviders : Form
    {
        private readonly Settings _settings;

        public EtwProviders(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
            InitializeComponent();
            LoadProviders();
        }

        public Settings Settings
        {
            get
            {
                return _settings;
            }
        }

        private static ListViewItem GetItem(EtwProvider provider)
        {
            ListViewItem item = new ListViewItem(provider.Active.ToString());
            item.SubItems.Add(provider.ProviderGuid.ToString());
            item.SubItems.Add(provider.Description ?? string.Empty);
            item.SubItems.Add(provider.TraceLevel.ToString());
            item.Tag = provider;
            return item;
        }

        private void LoadProviders()
        {
            listView.Items.Clear();
            foreach (EtwProvider provider in _settings.EtwProviders)
            {
                ListViewItem item = GetItem(provider);
                listView.Items.Add(item);
            }
        }

        private EtwProvider GetSelectedEtwProvider()
        {
            if (listView.SelectedItems.Count == 0)
                return null;

            return listView.SelectedItems[0].Tag as EtwProvider;
        }

        private void UpdateControls()
        {
            EtwProvider provider = GetSelectedEtwProvider();
            buttonRemove.Enabled = provider != null;
            buttonModify.Enabled = provider != null;
        }

        private EtwProvider GetExisting(EtwProvider provider)
        {
            return AllEtwProviders.FirstOrDefault(f => f.Equals(provider));
        }

        private IEnumerable<EtwProvider> AllEtwProviders
        {
            get
            {
                foreach (ListViewItem item in listView.Items)
                {
                    EtwProvider provider = item.Tag as EtwProvider;
                    if (provider != null)
                        yield return provider;
                }
            }
        }

        private void ButtonAddClick(object sender, EventArgs e)
        {
            EtwProviderEdit edit = new EtwProviderEdit(null);
            DialogResult dr = edit.ShowDialog(this);
            listView.Focus();
            if (dr != DialogResult.OK)
                return;

            EtwProvider existing = GetExisting(edit.EtwProvider);
            if (existing != null)
            {
                GetItem(existing).Selected = true;
                return;
            }

            listView.Items.Add(GetItem(edit.EtwProvider)).Selected = true;
        }

        private void Remove(IEquatable<EtwProvider> provider)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (provider.Equals(item.Tag as EtwProvider))
                {
                    item.Remove();
                    return;
                }
            }
        }

        private void ButtonRemoveClick(object sender, EventArgs e)
        {
            EtwProvider provider = GetSelectedEtwProvider();
            if (provider == null)
                return;

            listView.SelectedItems[0].Remove();
        }

        private void ButtonModifyClick(object sender, EventArgs e)
        {
            EtwProvider provider = GetSelectedEtwProvider();
            if (provider == null)
                return;

            EtwProviderEdit edit = new EtwProviderEdit(provider);
            DialogResult dr = edit.ShowDialog(this);
            listView.Focus();
            if (dr != DialogResult.OK)
                return;

            EtwProvider existing = GetExisting(edit.EtwProvider);
            if ((existing != null) && (existing != edit.EtwProvider))
            {
                Remove(edit.EtwProvider);
                return;
            }

            ListViewItem item = listView.SelectedItems[0];
            item.Text = edit.EtwProvider.Active.ToString();
            item.SubItems[1].Text = edit.EtwProvider.ProviderGuid.ToString();
            item.SubItems[2].Text = edit.EtwProvider.Description;
            item.SubItems[3].Text = edit.EtwProvider.TraceLevel.ToString();
        }

        private void ListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void EtwProvidersFormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) || (e.CloseReason == CloseReason.None))
            {
                if (DialogResult == DialogResult.OK)
                {
                    _settings.EtwProviders = new List<EtwProvider>(AllEtwProviders).ToArray();
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
