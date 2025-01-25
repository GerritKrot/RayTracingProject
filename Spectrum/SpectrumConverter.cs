using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace RayTracingProject
{
    internal class SpectrumConverter
    {
        public static int minWavelength = 380;

        public static int maxWavelength = 750;

        // MUST BE SET UP BEFORE ANYTHING ELSE
        public static Vector3[] WaveLengthColors;

        public static float[] WaveLengths;

        public static int specLength { get; private set; }

        // Used to normalize color back to a 0-1 range
        public static Vector3 colorAdj { get; private set; }
        //public static float scaleAdj { get; private set; }


        // Possible Improvements: Improve sampling of the color space here.
        public static void SetSpectrum(int l)
        {
            float[] wavelengths = new float[l];
            float increment = (maxWavelength - minWavelength) / l;
            for (int i = 0; i < l; i++)
            {
                wavelengths[i] = (int)(minWavelength + (increment * (i + 0.5)));
            }
            SetSpectrum(wavelengths);
        }

        public static void SetSpectrum(float[] wavelengths)
        {
            WaveLengths = new float[wavelengths.Length];
            Array.Copy(wavelengths, WaveLengths, wavelengths.Length);
            specLength = wavelengths.Length;
            colorAdj = Vector3.Zero;

            WaveLengthColors = new Vector3[specLength];
            for (int i = 0; i < specLength; i++)
            {
                WaveLengthColors[i] = LightColor.WaveColor(wavelengths[i]);
                colorAdj += WaveLengthColors[i];
                //scaleAdj += waveLengthColors[i].X + waveLengthColors[i].Y + waveLengthColors[i].Z;
            }
            //scaleAdj /= 3.0f;
        }

       
    }
}
