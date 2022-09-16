using System;
using System.Diagnostics;
using pngsmasher;
using ImageMagick;

namespace pngsmasher
{
    class Program
    {
        static void log(byte[] test)
        {
            foreach (byte b in test)
            {
                Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
            }
            Console.WriteLine();
        }

        static void Main()
        {
            var settings = new MagickReadSettings();
            settings.ColorType = ColorType.TrueColorAlpha;
            Types.SeedRand srand = new Types.SeedRand(2);

            Stopwatch time = new Stopwatch();

            using (var img = new MagickImage("eathan.png", settings))
            {
                img.ColorType = ColorType.TrueColorAlpha;
                Types.PFOptions options = new Types.PFOptions();
                options.corruptRegions = 2;
                /*options.blackBackground = true;
                options.bufferShiftBits = 0;
                options.imageSplits = 1;
                options.seed = 2;
                Console.WriteLine(img.Width);
                Console.WriteLine(img.Height);

                var pixels = img.GetPixels();
                var bytes = pixels.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGBA);

                time.Start();
                bytes = Corruption.OldStyleCorruptImage(bytes, options, srand, img.Width, img.Height);
                time.Stop();

                pixels.SetPixels(bytes);

                img.Write("out.png");
            }

            Console.WriteLine("Finished in " + time.Elapsed.TotalSeconds + "s (" + time.ElapsedMilliseconds + "ms)");
        }
    }
}