using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracingProject
{
    // Basic Ray, includes Origin & Direction
    internal class Ray
    {
        public Vector3 origin { get; private set; }
        public Vector3 direction { get; private set; }

        // Used in Checkpoint 5 to render transmitted light
        public float medRefIndex { get; private set; }

        // References the INDEX of the wavelength used by this ray. If -1, it contains all of them.
        public int wavelength { get; private set; }

        public Ray(Vector3 Origin, Vector3 Direction, float medRefIndex = 1.0f, int wavelength = -1)
        {
            origin = Origin;
            direction = Vector3.Normalize(Direction);
            this.medRefIndex = medRefIndex;
            this.wavelength = wavelength;
        }
    }
}
