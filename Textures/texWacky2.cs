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
    internal class texWacky2 : ITexture
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

            int cRed = (int) (255 * (Math.Abs(Math.Pow(Math.Sin(pos.X), 2) + Math.Pow(Math.Sin(pos.Z), 2)) % 1));
            int cBlue = (int)(255 * (Math.Abs(Math.Pow(Math.Cos(pos.X), 2) + Math.Pow(Math.Cos(pos.Z), 2)) % 1));

            return new SpectrumList(Color.FromArgb(cRed, cBlue, 255));
        }

        public texWacky2(float scale, Vector3 origin)
        {
            texScale = scale;
            this.origin = origin;

        }
    }
}
