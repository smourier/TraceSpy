namespace TraceSpy
{
    partial class ColorSetEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorSetEdit));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonFill = new System.Windows.Forms.RadioButton();
            this.radioButtonFrame = new System.Windows.Forms.RadioButton();
            this.labelFrameWidth = new System.Windows.Forms.Label();
            this.numericUpDownFrameWidth = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxBackColor = new System.Windows.Forms.TextBox();
            this.textBoxForeColor = new System.Windows.Forms.TextBox();
            this.buttonBackColor = new System.Windows.Forms.Button();
            this.buttonForeColor = new System.Windows.Forms.Button();
            this.buttonFont = new System.Windows.Forms.Button();
            this.textBoxFont = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrameWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(129, 190);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(48, 190);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 16;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(57, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(186, 20);
            this.textBoxName.TabIndex = 1;
            this.textBoxName.TextChanged += new System.EventHandler(this.TextBoxNameTextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 127);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Background Mode:";
            // 
            // radioButtonFill
            // 
            this.radioButtonFill.AutoSize = true;
            this.radioButtonFill.Checked = true;
            this.radioButtonFill.Location = new System.Drawing.Point(129, 125);
            this.radioButtonFill.Name = "radioButtonFill";
            this.radioButtonFill.Size = new System.Drawing.Size(37, 17);
            this.radioButtonFill.TabIndex = 12;
            this.radioButtonFill.TabStop = true;
            this.radioButtonFill.Text = "Fill";
            this.radioButtonFill.UseVisualStyleBackColor = true;
            this.radioButtonFill.CheckedChanged += new System.EventHandler(this.RadioButtonFillCheckedChanged);
            // 
            // radioButtonFrame
            // 
            this.radioButtonFrame.AutoSize = true;
            this.radioButtonFrame.Location = new System.Drawing.Point(189, 125);
            this.radioButtonFrame.Name = "radioButtonFrame";
            this.radioButtonFrame.Size = new System.Drawing.Size(54, 17);
            this.radioButtonFrame.TabIndex = 13;
            this.radioButtonFrame.Text = "Frame";
            this.radioButtonFrame.UseVisualStyleBackColor = true;
            this.radioButtonFrame.CheckedChanged += new System.EventHandler(this.RadioButtonFrameCheckedChanged);
            // 
            // labelFrameWidth
            // 
            this.labelFrameWidth.AutoSize = true;
            this.labelFrameWidth.Location = new System.Drawing.Point(13, 155);
            this.labelFrameWidth.Name = "labelFrameWidth";
            this.labelFrameWidth.Size = new System.Drawing.Size(70, 13);
            this.labelFrameWidth.TabIndex = 14;
            this.labelFrameWidth.Text = "Frame Width:";
            // 
            // numericUpDownFrameWidth
            // 
            this.numericUpDownFrameWidth.Location = new System.Drawing.Point(113, 153);
            this.numericUpDownFrameWidth.Name = "numericUpDownFrameWidth";
            this.numericUpDownFrameWidth.Size = new System.Drawing.Size(40, 20);
            this.numericUpDownFrameWidth.TabIndex = 15;
            this.numericUpDownFrameWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Background Color:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Foreground Color:";
            // 
            // textBoxBackColor
            // 
            this.textBoxBackColor.Location = new System.Drawing.Point(113, 96);
            this.textBoxBackColor.Name = "textBoxBackColor";
            this.textBoxBackColor.Size = new System.Drawing.Size(100, 20);
            this.textBoxBackColor.TabIndex = 9;
            this.textBoxBackColor.TextChanged += new System.EventHandler(this.textBoxBackColor_TextChanged);
            // 
            // textBoxForeColor
            // 
            this.textBoxForeColor.Location = new System.Drawing.Point(113, 68);
            this.textBoxForeColor.Name = "textBoxForeColor";
            this.textBoxForeColor.Size = new System.Drawing.Size(100, 20);
            this.textBoxForeColor.TabIndex = 6;
            this.textBoxForeColor.TextChanged += new System.EventHandler(this.textBoxForeColor_TextChanged);
            // 
            // buttonBackColor
            // 
            this.buttonBackColor.Location = new System.Drawing.Point(219, 94);
            this.buttonBackColor.Name = "buttonBackColor";
            this.buttonBackColor.Size = new System.Drawing.Size(24, 23);
            this.buttonBackColor.TabIndex = 10;
            this.buttonBackColor.Text = "...";
            this.buttonBackColor.UseVisualStyleBackColor = true;
            this.buttonBackColor.Click += new System.EventHandler(this.ButtonBackColorClick);
            // 
            // buttonForeColor
            // 
            this.buttonForeColor.Location = new System.Drawing.Point(219, 65);
            this.buttonForeColor.Name = "buttonForeColor";
            this.buttonForeColor.Size = new System.Drawing.Size(24, 23);
            this.buttonForeColor.TabIndex = 7;
            this.buttonForeColor.Text = "...";
            this.buttonForeColor.UseVisualStyleBackColor = true;
            this.buttonForeColor.Click += new System.EventHandler(this.ButtonForeColorClick);
            // 
            // buttonFont
            // 
            this.buttonFont.Location = new System.Drawing.Point(219, 37);
            this.buttonFont.Name = "buttonFont";
            this.buttonFont.Size = new System.Drawing.Size(24, 23);
            this.buttonFont.TabIndex = 4;
            this.buttonFont.Text = "...";
            this.buttonFont.UseVisualStyleBackColor = true;
            this.buttonFont.Click += new System.EventHandler(this.ButtonFontClick);
            // 
            // textBoxFont
            // 
            this.textBoxFont.Location = new System.Drawing.Point(57, 40);
            this.textBoxFont.Name = "textBoxFont";
            this.textBoxFont.Size = new System.Drawing.Size(156, 20);
            this.textBoxFont.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Font:";
            // 
            // ColorSetEdit
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(253, 224);
            this.Controls.Add(this.buttonFont);
            this.Controls.Add(this.textBoxFont);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonForeColor);
            this.Controls.Add(this.buttonBackColor);
            this.Controls.Add(this.textBoxForeColor);
            this.Controls.Add(this.textBoxBackColor);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDownFrameWidth);
            this.Controls.Add(this.labelFrameWidth);
            this.Controls.Add(this.radioButtonFrame);
            this.Controls.Add(this.radioButtonFill);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorSetEdit";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Color Set";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColorSetEditFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrameWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonFill;
        private System.Windows.Forms.RadioButton radioButtonFrame;
        private System.Windows.Forms.Label labelFrameWidth;
        private System.Windows.Forms.NumericUpDown numericUpDownFrameWidth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxBackColor;
        private System.Windows.Forms.TextBox textBoxForeColor;
        private System.Windows.Forms.Button buttonBackColor;
        private System.Windows.Forms.Button buttonForeColor;
        private System.Windows.Forms.Button buttonFont;
        private System.Windows.Forms.TextBox textBoxFont;
        private System.Windows.Forms.Label label3;
    }
}