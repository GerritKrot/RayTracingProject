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
    // Basic light Object
    internal class PointLight
    {
        public Color Color { get; }
        
        // Intensity added in CP7 so we can generate HDR images
        public float Intensity { get; }
        public Vector3 Position { get; }

        public SpectrumList spectrum { get; }

        public PointLight(Vector3 pos, Color color, float intensity = 1.0f)
        {
            Position = pos;
            Color = color;
            Intensity = intensity;
            spectrum = new SpectrumList(Color) * Intensity;
        }

        public PointLight(PointLight light, float f)
        {
            Position = light.Position;
            Color = light.Color;
            Intensity = light.Intensity * f;
            spectrum = new SpectrumList(Color) * Intensity;
        }

        
    }
}
