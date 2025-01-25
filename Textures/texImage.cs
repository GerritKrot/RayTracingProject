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
    internal class texImage : ITexture
    {
        Bitmap image;
        float scale;
        Vector3 origin;
        public Vector3 Normal(Vector3 pos, Vector3 normal)
        {
            return normal;
        }

        public SpectrumList TexColor(Vector3 pos)
        {
            int u = (int) (scale * Math.Abs((pos.X - origin.X))) % image.Width;
            int v = (int)(scale * Math.Abs((pos.Z - origin.Z))) % image.Height;
            return new SpectrumList(image.GetPixel(u, v));
        }

        public texImage(Bitmap img, float scale, Vector3 origin)
        {
            image = img;
            this.scale = scale;
            this.origin = origin;

        }
    }
}
