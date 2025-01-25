using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RayTracingProject
{
    // Class was implemented in Checkpoint 2.
    // Modifications: Ray block code was implement in Checkpoint 3 (supersampling
    internal class Camera
    {
        Vector3 position;
        Vector3 lookAt; // Equivalent to relative negative Z axis
        Vector3 up; // Equivalent to relative positive y axis

        readonly float targetWidthFOV;

        readonly float targetWidth;    // Resolution of Image
        readonly float targetHeight;

        public Ray[,] rays;
        public static int rayBlockDiv = 3; // Number of subdivisions in a ray tracing block.
                                           // There are x^2 rays in each block.
        public static int rayBlockSize => (int) Math.Pow(rayBlockDiv, 2);
        public Ray[,,] rayBlocks;

        public Camera(Vector3 Position, Vector3 LookAt, Vector3 Up, float FOV = MathF.PI / 4.0f, float Width = 800, float Height = 800)
        {
            rays = new Ray[(int)Width, (int)Height];
            rayBlocks = new Ray[(int)Width, (int)Height, (int) rayBlockSize];
            position = Position;
            up = Up;
            lookAt = LookAt;
            targetWidthFOV = FOV;
            targetWidth = Width;
            targetHeight = Height;
        }

        public Camera() : this(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY)
        {
        }

        // Creates Rays to trace
        public void makeRays()
        {
            Vector3 planeX = Vector3.Cross(lookAt, up);
            Vector3 planeY = Vector3.Normalize(up);
            float halfProjWidth = MathF.Tan(targetWidthFOV / 2);
            Vector3 projWInc = 2 * planeX * halfProjWidth / targetWidth;
            float halfProjHeight = MathF.Tan(targetWidthFOV * (targetHeight / targetWidth) / 2);
            Vector3 projHInc = 2 * planeY * halfProjHeight / targetHeight;
            Vector3 planeCenter = Vector3.Normalize(lookAt);
            Vector3 tLCorner = planeCenter - (halfProjWidth * planeX) - (halfProjHeight * planeY);

            for (int i = 0; i < targetWidth; i++)
            {
                for (int j = 0; j < targetHeight; j++)
                {
                    Vector3 rayTo = tLCorner + (projWInc * i) + (projHInc * j);
                    rays[i, j] = new Ray(position, rayTo);
                }
            }

        }


        // Creates Blocks of Rays to trace throughout the scene. 
        // Results of these Rays are averaged to produce better results.
        public void makeRayBlocks()
        {
            Vector3 planeX = Vector3.Cross(lookAt, up);
            Vector3 planeY = Vector3.Normalize(up);
            float halfProjWidth = MathF.Tan(targetWidthFOV / 2);
            Vector3 projWInc = 2 * planeX * halfProjWidth / targetWidth;
            float halfProjHeight = MathF.Tan(targetWidthFOV * (targetHeight / targetWidth) / 2);
            Vector3 projHInc = 2 * planeY * halfProjHeight / targetHeight;
            Vector3 planeCenter = Vector3.Normalize(lookAt);
            Vector3 tLCorner = planeCenter - (halfProjWidth * planeX) - (halfProjHeight * planeY);

            for (int i = 0; i < targetWidth; i++)
            {
                for (int j = 0; j < targetHeight; j++)
                {
                    // Instantiates rayblocks, evenly spaced throughout the pixel.
                    for(int x = 0; x < rayBlockDiv; x++)
                    {
                        for (int y = 0; y < rayBlockDiv; y++)
                        {
                            Vector3 rayTo = tLCorner + (projWInc * (i + (x / rayBlockDiv)) + 
                                (projHInc * (j + (y / rayBlockDiv))));
                            rayBlocks[i, j, (x * rayBlockDiv) + y] = new Ray(position, rayTo);
                        }
                    }
                }
            }
        }
    }
}
