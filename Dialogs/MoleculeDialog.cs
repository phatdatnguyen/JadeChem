using GraphMolWrap;

namespace JadeChem.Dialogs
{
    public partial class MoleculeDialog : Form
    {
        #region Fields
        private readonly RWMol molecule;
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
            string imageFileName = Path.Combine(Path.GetTempPath(), $"JadeChem-{Guid.NewGuid():N}.png");
            try
            {
                smilesTextBox.Text = molecule.MolToSmiles();
                RDKFuncs.prepareMolForDrawing(molecule);
                using MolDraw2DCairo view = new(1024, 1024);
                view.drawMolecule(molecule);
                view.finishDrawing();
                view.writeDrawingText(imageFileName);

                using Image image = Image.FromFile(imageFileName);
                moleculePictureBox.Image = new Bitmap(image);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Cannot display molecule", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                if (File.Exists(imageFileName))
                    File.Delete(imageFileName);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MoleculeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            moleculePictureBox.Image?.Dispose();
            moleculePictureBox.Image = null;
        }
        #endregion
    }
}
