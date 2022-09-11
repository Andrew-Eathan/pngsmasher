using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using pngsmasher;
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
                Utils.Shift(buff, buff, -shiftrand);
                rgba.BlitBuffer(buff, region.Start, -10);
            }
        }

        public static void ImageSplitCorrupt(ref byte[] rgba, int splits, int splitmin, int splitmax, Types.SeedRand rand, int width, int height)
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
                    sliceShifted = ShiftImage(sliceShifted, bitShiftAmnt);
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

        public static byte[] ShiftImage(byte[] rgba, int bitCount)
        {
            if (bitCount == 0) 
                return rgba;

            byte[] result = new byte[rgba.Length];

            Utils.Shift(rgba, result, bitCount);

            return result;
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

        public static byte[] CrunchImage(byte[] rgba, int width, int height, int crwidth, int crheight, bool returnSameSize = false)
        {
            // jimp source used as reference https://github.com/oliver-moran/jimp/blob/master/packages/plugin-resize/src/modules/resize2.js#L24
            byte[] crunched = new byte[
                returnSameSize
                 ? rgba.Length
                   : crwidth * crheight * 4
            ];

            int targetW = returnSameSize ? width : crwidth;
            int targetH = returnSameSize ? height : crheight;

            for (int y = 0; y < targetH; y++)
            {
                for (int x = 0; x < targetW; x++)
                {
                    var magic = width / crwidth;
                    var xCrunch = (int)(Math.Floor((double)x / width * crwidth) * magic);
                    var yCrunch = (int)(Math.Floor((double)y / height * crheight) * magic);
                    var dstBufferIdx = (y * targetW + x) * 4;
                    var crunchBufferIdx = (yCrunch * width + xCrunch) * 4;

                    for (int i = 0; i < 4; i++)
                        crunched[dstBufferIdx++] = rgba[crunchBufferIdx++];
                }
            }

            return crunched;
        }

        public static byte[] CrunchImage1(byte[] rgba, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            byte[] crunched = new byte[
                dstWidth * dstHeight * 4
            ];

            // source = the original image
            // dest = the resized image

            for (int yDst = 0; yDst < dstHeight; yDst++)
            {
                for (int xDst = 0; xDst < dstWidth; xDst++)
                {
                    int dstIdx = (yDst * dstWidth + xDst) * 4;
                    int xSrc = (int)((float)xDst / dstWidth * srcWidth);
                    int ySrc = (int)((float)yDst / dstHeight * srcHeight);
                    int srcIdx = (ySrc * srcWidth + xSrc) * 4;

                    for (int i = 0; i < 4; i++)
                    {
                        if (srcIdx >= rgba.Length) break;
                        if (dstIdx >= crunched.Length) break;

                        crunched[dstIdx++] = rgba[srcIdx++];
                    }
                }
            }

            return crunched;
        }

        public static byte[] CrunchImage2(byte[] rgba, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
        {
            return new byte[0];
        }

        public static void CorruptImage(MagickImage img, Types.PFOptions options, Types.SeedRand srand, int width, int height)
        {
            if (options.sizeMul / options.sizeDiv != 1 || (options.sizeMul != 0 && options.sizeDiv != 0))
            {
                // image size multiplier and divider
                Utils.Size calc = CalculateModifiedWH(width, height, options);
                img.InterpolativeResize(calc.Width, calc.Height, PixelInterpolateMethod.Nearest);
            }

            if (options.contrast != 1)
            {
                img.BrightnessContrast(new Percentage(100), new Percentage(options.contrast * 100));
            }

            // shid quality
            img.Quality = options.ExportQuality;

            // crunch effect
            float cwidth = -1;
            float cheight = -1;

            int imgwidth = width;
            int imgheight = height;

            if (options.crunchPercent != 100 || (options.crunchWidth != 0 && options.crunchHeight != 0))
            {
                bool usePercent = options.crunchWidth == 0 && options.crunchHeight == 0;
                cwidth = usePercent ? width * ((float)options.crunchPercent / 100) : options.crunchWidth;
                cheight = usePercent ? height * ((float)options.crunchPercent / 100) : options.crunchHeight;
                if (cwidth < 0) cwidth = width / Math.Abs(cwidth);
                if (cheight < 0) cheight = height / Math.Abs(cheight);

                imgwidth = (int)cwidth;
                imgheight = (int)cheight;

                var pixels1 = img.GetPixels();
                var bytes1 = pixels1.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGBA);
                bytes1 = CrunchImage1(bytes1, width, height, imgwidth, imgheight);
                pixels1.SetPixels(bytes1);
                //img.InterpolativeResize((int)cwidth, (int)cheight, PixelInterpolateMethod.Nearest);
            }


            // main corruption
            var pixels = img.GetPixels();
            var bytes = pixels.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGBA);

            /*var settings = new MagickReadSettings();
            settings.ColorType = ColorType.TrueColorAlpha;
            using (var img1 = new MagickImage("eathan.png", settings))
            {
                img1.ColorType = ColorType.TrueColorAlpha;
                img1.Resize(imgwidth, imgheight);
                img1.GetPixels().SetPixels(bytes);
                img1.Write("test.png");
            }*/

            if (options.bufferShiftBits != 0)
            {
                bytes = ShiftImage(bytes, options.bufferShiftBits);
            }

            if (options.bufferCorruptRegions > 0)
            {
                RegionalCorrupt(ref bytes, options.bufferCorruptRegions, options.regionMinSize, options.regionMaxSize, srand, imgwidth, imgheight);
            }

            if (options.imageSplits > 0)
            {
                ImageSplitCorrupt(ref bytes, options.imageSplits, options.splitsMin, options.splitsMax, srand, imgwidth, imgheight);
            }

            //pixels.SetPixels(bytes);

            // resize to normal after crunching
            if (cwidth != -1 && cheight != -1)
            {
                //bytes = CrunchImage1(bytes, imgwidth, imgheight, width, height);
                img.InterpolativeResize(width, height, PixelInterpolateMethod.Nearest);
            }
        }
    }
}
