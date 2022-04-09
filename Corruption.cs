using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using pngfucker.NET;
using Offset = pngfucker.NET.Utils.Offset;

namespace pngfucker.NET
{
    public static class Corruption
    {
        public static void RegionalCorrupt(byte[] rgba, Types.PFOptions options, Types.SeedRand rand, int width)
        {
            if (options.bufferCorruptRegions <= 0) return;
            List<Offset> regions = new List<Offset>();

            // generate regions based on seeded random
            for (int i = 0; i < options.bufferCorruptRegions; i++)
            {
                Offset offset = new Offset();
                offset.Start = Utils.PFFloor(0, rgba.Length, rand);
                offset.End = Math.Clamp(
                    offset.Start +
                    Utils.PFFloor(
                        offset.Start + width * options.regionMinSize,
                        offset.Start + width * options.regionMaxSize,
                        rand
                ), 0, rgba.Length - 1);

                regions.Add(offset);
            }

            // slice off subsections of the image bytes and bitshift them
            foreach (Offset region in regions) {
                byte[] regionBytes = new ArraySegment<byte>(rgba, region.Start, region.End - region.Start).ToArray();
                regionBytes = Utils.ShiftLeft(regionBytes, Utils.PFFloor(1, 32, rand));
                regionBytes.CopyTo(rgba, region.Start);

                int offsetStart = (int)(region.Start + (float)Utils.PFFloor(0, region.End - region.Start, rand) / 1.5);

                // swap pixels around
                for (var i = offsetStart; i < region.End; i++)
                {
                    byte tmp = rgba[i];
                    int i1 = Utils.PFFloor(-6, 6, rand);
                    if (i + i1 >= rgba.Length) continue;
                    rgba[i] = rgba[i + i1];
                    rgba[i + i1] = tmp;
                }
            }
        }

        public static byte[] ImageShiftCorrupt(byte[] rgba, Types.PFOptions options, Types.SeedRand rand, int width)
        {
            if (options.imageSplits <= 0) return null;
            var buffers = new List<byte[]>();

            for (int i = 0; i < options.imageSplits; i++)
            {
                var limit = Utils.PFFloor(0, rgba.Length, rand);

                var segment1 = new ArraySegment<byte>(rgba, 0, rgba.Length - limit - Utils.PFFloor(-width, width, rand));
                buffers.Add(segment1.ToArray());

                var segment2 = new ArraySegment<byte>(rgba, limit, rgba.Length - limit).ToArray();
                var amount = Utils.PFFloor(-40, 40, rand);
                var shifted = amount > 0 ? Utils.ShiftRight(segment2.ToArray(), amount) : Utils.ShiftLeft(segment2.ToArray(), Math.Abs(amount));
                buffers.Add(shifted);
            }

            byte[] yourMassiveMother = new byte[Utils.SnapNumber(buffers.Sum(a => a.Length), options.alpha ? 4 : 3)];
            int cursor = 0;
            foreach (byte[] buffer in buffers) {
                Buffer.BlockCopy(buffer, 0, yourMassiveMother, cursor, Math.Clamp(cursor + buffer.Length, 0, yourMassiveMother.Length - 1) - cursor);
                cursor += buffer.Length;
            }

            return yourMassiveMother;
        }
    }
}
