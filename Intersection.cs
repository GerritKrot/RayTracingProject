using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using RayTracingProject.Spectrum;

namespace RayTracingProject
{
    internal struct Intersection
    {
        public readonly IObject? oHit; // The object Hit
        public readonly Vector3 intersection; // The location of the intersection
        public readonly Vector3 normal; // The normal of the intersection
        public readonly SpectrumList spectrum; // The color at the point of intersection (Used for textures)
        public readonly float kAmbient; // kValues of all of the object at the point
        public readonly float kDiffuse;
        public readonly float kSpec;
        public readonly float kE;
        public readonly float kReflected;
        public readonly float kTransmitted;
        public readonly RefractionMaterial medRefIndex;

        public Intersection()
        {
            // Default values for an intersection.
            // We can test if it's real by verifying that oHit = null
            oHit = null;
            intersection = Vector3.Zero;
            normal = Vector3.UnitX;
            spectrum = new SpectrumList(0.0f);
            kAmbient = 0f;
            kDiffuse = 0f;
            kSpec = 0f;
            kE = 0f;
            kReflected = 0f;
            kTransmitted = 0f;
            medRefIndex = new RefractionMaterial(1f);
        }

        public Intersection(IObject? oHit, Vector3 intersection, Vector3 normal, SpectrumList color, float kAmbient, float kDiffuse, float kSpec, float kE, float kReflected, float kTransmitted, RefractionMaterial medRefIndex)
        {
            this.oHit = oHit;
            this.intersection = intersection;
            this.normal = Vector3.Normalize(normal);
            this.spectrum = color;
            this.kAmbient = kAmbient;
            this.kDiffuse = kDiffuse;
            this.kSpec = kSpec;
            this.kE = kE;
            this.kReflected = kReflected;
            this.kTransmitted = kTransmitted;
            this.medRefIndex = medRefIndex;
        }
    }
}
