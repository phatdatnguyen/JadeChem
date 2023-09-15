using GraphMolWrap;

namespace JadeChem.Utils
{
    public static class Conversion
    {
        #region Method
        public static int[] ExplicitBitVectToIntArray(ExplicitBitVect bitVect)
        {
            int[] bitVectInt = new int[bitVect.size()];
            for (uint bitIndex = 0; bitIndex < bitVect.size(); bitIndex++)
            {
                if (bitVect.getBit(bitIndex))
                    bitVectInt[bitIndex] = 1;
                else
                    bitVectInt[bitIndex] = 0;
            }

            return bitVectInt;
        }
        #endregion
    }
}
