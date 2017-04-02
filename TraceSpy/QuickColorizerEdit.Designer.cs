namespace TraceSpy
{
    partial class QuickColorizerEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickColorizerEdit));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxIgnoreCase = new System.Windows.Forms.CheckBox();
            this.textBoxDefinition = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxActive = new System.Windows.Forms.CheckBox();
            this.checkBoxWholeText = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxColor = new System.Windows.Forms.TextBox();
            this.buttonColor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(166, 102);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(85, 102);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // checkBoxIgnoreCase
            // 
            this.checkBoxIgnoreCase.AutoSize = true;
            this.checkBoxIgnoreCase.Checked = true;
            this.checkBoxIgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxIgnoreCase.Location = new System.Drawing.Point(78, 38);
            this.checkBoxIgnoreCase.Name = "checkBoxIgnoreCase";
            this.checkBoxIgnoreCase.Size = new System.Drawing.Size(83, 17);
            this.checkBoxIgnoreCase.TabIndex = 2;
            this.checkBoxIgnoreCase.Text = "Ignore Case";
            this.checkBoxIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // textBoxDefinition
            // 
            this.textBoxDefinition.Location = new System.Drawing.Point(78, 12);
            this.textBoxDefinition.Name = "textBoxDefinition";
            this.textBoxDefinition.Size = new System.Drawing.Size(237, 20);
            this.textBoxDefinition.TabIndex = 1;
            this.textBoxDefinition.TextChanged += new System.EventHandler(this.TextBoxDefinitionTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Text:";
            // 
            // checkBoxActive
            // 
            this.checkBoxActive.AutoSize = true;
            this.checkBoxActive.Checked = true;
            this.checkBoxActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxActive.Location = new System.Drawing.Point(259, 38);
            this.checkBoxActive.Name = "checkBoxActive";
            this.checkBoxActive.Size = new System.Drawing.Size(56, 17);
            this.checkBoxActive.TabIndex = 4;
            this.checkBoxActive.Text = "Active";
            this.checkBoxActive.UseVisualStyleBackColor = true;
            // 
            // checkBoxWholeText
            // 
            this.checkBoxWholeText.AutoSize = true;
            this.checkBoxWholeText.Checked = true;
            this.checkBoxWholeText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWholeText.Location = new System.Drawing.Point(166, 38);
            this.checkBoxWholeText.Name = "checkBoxWholeText";
            this.checkBoxWholeText.Size = new System.Drawing.Size(81, 17);
            this.checkBoxWholeText.TabIndex = 3;
            this.checkBoxWholeText.Text = "Whole Text";
            this.checkBoxWholeText.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Color:";
            // 
            // textBoxColor
            // 
            this.textBoxColor.Location = new System.Drawing.Point(78, 67);
            this.textBoxColor.Name = "textBoxColor";
            this.textBoxColor.ReadOnly = true;
            this.textBoxColor.Size = new System.Drawing.Size(206, 20);
            this.textBoxColor.TabIndex = 6;
            this.textBoxColor.TextChanged += new System.EventHandler(this.textBoxColor_TextChanged);
            // 
            // buttonColor
            // 
            this.buttonColor.Location = new System.Drawing.Point(290, 65);
            this.buttonColor.Name = "buttonColor";
            this.buttonColor.Size = new System.Drawing.Size(25, 23);
            this.buttonColor.TabIndex = 7;
            this.buttonColor.Text = "...";
            this.buttonColor.UseVisualStyleBackColor = true;
            this.buttonColor.Click += new System.EventHandler(this.buttonColor_Click);
            // 
            // QuickColorizerEdit
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(327, 139);
            this.Controls.Add(this.buttonColor);
            this.Controls.Add(this.textBoxColor);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxWholeText);
            this.Controls.Add(this.checkBoxActive);
            this.Controls.Add(this.checkBoxIgnoreCase);
            this.Controls.Add(this.textBoxDefinition);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QuickColorizerEdit";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quick Colorizer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickColorizerEditFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxIgnoreCase;
        private System.Windows.Forms.TextBox textBoxDefinition;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxActive;
        private System.Windows.Forms.CheckBox checkBoxWholeText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxColor;
        private System.Windows.Forms.Button buttonColor;
    }
}