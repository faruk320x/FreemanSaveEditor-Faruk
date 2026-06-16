using System;
using System.Windows.Forms;

namespace FreemanSaveEditor
{
    public partial class FrmMultiple : Form
    {
        public FrmMultiple(string title, string message, string acceptText, string cancelText = "", string accept2Text = "")
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(cancelText))
                btCancel.Visible = false;
            if (string.IsNullOrWhiteSpace(accept2Text))
                btAccept2.Visible = false;

            this.Text = title;
            lbMessage.Text = message;
            btAccept.Text = acceptText;
            btAccept2.Text = accept2Text;
            btCancel.Text = cancelText;
        }

        private void BtAccept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void BtAccept2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void BtCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}