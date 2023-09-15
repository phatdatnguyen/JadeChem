using GraphMolWrap;

namespace JadeChem.Dialogs
{
    public partial class MoleculeDialog : Form
    {
        #region Fields
        private readonly RWMol molecule;
        private readonly string imageFileName = Directory.GetCurrentDirectory() + "\\SelectedMolecule.png";
        #endregion

        #region Contructor
        public MoleculeDialog(RWMol molecule)
        {
            InitializeComponent();

            this.molecule = molecule;
        }
        #endregion

        #region Methods
        private void MoleculeDialog_Load(object sender, EventArgs e)
        {
            smilesTextBox.Text = molecule.MolToSmiles();

            // Make a picture of molecule and save it to a file
            RDKFuncs.prepareMolForDrawing(molecule);

            MolDraw2DCairo view = new(1024, 1024);
            view.drawMolecule(molecule);
            view.finishDrawing();
            view.writeDrawingText(imageFileName);

            // Load the picture from file
            Image image = Image.FromFile(imageFileName);
            moleculePictureBox.Image = image;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MoleculeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            moleculePictureBox.Image.Dispose();
            File.Delete(imageFileName);
        }
        #endregion
    }
}
