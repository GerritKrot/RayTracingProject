using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

// Interface for Objects
namespace RayTracingProject
{
    internal interface IObject
    {
        private static int curId = 0;

        public static int NewID()
        {
            var id = curId;
            curId++;
            return id;
        }

        // Calculates the point of intersection from a ray
        // This is used to ensure that we only get neccessary info for shadow rays
        abstract Vector3 Intersect(Ray ray);

        // Gets the set color of the object.
        // Used until CP4, when it was neccessary to get color data from a point on a texture 
        abstract Color GetColor();

        // Returns full intersection datapack from a point and a ray direction.
        // This calculates texture data and returns the normal, color, etc. at that point.
        // Should ONLY be used with intersections calculated from intersect above.
        abstract Intersection GetIntersectionData(Vector3 rayDirection, Vector3 position);


        // Returns the min & max of an axis-aligned bounding-box.
        abstract Vector3 AABBmin();
        abstract Vector3 AABBmax();

        abstract int GetID();
    }
}
