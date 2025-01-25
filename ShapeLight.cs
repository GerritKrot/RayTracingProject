using RayTracingProject.Spectrum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RayTracingProject
{
    internal class ShapeLight
    {
        public SpectrumList lightColor;

        /*private Plane nearPlane;
        private Plane topPlane;
        private Plane bottomPlane;
        private Plane leftPlane;
        private Plane rightPlane;*/

        Vector3 origin;
        Vector3 direction;

        Plane[] planes = new Plane[5];

        public Ray[] CornerRays { get; private set; }

        Vector3 relDownDir;
        Vector3 relRightDir;

        Vector3 topLeftNearCorner;
        Vector3 bottomRightNearCorner;

        float hIntensityScale;
        float vIntensityScale;

        float nearPlaneWidth;
        float nearPlaneHeight;

        public ShapeLight(Vector3 Origin, Vector3 LookAt, Vector3 Up, float width, float height, float angle, SpectrumList l)
        {
            lightColor = l;
            this.origin = Origin;
            this.direction = Vector3.Normalize(LookAt - origin);
            nearPlaneWidth = width;
            nearPlaneHeight = height;
            // Define containing planes based on this
            relDownDir = -Vector3.Normalize(Up);
            relRightDir = Vector3.Normalize(Vector3.Cross(direction, Up));

            float dToOrigin = -Vector3.Dot(origin, direction); // 0, 4, 0  Up 12, 4, 
            planes[0] = new Plane(direction, dToOrigin); // Correct

            topLeftNearCorner = (Origin - (relRightDir * width / 2)) - (relDownDir * height / 2); // Correct
            bottomRightNearCorner = Origin + (relRightDir * width / 2) + (relDownDir * height / 2); // Correct

            float sinAng = MathF.Sin(angle * MathF.PI / 180);
            float dist = Vector3.Distance(Origin, LookAt);

            Vector3 tLExt = LookAt + ((1 + (dist * sinAng)) * -((relDownDir * height / 2) + (relRightDir * width / 2)));
            Vector3 bRExt = LookAt + ((1 + (dist * sinAng)) * ((relDownDir * height / 2) + (relRightDir * width / 2)));

            hIntensityScale = (Vector3.Dot(bRExt - tLExt, relRightDir) - width)
                / (dist * width);
            vIntensityScale = (Vector3.Dot(bRExt - tLExt, relDownDir) - height)
                / (dist * height);

            Vector3 tRCorner = Origin + (relRightDir * width / 2) - (relDownDir * height / 2);
            Vector3 bLCorner = Origin - (relRightDir * width / 2) + (relDownDir * height / 2);
            Vector3 tRExt = LookAt + ((1 + (dist * sinAng)) * ((-relDownDir * height / 2) + (relRightDir * width / 2)));
            Vector3 bLExt = LookAt + ((1 + (dist * sinAng)) * ((relDownDir * height / 2) - (relRightDir * width / 2)));


            planes[1] = Plane.CreateFromVertices(topLeftNearCorner, tLExt, tRCorner);
            planes[2] = Plane.CreateFromVertices(bLCorner, tLExt, topLeftNearCorner);
            planes[3] = Plane.CreateFromVertices(bottomRightNearCorner, bRExt, bLCorner);
            planes[4] = Plane.CreateFromVertices(tRCorner, bRExt, bottomRightNearCorner);

            CornerRays = new Ray[4];
            CornerRays[0] = new Ray(topLeftNearCorner, tLExt - topLeftNearCorner);
            CornerRays[1] = new Ray(tRCorner, tRExt - tRCorner);
            CornerRays[2] = new Ray(bLCorner, bLExt - bLCorner);
            CornerRays[3] = new Ray(bottomRightNearCorner, bRExt - bottomRightNearCorner);

        }

        public ShapeLight(Ray tL, Ray tR, Ray bL, Ray bR, SpectrumList l)
        {
            lightColor = l;
            CornerRays = [tL, tR, bL, bR];

            //Possibly define planes as cross of ray directions 
            relDownDir = Vector3.Normalize(bL.origin - tL.origin);
            relRightDir = Vector3.Normalize(bR.origin - bL.origin);

            topLeftNearCorner = tL.origin;
            bottomRightNearCorner = bR.origin;
            nearPlaneHeight = Vector3.Dot(bottomRightNearCorner - topLeftNearCorner, relDownDir);
            nearPlaneWidth = Vector3.Dot(bottomRightNearCorner - topLeftNearCorner, relRightDir);

            origin = (tL.origin + bR.origin) / 2.0f;

            Vector3 tLExt = tL.origin + tL.direction;
            Vector3 bRExt = bR.origin + bR.direction;

            hIntensityScale = (Vector3.Dot(bRExt - tLExt, relRightDir)
                / Vector3.Dot(bottomRightNearCorner - topLeftNearCorner, relRightDir)) - 1;
            vIntensityScale = (Vector3.Dot(bRExt - tLExt, relDownDir)
                / Vector3.Dot(bottomRightNearCorner - topLeftNearCorner, relDownDir)) - 1;

            planes[0] = Plane.CreateFromVertices(tL.origin, bR.origin, bL.origin);
            direction = planes[0].Normal;

            planes[1] = Plane.CreateFromVertices(tL.origin, tLExt, tR.origin);
            planes[2] = Plane.CreateFromVertices(bL.origin, tLExt, tL.origin);
            planes[3] = Plane.CreateFromVertices(bR.origin, bRExt, bL.origin);
            planes[4] = Plane.CreateFromVertices(tR.origin, bRExt, bR.origin);
        }


        /// <summary>
        ///  Returns a point on our plane from which the light at the passed point is coming
        /// </summary>
        /// <param name="point"> the point to check if it is Lit</param>
        /// <returns></returns>
        public Vector3 LightingPoint(Vector3 point)
        {
            float distFromSource = Vector3.Dot(point, direction) + planes[0].D;

            // make sure that the point is in the area
            for (int i = 0; i < planes.Length; i++)
            {
                var pDist = Vector3.Dot(point, planes[i].Normal) + planes[i].D;
                if (pDist < 0)
                {
                    return point;
                }
            }
            
            Vector3 centerDiff = point - ((distFromSource * direction) + origin);
            float Hvalue = Vector3.Dot(centerDiff, relRightDir) / nearPlaneWidth;
            float Vvalue = Vector3.Dot(centerDiff, relDownDir) / nearPlaneHeight;
            Vector3 newPoint = origin + (Hvalue * relRightDir) + (Vvalue * relDownDir);
            return newPoint;
        }

        // Only to be called with points that are in-bounds
        public float getIntensityAtPoint(Vector3 point)
        {
            float distFromSource = Vector3.Dot(point, direction) - planes[0].D;
            return Math.Abs(1 / (1 + ( hIntensityScale * distFromSource)) * (1 + (vIntensityScale * distFromSource)));
        }
    }
}
