namespace FreemanSaveEditor
{
    partial class FrmMultiple
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbMessage = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btAccept = new System.Windows.Forms.Button();
            this.btAccept2 = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbMessage
            // 
            this.lbMessage.AutoSize = true;
            this.lbMessage.Location = new System.Drawing.Point(27, 20);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(35, 13);
            this.lbMessage.TabIndex = 0;
            this.lbMessage.Text = "label1";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btAccept);
            this.flowLayoutPanel1.Controls.Add(this.btAccept2);
            this.flowLayoutPanel1.Controls.Add(this.btCancel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 57);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(463, 43);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // btAccept
            // 
            this.btAccept.AutoSize = true;
            this.btAccept.Location = new System.Drawing.Point(30, 0);
            this.btAccept.Margin = new System.Windows.Forms.Padding(30, 0, 30, 0);
            this.btAccept.Name = "btAccept";
            this.btAccept.Size = new System.Drawing.Size(75, 23);
            this.btAccept.TabIndex = 0;
            this.btAccept.Text = "button1";
            this.btAccept.UseVisualStyleBackColor = true;
            this.btAccept.Click += new System.EventHandler(this.BtAccept_Click);
            // 
            // btAccept2
            // 
            this.btAccept2.AutoSize = true;
            this.btAccept2.Location = new System.Drawing.Point(165, 0);
            this.btAccept2.Margin = new System.Windows.Forms.Padding(30, 0, 30, 0);
            this.btAccept2.Name = "btAccept2";
            this.btAccept2.Size = new System.Drawing.Size(75, 23);
            this.btAccept2.TabIndex = 1;
            this.btAccept2.Text = "button2";
            this.btAccept2.UseVisualStyleBackColor = true;
            this.btAccept2.Click += new System.EventHandler(this.BtAccept2_Click);
            // 
            // btCancel
            // 
            this.btCancel.AutoSize = true;
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(300, 0);
            this.btCancel.Margin = new System.Windows.Forms.Padding(30, 0, 30, 0);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "button3";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.BtCancel_Click);
            // 
            // FrmMultiple
            // 
            this.AcceptButton = this.btAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(463, 100);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lbMessage);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMultiple";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmMultiple";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btAccept;
        private System.Windows.Forms.Button btAccept2;
        private System.Windows.Forms.Button btCancel;
    }
}