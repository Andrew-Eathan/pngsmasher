using System;
using System.Diagnostics;
using pngsmasher;
using ImageMagick;
using ArrayRange = pngsmasher.Utils.ArrayRange<byte>;

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

            using (var img = new MagickImage("eathan.png", settings))
            {
                img.ColorType = ColorType.TrueColorAlpha;
                Types.PFOptions options = new Types.PFOptions();
                options.blackBackground = true;
                options.imageSplits = 1;
                options.regionMaxSize = 30;
                options.regionMinSize = 10;
                options.seed = 2;
                Console.WriteLine(img.Width);
                Console.WriteLine(img.Height);

                Corruption.CorruptImage(img, options, srand, img.Width, img.Height);

                img.Write("out.png");
            }

            return;












            Stopwatch time = new Stopwatch();
            time.Start();

            //Types.SeedRand srand = new Types.SeedRand(69);

            using (var img = new MagickImage("img.png", settings))
            {
                img.ColorType = ColorType.TrueColorAlpha;
                Types.PFOptions options = new Types.PFOptions();
                Console.WriteLine(img.Width);
                Console.WriteLine(img.Height);
                var pman = img.GetPixels();

                Corruption.CorruptImage(img, options, srand, img.Width, img.Height);

                MagickImage background = new MagickImage(MagickColors.Black, img.Width, img.Height);
                img.Write("out.png");
            }
            time.Stop();
            Console.WriteLine("Finished in " + time.Elapsed.TotalSeconds + "s (" + time.ElapsedMilliseconds + "ms)");
        }
    }
}