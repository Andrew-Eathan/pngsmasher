using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Memory;
using System;
using System.Diagnostics;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;

namespace pngsmasher
{
    class Program
    {
        static void Main()
        {
            Stopwatch time = new();
            time.Start();

            Types.SeedRand srand = new(2);

            Configuration config = Configuration.Default.Clone();
            config.PreferContiguousImageBuffers = true;

            using Image<Rgba32> image = Image.Load<Rgba32>("eathan.png");

            if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
                throw new Exception("Couldn't get a memory handle to raw RGBA, blame imagesharp :(");

            unsafe
            {
                using (MemoryHandle pinHandle = memory.Pin())
                {
                    byte* ptr = (byte*)pinHandle.Pointer;
                    byte[] rgba = new byte[image.Width * image.Height * 4];

                    fixed (byte* le = rgba) {
                        Buffer.MemoryCopy(ptr, le, rgba.Length, image.Width * image.Height * 4);
                    }

                    Types.PFOptions options = new()
                    {
                        imageSplits = 6,
                        crunchPercent = 25
                    };

                    rgba = Corruption.OldStyleCorruptImage(rgba, options, srand, image.Width, image.Height);

                    fixed (byte* le = rgba)
                    {
                        Buffer.MemoryCopy(le, ptr, rgba.Length, image.Width * image.Height * 4);
                    }
                }
            }

            image.Save("out.png");

            time.Stop();
            Console.WriteLine("Finished in " + time.Elapsed.TotalSeconds + "s (" + time.ElapsedMilliseconds + "ms)");
        }
    }
}