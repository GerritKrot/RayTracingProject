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
    // Implemented in CP2. It's a triangle
    internal class Tri : IObject
    {
        Vector3 Point1;
        Vector3 Point2;
        Vector3 Point3;

        Color Color;

        public float kAmbient;
        public float kDiffuse;
        public float kSpec;
        public float kE;
        public float kReflected;
        public float kTransmitted;
        public RefractionMaterial medRefIndex;
        private static RefractionMaterial airRefIndex = new RefractionMaterial(1.0f);

        Vector3 edge1;
        Vector3 edge2;
        Vector3 normal;

        ITexture texture;
        private int ID;


        public Tri(Vector3 point1, Vector3 point2, Vector3 point3, Color color, float kAmbient = 1.0f, float kDiffuse = 0.4f, float kSpec = 0.4f, float kE = 0.9f, float kReflected = 0.0f, float kTransmitted = 0.0f, RefractionMaterial medRefIndex = null, ITexture? texture = null)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;

            this.kAmbient = kAmbient;
            this.kDiffuse = kDiffuse;
            this.kSpec = kSpec;
            this.kE = kE;
            this.kReflected = kReflected;
            this.kTransmitted = kTransmitted;

            if (medRefIndex != null)
            {
                this.medRefIndex = medRefIndex;
            }
            else
            {
                this.medRefIndex = new RefractionMaterial(1);
            }

            this.Color = color;


            edge1 = Point2 - Point1;
            edge2 = Point3 - Point1;
            normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
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

            Vector3 rD = ray.direction;
            Vector3 dirXe2 = Vector3.Cross(rD, edge2);
            float det = Vector3.Dot(edge1, dirXe2);

            if (MathF.Abs(det) < float.Epsilon) return Vector3.Zero;

            float invDet = 1.0f / det;

            Vector3 s = ray.origin - Point1;
            float u = invDet * Vector3.Dot(s, dirXe2);
            if (u < 0 || u > 1) return Vector3.Zero;

            Vector3 sXe1 = Vector3.Cross(s, edge1);
            float v = invDet * Vector3.Dot(rD, sXe1);
            if (v < 0 || u + v > 1) return Vector3.Zero;

            float t = invDet * Vector3.Dot(edge2, sXe1);
            if (t > float.Epsilon)
            {
                return ray.origin + (rD * t);
            }

            return Vector3.Zero;
        }

        /// <summary>
        ///  Assuming the point is on the surface of this object, return a full intersection.
        /// </summary>
        /// <param name="point"></param>
        public Intersection GetIntersectionData(Vector3 rayDir, Vector3 point)
        {
            // Calculate which direction the ray is coming from.
            float dir = Vector3.Dot(rayDir, normal);
            Vector3 n = -MathF.Sign(dir) * normal;
            RefractionMaterial refrac = dir < 0.0f ? airRefIndex : medRefIndex;
            SpectrumList c = texture.TexColor(point);
            n = texture.Normal(point, n);
            return new Intersection(this, point, n, c, kAmbient, kDiffuse, kSpec, kE, kReflected, kTransmitted, refrac);
        }

        // Returns the minimum vector of the objects AABB
        public Vector3 AABBmin()
        {
            Vector3 min = Vector3.Zero;
            min.X = MathF.Min(Point1.X, MathF.Min(Point2.X, Point3.X));
            min.Y = MathF.Min(Point1.Y, MathF.Min(Point2.Y, Point3.Y));
            min.Z = MathF.Min(Point1.Z, MathF.Min(Point2.Z, Point3.Z));
            return min;
        }

        // Returns the maximum vector of the objects AABB
        public Vector3 AABBmax()
        {
            Vector3 max = Vector3.Zero;
            max.X = MathF.Max(Point1.X, MathF.Max(Point2.X, Point3.X));
            max.Y = MathF.Max(Point1.Y, MathF.Max(Point2.Y, Point3.Y));
            max.Z = MathF.Max(Point1.Z, MathF.Max(Point2.Z, Point3.Z));
            return max;
        }

        public int GetID()
        {
            return ID;
        }
    }
}
