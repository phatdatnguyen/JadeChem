namespace JadeChem.Dialogs
{
    partial class MoleculeDialog
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
            tableLayoutPanel = new TableLayoutPanel();
            panel1 = new Panel();
            smilesTextBox = new TextBox();
            smilesLabel = new Label();
            moleculePictureBox = new PictureBox();
            panel2 = new Panel();
            closeButton = new Button();
            tableLayoutPanel.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)moleculePictureBox).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(panel1, 0, 0);
            tableLayoutPanel.Controls.Add(moleculePictureBox, 0, 1);
            tableLayoutPanel.Controls.Add(panel2, 0, 2);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 3;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.Size = new Size(284, 361);
            tableLayoutPanel.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(smilesTextBox);
            panel1.Controls.Add(smilesLabel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(278, 34);
            panel1.TabIndex = 0;
            // 
            // smilesTextBox
            // 
            smilesTextBox.Location = new Point(59, 5);
            smilesTextBox.Name = "smilesTextBox";
            smilesTextBox.ReadOnly = true;
            smilesTextBox.Size = new Size(210, 23);
            smilesTextBox.TabIndex = 0;
            // 
            // smilesLabel
            // 
            smilesLabel.AutoSize = true;
            smilesLabel.Location = new Point(8, 10);
            smilesLabel.Name = "smilesLabel";
            smilesLabel.Size = new Size(45, 15);
            smilesLabel.TabIndex = 0;
            smilesLabel.Text = "SMILES";
            // 
            // moleculePictureBox
            // 
            moleculePictureBox.Dock = DockStyle.Fill;
            moleculePictureBox.Location = new Point(3, 43);
            moleculePictureBox.Name = "moleculePictureBox";
            moleculePictureBox.Size = new Size(278, 275);
            moleculePictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            moleculePictureBox.TabIndex = 2;
            moleculePictureBox.TabStop = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(closeButton);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 324);
            panel2.Name = "panel2";
            panel2.Size = new Size(278, 34);
            panel2.TabIndex = 1;
            // 
            // closeButton
            // 
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.DialogResult = DialogResult.Cancel;
            closeButton.Location = new Point(200, 8);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 23);
            closeButton.TabIndex = 0;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Click += CloseButton_Click;
            // 
            // MoleculeDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(284, 361);
            Controls.Add(tableLayoutPanel);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(300, 400);
            Name = "MoleculeDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Molecule";
            FormClosed += MoleculeDialog_FormClosed;
            Load += MoleculeDialog_Load;
            tableLayoutPanel.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)moleculePictureBox).EndInit();
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private TextBox smilesTextBox;
        private Label smilesLabel;
        private PictureBox moleculePictureBox;
        private Panel panel1;
        private Panel panel2;
        private Button closeButton;
    }
}