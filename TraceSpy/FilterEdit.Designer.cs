namespace TraceSpy
{
    partial class FilterEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterEdit));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButtonIncludeAll = new System.Windows.Forms.RadioButton();
            this.radioButtonInclude = new System.Windows.Forms.RadioButton();
            this.radioButtonExclude = new System.Windows.Forms.RadioButton();
            this.textBoxDefinition = new System.Windows.Forms.TextBox();
            this.checkBoxIgnoreCase = new System.Windows.Forms.CheckBox();
            this.checkBoxActive = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButtonFilterText = new System.Windows.Forms.RadioButton();
            this.radioButtonFilterProcessName = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(240, 115);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(159, 115);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Definition:";
            // 
            // radioButtonIncludeAll
            // 
            this.radioButtonIncludeAll.AutoSize = true;
            this.radioButtonIncludeAll.Location = new System.Drawing.Point(78, 34);
            this.radioButtonIncludeAll.Name = "radioButtonIncludeAll";
            this.radioButtonIncludeAll.Size = new System.Drawing.Size(74, 17);
            this.radioButtonIncludeAll.TabIndex = 1;
            this.radioButtonIncludeAll.TabStop = true;
            this.radioButtonIncludeAll.Text = "Include All";
            this.radioButtonIncludeAll.UseVisualStyleBackColor = true;
            this.radioButtonIncludeAll.CheckedChanged += new System.EventHandler(this.RadioButtonIncludeAllCheckedChanged);
            // 
            // radioButtonInclude
            // 
            this.radioButtonInclude.AutoSize = true;
            this.radioButtonInclude.Location = new System.Drawing.Point(178, 34);
            this.radioButtonInclude.Name = "radioButtonInclude";
            this.radioButtonInclude.Size = new System.Drawing.Size(60, 17);
            this.radioButtonInclude.TabIndex = 2;
            this.radioButtonInclude.TabStop = true;
            this.radioButtonInclude.Text = "Include";
            this.radioButtonInclude.UseVisualStyleBackColor = true;
            this.radioButtonInclude.CheckedChanged += new System.EventHandler(this.RadioButtonIncludeCheckedChanged);
            // 
            // radioButtonExclude
            // 
            this.radioButtonExclude.AutoSize = true;
            this.radioButtonExclude.Location = new System.Drawing.Point(269, 34);
            this.radioButtonExclude.Name = "radioButtonExclude";
            this.radioButtonExclude.Size = new System.Drawing.Size(63, 17);
            this.radioButtonExclude.TabIndex = 3;
            this.radioButtonExclude.TabStop = true;
            this.radioButtonExclude.Text = "Exclude";
            this.radioButtonExclude.UseVisualStyleBackColor = true;
            this.radioButtonExclude.CheckedChanged += new System.EventHandler(this.RadioButtonExcludeCheckedChanged);
            // 
            // textBoxDefinition
            // 
            this.textBoxDefinition.Location = new System.Drawing.Point(78, 64);
            this.textBoxDefinition.Name = "textBoxDefinition";
            this.textBoxDefinition.Size = new System.Drawing.Size(385, 20);
            this.textBoxDefinition.TabIndex = 4;
            this.textBoxDefinition.TextChanged += new System.EventHandler(this.TextBoxDefinitionTextChanged);
            // 
            // checkBoxIgnoreCase
            // 
            this.checkBoxIgnoreCase.AutoSize = true;
            this.checkBoxIgnoreCase.Checked = true;
            this.checkBoxIgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIgnoreCase.Location = new System.Drawing.Point(78, 90);
            this.checkBoxIgnoreCase.Name = "checkBoxIgnoreCase";
            this.checkBoxIgnoreCase.Size = new System.Drawing.Size(83, 17);
            this.checkBoxIgnoreCase.TabIndex = 5;
            this.checkBoxIgnoreCase.Text = "Ignore Case";
            this.checkBoxIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // checkBoxActive
            // 
            this.checkBoxActive.AutoSize = true;
            this.checkBoxActive.Checked = true;
            this.checkBoxActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxActive.Location = new System.Drawing.Point(178, 90);
            this.checkBoxActive.Name = "checkBoxActive";
            this.checkBoxActive.Size = new System.Drawing.Size(56, 17);
            this.checkBoxActive.TabIndex = 6;
            this.checkBoxActive.Text = "Active";
            this.checkBoxActive.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Filter:";
            // 
            // radioButtonFilterText
            // 
            this.radioButtonFilterText.AutoSize = true;
            this.radioButtonFilterText.Location = new System.Drawing.Point(3, 5);
            this.radioButtonFilterText.Name = "radioButtonFilterText";
            this.radioButtonFilterText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonFilterText.TabIndex = 1;
            this.radioButtonFilterText.TabStop = true;
            this.radioButtonFilterText.Text = "Text";
            this.radioButtonFilterText.UseVisualStyleBackColor = true;
            this.radioButtonFilterText.CheckedChanged += new System.EventHandler(this.RadioButtonFilterTextCheckedChanged);
            // 
            // radioButtonFilterProcessName
            // 
            this.radioButtonFilterProcessName.AutoSize = true;
            this.radioButtonFilterProcessName.Location = new System.Drawing.Point(103, 5);
            this.radioButtonFilterProcessName.Name = "radioButtonFilterProcessName";
            this.radioButtonFilterProcessName.Size = new System.Drawing.Size(94, 17);
            this.radioButtonFilterProcessName.TabIndex = 2;
            this.radioButtonFilterProcessName.TabStop = true;
            this.radioButtonFilterProcessName.Text = "Process Name";
            this.radioButtonFilterProcessName.UseVisualStyleBackColor = true;
            this.radioButtonFilterProcessName.CheckedChanged += new System.EventHandler(this.RadioButtonFilterProcessNameCheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonFilterText);
            this.panel1.Controls.Add(this.radioButtonFilterProcessName);
            this.panel1.Location = new System.Drawing.Point(75, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(254, 21);
            this.panel1.TabIndex = 9;
            // 
            // FilterEdit
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(475, 146);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.checkBoxActive);
            this.Controls.Add(this.checkBoxIgnoreCase);
            this.Controls.Add(this.textBoxDefinition);
            this.Controls.Add(this.radioButtonExclude);
            this.Controls.Add(this.radioButtonInclude);
            this.Controls.Add(this.radioButtonIncludeAll);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterEdit";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FilterEditFormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButtonIncludeAll;
        private System.Windows.Forms.RadioButton radioButtonInclude;
        private System.Windows.Forms.RadioButton radioButtonExclude;
        private System.Windows.Forms.TextBox textBoxDefinition;
        private System.Windows.Forms.CheckBox checkBoxIgnoreCase;
        private System.Windows.Forms.CheckBox checkBoxActive;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButtonFilterText;
        private System.Windows.Forms.RadioButton radioButtonFilterProcessName;
        private System.Windows.Forms.Panel panel1;
    }
}