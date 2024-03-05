using Smartcrop;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var image = File.OpenRead("image.jpg"))
            {
                // find best crop
                var result = new ImageCrop(200, 200).Crop(image);
                
                Console.WriteLine(
                $"Best crop: {result.Area.X}, {result.Area.Y} - {result.Area.Width} x {result.Area.Height}");
            }
            
        }
    }
}
