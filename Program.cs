using System;
using System.Diagnostics;
using pngfucker.NET;
using ImageMagick;

namespace pngfucker.NET
{
    class Program
    {

        static void Main()
        {
            Stopwatch time = new Stopwatch();
            time.Start();

            Types.SeedRand srand = new Types.SeedRand(69);

            using (var img = new MagickImage("img.png"))
            {
                Types.PFOptions options = new Types.PFOptions();
                options.regionMinSize = img.Width / Math.Abs(options.regionMinSize);
                options.regionMaxSize = img.Height / Math.Abs(options.regionMaxSize);

                Console.WriteLine(img.Width);
                Console.WriteLine(img.Height);
                var pman = img.GetPixels();
                img.ColorType = ColorType.TrueColor;
                byte[] bytes = pman.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGB);

                bytes = Corruption.ImageShiftCorrupt(bytes, options, srand, img.Width);

                pman.SetPixels(bytes);
                img.Write("out.png");
            }
            time.Stop();
            Console.WriteLine("Finished in " + time.Elapsed.TotalSeconds + "s (" + time.ElapsedMilliseconds + "ms)");
        }
    }
}