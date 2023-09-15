namespace JadeChem.Dialogs
{
    partial class FeatureExtractionDialog
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
            ListViewGroup listViewGroup7 = new ListViewGroup("Molecular Properties", HorizontalAlignment.Left);
            ListViewGroup listViewGroup8 = new ListViewGroup("Calculated Properties", HorizontalAlignment.Left);
            ListViewGroup listViewGroup9 = new ListViewGroup("Molecular Fingerprints", HorizontalAlignment.Left);
            ListViewItem listViewItem67 = new ListViewItem(new string[] { "AMW", "", "Molecular average weight" }, -1);
            ListViewItem listViewItem68 = new ListViewItem(new string[] { "EMW", "", "Exact molecular weight of the molecule" }, -1);
            ListViewItem listViewItem69 = new ListViewItem(new string[] { "num_rings", "", "Total number of rings of the molecule" }, -1);
            ListViewItem listViewItem70 = new ListViewItem(new string[] { "num_het_rings", "", "Number of heterocycles " }, -1);
            ListViewItem listViewItem71 = new ListViewItem(new string[] { "num_sat_rings", "", "Number of saturated rings" }, -1);
            ListViewItem listViewItem72 = new ListViewItem(new string[] { "num_sat_carbo_rings", "", "Number of saturated carbocycles" }, -1);
            ListViewItem listViewItem73 = new ListViewItem(new string[] { "num_sat_het_rings", "", "Number of saturated heterocycles" }, -1);
            ListViewItem listViewItem74 = new ListViewItem(new string[] { "num_ali_rings", "", "Number of aliphatic rings" }, -1);
            ListViewItem listViewItem75 = new ListViewItem(new string[] { "num_ali_carbo_rings", "", "Number of aliphatic carbocycles" }, -1);
            ListViewItem listViewItem76 = new ListViewItem(new string[] { "num_ali_het_rings", "", "Number of aliphatic heterocycles" }, -1);
            ListViewItem listViewItem77 = new ListViewItem(new string[] { "num_arom_rings", "", "Number of aromatic rings" }, -1);
            ListViewItem listViewItem78 = new ListViewItem(new string[] { "num_arom_carbo_rings", "", "Number of aromatic carbocycles" }, -1);
            ListViewItem listViewItem79 = new ListViewItem(new string[] { "num_arom_het_rings", "", "Number of aromatic heterocycles" }, -1);
            ListViewItem listViewItem80 = new ListViewItem(new string[] { "num_atoms", "", "Number of atoms" }, -1);
            ListViewItem listViewItem81 = new ListViewItem(new string[] { "num_bridgehead_atoms", "", "Number of bridgehead atoms of the molecule." }, -1);
            ListViewItem listViewItem82 = new ListViewItem(new string[] { "num_spiro_atoms", "", "Number of spiro atoms of the molecule" }, -1);
            ListViewItem listViewItem83 = new ListViewItem(new string[] { "num_het_atoms", "", "Number of hetero atoms" }, -1);
            ListViewItem listViewItem84 = new ListViewItem(new string[] { "num_heavy_atoms", "", "Number of heavy atoms" }, -1);
            ListViewItem listViewItem85 = new ListViewItem(new string[] { "fraction_CSP3", "", "The ratio of sp3-hybridized carbons over the total carbon count of the molecule." }, -1);
            ListViewItem listViewItem86 = new ListViewItem(new string[] { "num_hba", "", "Number of hydrogenbond acceptors" }, -1);
            ListViewItem listViewItem87 = new ListViewItem(new string[] { "num_hbd", "", "Number of hydrogen bond donors" }, -1);
            ListViewItem listViewItem88 = new ListViewItem(new string[] { "num_rotatable_bonds", "", "Number of rotatable bonds of the molecule" }, -1);
            ListViewItem listViewItem89 = new ListViewItem(new string[] { "num_amide_bonds", "", "Number of amide bonds" }, -1);
            ListViewItem listViewItem90 = new ListViewItem(new string[] { "logP", "", "Partition coefficient of the molecule" }, -1);
            ListViewItem listViewItem91 = new ListViewItem(new string[] { "MR", "", "Molar refractivity of the molecule" }, -1);
            ListViewItem listViewItem92 = new ListViewItem(new string[] { "TPSA", "", "Topological polar surface area" }, -1);
            ListViewItem listViewItem93 = new ListViewItem(new string[] { "MACCS", "{ nBits=167 }", "MACCS keys" }, -1);
            ListViewItem listViewItem94 = new ListViewItem(new string[] { "AP_FP", "{ nBits=512 }", "Atom-pair fingerprint" }, -1);
            ListViewItem listViewItem95 = new ListViewItem(new string[] { "layered_FP", "{ nBits=2048 }", "Layered fingerprint" }, -1);
            ListViewItem listViewItem96 = new ListViewItem(new string[] { "Morgan_FP", "{ radius=2; nBits=512 }", "Morgan fingerprint" }, -1);
            ListViewItem listViewItem97 = new ListViewItem(new string[] { "pattern_FP", "{ nBits=2048 }", "Pattern fingerprint" }, -1);
            ListViewItem listViewItem98 = new ListViewItem(new string[] { "RDK_FP", "{ nBits=2048 }", "RDKit fingerprint" }, -1);
            ListViewItem listViewItem99 = new ListViewItem(new string[] { "TT_FP", "{ nBits=512 }", "Topological torsion fingerprint" }, -1);
            columnListBox = new ListBox();
            featuresListView = new ListView();
            featureColumnHeader = new ColumnHeader();
            parametersColumnHeader = new ColumnHeader();
            descriptionColumnHeader = new ColumnHeader();
            referenceColumnHeader = new ColumnHeader();
            columnLabel = new Label();
            featureLabel = new Label();
            okButton = new Button();
            cancelButton = new Button();
            editButton = new Button();
            SuspendLayout();
            // 
            // columnListBox
            // 
            columnListBox.FormattingEnabled = true;
            columnListBox.ItemHeight = 15;
            columnListBox.Location = new Point(13, 29);
            columnListBox.Margin = new Padding(4);
            columnListBox.Name = "columnListBox";
            columnListBox.Size = new Size(147, 439);
            columnListBox.TabIndex = 0;
            columnListBox.SelectedIndexChanged += ColumnListBox_SelectedIndexChanged;
            // 
            // featuresListView
            // 
            featuresListView.CheckBoxes = true;
            featuresListView.Columns.AddRange(new ColumnHeader[] { featureColumnHeader, parametersColumnHeader, descriptionColumnHeader, referenceColumnHeader });
            listViewGroup7.Header = "Molecular Properties";
            listViewGroup7.Name = "molecularPropertyListViewGroup";
            listViewGroup8.Header = "Calculated Properties";
            listViewGroup8.Name = "calculatedPropertiesListViewGroup";
            listViewGroup9.Header = "Molecular Fingerprints";
            listViewGroup9.Name = "molecularFingerprintsListViewGroup";
            featuresListView.Groups.AddRange(new ListViewGroup[] { listViewGroup7, listViewGroup8, listViewGroup9 });
            featuresListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listViewItem67.Group = listViewGroup7;
            listViewItem67.StateImageIndex = 0;
            listViewItem68.Group = listViewGroup7;
            listViewItem68.StateImageIndex = 0;
            listViewItem69.Group = listViewGroup7;
            listViewItem69.StateImageIndex = 0;
            listViewItem70.Group = listViewGroup7;
            listViewItem70.StateImageIndex = 0;
            listViewItem71.Group = listViewGroup7;
            listViewItem71.StateImageIndex = 0;
            listViewItem72.Group = listViewGroup7;
            listViewItem72.StateImageIndex = 0;
            listViewItem73.Group = listViewGroup7;
            listViewItem73.StateImageIndex = 0;
            listViewItem74.Group = listViewGroup7;
            listViewItem74.StateImageIndex = 0;
            listViewItem75.Group = listViewGroup7;
            listViewItem75.StateImageIndex = 0;
            listViewItem76.Group = listViewGroup7;
            listViewItem76.StateImageIndex = 0;
            listViewItem77.Group = listViewGroup7;
            listViewItem77.StateImageIndex = 0;
            listViewItem78.Group = listViewGroup7;
            listViewItem78.StateImageIndex = 0;
            listViewItem79.Group = listViewGroup7;
            listViewItem79.StateImageIndex = 0;
            listViewItem80.Group = listViewGroup7;
            listViewItem80.StateImageIndex = 0;
            listViewItem81.Group = listViewGroup7;
            listViewItem81.StateImageIndex = 0;
            listViewItem82.Group = listViewGroup7;
            listViewItem82.StateImageIndex = 0;
            listViewItem83.Group = listViewGroup7;
            listViewItem83.StateImageIndex = 0;
            listViewItem84.Group = listViewGroup7;
            listViewItem84.StateImageIndex = 0;
            listViewItem85.Group = listViewGroup7;
            listViewItem85.StateImageIndex = 0;
            listViewItem86.Group = listViewGroup7;
            listViewItem86.StateImageIndex = 0;
            listViewItem87.Group = listViewGroup7;
            listViewItem87.StateImageIndex = 0;
            listViewItem88.Group = listViewGroup7;
            listViewItem88.StateImageIndex = 0;
            listViewItem89.Group = listViewGroup7;
            listViewItem89.StateImageIndex = 0;
            listViewItem90.Group = listViewGroup8;
            listViewItem90.StateImageIndex = 0;
            listViewItem91.Group = listViewGroup8;
            listViewItem91.StateImageIndex = 0;
            listViewItem92.Group = listViewGroup8;
            listViewItem92.StateImageIndex = 0;
            listViewItem93.Group = listViewGroup9;
            listViewItem93.StateImageIndex = 0;
            listViewItem94.Group = listViewGroup9;
            listViewItem94.StateImageIndex = 0;
            listViewItem95.Group = listViewGroup9;
            listViewItem95.StateImageIndex = 0;
            listViewItem96.Group = listViewGroup9;
            listViewItem96.StateImageIndex = 0;
            listViewItem97.Group = listViewGroup9;
            listViewItem97.StateImageIndex = 0;
            listViewItem98.Group = listViewGroup9;
            listViewItem98.StateImageIndex = 0;
            listViewItem99.Group = listViewGroup9;
            listViewItem99.StateImageIndex = 0;
            featuresListView.Items.AddRange(new ListViewItem[] { listViewItem67, listViewItem68, listViewItem69, listViewItem70, listViewItem71, listViewItem72, listViewItem73, listViewItem74, listViewItem75, listViewItem76, listViewItem77, listViewItem78, listViewItem79, listViewItem80, listViewItem81, listViewItem82, listViewItem83, listViewItem84, listViewItem85, listViewItem86, listViewItem87, listViewItem88, listViewItem89, listViewItem90, listViewItem91, listViewItem92, listViewItem93, listViewItem94, listViewItem95, listViewItem96, listViewItem97, listViewItem98, listViewItem99 });
            featuresListView.Location = new Point(168, 29);
            featuresListView.Margin = new Padding(4);
            featuresListView.Name = "featuresListView";
            featuresListView.Size = new Size(723, 439);
            featuresListView.TabIndex = 1;
            featuresListView.UseCompatibleStateImageBehavior = false;
            featuresListView.View = View.Details;
            featuresListView.ItemChecked += FeaturesListView_ItemChecked;
            featuresListView.SelectedIndexChanged += FeaturesListView_SelectedIndexChanged;
            // 
            // featureColumnHeader
            // 
            featureColumnHeader.Text = "Feature";
            featureColumnHeader.Width = 150;
            // 
            // parametersColumnHeader
            // 
            parametersColumnHeader.Text = "Parameters";
            parametersColumnHeader.Width = 150;
            // 
            // descriptionColumnHeader
            // 
            descriptionColumnHeader.Text = "Description";
            descriptionColumnHeader.Width = 260;
            // 
            // referenceColumnHeader
            // 
            referenceColumnHeader.Text = "Reference";
            referenceColumnHeader.Width = 80;
            // 
            // columnLabel
            // 
            columnLabel.AutoSize = true;
            columnLabel.Location = new Point(14, 10);
            columnLabel.Margin = new Padding(4, 0, 4, 0);
            columnLabel.Name = "columnLabel";
            columnLabel.Size = new Size(50, 15);
            columnLabel.TabIndex = 0;
            columnLabel.Text = "Column";
            // 
            // featureLabel
            // 
            featureLabel.AutoSize = true;
            featureLabel.Location = new Point(168, 10);
            featureLabel.Margin = new Padding(4, 0, 4, 0);
            featureLabel.Name = "featureLabel";
            featureLabel.Size = new Size(51, 15);
            featureLabel.TabIndex = 0;
            featureLabel.Text = "Features";
            // 
            // okButton
            // 
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(733, 485);
            okButton.Margin = new Padding(4);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.TabIndex = 3;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(816, 485);
            cancelButton.Margin = new Padding(4);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 4;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // editButton
            // 
            editButton.Enabled = false;
            editButton.Location = new Point(650, 485);
            editButton.Margin = new Padding(4);
            editButton.Name = "editButton";
            editButton.Size = new Size(75, 23);
            editButton.TabIndex = 2;
            editButton.Text = "Edit...";
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += EditButton_Click;
            // 
            // FeatureExtractionDialog
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(904, 521);
            Controls.Add(editButton);
            Controls.Add(cancelButton);
            Controls.Add(okButton);
            Controls.Add(featureLabel);
            Controls.Add(columnLabel);
            Controls.Add(featuresListView);
            Controls.Add(columnListBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FeatureExtractionDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Feature Extraction";
            Load += FeatureExtractionDialog_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox columnListBox;
        private ListView featuresListView;
        private Label columnLabel;
        private Label featureLabel;
        private Button okButton;
        private Button cancelButton;
        private ColumnHeader featureColumnHeader;
        private ColumnHeader parametersColumnHeader;
        private ColumnHeader descriptionColumnHeader;
        private ColumnHeader referenceColumnHeader;
        private Button editButton;
    }
}