using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using RayTracingProject.Spectrum;

namespace RayTracingProject
{
    internal class texCheckerboard : ITexture
    {
        SpectrumList Color1;
        SpectrumList Color2;
        float texScale;
        Vector3 origin;
        public Vector3 Normal(Vector3 pos, Vector3 normal)
        {
            return normal;
        }

        public SpectrumList TexColor(Vector3 pos)
        {

            if (getTile(pos) == 0)
                return Color1;
            return Color2;
        }

        public texCheckerboard(Color color1, Color color2, float scale, Vector3 origin)
        {
            Color1 = new SpectrumList(color1);
            Color2 = new SpectrumList(color2);
            texScale = scale;
            this.origin = origin;

        }

        private int getTile(Vector3 pos)
        {
            pos = (pos - origin) / texScale;


            double TileDet = (Math.Floor(pos.X) + Math.Floor(pos.Z)) % 2;
            return (int) TileDet;
        }
    }
}
