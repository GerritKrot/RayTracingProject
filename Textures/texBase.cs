using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using RayTracingProject.Spectrum;

namespace RayTracingProject
{
    internal class texBase : ITexture
    {
        SpectrumList baseColor;
        public Vector3 Normal(Vector3 relPos, Vector3 normal)
        {
            return normal;
        }

        public SpectrumList TexColor(Vector3 relPos)
        {
            return baseColor;
        }

        public texBase(Color color)
        {
            baseColor = new SpectrumList(color);
        }
    }
}
