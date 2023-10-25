namespace JadeChem.CustomControls.ModelControls
{
    partial class MinimumMeanDistanceModelControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            minimumMeanDistanceLabel = new Label();
            trainButton = new Button();
            SuspendLayout();
            // 
            // minimumMeanDistanceLabel
            // 
            minimumMeanDistanceLabel.AutoSize = true;
            minimumMeanDistanceLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            minimumMeanDistanceLabel.Location = new Point(20, 20);
            minimumMeanDistanceLabel.Name = "minimumMeanDistanceLabel";
            minimumMeanDistanceLabel.Size = new Size(145, 15);
            minimumMeanDistanceLabel.TabIndex = 0;
            minimumMeanDistanceLabel.Text = "Minimum Mean Distance";
            // 
            // trainButton
            // 
            trainButton.Location = new Point(20, 116);
            trainButton.Name = "trainButton";
            trainButton.Size = new Size(75, 23);
            trainButton.TabIndex = 1;
            trainButton.Text = "Train";
            trainButton.UseVisualStyleBackColor = true;
            trainButton.Click += TrainButton_Click;
            // 
            // MinimumMeanDistanceModelControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(trainButton);
            Controls.Add(minimumMeanDistanceLabel);
            Name = "MinimumMeanDistanceModelControl";
            Size = new Size(790, 350);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label minimumMeanDistanceLabel;
        private Button trainButton;
    }
}
