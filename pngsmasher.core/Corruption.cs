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
                    // the start of the buffer, take care not to run into max buffer length due to the shrinking caused by splitting
                    var splitpos = Math.Min(splits[i].SplitBufferPos, rgba.Length);
                    var bitShiftAmnt = splits[i].BitshiftAmount;

                    if (splitpos == rgba.Length) continue;

                    // makes the image look sliced and shifted midway
                    var shift = splits[i].HorizontalShift;

                    var sliceClean = rgba[..Math.Clamp(splitpos + shift, 0, rgba.Length - 1)];
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

        public static Size CalculateModifiedWH(int width, int height, float sizeMul, float sizeDiv)
        {
            var fmul = sizeMul / sizeDiv;

            return new Size
            {
                Width = (int)(width * fmul),
                Height = (int)(height * fmul)
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
                        output[posDst++] = rgba[Math.Min(posSrc++, rgba.Length - 1)];
                }
            }

            return output;
        }

        private static byte ContrastValue(float input, float factor)
        {
            int value = (int)Math.Round(factor * (input - 127) + 127);

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

            float factor = Math.Clamp((val + 1) / (1 - val), -1048576, 1048576);

            for (int i = 0; i < output.Length; i += 4)
                for (int j = 0; j < 3; j++)
                    output[i + j] = ContrastValue(input[i + j], factor);
        }

        public static void BrightenImage(byte[] input, byte[] output, float val)
        {
            if (val < -1 || val > +1)
                throw new ArgumentException("Brightness value must be between -1 and +1 (zero is no change)");

            for (int i = 0; i < output.Length; i += 4)
                for (int j = 4; --j > 0;)
                    output[i + j] = BrightenValue(input[i + j], val);
        }

        public static float Lerp(float p, float first, float second)
        {
            return first * (1 - p) + second * p;
        }

        // Underlays a background of specified color under the image
        // If the original uncorrupted image variable isn't null, it tries to only underlay a background under translucent pixels, which helps prevent flashing imagery on APNGs
        public static void UnderlayBackground(byte[] input, byte[] output, byte red, byte green, byte blue, byte[] original = null)
        {
            for (int i = 0; i < output.Length; i += 4)
            {
                float r = input[i];
                float g = input[i + 1];
                float b = input[i + 2];

                // skip pixel if it's mostly transparent
                if (original != null && i < original.Length - 1 && original[i + 3] < 32)
                {
                    output[i] = (byte)r;
                    output[i + 1] = (byte)g;
                    output[i + 2] = (byte)b;
                    output[i + 3] = input[i + 3];
                    continue;
                }

                float a_div = (float)input[i + 3] / 255;

                r = Lerp(a_div, red, r); 
                g = Lerp(a_div, green, g); 
                b = Lerp(a_div, blue, b);
                a_div = 255;

                output[i] = (byte)r;
                output[i + 1] = (byte)g;
                output[i + 2] = (byte)b;
                output[i + 3] = (byte)a_div;
            }
        }

        // got this by accident, looked cool lol
        /*public static void UnderlayScanlineBackground(byte[] input, byte[] output, byte red, byte green, byte blue)
        {
            for (int i = 0; i < output.Length; i += 4)
            {
                float r = input[i];
                float g = input[i + 1];
                float b = input[i + 2];
                float a_div = input[i + 3] / 255;

                r = Lerp(a_div, red, r);
                g = Lerp(a_div, green, g);
                b = Lerp(a_div, blue, b);
                a_div = 255;

                output[i++] = (byte)r;
                output[i++] = (byte)g;
                output[i++] = (byte)b;
                output[i++] = (byte)a_div;
            }
        }*/

        // Clamps corrupted areas using the original uncorrupted image as reference for what's opaque and what's not
        public static void ClampTransparency(byte[] input, byte[] output, byte[] original, int inputWidth, int inputHeight, int originalWidth, int originalHeight)
        {
            if (output.Length != input.Length)
                throw new ArgumentException("Output byte array length is not equal to input");

            for (int y = 0; y < inputHeight; y++)
            {
                for (int x = 0; x < inputWidth; x++)
                {
                    float wRatio = originalWidth / (float)inputWidth;
                    float hRatio = originalHeight / (float)inputHeight;
                    int inputIdx = (x + y * inputWidth) * 4;
                    int originalIdx = (int)(x * wRatio + y * hRatio * originalWidth) * 4;

                    float r = input[inputIdx];
                    float g = input[inputIdx + 1];
                    float b = input[inputIdx + 2];
                    float a = input[inputIdx + 3];

                    float r1 = original[originalIdx];
                    float g1 = original[originalIdx + 1];
                    float b1 = original[originalIdx + 2];
                    float a1 = original[originalIdx + 3];

                    float delta = (r1 - r + g1 - g + b1 - b) / 3;

                    if (a != a1 && delta < 64)
                    {
                        output[inputIdx++] = (byte)r;
                        output[inputIdx++] = (byte)g;
                        output[inputIdx++] = (byte)b;
                        output[inputIdx++] = (byte)a;
                    }
                }
            }
        }

        public static byte[] NTSCEffect(byte[] rgba, int width, int height, int fringeAmount, int xBlurSize, float xBlurPower)
        {
            byte[] output = rgba.ToArray(); // clone

            // fringe
            if (fringeAmount != 0)
            {
                bool fringeAlternate = false;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int thisFringe = fringeAlternate ? fringeAmount : 0;
                        int rawIdx = (x + y * width) * 4;
                        int idx = (Math.Clamp(x + thisFringe, 0, width - 1) + y * width) * 4;

                        output[rawIdx] = rgba[idx];
                    }

                    fringeAlternate = !fringeAlternate;
                }
            }

            // horizontal blur pass
            if (xBlurSize > 0)
            {
                byte[] reference = output.ToArray();

                // iterate over the whole image
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (x + y * width) * 4;

                        // run on all 3 color channels
                        for (int i = 0; i < 3; i++)
                        {
                            float sample = 0;
                            float div = 0;

                            // sample abbWidth pixels to the right of the one we're coloring
                            for (float s = 0; s < xBlurSize; s++)
                            {
                                float divDelta = 1 + s / xBlurPower;

                                // sort of a non-linear average of multiple color samples
                                div += 1 / divDelta;

                                // stays within buffer border, and samples each next pixel to the right
                                int ridx = Math.Clamp(idx + (int)s * 4 + i, 0, rgba.Length - 1);
                                sample += reference[ridx] / divDelta;
                            }

                            // get average
                            sample /= div;

                            // apply
                            output[idx + i] = (byte)sample;
                        }
                    }
            }

            return output;
        }

        // this was supposed to be vertical blur pass for my NTSC filter, but i got a very goofy chroma abberation effect out of it so im keeping it
        public static byte[] GrayAbberation(byte[] rgba, int width, int height, float abbWidth, float abbPower, int abbDetune)
        {
            byte[] output = rgba.ToArray(); // clone
            byte[] reference = output.ToArray(); // clone again

            if (abbWidth == 0)
                abbWidth = 1;

            // iterate over the whole image
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int idx = (x + y * height) * 4;
                    
                    // run on all 3 color channels
                    for (int i = 0; i < 3; i++)
                    {
                        float sample = 0;
                        float div = 0;

                        // sample abbWidth pixels to the right of the one we're coloring
                        for (float s = 0; s < abbWidth; s++)
                        {
                            float divDelta = 1 + s / abbPower;

                            // sort of a non-linear average of multiple color samples
                            div += 1 / divDelta;

                            // witchcraft
                            // stays within buffer border, and samples by trespassing several
                            // color channels in such a way that it causes a color abberation effect
                            int ridx = Math.Clamp(idx + (int)s * 4 + i * 12 * abbDetune, 0, rgba.Length - 1);
                            sample += reference[ridx] / divDelta;
                        }

                        // get average
                        sample /= div;
                        
                        // apply
                        output[idx + i] = (byte)sample;
                    }
                }

            return output;
        }

        // this reinterprets the current color space (rgba8888) to another (by default rgba1616168), simulates the visuals, and converts back to rgba8888
		public static byte[] ImageWidthReinterpret(byte[] rgba, int width, int height, int startHeight, int byteLossCount)
        {
            byte[] rgba_out = new byte[rgba.Length];

            int input = 0;
            for (int i = 0; i < rgba.Length; i++)
            {
                if (input >= rgba_out.Length) break;

                rgba_out[input++] = rgba[i];

                int ypos = (i / 4) / width;
                bool condition;

                if (byteLossCount > 0)
                {
                    condition = input % (width * 4 - byteLossCount) == 0;
                } else condition = i % (width * 4 - byteLossCount) == 0;

                if (ypos > startHeight && condition)
                {
                    input += byteLossCount;
                }
            }

            return rgba_out;
        }


        public static (byte[] rgbaout, int imagewidth, int imageheight, bool tookABreak) OldStyleCorruptImage(byte[] rgba, CLIOptions options, SeedRand srand, int width, int height, bool log, Logging logging)
        {
            byte[] rgba_out = rgba.ToArray(); // clone
            bool tookABreak = false;
            bool applyAuxFX = tookABreak ? !options.noFXOnBreak : true;

            if (options.breaks > 0)
            {
                double chance = srand.Generate(0, 100, true);
                if (chance <= options.breaks)
                {
                    srand.Generate(); // shuffle seed just in case
                    tookABreak = true;
                }
            }

            int imgwidth = width;
            int imgheight = height;

            if (options.sizeMul / options.sizeDiv != 1)
            {
                // image size multiplier and divider
                Size calc = CalculateModifiedWH(width, height, options.sizeMul, options.sizeDiv);
                rgba_out = CrunchImage(rgba_out, width, height, calc.Width, calc.Height);
                imgwidth = calc.Width;
                imgheight = calc.Height;

                if (log)
                    logging.Write("\tResized image from " + width + "x" + height + " to " + imgwidth + "x" + imgheight, false);
            }

            // apply contrast aux effect
            if (applyAuxFX && options.contrast != 0)
            {
                ContrastImage(rgba_out, rgba_out, options.contrast);
                if (log)
                    logging.Write("\tContrasted the image by " + (options.contrast < 0 ? "" : "+") + Convert.ToString(options.contrast), false);
            }

            // apply crunch aux effect
            float cwidth = -1;
            float cheight = -1;

            if (applyAuxFX && (options.crunchPercent != 100 || (options.crunchWidth != 0 && options.crunchHeight != 0)))
            {
                bool usePercent = options.crunchWidth == 0 && options.crunchHeight == 0;
                cwidth = usePercent ? width * ((float)options.crunchPercent / 100) : options.crunchWidth;
                cheight = usePercent ? height * ((float)options.crunchPercent / 100) : options.crunchHeight;
                if (cwidth < 0) cwidth = width / Math.Abs(cwidth);
                if (cheight < 0) cheight = height / Math.Abs(cheight);

                if (log)
                    logging.Write("\tCrunched the image from " + imgwidth + "x" + imgheight + " to " + (int)cwidth + "x" + (int)cheight, false);

                rgba_out = CrunchImage(rgba_out, imgwidth, imgheight, (int)cwidth, (int)cheight);

                imgwidth = (int)cwidth;
                imgheight = (int)cheight;
            }

            if (!tookABreak && options.rewidths > 0) {
				for (int i = 0; i < options.rewidths; i++)
				{
					int deviation = (int)srand.Generate(-3, 3);
					int randheight = (int)srand.Generate(0, imgheight);
					rgba_out = ImageWidthReinterpret(rgba_out, imgwidth, imgheight, randheight, deviation);
                }
			}

            // apply buffer bitshift corruption effect
            int shiftAmount = options.bufferShiftBits;
            if (!tookABreak && options.randomShiftAmount != 0)
            {
                shiftAmount += (int)Math.Round(srand.Generate(-options.randomShiftAmount, options.randomShiftAmount));
            }
            if (!tookABreak && shiftAmount != 0)
            {
                BitShift(rgba_out, rgba_out, shiftAmount);
                if (log)
                    logging.Write("\tBitshifted the image " + Math.Abs(shiftAmount) + " bits to the " + (shiftAmount > 0 ? "right" : "left"), false);
            }

            // apply regional corruption effect
            if (!tookABreak && options.corruptRegions > 0)
            {
                List<Region> regionArray = new();

                if (options.regionMinSize < 0)
                {
                    options.regionMinSize = imgheight / Math.Abs(options.regionMinSize);
                    options.regionMaxSize = imgheight / Math.Abs(options.regionMaxSize);
                }

                for (int i = 0; i < options.corruptRegions; i++)
                {
                    int start = PFFloor(0, imgwidth * imgheight * 4, srand);
                    int end = start + PFFloor(imgwidth * 4 * options.regionMinSize, imgwidth * 4 * options.regionMaxSize, srand);
                    regionArray.Add(new Region(start, end, 0, -10));
                }

                // to keep compatibility with nodejs png****er's outputs this data needs to be generated separately
                for (int i = 0; i < regionArray.Count; i++)
                {
                    Region lol = regionArray[i];
                    lol.BitshiftAmount = -PFFloor(1, 32, srand);
                    regionArray[i] = lol;
                }

                RegionalCorrupt(ref rgba_out, regionArray);
                if (log)
                    logging.Write("\tApplied " + options.corruptRegions + " corrupted regions to the image", false);
            }

            // apply image splits corruption effect
            if (!tookABreak && options.imageSplits > 0)
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
                if (log)
                    logging.Write("\tCreated " + options.corruptRegions + " corrupted splits", false);
            }

            // image clamp aux effect
            if (applyAuxFX && options.clamp)
            {
                Size calc = CalculateModifiedWH(width, height, options.sizeMul, options.sizeDiv);
                ClampTransparency(rgba_out, rgba_out, rgba, imgwidth, imgheight, calc.Width, calc.Height);

                if (log)
                    logging.Write("\tClamped image corruption to opaque pixels", false);
            }

            // image background underlay aux effect
            if (applyAuxFX && options.bg)
            {
                UnderlayBackground(rgba_out, rgba_out, (byte)options.bgRed, (byte)options.bgGreen, (byte)options.bgBlue, options.bgClamp ? rgba_out : null);

                if (log)
                    logging.Write("\tUnderlayed a background in the image", false);
            }

            // resize to normal after crunching
            if (applyAuxFX && cwidth != -1 && cheight != -1)
            {
                Size calc = CalculateModifiedWH(width, height, options.sizeMul, options.sizeDiv);

                rgba_out = CrunchImage(rgba_out, imgwidth, imgheight, calc.Width, calc.Height);
                if (log)
                    logging.Write("\tCrunched image from " + imgwidth + "x" + imgheight + " back to " + calc.Width + "x" + calc.Height, false);

                imgwidth = calc.Width;
                imgheight = calc.Height;
            }

            if (applyAuxFX && options.ntsc)
            {
                rgba_out = NTSCEffect(rgba_out, imgwidth, imgheight, options.fringe, options.xBlur, options.xBlurPower);
                if (log)
                    logging.Write("\tApplied pngsmasher's NTSC filtering", false);
            }

            if (applyAuxFX && options.grayAbberation)
            {
                rgba_out = GrayAbberation(rgba_out, imgwidth, imgheight, options.grayAbberationWidth, options.grayAbberationPower, options.grayAbberationDetune);
                if (log)
                    logging.Write("\tApplied pngsmasher's gray abberation", false);
            }

            return (rgba_out, imgwidth, imgheight, tookABreak);
        }
    }
}
