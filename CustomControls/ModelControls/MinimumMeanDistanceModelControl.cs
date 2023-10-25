namespace JadeChem.CustomControls.ModelControls
{
    public partial class MinimumMeanDistanceModelControl : UserControl
    {
        #region Event
        public delegate void TrainButtonClickedEventHandler(EventArgs e);
        public event TrainButtonClickedEventHandler? TrainButtonClicked;
        #endregion

        #region Constructor
        public MinimumMeanDistanceModelControl()
        {
            InitializeComponent();
        }
        #endregion

        #region Method
        private void TrainButton_Click(object sender, EventArgs e)
        {
            // Raise the event
            TrainButtonClicked?.Invoke(new EventArgs());
        }
        #endregion
    }
}
