using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using pngsmasher;
using static pngsmasher.Utils;
using Offset = pngsmasher.Utils.Offset;

namespace pngsmasher
{
    public static class Corruption
    {
        public static void RegionalCorrupt(ref byte[] rgba, int regions, int minheight, int maxheight, Types.SeedRand rand, int width, int height)
        {
            List<Offset> regionArray = new List<Offset>();

            for (int i = 0; i < regions; i++)
            {
                int start = Utils.PFFloor(0, width * height * 4, rand);
                int end = start + Utils.PFFloor(width * 4 * minheight, width * 4 * maxheight, rand);
                regionArray.Add(new Offset(start, end));
            }

            int i11 = 0;
            foreach (Offset i in regionArray)
            {
                i11++;
                Console.WriteLine(i11 + ": " + i.Start + " to " + i.End);
            }

            foreach (Offset region in regionArray)
            {
                var buff = rgba[region.Start..region.End];
                var shiftrand = Utils.PFFloor(1, 32, rand);
                BitShift(buff, buff, -shiftrand);
                rgba.BlitBuffer(buff, region.Start, -10);
            }
        }

        public static void ImageSplitCorrupt(ref byte[] rgba, List<Split> splits, int splitmin, int splitmax, Types.SeedRand rand, int width, int height)
        {
            if (splits.Count > 0)
            {
                List<byte[]> buffers = new List<byte[]>();

                for (int i = 0; i < splits.Count; i++)
                {
                    // the start of the buffer
                    var splitpos = splits[i].SplitBufferPos;

                    var bitShiftAmnt = splits[i].BitshiftAmount;
                    var shift = splits[i].HorizontalShift;

                    // to make the image look sliced and shifted midway

                    var sliceClean = rgba[..(splitpos + shift)];
                    buffers.Add(sliceClean);

                    var sliceShifted = rgba[splitpos..rgba.Length];
                    BitShift(sliceShifted, sliceShifted, bitShiftAmnt);
                    buffers.Add(sliceShifted);

                    var combined = Utils.Combine(buffers);
                    rgba = combined;
                }

                // pad array in case of a shift that takes away too much data
                var temp = new byte[width * height * 4];
                temp.BlitBuffer(rgba, 0);
                rgba = temp;
            }
        }

        public static void ImageSplitCorruptOld(ref byte[] rgba, int splits, int splitmin, int splitmax, Types.SeedRand rand, int width, int height)
        {
            if (splits > 0)
            {
                List<byte[]> buffers = new List<byte[]>();

                for (int i = 0; i < splits; i++)
                {
                    // the start of the buffer
                    var max = width * height * 4;
                    var splitpos = Utils.PFFloor(
                        max * (float)splitmin / 100f,
                        max * (float)splitmax / 100f,
                        rand
                    );

                    var bitShiftAmnt = Utils.PFFloor(-40, 40, rand);
                    var shift = Utils.PFFloor(-width, width, rand);

                    // to make the image look sliced and shifted midway

                    var sliceClean = rgba[..(splitpos + shift)];
                    buffers.Add(sliceClean);

                    var sliceShifted = rgba[splitpos..rgba.Length];
                    BitShift(sliceShifted, sliceShifted, bitShiftAmnt);
                    buffers.Add(sliceShifted);

                    var combined = Utils.Combine(buffers);
                    rgba = combined;
                }

                // pad array in case of a shift that takes away too much data
                var temp = new byte[width * height * 4];
                temp.BlitBuffer(rgba, 0);
                rgba = temp;
            }
        }


        public static void BitShift(Span<byte> input, Span<byte> output, int direction)
        {
            if (input.Length != output.Length)
                throw new InvalidOperationException("Input and output must have the same size.");

            bool shiftRight = direction > 0;
            int bits = Math.Abs(direction);

            int byteShifts = bits / 8;
            int bitShifts = bits % 8;

            var from = shiftRight ? input[..^byteShifts] : input[byteShifts..];
            var to = shiftRight ? output[byteShifts..] : output[..^byteShifts];

            from.CopyTo(to);

            if (bitShifts != 0)
            {
                if (shiftRight)
                {
                    for (int i = output.Length - 1; i > 0; i--)
                        output[i] = (byte)((output[i] >> bitShifts) | (output[i - 1] << 8 - bitShifts));

                    output[0] >>= bitShifts;
                }
                else
                {
                    for (int i = 0; i < output.Length - 1; i++)
                        output[i] = (byte)((output[i] << bitShifts) | (output[i + 1] >> 8 - bitShifts));

                    output[^1] <<= bitShifts;
                }
            }
        }

        public static Utils.Size CalculateModifiedWH(int width, int height, Types.PFOptions options)
        {
            var fmul = options.sizeMul / options.sizeDiv;

            return new Utils.Size
            {
                Width = width * fmul,
                Height = height * fmul
            };
        }

