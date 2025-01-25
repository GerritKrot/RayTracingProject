using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracingProject
{
    internal class KdTree
    {
        List<IObject> Objects;


        int nLeaf;

        bool isLeaf;


        // Stores information on where the partition for this node is
        Plane Partition;
        int PartitionAxis;
        float PartitionSpot;
        
        // Borders of this node
        Vector3 MinVec;
        Vector3 MaxVec;

        // Front Child is always in direction of positive x, y, z
        KdTree? frontChild;
        // Rear Child is always in direction of negative x, y, z
        KdTree? rearChild;

        private int maxDepth;


        // Creates a KdTree from minVec to MaxVec, containing every object in objects.
        public KdTree(List<IObject> objects, int nLeaf, Vector3 minVec, Vector3 maxVec, int depth)
        {
            Objects = objects;
            MinVec = minVec;
            MaxVec = maxVec;
            maxDepth = depth;
            // If we haven't reached in maximum depth
            // and the node contains too many objects,
            // make new partition and new children nodes
            if (objects.Count > nLeaf && maxDepth > 0)
            {
                isLeaf = false;
                FindPartition();
                MakeChildren();
            }
            else
            {
                // Otherwise this is a leaf node.
                isLeaf = true;
            }
        }

        // Finds an "optimal" partition across the objects.
        private void FindPartition()
        {
            SortedSet<float> xList = new SortedSet<float>();
            SortedSet<float> yList = new SortedSet<float>();
            SortedSet<float> zList = new SortedSet<float>();
            Vector3 axis = Vector3.UnitX;

            foreach (IObject obj in Objects)
            {
                Vector3 objMax = obj.AABBmax();
                xList.Add(objMax.X);
                yList.Add(objMax.Y);
                zList.Add(objMax.Z);
            }

            // Finds a point at which half of the objects are not in one of the nodes.
            // Does this for each axis.
            if (xList.Count > 1 && yList.Count > 1 && zList.Count > 1)
            {
                int xsplit = xList.Count / 2;
                int ysplit = yList.Count / 2;
                int zsplit = zList.Count / 2;

                // Compares each axis to see which split is most definitive.
                float xSplit = xList.ElementAt(xsplit) - xList.ElementAt(xsplit - 1);
                float ySplit = yList.ElementAt(ysplit) - yList.ElementAt(ysplit - 1);
                float zSplit = zList.ElementAt(zsplit) - zList.ElementAt(zsplit - 1);

                if (xSplit < MathF.Min(ySplit, zSplit))
                {
                    PartitionSpot = xList.ElementAt(xsplit) + 0.00001f;
                    PartitionAxis = 0;
                    axis = Vector3.UnitX;
                }
                else if (ySplit <= zSplit)
                {
                    PartitionSpot = yList.ElementAt(ysplit) + 0.00001f;
                    PartitionAxis = 1;
                    axis = Vector3.UnitY;
                }
                else
                {
                    PartitionSpot = zList.ElementAt(zsplit) + 0.00001f;
                    PartitionAxis = 2;
                    axis = Vector3.UnitZ;
                }
            }
            else
            {
                // If the data is close together, default to seperating it in half in the x axis.
                // This is not optimal, but is functional.
                PartitionSpot = (MaxVec.X + MinVec.X) / 2.0f;
                PartitionAxis = 0;
                axis = Vector3.UnitX;
            }

            Partition = new Plane(axis, PartitionSpot);

        }

        // Divides the objects along the partition to distribute them to the children nodes.
        private List<IObject> SplitObjects(bool front)
        {
            List<IObject> objects = new List<IObject>();
            if (front)
            {
                foreach (IObject obj in Objects)
                {
                    Vector3 objMax = obj.AABBmax();
                    float[] max = { objMax.X, objMax.Y, objMax.Z };
                    if (max[PartitionAxis] >= PartitionSpot)
                        objects.Add(obj);
                }
            }
            else
            {
                foreach (IObject obj in Objects)
                {
                    Vector3 objMin = obj.AABBmin();
                    float[] min = { objMin.X, objMin.Y, objMin.Z };
                    if (min[PartitionAxis] <= PartitionSpot)
                        objects.Add(obj);
                }
            }
            return objects;
        }

        // Creates the Children KdTrees
        private void MakeChildren()
        {
            float[] min = { MinVec.X, MinVec.Y, MinVec.Z };
            float[] max = { MaxVec.X, MaxVec.Y, MaxVec.Z };
            float[] frontMin = { MinVec.X, MinVec.Y, MinVec.Z };
            float[] rearMax = { MaxVec.X, MaxVec.Y, MaxVec.Z };
            frontMin[PartitionAxis] = PartitionSpot;
            rearMax[PartitionAxis] = PartitionSpot;

            frontChild = new KdTree(SplitObjects(true), nLeaf, new Vector3(frontMin), new Vector3(max), maxDepth - 1);
            rearChild = new KdTree(SplitObjects(false), nLeaf, new Vector3(min), new Vector3(rearMax), maxDepth - 1);
        }


        // Gets the first intersection in the tree. 
        public Intersection GetFirstIntersection(Ray ray)
        {
            if (isLeaf)
            {
                float firstDist = float.PositiveInfinity;
                Vector3 firstIntersect = Vector3.Zero;
                IObject? firstObj = null;
                foreach (IObject obj in Objects)
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
                if (firstObj != null)
                {
                    Intersection i = firstObj.GetIntersectionData(ray.direction, firstIntersect);
                    return i;
                }
                return new Intersection();
            }

            // Doesn't use T-a b algorithm (couldn't get it to work)
            #region Traversal Algorithm

            KdTree fChild = frontChild!;
            KdTree bChild = rearChild!;

            // Sorts the children in the order they will be traversed by the ray.
            float[] rd = { ray.direction.X, ray.direction.Y, ray.direction.Z };

            if ((rd[PartitionAxis]) > 0)
            {
                fChild = rearChild!;
                bChild = frontChild!;
            }


            // If the ray intersects the first box,
            // If the ray interesects an object in it, we will return that object.
            Intersection j = new Intersection();
            if (fChild.intersectsBox(ray))
            {
                j = fChild.GetFirstIntersection(ray);
            }

            // If the ray intersects the second box,
            // If the ray interesects an object in it, we will return that object.
            if (bChild.intersectsBox(ray) && j.oHit == null)
            {
                j = bChild.GetFirstIntersection(ray);
            }
            #endregion Traversal Algorithm

            // Return whatever we have
            return j;
        }

        // Checks if the ray interscts this node.
        private bool intersectsBox(Ray ray)
        {
            Vector3 rO = ray.origin;
            float[] rayOrg = { rO.X, rO.Y, rO.Z };
            Vector3 rD = ray.direction;
            float[] rayDir = { rD.X, rD.Y, rD.Z };

            float tMin = float.MinValue, tMax = float.MaxValue;

            float[] minBounds = { MinVec.X, MinVec.Y, MinVec.Z };
            float[] maxBounds = { MaxVec.X, MaxVec.Y, MaxVec.Z };

            for (int i = 0; i < 3; i++)
            {
                if(rayDir[i] == 0)
                {
                    if (rayOrg[i] < minBounds[i] || rayOrg[i] > maxBounds[i])
                        return false;
                }
                else
                {
                    float t1 = (minBounds[i] - rayOrg[i]) / rayDir[i];
                    float t2 = (maxBounds[i] - rayOrg[i]) / rayDir[i];
                    tMin = MathF.Max(tMin, MathF.Min(t1, t2));
                    tMax = MathF.Min(tMax, MathF.Max(t1, t2));
                }
            }
            if (tMax < 0 || tMin > tMax)
            {
                return false;
            }
            return true;
        }

        // Was used for T-a b algorithm, no longer used.
        private Vector3 planeIntersection(Ray ray, Plane p)
        {
            Vector3 pN = p.Normal;
            float D = p.D;
            Vector3 rayO = ray.origin;
            Vector3 rayD = ray.direction;
            float num = (pN.X * rayO.X) + (pN.Y * rayO.Y) + (pN.Z * rayO.Z) + D;
            float denom = (pN.X * rayD.X) + (pN.Y * rayD.Y) + (pN.Z * rayD.Z);
            float omega = -num / denom;

            if (omega != 0)
            {
                return ray.origin + (ray.direction * omega);
            }

            return Vector3.Zero;
        }
    }
}
