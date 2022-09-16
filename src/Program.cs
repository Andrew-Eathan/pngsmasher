using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Memory;
using System;
using System.Diagnostics;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using System.Drawing;
using static pngsmasher.Core.Types;

namespace pngsmasher
{
    class Program
    {
        static void Main()
        {
            Stopwatch time = new();
            time.Start();

            SeedRand srand = new(2);

            Configuration config = Configuration.Default.Clone();
            config.PreferContiguousImageBuffers = true;

            using Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>("eathan.png");

            if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
                throw new Exception("Couldn't get a memory handle to raw RGBA, blame imagesharp :(");

            Stopwatch time2 = new();
            time2.Start();
            unsafe
            {
                using (MemoryHandle pinHandle = memory.Pin())
                {
                    byte* ptr = (byte*)pinHandle.Pointer;
                    byte[] rgba = new byte[image.Width * image.Height * 4];

                    Stopwatch time1 = new();
                    time1.Start();
                    fixed (byte* le = rgba) {
                        Buffer.MemoryCopy(ptr, le, rgba.Length, image.Width * image.Height * 4);
                    }
                    time1.Stop();
                    Console.WriteLine(time1.ElapsedMilliseconds + "ms - copy1");
                    time1.Reset();

                    PFOptions options = new()
                    {
                        imageSplits = 6,
                        crunchPercent = 25
                    };

                    time1.Start();
                    rgba = Corruption.OldStyleCorruptImage(rgba, options, srand, image.Width, image.Height);
                    time1.Stop();
                    Console.WriteLine(time1.ElapsedMilliseconds + "ms - corruption");
                    time1.Reset();

                    time1.Start();
                    fixed (byte* le = rgba)
                    {
                        Buffer.MemoryCopy(le, ptr, rgba.Length, image.Width * image.Height * 4);
                    }
                    time1.Stop();
                    Console.WriteLine(time1.ElapsedMilliseconds + "ms - copy2");
                    time1.Reset();
                }
            }
            time2.Stop();
            Console.WriteLine(time2.ElapsedMilliseconds + "ms - processing");
            time2.Reset();

            long mv = 0;
            time2.Start();
            for (int i = 0; i < 200; i++)
            {
                Stopwatch time3 = new();
                time3.Start();
                image.Save("out.png");
                time3.Stop();
                mv += time3.ElapsedMilliseconds;
                Console.WriteLine(i + ": " + time3.ElapsedMilliseconds + "ms");
            }
            time2.Stop();
            Console.WriteLine("average: " + (mv / 200) + "ms");
            Console.WriteLine(time2.ElapsedMilliseconds + "ms - save");
            time2.Reset();

            time.Stop();
            Console.WriteLine("Finished in " + time.Elapsed.TotalSeconds + "s (" + time.ElapsedMilliseconds + "ms)");
        }
    }
}