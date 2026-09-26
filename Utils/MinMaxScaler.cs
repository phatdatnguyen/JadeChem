namespace JadeChem.Utils
{
    public class MinMaxScaler
    {
        #region Fields
        readonly double minOutput = 0;
        readonly double maxOutput = 1;
        double minInput = 0;
        double maxInput = 1;
        double inputRange = 1;
        #endregion

        #region Constructor
        public MinMaxScaler(double minOutput = 0, double maxOutput = 1)
        {
            if (!double.IsFinite(minOutput) || !double.IsFinite(maxOutput) || minOutput >= maxOutput)
                throw new ArgumentException("The output range must have finite bounds with minimum less than maximum.");

            this.minOutput = minOutput;
            this.maxOutput = maxOutput;
        }
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

            minInput = inputs.Min();
            maxInput = inputs.Max();
            // Use unit scale for constant columns so transforms remain finite and
            // inverse transforms still recover the original values.
            inputRange = maxInput == minInput ? 1 : maxInput - minInput;
        }

        public double[] Transform(double[] inputs)
        {
            double[] outputs = new double[inputs.Length];
            for (int elementIndex = 0; elementIndex < inputs.Length; elementIndex++)
            {
                // Scale to range (0, 1)
                outputs[elementIndex] = (inputs[elementIndex] - minInput) / inputRange;

                // Scale to range (minOutput, maxOutput)
                outputs[elementIndex] = outputs[elementIndex] * (maxOutput - minOutput) + minOutput;
            }

            return outputs;
        }

        public double Transform(double input)
        {
            // Scale to range (0, 1)
            double output = (input - minInput) / inputRange;

            // Scale to range (minOutput, maxOutput)
            output = output * (maxOutput - minOutput) + minOutput;

            return output;
        }

        public double[] InverseTransform(double[] outputs)
        {
            double[] inputs = new double[outputs.Length];
            for (int elementIndex = 0; elementIndex < outputs.Length; elementIndex++)
            {
                // Scale to range (0, 1)
                inputs[elementIndex] = (outputs[elementIndex] - minOutput) / (maxOutput - minOutput);

                // Scale to range (minInput, maxInput)
                inputs[elementIndex] = inputs[elementIndex] * inputRange + minInput;
            }

            return inputs;
        }

        public double InverseTransform(double output)
        {
            // Scale to range (0, 1)
            double input = (output - minOutput) / (maxOutput - minOutput);

            // Scale to range (minInput, maxInput)
            input = input * inputRange + minInput;

            return input;
        }
        #endregion
    }
}
