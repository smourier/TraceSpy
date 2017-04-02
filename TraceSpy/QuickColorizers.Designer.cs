namespace TraceSpy
{
    partial class QuickColorizers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickColorizers));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewQuickColorizers = new System.Windows.Forms.ListView();
            this.columnHeaderActive = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderIgnoreCase = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderWholeWord = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderColor = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderText = new System.Windows.Forms.ColumnHeader();
            this.buttonModifyQuickColorizer = new System.Windows.Forms.Button();
            this.buttonRemoveQuickColorizer = new System.Windows.Forms.Button();
            this.buttonAddQuickColorizer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(558, 183);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(477, 183);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Texts to colorize:";
            // 
            // listViewQuickColorizers
            // 
            this.listViewQuickColorizers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderActive,
            this.columnHeaderIgnoreCase,
            this.columnHeaderWholeWord,
            this.columnHeaderColor,
            this.columnHeaderText});
            this.listViewQuickColorizers.FullRowSelect = true;
            this.listViewQuickColorizers.Location = new System.Drawing.Point(16, 30);
            this.listViewQuickColorizers.MultiSelect = false;
            this.listViewQuickColorizers.Name = "listViewQuickColorizers";
            this.listViewQuickColorizers.Size = new System.Drawing.Size(617, 147);
            this.listViewQuickColorizers.TabIndex = 1;
            this.listViewQuickColorizers.UseCompatibleStateImageBehavior = false;
            this.listViewQuickColorizers.View = System.Windows.Forms.View.Details;
            this.listViewQuickColorizers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListViewQuickColorizersMouseDoubleClick);
            this.listViewQuickColorizers.SelectedIndexChanged += new System.EventHandler(this.ListViewQuickColorizersSelectedIndexChanged);
            // 
            // columnHeaderActive
            // 
            this.columnHeaderActive.Text = "Active";
            this.columnHeaderActive.Width = 56;
            // 
            // columnHeaderIgnoreCase
            // 
            this.columnHeaderIgnoreCase.Text = "Ignore Case";
            this.columnHeaderIgnoreCase.Width = 76;
            // 
            // columnHeaderWholeWord
            // 
            this.columnHeaderWholeWord.Text = "Whole Text";
            this.columnHeaderWholeWord.Width = 84;
            // 
            // columnHeaderColor
            // 
            this.columnHeaderColor.Text = "Color Set";
            this.columnHeaderColor.Width = 86;
            // 
            // columnHeaderText
            // 
            this.columnHeaderText.Text = "Text";
            this.columnHeaderText.Width = 300;
            // 
            // buttonModifyQuickColorizer
            // 
            this.buttonModifyQuickColorizer.Enabled = false;
            this.buttonModifyQuickColorizer.Location = new System.Drawing.Point(177, 183);
            this.buttonModifyQuickColorizer.Name = "buttonModifyQuickColorizer";
            this.buttonModifyQuickColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonModifyQuickColorizer.TabIndex = 4;
            this.buttonModifyQuickColorizer.Text = "&Modify";
            this.buttonModifyQuickColorizer.UseVisualStyleBackColor = true;
            this.buttonModifyQuickColorizer.Click += new System.EventHandler(this.buttonModifyQuickColorizer_Click);
            // 
            // buttonRemoveQuickColorizer
            // 
            this.buttonRemoveQuickColorizer.Enabled = false;
            this.buttonRemoveQuickColorizer.Location = new System.Drawing.Point(96, 183);
            this.buttonRemoveQuickColorizer.Name = "buttonRemoveQuickColorizer";
            this.buttonRemoveQuickColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveQuickColorizer.TabIndex = 3;
            this.buttonRemoveQuickColorizer.Text = "&Remove";
            this.buttonRemoveQuickColorizer.UseVisualStyleBackColor = true;
            this.buttonRemoveQuickColorizer.Click += new System.EventHandler(this.buttonRemoveQuickColorizer_Click);
            // 
            // buttonAddQuickColorizer
            // 
            this.buttonAddQuickColorizer.Location = new System.Drawing.Point(15, 183);
            this.buttonAddQuickColorizer.Name = "buttonAddQuickColorizer";
            this.buttonAddQuickColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonAddQuickColorizer.TabIndex = 2;
            this.buttonAddQuickColorizer.Text = "&Add";
            this.buttonAddQuickColorizer.UseVisualStyleBackColor = true;
            this.buttonAddQuickColorizer.Click += new System.EventHandler(this.buttonAddQuickColorizer_Click);
            // 
            // QuickColorizers
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(645, 216);
            this.Controls.Add(this.buttonModifyQuickColorizer);
            this.Controls.Add(this.buttonRemoveQuickColorizer);
            this.Controls.Add(this.buttonAddQuickColorizer);
            this.Controls.Add(this.listViewQuickColorizers);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QuickColorizers";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quick Colorizers";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickColorizersFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewQuickColorizers;
        private System.Windows.Forms.ColumnHeader columnHeaderActive;
        private System.Windows.Forms.ColumnHeader columnHeaderIgnoreCase;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.Button buttonModifyQuickColorizer;
        private System.Windows.Forms.Button buttonRemoveQuickColorizer;
        private System.Windows.Forms.Button buttonAddQuickColorizer;
        private System.Windows.Forms.ColumnHeader columnHeaderWholeWord;
        private System.Windows.Forms.ColumnHeader columnHeaderColor;
    }
}