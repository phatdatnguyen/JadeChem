namespace JadeChem.Dialogs
{
    public partial class FeatureExtractionDialog : Form
    {
        #region Field
        // flag
        private bool resettingFeatures = false;
        #endregion

        #region Property
        public Dictionary<string, Dictionary<string, Dictionary<string, double>>> FeaturesDictionary { get; }
        #endregion

        #region Constructor
        public FeatureExtractionDialog(Dictionary<string, Dictionary<string, Dictionary<string, double>>> featuresDictionary)
        {
            InitializeComponent();

            FeaturesDictionary = featuresDictionary;

        }
        #endregion

        #region Methods
        private void FeatureExtractionDialog_Load(object sender, EventArgs e)
        {
            // Clear the list box then add new items
            columnListBox.Items.Clear();
            for (int columnIndex = 0; columnIndex < FeaturesDictionary.Keys.Count; columnIndex++)
                columnListBox.Items.Add(FeaturesDictionary.Keys.ElementAt(columnIndex));

            // Select the first column
            columnListBox.SelectedIndex = 0;
        }

        private void ColumnListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Loop through the list of features
            foreach (ListViewItem item in featuresListView.Items)
            {
                // Check if the feature was selected for selected column (presents in the dictionary)
                if (FeaturesDictionary[columnListBox.Text].TryGetValue(item.Text, out var feature))
                {
                    // Check the checkbox
                    resettingFeatures = true;
                    item.Checked = true;
                    resettingFeatures = false;

                    // Show the parameters
                    if (item.Text == "AP_FP" || item.Text == "TT_FP")
                    {
                        double nBits = feature["nBits"];
                        item.SubItems[1].Text = "{ nBits=" + nBits.ToString() + " }";
                    }
                    else if (item.Text == "Morgan_FP")
                    {
                        double radius = feature["radius"];
                        double nBits = feature["nBits"];
                        item.SubItems[1].Text = "{ radius=" + radius.ToString() + "; nBits=" + nBits.ToString() + " }";
                    }
                }
                else
                {
                    // Uncheck the checkbox
                    resettingFeatures = true;
                    item.Checked = false;
                    resettingFeatures = false;

                    // Reset the parameters
                    if (item.Text == "AP_FP" || item.Text == "TT_FP")
                        item.SubItems[1].Text = "{ nBits=512 }";
                    else if (item.Text == "Morgan_FP")
                        item.SubItems[1].Text = "{ radius=2; nBits=512 }";
                }
            }

            // Enable/disable the edit button
            if (featuresListView.SelectedItems.Count == 0)
                return;

            if (featuresListView.SelectedItems[0].Checked &&
                (featuresListView.SelectedItems[0].Text == "AP_FP" ||
                featuresListView.SelectedItems[0].Text == "Morgan_FP" ||
                featuresListView.SelectedItems[0].Text == "TT_FP"))
                editButton.Enabled = true;
            else
                editButton.Enabled = false;
        }

        private void FeaturesListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // Do nothing the the item was checked/unchecked by code
            if (resettingFeatures)
                return;

            // Add/remove the item from the dictionary
            if (e.Item.Checked)
            {
                if (e.Item.Text == "AP_FP" || e.Item.Text == "TT_FP")
                {
                    if (FeaturesDictionary[columnListBox.Text].TryGetValue(e.Item.Text, out var feature) && feature.Count == 1)
                    {
                        // Check if the parameters is already in the dictionary
                        int nBits = (int)feature["nBits"];
                        e.Item.SubItems[1].Text = "{ nBits=" + nBits.ToString() + " }";
                    }
                    else
                    {
                        // If not, add the parameters to the dictionary
                        Dictionary<string, double> parameters = new()
                        {
                            { "nBits", 512 }
                        };
                        FeaturesDictionary[columnListBox.Text][e.Item.Text] = parameters;
                        e.Item.SubItems[1].Text = "{ nBits=512 }";
                    }
                }
                else if (e.Item.Text == "Morgan_FP")
                {
                    if (FeaturesDictionary[columnListBox.Text].TryGetValue(e.Item.Text, out var feature) && feature.Count == 1)
                    {
                        // Check if the parameters is already in the dictionary
                        double radius = feature["radius"];
                        int nBits = (int)feature["nBits"];
                        e.Item.SubItems[1].Text = "{ radius=" + radius.ToString() + "; nBits=" + nBits.ToString() + " }";
                    }
                    else
                    {
                        Dictionary<string, double> parameters = new()
                        {
                            { "radius", 2 }, { "nBits", 512 }
                        };
                        FeaturesDictionary[columnListBox.Text][e.Item.Text] = parameters;
                        e.Item.SubItems[1].Text = "{ radius=2; nBits=512 }";
                    }
                }
                else
                {
                    FeaturesDictionary[columnListBox.Text][e.Item.Text] = new Dictionary<string, double>();
                }
            }
            else
                FeaturesDictionary[columnListBox.Text].Remove(e.Item.Text);

            // Enable/disable the edit button
            if (featuresListView.SelectedItems.Count == 0)
                return;

            if (featuresListView.SelectedItems[0].Checked &&
                (featuresListView.SelectedItems[0].Text == "AP_FP" ||
                featuresListView.SelectedItems[0].Text == "Morgan_FP" ||
                featuresListView.SelectedItems[0].Text == "TT_FP"))
                editButton.Enabled = true;
            else
                editButton.Enabled = false;
        }

        private void FeaturesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable the edit button
            if (featuresListView.SelectedItems.Count == 0)
                return;

            if (featuresListView.SelectedItems[0].Checked &&
                (featuresListView.SelectedItems[0].Text == "AP_FP" ||
                featuresListView.SelectedItems[0].Text == "Morgan_FP" ||
                featuresListView.SelectedItems[0].Text == "TT_FP"))
                editButton.Enabled = true;
            else
                editButton.Enabled = false;
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (featuresListView.SelectedItems.Count == 0)
                return;

            if (featuresListView.SelectedItems[0].Text == "AP_FP")
            {
                EditParametersDialog editParametersDialog = new();
                editParametersDialog.Parameters["nBits"] = FeaturesDictionary[columnListBox.Text]["AP_FP"]["nBits"];

                if (editParametersDialog.ShowDialog(this) == DialogResult.OK)
                {
                    int nBits = (int)editParametersDialog.Parameters["nBits"];

                    featuresListView.Items[27].SubItems[1].Text = "{ nBits=" + nBits.ToString() + " }";
                    FeaturesDictionary[columnListBox.Text]["AP_FP"]["nBits"] = nBits;
                }
            }
            else if (featuresListView.SelectedItems[0].Text == "Morgan_FP")
            {
                EditParametersDialog editParametersDialog = new();
                editParametersDialog.Parameters["radius"] = FeaturesDictionary[columnListBox.Text]["Morgan_FP"]["radius"];
                editParametersDialog.Parameters["nBits"] = FeaturesDictionary[columnListBox.Text]["Morgan_FP"]["nBits"];

                if (editParametersDialog.ShowDialog(this) == DialogResult.OK)
                {
                    int radius = (int)editParametersDialog.Parameters["radius"];
                    int nBits = (int)editParametersDialog.Parameters["nBits"];

                    featuresListView.Items[29].SubItems[1].Text = "{ radius=" + radius.ToString() + "; nBits=" + nBits.ToString() + " }";
                    FeaturesDictionary[columnListBox.Text]["Morgan_FP"]["radius"] = radius;
                    FeaturesDictionary[columnListBox.Text]["Morgan_FP"]["nBits"] = nBits;
                }
            }
            else if (featuresListView.SelectedItems[0].Text == "TT_FP")
            {
                EditParametersDialog editParametersDialog = new();
                editParametersDialog.Parameters["nBits"] = FeaturesDictionary[columnListBox.Text]["TT_FP"]["nBits"];

                if (editParametersDialog.ShowDialog(this) == DialogResult.OK)
                {
                    int nBits = (int)editParametersDialog.Parameters["nBits"];

                    featuresListView.Items[32].SubItems[1].Text = "{ nBits=" + nBits.ToString() + " }";
                    FeaturesDictionary[columnListBox.Text]["TT_FP"]["nBits"] = nBits;
                }
            }
        }
        #endregion
    }
}
