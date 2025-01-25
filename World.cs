using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using RayTracingProject.Spectrum;
using RayTracingProject.Textures;
using System.Reflection;

namespace RayTracingProject
{
    internal class World
    {
        // Modification: Originally, this was just sky blue,
        // but in Checkpoint 7 it was far too dark.
        public SpectrumList backgroundColor = new SpectrumList(Color.SkyBlue) * 2f;

        // Ambient Light Color. Should probably be updated to a vec.
        public SpectrumList ambientLight = new SpectrumList(Color.White);

        // List of objects in the Scene
        public List<IObject> scene = new();

        // List of point lights in the scene
        public List<PointLight> lights = new();
        public List<ShapeLight> shapeLights = new();

        // maximum depth of tracing rays. Implemented in Checkpoint 5
        public int maxDepth = 4;

        // Determines whether we use KDTree or Naive approach
        public bool useKDTree = false; // NOTE: Unknown issue with KDTrees is causing strange clipping
        private KdTree? tree;
        public KdTree Tree
        {
            get
            {
                if (tree == null)
                    CreateKDTree();
                return tree!;
            }
        }
        // Desired leaf number for KDTree
        private int nLeaf = 5;

        // Antiquated Method, returns color of a ray.
        // Used to actually return a color
        // But Tone reproduction required working with Vectors.
        public SpectrumList getRayColor(Ray ray)
        {
            return RayTrace(ray, 0);
        }


