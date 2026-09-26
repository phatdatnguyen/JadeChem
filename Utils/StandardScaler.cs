using Accord.Statistics;

namespace JadeChem.Utils
{
    public class StandardScaler
    {
        #region Fields
        private double mean;
        private double std;
        #endregion

        #region Constructor
        public StandardScaler() { }
        #endregion

        #region Methods
        public double[] FitTransform(double[] inputs)
        {
            Fit(inputs);
            return Transform(inputs);
        }

        public void Fit(double[] inputs)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Length == 0 || inputs.Any(value => !double.IsFinite(value)))
                throw new ArgumentException("Scaling requires at least one finite value and no missing or infinite values.", nameof(inputs));

            mean = inputs.Mean();
            std = inputs.Length == 1 ? 0 : inputs.StandardDeviation();
            // A constant column carries no variance but must still produce finite
            // values, including when fitted to a single training row.
            if (std == 0)
                std = 1;
        }

        public double[] Transform(double[] inputs)
        {
            double[] output = new double[inputs.Length];
            for (int elementIndex = 0; elementIndex < inputs.Length; elementIndex++)
            {
                output[elementIndex] = (inputs[elementIndex] - mean) / std;
            }

            return output;
        }

        public double Transform(double input)
        {
            double output = (input - mean) / std;

            return output;
        }

        public double[] InverseTransform(double[] outputs)
        {
            double[] inputs = new double[outputs.Length];
            for (int elementIndex = 0; elementIndex < outputs.Length; elementIndex++)
            {
                inputs[elementIndex] = outputs[elementIndex] * std + mean;
            }

            return inputs;
        }

        public double InverseTransform(double output)
        {
            double input = output * std + mean;

            return input;
        }
        #endregion
    }
}
