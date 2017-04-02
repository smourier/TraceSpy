using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class EtwProviderEdit : Form
    {
        public EtwProviderEdit(EtwProvider provider)
        {
            if (provider == null)
            {
                provider = new EtwProvider();
            }

            EtwProvider = provider;
            InitializeComponent();

            comboBoxLevels.DrawMode = DrawMode.OwnerDrawVariable;
            comboBoxLevels.MeasureItem += OnComboBoxLevelsMeasureItem;
            comboBoxLevels.DrawItem += OnComboBoxLevelsDrawItem;

            textBoxGuid.Text = provider.ProviderGuid.ToString();
            textBoxDescription.Text = provider.Description;
            checkBoxActive.Checked = provider.Active;

            LoadLevels();
            UpdateControls();
        }

        private void comboBoxLevels_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedItem is string)
            {
                cb.SelectedItem = null;
            }
        }

        private void OnComboBoxLevelsMeasureItem(object sender, MeasureItemEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            object o = cb.Items[e.Index];
            if (o is string)
            {
                e.ItemHeight = 1;
            }
        }

        private void OnComboBoxLevelsDrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            const int ComboBoxColumnCount = 3;

            object o = cb.Items[e.Index];
            if (o is string)
            {
                e.Graphics.FillRectangle(SystemBrushes.ButtonFace, e.Bounds);
                return;
            }

            int x = e.Bounds.Location.X;
            e.DrawBackground();
            for (int i = 0; i < ComboBoxColumnCount; i++)
            {
                string text = o.GetType().GetProperty("DisplayName", BindingFlags.Public | BindingFlags.Instance).GetValue(o, null) as string;
                string[] split = text.Split('\t');
                if (split.Length > i)
                {
                    TextRenderer.DrawText(e.Graphics, split[i], e.Font, new Point(x, e.Bounds.Location.Y), e.ForeColor);
                }

                x += e.Bounds.Width / ComboBoxColumnCount;
                if (i < (ComboBoxColumnCount - 1))
                {
                    e.Graphics.DrawLine(SystemPens.ButtonFace, x, e.Bounds.Location.Y, x, e.Bounds.Location.Y + e.Bounds.Height);
                }
            }
            e.DrawFocusRectangle();
        }

        private void LoadLevels()
        {
            comboBoxLevels.DisplayMember = "Name";
            comboBoxLevels.ValueMember = "Value";
            comboBoxLevels.Items.Clear();

            comboBoxLevels.Items.Add(new LevelItem("Native\tNone\t0", EtwTraceLevel.None));
            comboBoxLevels.Items.Add(new LevelItem("Native\tFatal\t1", EtwTraceLevel.Fatal));
            comboBoxLevels.Items.Add(new LevelItem("Native\tError\t2", EtwTraceLevel.Error));
            comboBoxLevels.Items.Add(new LevelItem("Native\tWarning\t3", EtwTraceLevel.Warning));
            comboBoxLevels.Items.Add(new LevelItem("Native\tInformation\t4", EtwTraceLevel.Information));
            comboBoxLevels.Items.Add(new LevelItem("Native\tVerbose\t5", EtwTraceLevel.Verbose));
            comboBoxLevels.Items.Add(string.Empty);
            comboBoxLevels.Items.Add(new LevelItem(".NET\tCritical\t1", TraceEventType.Critical));
            comboBoxLevels.Items.Add(new LevelItem(".NET\tError\t2", TraceEventType.Error));
            comboBoxLevels.Items.Add(new LevelItem(".NET\tWarning\t4", TraceEventType.Warning));
            comboBoxLevels.Items.Add(new LevelItem(".NET\tInformation\t8", TraceEventType.Information));
            comboBoxLevels.Items.Add(new LevelItem(".NET\tVerbose\t16", TraceEventType.Verbose));

            foreach(object o in comboBoxLevels.Items)
            {
                LevelItem item = o as LevelItem;
                if (item != null && item.Name == ((byte)EtwProvider.TraceLevel).ToString())
                {
                    comboBoxLevels.SelectedItem = item;
                    return;
                }
            }
        }

        private class LevelItem : IFormattable
        {
            public LevelItem(string displayName, object value)
            {
                DisplayName = displayName;
                Value = value;
                Name = Convert.ToByte(value).ToString();
            }

            public object Value { get; set; }
            public string DisplayName { get; set; }
            public string Name { get; set; }

            public string ToString(string format, IFormatProvider formatProvider)
            {
                return DisplayName;
            }
        }

        public EtwProvider EtwProvider { get; private set; }

        private bool IsValidProviderId()
        {
            try
            {
                return new Guid(textBoxGuid.Text) != Guid.Empty;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateControls()
        {
            byte b;
            buttonOK.Enabled = textBoxDescription.Text.Trim().Length > 0 & IsValidProviderId() && byte.TryParse(comboBoxLevels.Text, out b);
        }

        private void EtwProviderEditFormClosing(object sender, FormClosingEventArgs e)
        {
            if (((e.CloseReason == CloseReason.UserClosing) || (e.CloseReason == CloseReason.None)) && (DialogResult == DialogResult.OK))
            {
                EtwProvider.ProviderId = textBoxGuid.Text.Trim();
                EtwProvider.Description = textBoxDescription.Text.Trim();
                EtwProvider.Active = checkBoxActive.Checked;

                byte level = Convert.ToByte(((LevelItem)comboBoxLevels.SelectedItem).Value);
                EtwProvider.TraceLevel = level;
            }
        }

        private void TextBoxDefinitionTextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void textBoxGuid_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void checkBoxActive_CheckedChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void comboBoxLevels_TextUpdate(object sender, EventArgs e)
        {
            UpdateControls();
        }
    }
}
