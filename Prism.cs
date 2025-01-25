using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using RayTracingProject.Spectrum;
using System.Runtime.InteropServices.JavaScript;
using System.Reflection;

namespace RayTracingProject
{
    internal class Prism
    {
        private static Vector3[] PrismPoints = {
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(0, 0.866f, 0.5f),
                new Vector3(0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(0, 0.866f, -0.5f),
            };

        private static int[,] PointList =
        {
            { 0, 1, 2 }, //Side Face  +Z
            { 3, 4, 5 }, //Side Face -Z
            { 0, 4, 5 }, // Rect Face -X 1
            { 0, 5, 2 }, // Rect Face -X 2
            { 3, 1, 5 }, // Rect Face +X 1
            { 1, 2, 5 }, // Rect Face +X 2
            { 0, 4, 1 }, // Rect Face bot 1
            { 3, 1, 4 }, // Rect Face bot 2
        };


        private Vector3[] localPrismPoints;
        private List<Tri> faces = new List<Tri>();

        private static Color c = Color.White;

        // Needs a reference so we can backtrace for shapelights

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="Scale"></param>
        /// <param name="rotation">A Euler Angle Rotation in degrees </param>
        public Prism(Vector3 position, Vector3 Scale, Vector3 rotation, float kAmbient = 0.075f, float kDiffuse = 0.075f, float kSpec = 0.2f, float kE = 20f, float kReflected = 0.1f, float kTransmitted = 0.9f, RefractionMaterial? refIndex = null, ITexture? texture = null)
        {
            rotation = rotation * MathF.PI / 180.0f;
            Matrix4x4 transform = Matrix4x4.CreateScale(Scale, Vector3.Zero)
                * Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z)
                * Matrix4x4.CreateTranslation(position);
            localPrismPoints = new Vector3[PrismPoints.Length];

            if (refIndex == null)
            {
                refIndex = new RefractionMaterial(1.4f, 2.5f);
            }

            if (texture == null)
            {
                texture = new texBase(c);
            }

            for (int i = 0; i < PrismPoints.Length; i++)
            {
                localPrismPoints[i] = Vector3.Transform(PrismPoints[i], transform);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector3 point1 = localPrismPoints[PointList[i, 0]];
                Vector3 point2 = localPrismPoints[PointList[i, 1]];
                Vector3 point3 = localPrismPoints[PointList[i, 2]];

                faces.Add(new Tri(point1, point2, point3, c, kAmbient, kDiffuse, kSpec, kE, kReflected, kTransmitted, refIndex, texture));
            }
        }

        public bool IsInPrism(int id)
        {
            foreach (var face in faces)
            {
                if (id == face.GetID())
                    return true;
            }
            return false;
        }

        public List<Tri> getFaces()
        {
            return faces;
        }

        /*
        private int[,] FaceList = {
            { 0, 2, 4, 5 },
            { 1, 2, 3, 5 },
            { 0, 1, 3, 4 } };

        /// <summary>
        /// Refracts the Shapelight through the prism and attempts
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<ShapeLight> calcRefractedLights(ShapeLight s)
        {
            int faceIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                bool litSide = true;
                Ray[] rays = new Ray[4];
                Vector3 centerPoint = Vector3.Zero;
                for (int j = 0; i < 4; j++)
                {
                    Vector3 cornerPoint = localPrismPoints[FaceList[i, j]];
                    centerPoint += cornerPoint;
                    Ray? r = getRay(s, cornerPoint);
                    if (r == null)
                    {
                        litSide = false;
                        break;
                    }
                    else
                    {
                        rays[j] = r;
                    }
                }
                if (litSide)
                {
                    centerPoint /= 4;
                    if (getRay(s, centerPoint) != null)
                    {
                        // Seperate Rays :)
                        Ray[,] seperatedRays = new Ray[4, SpectrumConverter.specLength];
                        for (int j = 0; i < 4; j++)
                        {
                            Ray ray = rays[j];
                            
                            // For each ray, refract it
                            // Assume that the the ray hits the opposite face 

                            float cosThetaI = -Vector3.Dot(ray.direction, i.normal);
                            float detPart = (MathF.Pow(cosThetaI, 2) - 1);
                            for (int wv = 0; wv < SpectrumConverter.specLength; wv++)
                            {
                                float refIndex = i.medRefIndex.getInd(wv);
                                float nIT = ray.medRefIndex / refIndex;
                                float det = 1 + (MathF.Pow(nIT, 2) * detPart);
                                if (det < 0)
                                {
                                    Itransmitted.SetResponse(wv, Ireflected.GetResponse(wv));
                                }
                                else
                                {
                                    float beta = (nIT * cosThetaI) - MathF.Sqrt(det);
                                    Vector3 dir = Vector3.Normalize((nIT * ray.direction) + (beta * i.normal));
                                    Ray transmitted = new Ray(i.intersection + (0.01f * dir), dir, refIndex, wv);
                                    Itransmitted.SetResponse(wv, RayTrace(transmitted, depth + 1).GetResponse(wv));
                                }
                            }
                        }
                    }
                    else
                    {
                        float cosThetaI = -Vector3.Dot(ray.direction, i.normal);
                        float detPart = (MathF.Pow(cosThetaI, 2) - 1);
                        float refIndex = i.medRefIndex.getInd(ray.wavelength);
                        float nIT = ray.medRefIndex / refIndex;
                        float det = 1 + (MathF.Pow(nIT, 2) * detPart);
                        if (det < 0)
                        {
                            Itransmitted.SetResponse(ray.wavelength, Ireflected.GetResponse(ray.wavelength));
                        }
                        else
                        {
                            float beta = (nIT * cosThetaI) - MathF.Sqrt(det);
                            Vector3 dir = Vector3.Normalize((nIT * ray.direction) + (beta * i.normal));
                            Ray transmitted = new Ray(i.intersection + (0.01f * dir), dir, refIndex, ray.wavelength);
                            Itransmitted.SetResponse(ray.wavelength, RayTrace(transmitted, depth + 1).GetResponse(ray.wavelength));
                        }

                    }
                    // Get that light through :)
                }

            }
            return null;
        }


        private Ray? getRay(ShapeLight s, Vector3 v)
        {
            Vector3 lightPoint = s.LightingPoint(v);
            Vector3 dir = v - lightPoint;
            float dist = Vector3.Distance(v, lightPoint);
            Ray r = new Ray(lightPoint + dir * 0.01f, dir);
            Intersection intersect = world.CastRay(r);
            if (intersect.oHit == null // Technically this shouldn't happen but if it does it's fine
                        || Vector3.Distance(intersect.intersection, lightPoint) > dist - 0.05f)
            {
                return r;
            }
            return null;
        }
*/
    }
}
