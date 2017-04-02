namespace TraceSpy
{
    partial class RecordView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordView));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxDefinition = new System.Windows.Forms.TextBox();
            this.panelMain = new System.Windows.Forms.Panel();
            this.labelProcessValue = new System.Windows.Forms.Label();
            this.labelProcess = new System.Windows.Forms.Label();
            this.labelTicksValue = new System.Windows.Forms.Label();
            this.labelTicks = new System.Windows.Forms.Label();
            this.labelIndexValue = new System.Windows.Forms.Label();
            this.labelIndex = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.buttonOK, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.textBoxDefinition, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.panelMain, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(599, 448);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(262, 421);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "&Close";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // textBoxDefinition
            // 
            this.textBoxDefinition.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxDefinition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDefinition.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDefinition.Location = new System.Drawing.Point(3, 33);
            this.textBoxDefinition.Multiline = true;
            this.textBoxDefinition.Name = "textBoxDefinition";
            this.textBoxDefinition.ReadOnly = true;
            this.textBoxDefinition.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDefinition.Size = new System.Drawing.Size(593, 382);
            this.textBoxDefinition.TabIndex = 1;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.labelProcessValue);
            this.panelMain.Controls.Add(this.labelProcess);
            this.panelMain.Controls.Add(this.labelTicksValue);
            this.panelMain.Controls.Add(this.labelTicks);
            this.panelMain.Controls.Add(this.labelIndexValue);
            this.panelMain.Controls.Add(this.labelIndex);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(599, 30);
            this.panelMain.TabIndex = 0;
            // 
            // labelProcessValue
            // 
            this.labelProcessValue.AutoSize = true;
            this.labelProcessValue.Location = new System.Drawing.Point(274, 9);
            this.labelProcessValue.Name = "labelProcessValue";
            this.labelProcessValue.Size = new System.Drawing.Size(14, 13);
            this.labelProcessValue.TabIndex = 5;
            this.labelProcessValue.Text = "#";
            // 
            // labelProcess
            // 
            this.labelProcess.AutoSize = true;
            this.labelProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProcess.Location = new System.Drawing.Point(220, 9);
            this.labelProcess.Name = "labelProcess";
            this.labelProcess.Size = new System.Drawing.Size(56, 13);
            this.labelProcess.TabIndex = 4;
            this.labelProcess.Text = "Process:";
            // 
            // labelTicksValue
            // 
            this.labelTicksValue.AutoSize = true;
            this.labelTicksValue.Location = new System.Drawing.Point(137, 9);
            this.labelTicksValue.Name = "labelTicksValue";
            this.labelTicksValue.Size = new System.Drawing.Size(14, 13);
            this.labelTicksValue.TabIndex = 3;
            this.labelTicksValue.Text = "#";
            // 
            // labelTicks
            // 
            this.labelTicks.AutoSize = true;
            this.labelTicks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTicks.Location = new System.Drawing.Point(95, 9);
            this.labelTicks.Name = "labelTicks";
            this.labelTicks.Size = new System.Drawing.Size(42, 13);
            this.labelTicks.TabIndex = 2;
            this.labelTicks.Text = "Ticks:";
            // 
            // labelIndexValue
            // 
            this.labelIndexValue.AutoSize = true;
            this.labelIndexValue.Location = new System.Drawing.Point(54, 9);
            this.labelIndexValue.Name = "labelIndexValue";
            this.labelIndexValue.Size = new System.Drawing.Size(14, 13);
            this.labelIndexValue.TabIndex = 1;
            this.labelIndexValue.Text = "#";
            // 
            // labelIndex
            // 
            this.labelIndex.AutoSize = true;
            this.labelIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelIndex.Location = new System.Drawing.Point(12, 9);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(42, 13);
            this.labelIndex.TabIndex = 0;
            this.labelIndex.Text = "Index:";
            // 
            // RecordView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 448);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecordView";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Record";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecordView_KeyDown);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TextBox textBoxDefinition;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label labelIndexValue;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.Label labelTicksValue;
        private System.Windows.Forms.Label labelTicks;
        private System.Windows.Forms.Label labelProcessValue;
        private System.Windows.Forms.Label labelProcess;

    }
}