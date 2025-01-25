using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracingProject.Spectrum
{
    internal class RefractionMaterial
    {
        public float[] refractiveIndexes { get; private set; }

        public RefractionMaterial()
        {
            refractiveIndexes = new float[SpectrumConverter.specLength];
            SellmeierRefraction();
        }

        public RefractionMaterial(float f)
        {
            refractiveIndexes = new float[SpectrumConverter.specLength];
            for (int i = 0; i < refractiveIndexes.Length; i++)
            {
                refractiveIndexes[i] = f;
            }
        }

        public RefractionMaterial(float fMin, float fMax)
        {
            float increment = (fMax - fMin) / SpectrumConverter.specLength;
            refractiveIndexes = new float[SpectrumConverter.specLength];
            for (int i = 0; i < refractiveIndexes.Length; i++)
            {
                refractiveIndexes[i] = fMax - (i * increment);
            }
        }

        public float getInd(int index)
        {
            return refractiveIndexes[index];
        }


        #region Sellmeier consts
        // These constants are from 2 different materials 
        // Dense Flint
        /*private const float sellmeierB1 = 1.55912923f;
        private const float sellmeierB2 = 0.284246288f;
        private const float sellmeierB3 = 0.968842826f;
        private const float sellmeierC1 = 0.0121481001f;
        private const float sellmeierC2 = 0.0534549042f;
        private const float sellmeierC3 = 112.174809f;*/

        // Fused Silica
        private const float sellmeierB1 = 0.6961663f;
        private const float sellmeierB2 = 0.4079426f;
        private const float sellmeierB3 = 0.8974794f;
        private const float sellmeierC1 = 0.0684043f;
        private const float sellmeierC2 = 0.1162414f;
        private const float sellmeierC3 = 9.896161f;
        #endregion

        private void SellmeierRefraction()
        {
            for (int i = 0; i < SpectrumConverter.specLength; i++)
            {
                // wavelengths are expressed in nanometers, the function takes micrometers
                float wv = SpectrumConverter.WaveLengths[i] / 1000.0f;
                float t1 = SellmeierTerm(wv, sellmeierB1, sellmeierC1);
                float t2 = SellmeierTerm(wv, sellmeierB2, sellmeierC2);
                float t3 = SellmeierTerm(wv, sellmeierB3, sellmeierC3);
                float sum = 1 + t1 + t2 + t3;
                refractiveIndexes[i] = MathF.Sqrt(sum);
            }
        }

        /// <summary>
        ///  Returns the index of refraction for a wavelength
        /// </summary>
        /// <param name="wv"> The wavelength of light in micrometers</param>
        /// <param name="B"> The sellmeier 'B' constant</param>
        /// <param name="C">The sellmeier 'C' constant</param>
        /// <returns></returns>
        private float SellmeierTerm(float wv, float B, float C)
        {
            float wv2 = MathF.Pow(wv, 2);

            return (wv2 * B) / (wv2 - (C * C));
        }
    }
}
