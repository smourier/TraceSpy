using System.Text;
using System.Windows.Forms;

namespace TraceSpy
{
    public partial class RecordView : Form
    {
        public RecordView(ListViewItem item)
        {
            InitializeComponent();

            labelIndexValue.Text = item.Text;;
            labelProcessValue.Text = item.SubItems[2].Text;
            labelTicksValue.Text = item.SubItems[1].Text;

            StringBuilder sb = new StringBuilder(item.SubItems[3].Text);
            int itemIndex = item.Index + 1;
            while (itemIndex < item.ListView.Items.Count)
            {
                if (!string.IsNullOrEmpty(item.ListView.Items[itemIndex].Text))
                    break;

                sb.AppendLine();
                sb.Append(item.ListView.Items[itemIndex].SubItems[3].Text);
                itemIndex++;
            }

            textBoxDefinition.Text = sb.ToString();
            buttonOK.Focus();
        }

        private void RecordView_KeyDown(object sender, KeyEventArgs e)
        {
            //Main.Log("KeyCode:" + e.KeyCode + " KeyData:" + e.KeyData + " Modifiers:" + e.Modifiers);
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                return;
            }

            if ((e.Modifiers & Keys.Control) == Keys.Control && e.KeyCode == Keys.A)
            {
                textBoxDefinition.Focus();
                textBoxDefinition.SelectAll();
                return;
            }

            if ((e.Modifiers & Keys.Control) == Keys.Control && e.KeyCode == Keys.C)
            {
                Clipboard.SetText(textBoxDefinition.Text);
                return;
            }
        }
    }
}
