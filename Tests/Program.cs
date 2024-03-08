using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string inPath = "C:\\Users\\Leerlingen\\Desktop\\Vision AI\\GitClone2\\Worker\\Watched\\";
            string outPath = "C:\\Users\\Leerlingen\\Desktop\\Vision AI\\GitClone2\\Tests\\Resized\\";

            using (Stream inStream = File.OpenRead(inPath + "1d7d20dbf5af4298aff073f919f6e383.jpg")) 
            {
                //thum 320 180
                //middel 800 450
                //thumb
                using (Image image = Image.Load(inStream))
                {
                    int width = 180;
                    int height = 320;
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(outPath + "resized.jpg"); 
                }

                //Detail
                using (Image image = Image.Load(inStream))
                {
                    int width = 450;
                    int height = 800;
                    image.Mutate(x => x.Resize(width, height));

                    image.Save(outPath + "resized.jpg");
                }
            }

        }
    }
}
