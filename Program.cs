using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using RayTracingProject;
using RayTracingProject.Spectrum;

class Program
{

    static void Main(string[] args)
    {

        Bitmap bmp = new Bitmap(400, 100);
        for (int i = 380; i < 780; i++)
        {
            Vector3 c = LightColor.WaveColor(i);
            Color c1 = ToneReproducer.VecToColor(c);
            for (int j = 0; j < 100; j++)
            {
                bmp.SetPixel(i - 380, j, c1);
            }
        }
        bmp.Save("Color Test.jpg");

        // Spectrum MUST be set before everything else
        SpectrumConverter.SetSpectrum(7);

        #region Variable Declarations
        // Camera Variables
        Vector3 camPosition = Vector3.Zero;
        Vector3 camLookAt = -Vector3.UnitZ;
        Vector3 camUp = Vector3.UnitY;
        float FOV = MathF.PI / 4.0f;
        int ImgWidth = 1920;
        int ImgHeight = 1080;

        // World Variables
        bool useKDTree = true; // There is an issue with KDTrees right now. not sure why :(
        Scene curScene = Scene.standard; //Which scene we are showing

        #region Default Scene Variables
        // Default Scene Variables
        Vector3 sphere1Pos = new Vector3(0.5f, 0, -6);
        Vector3 sphere2Pos = new Vector3(-1.2f, -0.8f, -7.5f);
        Vector3[] floorPoints = {
                new Vector3( -5, -2, 0.5f ),
                new Vector3( -5, -2, -25),
                new Vector3(2.5f, -2, 0.5f),
                new Vector3(2.5f, -2, -25),
            };
        Sphere sphere1 = new Sphere(sphere1Pos, 1.0f, Color.White,
                kAmbient: 0.075f, kDiffuse: 0.075f, kSpec: 0.2f, kE: 20f,
                kReflected: 0.01f, kTransmitted: 0.8f, refIndex: new RefractionMaterial(0.85f, 0.95f));
        Sphere sphere2 = new Sphere(sphere2Pos, 0.75f, Color.White,
             kAmbient: 0.105f, kDiffuse: 0.175f, kSpec: 1.0f, kE: 20f,
            kReflected: 0.75f);
        ITexture floorTex = new texCheckerboard(Color.Red, Color.Yellow, 0.5f, floorPoints[0]);
        #endregion

        #region Prism Scene Variables
        Vector3[] prismFloorPoints = {
            new Vector3( -5, -2, 0.5f ),
            new Vector3( -5, -2, -25),
            new Vector3( 5, -2, 0.5f),
            new Vector3( 5, -2, -25),
          };

        Vector3[] prismWallPoints = {
            new Vector3( -5, 10, 0.5f ),
            new Vector3( -5, 10, -25),
            new Vector3( 5, 10, 0.5f),
            new Vector3( 5, 10, -25),
        };

        Prism prism = new Prism(new Vector3(-2f, 0f, -10.0f), new Vector3(2, 2, 1.5f), new Vector3(90, 50, 0), 
            refIndex: new RefractionMaterial(1.4f, 2.5f));

        ShapeLight shLight = new ShapeLight(new Vector3(-2.0f, 0.01f, -9f), new Vector3(-1.5f, 0, -10f),
        Vector3.Normalize(new Vector3(0, 1, 0)), 0.3f, 1.4f, 5.0f, new SpectrumList(8));
        
        // ShapeLights can also be defined with 4 rays as corner points.
        /*Ray tl = new Ray(new Vector3(-1.9f, 5, -15.1f), new Vector3(0.1f, -1, -0.1f));
        Ray tR = new Ray(new Vector3(-1.9f, 5, -14.9f), new Vector3(0.1f, -1, 0.1f));
        Ray bl = new Ray(new Vector3(-2.1f, 5, -15.1f), new Vector3(-0.1f, -1, -0.1f));
        Ray bR = new Ray(new Vector3(-2.1f, 5, -14.9f), new Vector3(-0.1f, -1, 0.1f));
        ShapeLight shLight = new ShapeLight(tl, tR, bl, bR, new SpectrumList(4));*/
        #endregion


        // Light Variables
        Vector3 lightPosition = new Vector3(2, 5, 3);
        Color lightColor = Color.White;
        float lightIntensity = 3.0f;

        #endregion

        // Creates Scene
        World world = new World();
        world.useKDTree = useKDTree;

        // Creates new Camera
        Camera camera = new Camera(camPosition, camLookAt, camUp, FOV, ImgWidth, ImgHeight);

        // Populates the World with objects
        if (curScene == Scene.rabbit)
        {
            PlyReader plyReader = new PlyReader("bun_zipper_res2.ply",
           new Vector3(0, 0, -8), new Vector3(12, 12, 12), Color.Tomato);

            world.scene = plyReader.GetObjects();
        }
        else if (curScene == Scene.standard)
        {
            // Creates the scene using the values found in CP 1.
            // Color in CP2, various k values in CP3-6 as needed. (See above)

            world.scene.Add(sphere1);
            world.scene.Add(sphere2);

            Tri tri1 = new Tri(prismFloorPoints[0], prismFloorPoints[1], prismFloorPoints[2], Color.Red, texture: floorTex);
            Tri tri2 = new Tri(prismFloorPoints[1], prismFloorPoints[2], prismFloorPoints[3], Color.Yellow, texture: floorTex);

            world.scene.Add(tri1);
            world.scene.Add(tri2);
        }
        else
        {
            Tri tri1 = new Tri(prismFloorPoints[0], prismFloorPoints[1], prismFloorPoints[2], Color.Red, texture: floorTex);
            Tri tri2 = new Tri(prismFloorPoints[1], prismFloorPoints[2], prismFloorPoints[3], Color.Yellow, texture: floorTex);

            Tri Wall1 = new Tri(prismFloorPoints[0], prismFloorPoints[1], prismWallPoints[0],  Color.Gray);
            Tri Wall2 = new Tri(prismFloorPoints[1], prismWallPoints[1], prismWallPoints[0],  Color.Gray);
            

            Tri Wall3 = new Tri(prismFloorPoints[2], prismWallPoints[2], prismFloorPoints[3], Color.Gray);
            Tri Wall4 = new Tri(prismWallPoints[2], prismWallPoints[3], prismFloorPoints[3], Color.Gray);


            Tri Wall5 = new Tri(prismFloorPoints[1], prismFloorPoints[3], prismWallPoints[3], Color.DarkGray);
            Tri Wall6 = new Tri(prismWallPoints[3], prismWallPoints[1], prismFloorPoints[1], Color.DarkGray);

            world.scene.Add(tri1);
            world.scene.Add(tri2);
            world.scene.Add(Wall1);
            world.scene.Add(Wall2);
            world.scene.Add(Wall3);
            world.scene.Add(Wall4);
            world.scene.Add(Wall5);
            world.scene.Add(Wall6);
            List<Tri> prismFaces = prism.getFaces();
            foreach (Tri tri in prismFaces)
            {
                world.scene.Add(tri);
            }
            //world.AddShapeLight(shLight);
            
            List<ShapeLight> newLights = world.MakeRefractedLights(prism, shLight);

            if(newLights.Count > 0)
            {
                foreach(ShapeLight light in newLights)
                {
                    world.shapeLights.Add(light);
                }
            }
        }

        if (useKDTree)
        {
            world.useKDTree = useKDTree;
            var w = System.Diagnostics.Stopwatch.StartNew();
            world.CreateKDTree();
            w.Stop();
            Console.WriteLine("Time to create KDTree: " + w.ElapsedMilliseconds / 1000.0f);
        }

        // Note: image used to be a bitmap, but needed to be converted into a vec3 array to save info for TR.
        Vector3[,] image = new Vector3[ImgWidth, ImgHeight];

        // Intensity value of 10 used for HDR imaging, but can be set to 1 for non-HDR images
        world.AddPointLight(new PointLight(lightPosition, lightColor, lightIntensity));

        var watch = System.Diagnostics.Stopwatch.StartNew();
        Console.WriteLine("Tracing Rays...");
        camera.makeRayBlocks();
        for (int i = 0; i < ImgHeight; i++)
        {
            if (i % 10 == 0)
                Console.WriteLine((i * 100 / ImgHeight) + "%");
            for (int j = 0; j < ImgWidth; j++)
            {
                SpectrumList cVec = new SpectrumList(0);
                for (int k = 0; k < Camera.rayBlockSize; k++)
                {
                    Ray ray = camera.rayBlocks[j, i, k];

                    cVec += world.getRayColor(ray);
                }
                image[j, ImgHeight - 1 - i] = cVec.totalValue() / Camera.rayBlockSize;
            }
        }
        watch.Stop();
        // Prints time to trace rays in seconds.
        Console.WriteLine("Raytracing Time: " + watch.ElapsedMilliseconds / 1000.0f);

        SaveArrToBmp(image, "Test1");


        // Tone Reproduction
        Console.WriteLine("Reproducing Tone...");
        ToneReproducer t = new ToneReproducer(image, displayMax: 500);

        t.ToneImage(ldMax: 500, lMax: 10).Save("Ward10.jpg");
        t.ToneImage(ldMax: 500, lMax: 100).Save("Ward100.jpg");
        t.ToneImage(ldMax: 500, lMax: 1000).Save("Ward1000.jpg");

        // Adding a compressionMethod changes it
        // meaning we don't have to pass the argument each time.
        t.ToneImage(ldMax: 500, lMax: 10, cMethod: CompressionMethod.AdaptiveLuminance).Save("AdaptiveLuminance10.jpg");
        t.ToneImage(ldMax: 500, lMax: 100).Save("AdaptiveLuminance100.jpg");
        t.ToneImage(ldMax: 500, lMax: 1000).Save("AdaptiveLuminance1000.jpg");


        t.ToneImage(ldMax: 500, lMax: 10, cMethod: CompressionMethod.Reinhard).Save("Reinhard10_18.jpg");
        t.ToneImage(ldMax: 500, lMax: 100).Save("Reinhard100_18.jpg");
        t.ToneImage(ldMax: 500, lMax: 1000).Save("Reinhard1000_18.jpg");

        t.ToneImage(ldMax: 500, reinhardAlpha: 0.5f).Save("Reinhard1000_50.jpg");
        t.ToneImage(ldMax: 500, reinhardAlpha: 0.01f).Save("Reinhard1000_01.jpg");
        t.ToneImage(ldMax: 500, reinhardAlpha: 0.89f).Save("Reinhard1000_89.jpg");

    }

    // Implemented in Checkpoint 7, after using vec3 arrays instead of colors.
    public static Bitmap SaveArrToBmp(Vector3[,] img, string Name)
    {
        Bitmap bmp = new Bitmap(img.GetLength(0), img.GetLength(1));
        for (int i = 0; i < img.GetLength(0); i++)
        {
            for (int j = 0; j < img.GetLength(1); j++)
            {
                bmp.SetPixel(i, j, ToneReproducer.VecToColor(img[i, j]));
            }
        }
        bmp.Save(Name + ".jpg");
        return bmp;
    }

    public enum Scene
    {
        standard,
        rabbit,
        prism,
    };
}

