using Accord.Math;

namespace JadeChem.Utils
{
    public class OneHotEncoder
    {
        #region Fields
        private string[] classLabels = Array.Empty<string>();
        private Dictionary<string, int> classIndexLookup = new();
        #endregion

        #region Constructor
        public OneHotEncoder() { }
        #endregion

        #region Methods
        public byte[][] FitTransform(string[] inputs)
        {
            Fit(inputs);
            return Transform(inputs);
        }

        public void Fit(string[] inputs)
        {
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            classLabels = inputs.Distinct().OrderBy(x => x).ToArray();
            classIndexLookup = classLabels
                .Select((label, idx) => (label, idx))
                .ToDictionary(t => t.label, t => t.idx);
        }

        public byte[][] Transform(string[] inputs)
        {
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            if (classLabels.Length == 0)
                throw new InvalidOperationException("OneHotEncoder must be fit before calling Transform.");

            byte[][] outputs = new byte[inputs.Length][];
            for (int rowIndex = 0; rowIndex < inputs.Length; rowIndex++)
            {
                outputs[rowIndex] = new byte[classLabels.Length];
                if (!classIndexLookup.TryGetValue(inputs[rowIndex], out int classIndex))
                    throw new ArgumentException(
                        $"Unseen category '{inputs[rowIndex]}' at row {rowIndex}. Encoder must be re-fit or input filtered.");
                outputs[rowIndex][classIndex] = 1;
            }

            return outputs;
        }

        public string[] InverseTransform(byte[][] outputs)
        {
            if (outputs == null) throw new ArgumentNullException(nameof(outputs));

            string[] inputs = new string[outputs.Length];
            for (int rowIndex = 0; rowIndex < inputs.Length; rowIndex++)
            {
                int classIndex = outputs[rowIndex].IndexOf((byte)1);
                if (classIndex < 0 || classIndex >= classLabels.Length)
                    throw new ArgumentException($"Output row {rowIndex} has no valid one-hot bit set.");
                inputs[rowIndex] = classLabels[classIndex];
            }

            return inputs;
        }
        #endregion
    }
}
