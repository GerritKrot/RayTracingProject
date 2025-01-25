using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RayTracingProject
{
    internal class ToneReproducer
    {
        private LuminanceMethod lumMethod;
        private CompressionMethod compMethod;
        private Vector3[,] image;
        private float logAvgLum;
        private float lumMax;
        private float ldisplayMax;
        private float lumBias;


        public ToneReproducer(Vector3[,] img, float lMax = 500f, float displayMax = 500f,
            float bias = 0.85f,
            LuminanceMethod lMethod = LuminanceMethod.Quick,
            CompressionMethod cMethod = CompressionMethod.Ward)
        {
            image = img;
            lumMax = lMax;

            lumBias = MathF.Log2(bias) / MathF.Log2(0.5f);
            ldisplayMax = displayMax;
            lumMethod = lMethod;
            compMethod = cMethod;
        }

        public Bitmap ToneImage(float ldMax = 0.0f, float lMax = 0.0f,
            float bias = float.NaN,
            LuminanceMethod lMethod = LuminanceMethod.def,
            CompressionMethod cMethod = CompressionMethod.def,
            float reinhardAlpha = 0.18f)
        {
            if (ldMax != 0.0f) ldisplayMax = ldMax;
            if (lMax != 0.0f) lumMax = lMax;
            if (!float.IsNaN(bias)) lumBias = MathF.Log2(bias) / MathF.Log2(0.5f);
            if (lMethod != LuminanceMethod.def) lumMethod = lMethod;
            if (cMethod != CompressionMethod.def) compMethod = cMethod;
            if (cMethod != CompressionMethod.def) compMethod = cMethod;

            // Calulcate Log Average Luminance
            logAvgLum = 0.0f;
            wardScale = 0.0f;
            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    float l = Luminance(image[x, y]);
                    logAvgLum += MathF.Log(0.000001f + (l));
                }


            logAvgLum /= image.Length;
            logAvgLum = MathF.Exp(logAvgLum);

            // Compress the HDR Image
            Bitmap bmp = new Bitmap(image.GetLength(0), image.GetLength(1));

            if (compMethod == CompressionMethod.Reinhard)
                ReinhardScalar = reinhardAlpha / logAvgLum;

            if (compMethod == CompressionMethod.AdaptiveLuminance)
            {
                adapMaxL = lumMax / logAvgLum;
                adapTerm1 = 1 / MathF.Log10(adapMaxL + 1);
            }
                

            for (int x = 0; x < image.GetLength(0); x++)
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    bmp.SetPixel(x, y, CompressColor(image[x, y]));
                }
            return bmp;
        }

        public float Luminance(Color c)
        {
            return Luminance(VecFromColor(c));
        }
        public float Luminance(Vector3 v)
        {
            switch (lumMethod)
            {
                case LuminanceMethod.Quick: return QuickLuminance(v);
                default: return QuickLuminance(v);
            }
        }

        private float QuickLuminance(Vector3 v)
        {
            return MathF.Max(0.0f, lumMax * ((0.27f * v.X) + (0.67f * v.Y) + (0.06f * v.Z)));
        }

        public Color CompressColor(Vector3 C)
        {
            switch (compMethod)
            {
                case CompressionMethod.Ward: return WardCompression(C);
                case CompressionMethod.Reinhard: return ReinhardCompression(C);
                case CompressionMethod.AdaptiveLuminance: return AdapativeLuminance(C);
                default: return VecToColor(C);
            }
        }
        private float wardScale = 0.0f;
        private Color WardCompression(Vector3 C)
        {
            if (wardScale == 0.0f)
            {
                wardScale = 1.219f + MathF.Pow(ldisplayMax / 2, 0.4f);
                float denom = 1.219f + MathF.Pow(logAvgLum, 0.4f);
                wardScale = MathF.Pow(wardScale / denom, 2.5f);
            }

            return VecToColor(C * Luminance(C) * wardScale / ldisplayMax);
        }

        private float ReinhardScalar = 0.0f; // reinhard Alpha divided by our logavglum.
        private Color ReinhardCompression(Vector3 C)
        {
            Vector3 scaleColor = C * Luminance(C) * ReinhardScalar;
            scaleColor.X /= 1 + scaleColor.X;
            scaleColor.Y /= 1 + scaleColor.Y;
            scaleColor.Z /= 1 + scaleColor.Z;
            return VecToColor(scaleColor);
        }


        // Adaptive Luminance implemented for Advanced CP 4.
        float adapMaxL = 0.0f;
        float adapTerm1 = 0.0f;
        private Color AdapativeLuminance(Vector3 C)
        {
            C *= lumMax / logAvgLum;

            return VecToColor(new Vector3(AdapLumHelper(C.X), 
                AdapLumHelper(C.Y), AdapLumHelper(C.Z)) / 2); // <- Heuristically chosen number (it looks good)
        }

        private float AdapLumHelper(float f)
        {
            float term2 = MathF.Log2(f + 1);
            float denom = MathF.Pow(f / adapMaxL, lumBias); // Pre-calculated for ease of access :)
            denom = MathF.Log2(2 + (denom * 8));
            return adapTerm1 * (term2 / denom);
        }

        #region Static Utility Methods
        // Note: These methods were originally in world
        // But I moved them during CP 7 because I thought they fit here better
        public static Color VecToColor(Vector3 v)
        {
            v = 255 * v;
            return Color.FromArgb((int)Math.Clamp(v.X, 0, 255), (int)Math.Clamp(v.Y, 0, 255), (int)Math.Clamp(v.Z, 0, 255));
        }
        public static Vector3 VecFromColor(Color c)
        {
            Vector3 v = new Vector3(c.R, c.G, c.B) * (c.A / 255.0f);
            return v / 255.0f;
        }
        #endregion
    }

    enum CompressionMethod
    {
        Ward,
        Reinhard,
        AdaptiveLuminance,
        def
    }

    enum LuminanceMethod
    {
        Quick,
        def
    }
}
