using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using RayTracingProject.Spectrum;

namespace RayTracingProject.Textures
{
    internal class texWacky : ITexture
    {
        float texScale;
        Vector3 origin;
        public Vector3 Normal(Vector3 pos, Vector3 normal)
        {
            pos = Vector3.Normalize(pos - origin);
            return Vector3.Normalize(normal - pos);
        }

        public SpectrumList TexColor(Vector3 pos)
        {
            pos = (pos - origin) / texScale;

            int cRed = (int) Math.Abs(Math.Tan(pos.X) * 255 % 255);
            int cBlue = (int) Math.Abs(Math.Tan(pos.Z) * 255 % 255);
            int cGreen = (int) Math.Abs(Math.Tan(pos.X + pos.Z) % 255);

            return new SpectrumList(Color.FromArgb(cRed, cGreen, cBlue));
        }

        public texWacky(float scale, Vector3 origin)
        {
            texScale = scale;
            this.origin = origin;

        }
    }
}
