namespace TraceSpy
{
    partial class Find
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Find));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxWholeWord = new System.Windows.Forms.CheckBox();
            this.checkBoxCase = new System.Windows.Forms.CheckBox();
            this.comboBoxFind = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fi&nd what:";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(336, 37);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(336, 8);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&Find Next";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // checkBoxWholeWord
            // 
            this.checkBoxWholeWord.AutoSize = true;
            this.checkBoxWholeWord.Location = new System.Drawing.Point(16, 37);
            this.checkBoxWholeWord.Name = "checkBoxWholeWord";
            this.checkBoxWholeWord.Size = new System.Drawing.Size(135, 17);
            this.checkBoxWholeWord.TabIndex = 2;
            this.checkBoxWholeWord.Text = "Match &whole word only";
            this.checkBoxWholeWord.UseVisualStyleBackColor = true;
            // 
            // checkBoxCase
            // 
            this.checkBoxCase.AutoSize = true;
            this.checkBoxCase.Location = new System.Drawing.Point(16, 60);
            this.checkBoxCase.Name = "checkBoxCase";
            this.checkBoxCase.Size = new System.Drawing.Size(82, 17);
            this.checkBoxCase.TabIndex = 3;
            this.checkBoxCase.Text = "Match &case";
            this.checkBoxCase.UseVisualStyleBackColor = true;
            // 
            // comboBoxFind
            // 
            this.comboBoxFind.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxFind.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxFind.FormattingEnabled = true;
            this.comboBoxFind.Location = new System.Drawing.Point(75, 10);
            this.comboBoxFind.Name = "comboBoxFind";
            this.comboBoxFind.Size = new System.Drawing.Size(255, 21);
            this.comboBoxFind.TabIndex = 1;
            this.comboBoxFind.SelectedIndexChanged += new System.EventHandler(this.comboBoxFind_SelectedIndexChanged);
            this.comboBoxFind.TextUpdate += new System.EventHandler(this.comboBoxFind_TextUpdate);
            // 
            // Find
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(423, 101);
            this.Controls.Add(this.comboBoxFind);
            this.Controls.Add(this.checkBoxCase);
            this.Controls.Add(this.checkBoxWholeWord);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Find";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Find";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox checkBoxWholeWord;
        private System.Windows.Forms.CheckBox checkBoxCase;
        internal System.Windows.Forms.ComboBox comboBoxFind;
    }
}