        // Traces the rays and returns a vec3 color of the result.
        private SpectrumList RayTrace(Ray ray, int depth)
        {
            Intersection i = CastRay(ray);

            if (i.oHit == null) return backgroundColor;
            if (Vector3.Distance(ray.origin, i.intersection) < 0.001f)
            {
                return new SpectrumList(0f);
            }

            #region Phong Illumination: Checkpoint 3
            List<PointLight> pLights = new List<PointLight>();
            foreach (PointLight light in lights)
            {
                Vector3 dir = Vector3.Normalize(light.Position - i.intersection);
                Ray r = new Ray(i.intersection + (0.01f * dir), dir);
                Intersection intersection = CastRay(r);
                if (intersection.oHit == null)
                {
                    pLights.Add(light);
                }
                else
                {
                    if (intersection.kTransmitted > float.Epsilon)
                    {
                        // TODO: Make this spectral, and refract it for real
                        pLights.Add(new PointLight(light, intersection.kTransmitted));
                    }
                }
            }


            SpectrumList objColor = i.spectrum;

            SpectrumList ambientColor = i.kAmbient * ambientLight * objColor;

            SpectrumList pointColor = new SpectrumList(0);
            SpectrumList specColor = new SpectrumList(0);

            foreach (PointLight light in pLights)
            {
                SpectrumList lColor = light.spectrum * objColor;
                Vector3 Si = Vector3.Normalize(light.Position - i.intersection);
                Vector3 Ri = Vector3.Normalize(Vector3.Reflect(Si, i.normal));
                Vector3 V = Vector3.Normalize(i.intersection - ray.origin);

                pointColor += lColor * MathF.Max(Vector3.Dot(Si, i.normal), 0.0f);//SI . N;

                specColor += lColor * MathF.Pow(MathF.Max(Vector3.Dot(Ri, V), 0.0f), i.kE); // Ri . V ^ ke
            }

            foreach (ShapeLight l in shapeLights)
            {
                Vector3 p = l.LightingPoint(i.intersection);
                if (p != i.intersection)
                {
                    Vector3 Si = Vector3.Normalize(p - i.intersection);
                    float dist = Vector3.Distance(p, i.intersection);
                    Ray r = new Ray(i.intersection + (0.01f * Si), Si);
                    Intersection checkShape = CastRay(r);
                    // This should check for intersections to cast shadows, but it was causing issues so it doesn't now :)
                    /*if (checkShape.oHit == null
                        || Vector3.Distance(i.intersection, checkShape.intersection) >= dist - 0.05f)
                    {
                    }*/
                    SpectrumList lColor = l.lightColor * objColor * l.getIntensityAtPoint(i.intersection);
                        Vector3 Ri = Vector3.Normalize(Vector3.Reflect(Si, i.normal));
                        Vector3 V = Vector3.Normalize(i.intersection - ray.origin);

                        pointColor += lColor * MathF.Max(Vector3.Dot(Si, i.normal), 0.0f);//SI . N;

                        specColor += lColor * MathF.Pow(MathF.Max(Vector3.Dot(Ri, V), 0.0f), i.kE);
                }
            }

            SpectrumList Ilocal = ambientColor + (i.kDiffuse * pointColor) + (i.kSpec * specColor);
            #endregion

            SpectrumList L = Ilocal;

            // Recursive Raytracing, Checkpoint 4 and 5
            if (depth < maxDepth)
            {
                if (i.kReflected > float.Epsilon || i.kTransmitted > float.Epsilon)
                {

                    SpectrumList Ireflected = new SpectrumList(0);
                    SpectrumList Itransmitted = new SpectrumList(0);

                    // Checkpoint 4
                    if (i.kReflected > float.Epsilon)
                    {
                        Vector3 dir = Vector3.Normalize(Vector3.Reflect(ray.direction, i.normal));
                        Ray reflected = new Ray(i.intersection + (0.01f * dir), dir);
                        Ireflected = RayTrace(reflected, depth + 1);
                    }

                    // Checkpoint 5
                    if (i.kTransmitted > float.Epsilon)
                    {
                        if (ray.wavelength == -1)
                        {
                            // Seperate Rays :)

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
                    }

                    L = Ilocal + (i.kReflected * Ireflected) + (i.kTransmitted * Itransmitted);// Calculate Ireflected & Itransmitted;
                }
            }
            foreach(float spec in L.responseList)
            {
                if (float.IsNaN(spec))
                {
                    Console.WriteLine("Error: Spectrum List not real numbers");
                    return new SpectrumList(0f);
                }
            }
            return L;
        }

        // Gets the first intersection of a ray in the scene. 
        // Naive method implemented in CP2.
        public Intersection CastRay(Ray ray)
        {
            float firstDist = float.PositiveInfinity;
            Vector3 firstIntersect = Vector3.Zero;
            IObject? firstObj = null;
            if (useKDTree)
            {
                // Implemented in Advanced CP 1 (KDTree)
                return Tree.GetFirstIntersection(ray);
            }
            else
            {
                foreach (IObject obj in scene)
                {
                    Vector3 intersection = obj.Intersect(ray);
                    if (intersection != Vector3.Zero) // This should probably be changed at some point.
                    {
                        var dist = Vector3.DistanceSquared(intersection, ray.origin);
                        if (dist < firstDist)
                        {
                            firstObj = obj;
                            firstDist = dist;
                            firstIntersect = intersection;
                        }
                    }
                }
            }
            if (firstObj != null)
            {
                Intersection i = firstObj.GetIntersectionData(ray.direction, firstIntersect);
                return i;
            }

            return new Intersection();
        }

        public void SetAmbient(Color color)
        {
            ambientLight = new SpectrumList(color);
        }

        public void AddPointLight(PointLight light)
        {
            lights.Add(light);
        }

        public void AddShapeLight(ShapeLight light)
        {
            shapeLights.Add(light);
        }

        public void AddPointLight(Vector3 pos, Color color)
        {
            lights.Add(new PointLight(pos, color));
        }

        public List<ShapeLight> MakeRefractedLights(Prism prism, ShapeLight shapeLight)
        {
            Ray[,] newRays = new Ray[4, SpectrumConverter.specLength];
            for (int i = 0; i < shapeLight.CornerRays.Length; i++)
            {

                Ray ray = shapeLight.CornerRays[i];
                Intersection intersect = CastRay(ray);
                if (intersect.oHit == null || !prism.IsInPrism(intersect.oHit.GetID()))
                {
                    Console.WriteLine("Warning: Shape Light not fully contained in prism");
                    continue;
                }
                float cosThetaI = -Vector3.Dot(ray.direction, intersect.normal);
                float detPart = (MathF.Pow(cosThetaI, 2) - 1);
                for (int wv = 0; wv < SpectrumConverter.specLength; wv++)
                {
                    // Cast the ray once
                    float refIndex = intersect.medRefIndex.getInd(wv);
                    float nIT = ray.medRefIndex / refIndex;
                    float det = 1 + (MathF.Pow(nIT, 2) * detPart);
                    if (det < 0)
                    {
                        Console.WriteLine("Warning: Shape Light experiences Total Internal Refraction");
                        continue;
                    }
                    float beta = (nIT * cosThetaI) - MathF.Sqrt(det);
                    Vector3 dir = Vector3.Normalize((nIT * ray.direction) + (beta * intersect.normal));
                    Ray transmitted = new Ray(intersect.intersection + (0.02f * dir), dir, refIndex, wv);

                    // Cast the new ray
                    Intersection rfIntersection = CastRay(transmitted);
                    if (intersect.oHit == null || !prism.IsInPrism(intersect.oHit.GetID()))
                    {
                        Console.WriteLine("Warning: Shape Light not fully contained in prism");
                        continue;
                    }
                    cosThetaI = -Vector3.Dot(transmitted.direction, rfIntersection.normal);
                    detPart = (MathF.Pow(cosThetaI, 2) - 1);
                    nIT = transmitted.medRefIndex / refIndex;
                    det = 1 + (MathF.Pow(nIT, 2) * detPart);
                    if (det < 0)
                    {
                        Console.WriteLine("Warning: Shape Light experiences Total Internal Refraction");
                        continue;
                    }
                    beta = (nIT * cosThetaI) - MathF.Sqrt(det);
                    dir = Vector3.Normalize((nIT * transmitted.direction) + (beta * rfIntersection.normal));
                    
                    //Need to make sure we're out of the prism
                    newRays[i, wv] = new Ray(rfIntersection.intersection + (0.5f * dir), dir, refIndex, transmitted.wavelength);
                }
            }

            List<ShapeLight> newLights = new List<ShapeLight>();
            int count = 0;
            for(int i = 0; i < SpectrumConverter.specLength; i++)
            {
                float response = shapeLight.lightColor.GetResponse(i);
                Ray tL = newRays[0, i]; //Testing, something is still weird though
                Ray tR = newRays[1, i]; 
                Ray bL = newRays[2, i]; 
                Ray bR = newRays[3, i];
                if(tL != null && tR != null && bL != null && bR != null)
                {
                    // Check if the rays have flipped somehow
                    Vector3 trueDir = Vector3.Normalize(tL.direction + tR.direction + bL.direction + bR.direction);
                    Vector3 calcDir = Plane.CreateFromVertices(tL.origin, tR.origin, bR.origin).Normal;
                    if(Vector3.Dot(trueDir, calcDir) < 0.0f)
                    {
                        // If the rays flipped, swap them left & right
                        tL = newRays[1, i];
                        tR = newRays[0, i];
                        bL = newRays[3, i];
                        bR = newRays[2, i];
                        Console.WriteLine("Warning: Ray Flipped at " + SpectrumConverter.WaveLengths[i]);
                    }
                    SpectrumList spectrum = new SpectrumList(0);
                    spectrum.SetResponse(i, response);
                    ShapeLight s = new ShapeLight(tL, tR, bL, bR, spectrum);

                    newLights.Add(s);
                    count++;
                }
            }
            Console.WriteLine(count + " / " + SpectrumConverter.specLength + " Lights added successfully");
            return newLights;
        }

        // Creates a KDTree
        public void CreateKDTree()
        {
            Vector3 maxVec = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 minVec = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            // Finds the min & max of objects in the scene.
            foreach (IObject obj in scene)
            {
                Vector3 vMin = obj.AABBmin();
                Vector3 vMax = obj.AABBmax();
                minVec = Vector3.Min(minVec, vMin);
                maxVec = Vector3.Max(maxVec, vMax);
            }

            tree = new KdTree(scene, nLeaf, minVec, maxVec, 8); // Max Depth = 6.
        }
    }
}
