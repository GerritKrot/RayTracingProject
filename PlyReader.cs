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
    // Reads in a PLY file and creates a list of object faces from it.
    internal class PlyReader
    {
        string FileName;
        Vector3 Position;
        Vector3 Scale;
        Color modelColor;

        public PlyReader(string fileName, Vector3 position, Vector3 scale, Color color)
        {
            FileName = fileName;
            Scale = scale;
            Position = position;
            modelColor = color;
        }

        // Returns a list of faces in the object
        public List<IObject> GetObjects()
        {
            RefractionMaterial rfMaterial = new RefractionMaterial(0.85f, 0.95f);

            List<Vector3> verticies = new List<Vector3>();
            List<IObject> tris = new List<IObject>();
            String[] lines = File.ReadAllLines(FileName);
            int index = 0;

            while (lines[index].Any(x => char.IsLetter(x)))
                index++;
            for (int i = index; i < lines.Length; i++)
            {
                if (i % 1000 == 0)
                    Console.WriteLine("Reading Line " + i);
                String[] nums = lines[i].Split(' ');
                if (nums[0] == "3")
                {
                    Vector3 point1 = verticies[int.Parse(nums[1])];
                    Vector3 point2 = verticies[int.Parse(nums[2])];
                    Vector3 point3 = verticies[int.Parse(nums[3])];

                    // Transforms the vectors to a given scale and position
                    // Should probably also include rotation, but did not
                    Vector3 p1 = (point1 * Scale) + Position;
                    Vector3 p2 = (point2 * Scale) + Position;
                    Vector3 p3 = (point3 * Scale) + Position;

                    tris.Add(new Tri(p1, p2, p3, modelColor, kTransmitted: 0.85f, medRefIndex:rfMaterial));
                }
                else
                {
                    float X = float.Parse(nums[0]);
                    float Y = float.Parse(nums[1]);
                    float Z = float.Parse(nums[2]);

                    verticies.Add(new Vector3(X, Y, Z));
                }
            }
            return tris;
        }
    }
}