        public static byte[] CrunchImage(byte[] rgba, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            // jimp source used as reference https://github.com/oliver-moran/jimp/blob/master/packages/plugin-resize/src/modules/resize2.js#L24

            byte[] output = new byte[
                dstWidth * dstHeight * 4
            ];

            for (int i = 0; i < dstHeight; i++)
            {
                for (int j = 0; j < dstWidth; j++)
                {
                    int posDst = (i * dstWidth + j) * 4;

                    var iSrc = Math.Floor((double)i * srcHeight / dstHeight);
                    var jSrc = Math.Floor((double)j * srcWidth / dstWidth);
                    int posSrc = (int)((iSrc * srcWidth + jSrc) * 4);

                    for (int k = 0; k < 4; k++)
                        output[posDst++] = rgba[posSrc++];
                }
            }

            return output;
        }

        private static byte ContrastValue(int input, float factor)
        {
            int value = (int)Math.Floor(factor * (input - 127) + 127);

            return (byte)
            (
                value < 0
                    ? 0
                : value > 255
                    ? 255
                : value
            );
        }

        private static byte BrightenValue(int input, float val)
        {
            if (val < 0)
                return (byte)(input * (1 + val));
            else
                return (byte)(input + (255 - input) * val);
        }

        public static void ContrastImage(byte[] input, byte[] output, float val)
        {
            if (val < -1 || val > +1)
                throw new ArgumentException("Contrast value must be between -1 and +1 (zero is no change)");

            float factor = (val + 1) / (1 - val);

            for (int i = 0; i < output.Length; i += 4)
                for (int j = 4; --j > 0;)
                    output[i] = ContrastValue(input[i], factor);
        }

        public static void BrightenImage(byte[] input, byte[] output, float val)
        {
            if (val < -1 || val > +1)
                throw new ArgumentException("Brightness value must be between -1 and +1 (zero is no change)");

            for (int i = 0; i < output.Length; i += 4)
                for (int j = 4; --j > 0;)
                    output[i] = BrightenValue(input[i], val);
        }

        public static byte[] OldStyleCorruptImage(byte[] rgba, Types.PFOptions options, Types.SeedRand srand, int width, int height)
        {
            byte[] rgba_out = rgba.ToArray(); // clone
            int imgwidth = width;
            int imgheight = height;

            if (options.sizeMul / options.sizeDiv != 1 || (options.sizeMul != 0 && options.sizeDiv != 0))
            {
                // image size multiplier and divider
                Utils.Size calc = CalculateModifiedWH(width, height, options);
                rgba_out = CrunchImage(rgba_out, width, height, calc.Width, calc.Height);
            }

            // contrast
            if (options.contrast != 0)
            {
                ContrastImage(rgba_out, rgba_out, options.contrast);
            }

            // crunch effect
            float cwidth = -1;
            float cheight = -1;

            if (options.crunchPercent != 100 || (options.crunchWidth != 0 && options.crunchHeight != 0))
            {
                bool usePercent = options.crunchWidth == 0 && options.crunchHeight == 0;
                cwidth = usePercent ? width * ((float)options.crunchPercent / 100) : options.crunchWidth;
                cheight = usePercent ? height * ((float)options.crunchPercent / 100) : options.crunchHeight;
                if (cwidth < 0) cwidth = width / Math.Abs(cwidth);
                if (cheight < 0) cheight = height / Math.Abs(cheight);

                imgwidth = (int)cwidth;
                imgheight = (int)cheight;

                rgba_out = CrunchImage(rgba_out, width, height, imgwidth, imgheight);
            }

            if (options.bufferShiftBits != 0)
            {
                BitShift(rgba_out, rgba_out, options.bufferShiftBits);
            }

            if (options.bufferCorruptRegions > 0)
            {
                RegionalCorrupt(ref rgba_out, options.bufferCorruptRegions, options.regionMinSize, options.regionMaxSize, srand, imgwidth, imgheight);
            }

            if (options.imageSplits > 0)
            {
                // the start of the buffer
                /*var max = width * height * 4;
                var splitpos = Utils.PFFloor(
                    max * (float)options.splitsMin / 100f,
                    max * (float)options.splitsMax / 100f,
                    srand
                );

                var bitShiftAmnt = Utils.PFFloor(-40, 40, srand);
                var shift = Utils.PFFloor(-width, width, srand);*/

                //ImageSplitCorrupt(ref rgba_out, options.imageSplits, options.splitsMin, options.splitsMax, srand, imgwidth, imgheight);
                ImageSplitCorruptOld(ref rgba_out, options.imageSplits, options.splitsMin, options.splitsMax, srand, imgwidth, imgheight);
            }

            // resize to normal after crunching
            if (cwidth != -1 && cheight != -1)
            {
                rgba_out = CrunchImage(rgba_out, imgwidth, imgheight, width, height);
            }

            return rgba_out;
        }
    }
}
