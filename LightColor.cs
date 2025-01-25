using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace RayTracingProject
{
    // This class is not used. Only a theory I was working on.
    internal class LightColor
    {
        private static bool useCIE = false;
        public static float negSqrExp(float num)
        {
            return MathF.Exp(-0.5f * num * num);
        }

        // Implements the Piecewise Gaussian Fit methods generated via the Simplex method.
        public static float RedCIE(float wavelength)
        {
            float term1 = (wavelength - 442.0f) * ((wavelength < 442.0f) ? 0.0624f : 0.0374f);
            term1 = negSqrExp(term1);
            float term2 = (wavelength - 599.8f) * ((wavelength < 599.8f) ? 0.0264f : 0.0323f);
            term2 = negSqrExp(term2);
            float term3 = (wavelength - 501.1f) * ((wavelength < 501.1f) ? 0.0490f : 0.0382f);
            term3 = negSqrExp(term3);
            return (0.362f * term1) + (1.056f * term2) - (0.065f * term3);
        }

        public static float GreenCIE(float wavelength)
        {
            float term1 = (wavelength - 568.8f) * ((wavelength < 568.8f) ? 0.0213f : 0.0247f);
            term1 = negSqrExp(term1);
            float term2 = (wavelength - 530.9f) * ((wavelength < 530.9f) ? 0.0613f : 0.0322f);
            term2 = negSqrExp(term2);
            return (0.821f * term1) + (0.286f * term2);
        }

        public static float BlueCIE(float wavelength)
        {
            float term1 = (wavelength - 437.0f) * ((wavelength < 437.0f) ? 0.0845f : 0.0278f);
            term1 = negSqrExp(term1);
            float term2 = (wavelength - 459.0f) * ((wavelength < 459.0f) ? 0.0385f : 0.0725f);
            term2 = negSqrExp(term2);
            return (1.217f * term1) + (0.681f * term2);
        }

        private static Vector3 CIEApprox(float waveLength)
        {
            return new Vector3(RedCIE(waveLength), GreenCIE(waveLength), BlueCIE(waveLength));
        }
        private static Vector3 MihaiStajescuApprox(float wavelength)
        {
            Vector3 color = Vector3.Zero;

            if (wavelength >= 380 && wavelength < 410)
            {
                // Note: The paper had this value wrong.
                color.X = 0.19f + (0.41f * (410.0f - wavelength) / 30.0f);
                color.Y = 0;
                color.Z = 0.99f - (0.6f * (410.0f - wavelength) / 30.0f);
            }

            if (wavelength >= 410 && wavelength < 440)
            {
                // Note: The paper had this value wrong.
                color.X = (0.19f * (440.0f - wavelength) / 30.0f);
                color.Y = 0;
                color.Z = 1;
            }

            if (wavelength >= 440 && wavelength < 490)
            {
                color.X = 0;
                color.Y = 1.0f - ((490.0f - wavelength) / 50.0f);
                color.Z = 1;
            }

            if (wavelength >= 490 && wavelength < 510)
            {
                color.X = 0;
                color.Y = 1;
                color.Z = ((510.0f - wavelength) / 20.0f);
            }

            if (wavelength >= 510 && wavelength < 580)
            {
                color.X = 1.0f - ((580.0f - wavelength) / 70.0f);
                color.Y = 1;
                color.Z = 0;
            }

            if (wavelength >= 580 && wavelength < 640)
            {
                color.X = 1;
                color.Y = (640.0f - wavelength) / 60.0f;
                color.Z = 0;
            }

            if (wavelength >= 640 && wavelength < 700)
            {
                color.X = 1;
                color.Y = 0;
                color.Z = 0;
            }

            if (wavelength >= 700 && wavelength <= 780)
            {
                // Note: The paper had this value wrong.
                color.X = 0.35f + (0.65f * (780.0f - wavelength) / 80.0f);
                color.Y = 0;
                color.Z = 0;
            }

            return color;
        }


        public static Vector3 WaveColor(float wv)
        {
            if (useCIE) return CIEApprox(wv);
            return MihaiStajescuApprox(wv);
        }
    }
}
