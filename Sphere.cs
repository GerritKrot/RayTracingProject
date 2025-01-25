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
    // Implemented in CP2. It's a sphere.
    internal class Sphere : IObject
    {
        Vector3 Origin;
        float Radius;

        Color Color;

        public float kAmbient;
        public float kDiffuse;
        public float kSpec;
        public float kE;
        public float kReflected;
        public float kTransmitted;
        public RefractionMaterial medRefIndex;
        private static RefractionMaterial airRefIndex = new  RefractionMaterial(1.0f);
        public ITexture texture;
        private int ID;


        public Sphere(Vector3 origin, float radius, Color color, float kAmbient = 1.0f, float kDiffuse = 0.43f, float kSpec = 0.55f, float kE = 0.85f, float kReflected = 0.0f, float kTransmitted = 0.0f, RefractionMaterial refIndex = null, ITexture? texture = null)
        {
            this.Origin = origin;
            this.Radius = radius;
            this.Color = color;
            this.kAmbient = kAmbient;
            this.kDiffuse = kDiffuse;
            this.kSpec = kSpec;
            this.kE = kE;
            this.kReflected = kReflected;
            this.kTransmitted = kTransmitted;
            if (refIndex != null)
            {
                medRefIndex = refIndex;
            }
            else
            {
                this.medRefIndex = new RefractionMaterial(1);
            }

            if (texture != null)
            {
                this.texture = texture;
            }
            else
            {
                this.texture = new texBase(this.Color);
            }
            ID = IObject.NewID();
        }

        public Color GetColor()
        {
            return Color;
        }

        // returns the intersection of a ray and this object
        public Vector3 Intersect(Ray ray)
        {
            Vector3 rDifference = ray.origin - Origin;
            Vector3 rD = ray.direction;

            float B = 2 * ((rD.X * rDifference.X) + (rD.Y * rDifference.Y) + (rD.Z * rDifference.Z));
            float C = (rDifference.X * rDifference.X) + (rDifference.Y * rDifference.Y) + (rDifference.Z * rDifference.Z) - (Radius * Radius);

            float root = (B * B) - (4 * C);

            if (root < 0) return Vector3.Zero;
            root = MathF.Sqrt(root);
            float omegaP = (-B + root) / 2;
            float omegaN = (-B - root) / 2;

            if (omegaP < 0)
                return Vector3.Zero;

            Vector3 intersect;

            if (omegaN >= 0)
                intersect = ray.origin + (rD * omegaN);
            else
            {
                intersect = ray.origin + (rD * omegaP);
            }


            return intersect;
        }

        /// <summary>
        ///  Assuming the point is on the surface of this object, return a full intersection.
        ///  I love summary comments
        /// </summary>
        /// <param name="point"></param>
        public Intersection GetIntersectionData(Vector3 rayDir, Vector3 point)
        {
            Vector3 normal = Vector3.Normalize(point - Origin);
            RefractionMaterial refrac = medRefIndex;
            bool dir = Vector3.Dot(rayDir, normal) > 0.0f;
            if (dir)
            {
                refrac = airRefIndex;
                normal = -normal;
            }
            // Gets the texture & color from the object
            SpectrumList c = texture.TexColor(point);
            normal = texture.Normal(point, normal);
            return new Intersection(this, point, normal, c, kAmbient, kDiffuse, kSpec, kE, kReflected, kTransmitted, refrac);
        }

        // Returns the minimum vector of the objects AABB
        public Vector3 AABBmin()
        {
            return new Vector3(Origin.X - Radius, Origin.Y - Radius, Origin.Z - Radius);
        }

        // Returns the maximum vector of the objects AABB
        public Vector3 AABBmax()
        {
            return new Vector3(Origin.X + Radius, Origin.Y + Radius, Origin.Z + Radius);
        }

        public int GetID()
        {
            return ID;
        }
    }
}
