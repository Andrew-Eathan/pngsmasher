using System;
using System.Collections.Generic;
using System.Linq;
using static pngsmasher.Core.Types;
using static pngsmasher.Core.Utils;

namespace pngsmasher.Core
{
    public static class Corruption
    {
        public static void RegionalCorrupt(ref byte[] rgba, List<Region> regions)
        {
            foreach (Region region in regions)
            {
                var buff = rgba[region.Start..Math.Clamp(region.End, 0, rgba.Length - 1)];
                BitShift(buff, buff, region.BitshiftAmount);
                rgba.BlitBuffer(buff, region.Start, region.AddValue);
            }
        }

        public static void ImageSplitCorrupt(ref byte[] rgba, List<Split> splits, int width, int height)
        {
            if (splits.Count > 0)
            {
                List<byte[]> buffers = new();

                for (int i = 0; i < splits.Count; i++)
                {
                    // the start of the buffer
                    var splitpos = splits[i].SplitBufferPos;
                    var bitShiftAmnt = splits[i].BitshiftAmount;

                    // makes the image look sliced and shifted midway
                    var shift = splits[i].HorizontalShift;

                    var sliceClean = rgba[..(splitpos + shift)];
                    buffers.Add(sliceClean);

                    var sliceShifted = rgba[splitpos..rgba.Length];
                    BitShift(sliceShifted, sliceShifted, bitShiftAmnt);
                    buffers.Add(sliceShifted);

                    var combined = Combine(buffers);
                    rgba = combined;

                    buffers.Clear();
                }

                // keep array length in case of a shift that takes away/gives too much data
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

        public static Size CalculateModifiedWH(int width, int height, PFOptions options)
        {
            var fmul = options.sizeMul / options.sizeDiv;

            return new Size
            {
                Width = width * fmul,
                Height = height * fmul
            };
        }

        public static byte[] CrunchImage(byte[] rgba, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            // jimp source used as reference https://github.com/oliver-moran/jimp/blob/master/packages/plugin-resize/src/modules/resize2.js#L24
            // this is just nearest-neighbor resizing

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

        private static byte ContrastValue(float input, float factor)
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

        public static void UnderlayBlack(byte[] input, byte[] output)
        {
            for (int i = 0; i < output.Length; i += 4)
            {
                float r = input[i];
                float g = input[i + 1];
                float b = input[i + 2];
                float a_div = (float)input[i + 3] / 255;

                r *= a_div;
                g *= a_div;
                b *= a_div;
                a_div = 255;

                output[i] = (byte)r;
                output[i + 1] = (byte)g;
                output[i + 2] = (byte)b;
                output[i + 3] = (byte)a_div;
            }
        }

        // got this by accident, looked cool lol
        public static void UnderlayScanlineBlack(byte[] input, byte[] output)
        {
            for (int i = 0; i < output.Length; i += 4)
            {
                float r = input[i];
                float g = input[i + 1];
                float b = input[i + 2];
                float a_div = input[i + 3] / 255;

                r *= a_div;
                g *= a_div;
                b *= a_div;
                a_div = 255;

                output[i++] = (byte)r;
                output[i++] = (byte)g;
                output[i++] = (byte)b;
                output[i++] = (byte)a_div;
            }
        }

        // Clamps corruption area using the original image as reference
        public static void ClampTransparency(byte[] input, byte[] output, byte[] original)
        {
            for (int i = 0; i < output.Length; i += 4)
            {
                float r = input[i];
                float g = input[i + 1];
                float b = input[i + 2];
                float a = input[i + 3];

                float r1 = original[i];
                float g1 = original[i + 1];
                float b1 = original[i + 2];
                float a1 = original[i + 3];

                float delta = (r1 - r + g1 - g + b1 - b) / 3;

                if (a != a1 && delta < 64)
                {
                    output[i++] = (byte)r;
                    output[i++] = (byte)g;
                    output[i++] = (byte)b;
                    output[i++] = (byte)a;
                }
            }
        }

        // simulates the behavior of old pngf***er as accurately as it can
        public static byte[] OldStyleCorruptImage(byte[] rgba, PFOptions options, SeedRand srand, int width, int height)
        {
            byte[] rgba_out = rgba.ToArray(); // clone
            int imgwidth = width;
            int imgheight = height;

            if (options.sizeMul / options.sizeDiv != 1 || (options.sizeMul != 0 && options.sizeDiv != 0))
            {
                // image size multiplier and divider
                Size calc = CalculateModifiedWH(width, height, options);
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

            if (options.corruptRegions > 0)
            {
                List<Region> regionArray = new();

                for (int i = 0; i < options.corruptRegions; i++)
                {
                    int start = PFFloor(0, imgwidth * imgheight * 4, srand);
                    int end = start + PFFloor(imgwidth * 4 * options.regionMinSize, imgwidth * 4 * options.regionMaxSize, srand);
                    regionArray.Add(new Region(start, end, 0, -10));
                }

                // to keep compatibility with nodejs png****er's outputs these need to be generated separately
                for (int i = 0; i < regionArray.Count; i++)
                {
                    Region lol = regionArray[i];
                    lol.BitshiftAmount = -PFFloor(1, 32, srand);
                    regionArray[i] = lol;
                }

                RegionalCorrupt(ref rgba_out, regionArray);
            }

            if (options.imageSplits > 0)
            {
                List<Split> splits = new();

                for (int i = 0; i < options.imageSplits; i++)
                {
                    // the start of the buffer
                    var max = imgwidth * imgheight * 4;
                    var splitpos = PFFloor(
                        max * (float)options.splitsMin / 100f,
                        max * (float)options.splitsMax / 100f,
                        srand
                    );

                    var bitShiftAmnt = PFFloor(-40, 40, srand);
                    var shift = PFFloor(-imgwidth, imgwidth, srand);

                    splits.Add(new Split(splitpos, bitShiftAmnt, shift));
                }

                ImageSplitCorrupt(ref rgba_out, splits, imgwidth, imgheight);
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
