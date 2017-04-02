namespace TraceSpy
{
    partial class Colorizers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Colorizers));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewColorizers = new System.Windows.Forms.ListView();
            this.columnHeaderActive = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderIgnoreCase = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDefinition = new System.Windows.Forms.ColumnHeader();
            this.buttonModifyColorizer = new System.Windows.Forms.Button();
            this.buttonRemoveColorizer = new System.Windows.Forms.Button();
            this.buttonAddColorizer = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listViewColorSets = new System.Windows.Forms.ListView();
            this.columnHeaderName = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderForeColor = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderBackColor = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderMode = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderFrameWidth = new System.Windows.Forms.ColumnHeader();
            this.buttonModifyColorSet = new System.Windows.Forms.Button();
            this.buttonRemoveColorSet = new System.Windows.Forms.Button();
            this.buttonAddColorSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(325, 432);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(244, 432);
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
            this.label1.Size = new System.Drawing.Size(150, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Regular Expressions to match:";
            // 
            // listViewColorizers
            // 
            this.listViewColorizers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderActive,
            this.columnHeaderIgnoreCase,
            this.columnHeaderDefinition});
            this.listViewColorizers.FullRowSelect = true;
            this.listViewColorizers.Location = new System.Drawing.Point(16, 30);
            this.listViewColorizers.MultiSelect = false;
            this.listViewColorizers.Name = "listViewColorizers";
            this.listViewColorizers.Size = new System.Drawing.Size(617, 147);
            this.listViewColorizers.TabIndex = 1;
            this.listViewColorizers.UseCompatibleStateImageBehavior = false;
            this.listViewColorizers.View = System.Windows.Forms.View.Details;
            this.listViewColorizers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListViewColorizersMouseDoubleClick);
            this.listViewColorizers.SelectedIndexChanged += new System.EventHandler(this.ListViewColorizersSelectedIndexChanged);
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
            // columnHeaderDefinition
            // 
            this.columnHeaderDefinition.Text = "Definition";
            this.columnHeaderDefinition.Width = 436;
            // 
            // buttonModifyColorizer
            // 
            this.buttonModifyColorizer.Enabled = false;
            this.buttonModifyColorizer.Location = new System.Drawing.Point(177, 183);
            this.buttonModifyColorizer.Name = "buttonModifyColorizer";
            this.buttonModifyColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonModifyColorizer.TabIndex = 4;
            this.buttonModifyColorizer.Text = "&Modify";
            this.buttonModifyColorizer.UseVisualStyleBackColor = true;
            this.buttonModifyColorizer.Click += new System.EventHandler(this.ButtonModifyColorizerClick);
            // 
            // buttonRemoveColorizer
            // 
            this.buttonRemoveColorizer.Enabled = false;
            this.buttonRemoveColorizer.Location = new System.Drawing.Point(96, 183);
            this.buttonRemoveColorizer.Name = "buttonRemoveColorizer";
            this.buttonRemoveColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveColorizer.TabIndex = 3;
            this.buttonRemoveColorizer.Text = "&Remove";
            this.buttonRemoveColorizer.UseVisualStyleBackColor = true;
            this.buttonRemoveColorizer.Click += new System.EventHandler(this.ButtonRemoveColorizerClick);
            // 
            // buttonAddColorizer
            // 
            this.buttonAddColorizer.Location = new System.Drawing.Point(15, 183);
            this.buttonAddColorizer.Name = "buttonAddColorizer";
            this.buttonAddColorizer.Size = new System.Drawing.Size(75, 23);
            this.buttonAddColorizer.TabIndex = 2;
            this.buttonAddColorizer.Text = "&Add";
            this.buttonAddColorizer.UseVisualStyleBackColor = true;
            this.buttonAddColorizer.Click += new System.EventHandler(this.ButtonAddColorizerClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 228);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Color Sets to use:";
            // 
            // listViewColorSets
            // 
            this.listViewColorSets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderForeColor,
            this.columnHeaderBackColor,
            this.columnHeaderMode,
            this.columnHeaderFrameWidth});
            this.listViewColorSets.FullRowSelect = true;
            this.listViewColorSets.Location = new System.Drawing.Point(16, 244);
            this.listViewColorSets.MultiSelect = false;
            this.listViewColorSets.Name = "listViewColorSets";
            this.listViewColorSets.Size = new System.Drawing.Size(617, 147);
            this.listViewColorSets.TabIndex = 6;
            this.listViewColorSets.UseCompatibleStateImageBehavior = false;
            this.listViewColorSets.View = System.Windows.Forms.View.Details;
            this.listViewColorSets.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListViewColorSetsMouseDoubleClick);
            this.listViewColorSets.SelectedIndexChanged += new System.EventHandler(this.ListViewColorSetsSelectedIndexChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 56;
            // 
            // columnHeaderForeColor
            // 
            this.columnHeaderForeColor.Text = "Fore Color";
            this.columnHeaderForeColor.Width = 86;
            // 
            // columnHeaderBackColor
            // 
            this.columnHeaderBackColor.Text = "Back Color";
            this.columnHeaderBackColor.Width = 109;
            // 
            // columnHeaderMode
            // 
            this.columnHeaderMode.Text = "Fill Mode";
            // 
            // columnHeaderFrameWidth
            // 
            this.columnHeaderFrameWidth.Text = "Frame Width";
            this.columnHeaderFrameWidth.Width = 88;
            // 
            // buttonModifyColorSet
            // 
            this.buttonModifyColorSet.Enabled = false;
            this.buttonModifyColorSet.Location = new System.Drawing.Point(178, 397);
            this.buttonModifyColorSet.Name = "buttonModifyColorSet";
            this.buttonModifyColorSet.Size = new System.Drawing.Size(75, 23);
            this.buttonModifyColorSet.TabIndex = 9;
            this.buttonModifyColorSet.Text = "&Modify";
            this.buttonModifyColorSet.UseVisualStyleBackColor = true;
            this.buttonModifyColorSet.Click += new System.EventHandler(this.ButtonModifyColorSetClick);
            // 
            // buttonRemoveColorSet
            // 
            this.buttonRemoveColorSet.Enabled = false;
            this.buttonRemoveColorSet.Location = new System.Drawing.Point(97, 397);
            this.buttonRemoveColorSet.Name = "buttonRemoveColorSet";
            this.buttonRemoveColorSet.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveColorSet.TabIndex = 8;
            this.buttonRemoveColorSet.Text = "&Remove";
            this.buttonRemoveColorSet.UseVisualStyleBackColor = true;
            this.buttonRemoveColorSet.Click += new System.EventHandler(this.ButtonRemoveColorSetClick);
            // 
            // buttonAddColorSet
            // 
            this.buttonAddColorSet.Location = new System.Drawing.Point(16, 397);
            this.buttonAddColorSet.Name = "buttonAddColorSet";
            this.buttonAddColorSet.Size = new System.Drawing.Size(75, 23);
            this.buttonAddColorSet.TabIndex = 7;
            this.buttonAddColorSet.Text = "&Add";
            this.buttonAddColorSet.UseVisualStyleBackColor = true;
            this.buttonAddColorSet.Click += new System.EventHandler(this.ButtonAddColorSetClick);
            // 
            // Colorizers
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(645, 469);
            this.Controls.Add(this.buttonModifyColorSet);
            this.Controls.Add(this.buttonRemoveColorSet);
            this.Controls.Add(this.buttonAddColorSet);
            this.Controls.Add(this.listViewColorSets);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonModifyColorizer);
            this.Controls.Add(this.buttonRemoveColorizer);
            this.Controls.Add(this.buttonAddColorizer);
            this.Controls.Add(this.listViewColorizers);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Colorizers";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Regex Colorizers";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColorizersFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewColorizers;
        private System.Windows.Forms.ColumnHeader columnHeaderActive;
        private System.Windows.Forms.ColumnHeader columnHeaderIgnoreCase;
        private System.Windows.Forms.ColumnHeader columnHeaderDefinition;
        private System.Windows.Forms.Button buttonModifyColorizer;
        private System.Windows.Forms.Button buttonRemoveColorizer;
        private System.Windows.Forms.Button buttonAddColorizer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listViewColorSets;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderForeColor;
        private System.Windows.Forms.ColumnHeader columnHeaderBackColor;
        private System.Windows.Forms.ColumnHeader columnHeaderMode;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameWidth;
        private System.Windows.Forms.Button buttonModifyColorSet;
        private System.Windows.Forms.Button buttonRemoveColorSet;
        private System.Windows.Forms.Button buttonAddColorSet;
    }
}