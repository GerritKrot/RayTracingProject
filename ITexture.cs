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
    // Texture Interface
    internal interface ITexture
    {
        // Gets the color at a position on the point.
        abstract SpectrumList TexColor(Vector3 pos);

        // Gets the normal at a position given and actual object normal.
        abstract Vector3 Normal(Vector3 pos, Vector3 normal);
    }
}
