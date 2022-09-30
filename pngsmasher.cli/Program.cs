using System.Reflection;
using static pngsmasher.Core.Types;
using static pngsmasher.Core.Utils;
using pngsmasher.Core;
using ImageMagick;
using System.Diagnostics;
using ImageMagick.Formats;

namespace pngsmasher.CLI
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class CLIValueAttribute : System.Attribute
    {
        public string ParameterName;

        public CLIValueAttribute(string name)
        {
            ParameterName = name;
        }
    }


    static class Program
    {
        static CLIOptions options = new CLIOptions();
        private static Type _stringType = typeof(string);
        private static Type _intType = typeof(int);
        private static Type _floatType = typeof(float);
        private static Type _boolType = typeof(bool);

        static void Write(object str)
        {
            if (!options.silent) Console.WriteLine(str);
        }

        static void WError(object str)
        {
            if (!options.silent)
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(str);
                Console.ForegroundColor = old;
            }
        }

        static void WWarn(object str)
        {
            if (!options.silent)
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str);
                Console.ForegroundColor = old;
            }
        }

        static void Main(string[] args)
        {
            // parse cli args

            PropertyInfo[] @params = options.GetType().GetProperties();
            Dictionary<string, PropertyInfo> vlist = new Dictionary<string, PropertyInfo>();

            foreach (PropertyInfo fi in @params)
            {
                var attr = fi.GetCustomAttribute<CLIValueAttribute>();
                vlist.Add(attr?.ParameterName ?? fi.Name, fi);
            }

            string paramName = "";

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (paramName == "")
                {
                    if (arg[0] == '-')
                    {
                        paramName = arg[1..];

                        if (!vlist.ContainsKey(paramName))
                        {
                            WError("Unknown parameter " + arg + "!");
                            paramName = "";
                        }
                        else
                        {
                            if (vlist[paramName].PropertyType == _boolType)
                            {
                                vlist[paramName].SetValue(options, true);
                                paramName = "";
                            }
                        }
                        continue;
                    }
                }
                else
                {
                    PropertyInfo field = vlist[paramName];
                    Type type = field.PropertyType;
                    object? data = null;
                    paramName = "";

                    // switch case doesn't like the types not being constant ssssooooooooo
                    if (type == _stringType)
                        data = Convert.ToString(arg);
                    else if (type == _intType)
                    {
                        int data_sub;
                        if (!int.TryParse(arg, out data_sub))
                        {
                            WError("Failed to parse int argument " + arg + " for parameter -" + paramName);
                            continue;
                        }
                        else
                        {
                            data = data_sub;
                        }
                    }
                    else if (type == _floatType)
                    {
                        float data_sub;
                        if (!float.TryParse(arg, out data_sub))
                        {
                            WError("Failed to parse float argument " + arg + " for parameter -" + paramName);
                            continue;
                        }
                        else
                        {
                            data = data_sub;
                        }
                    }

                    field.SetValue(options, data);
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkMagenta; Write("   ___    _  __  _____   ____   __  ___   ___    ____   __ __   ____   ___ ");
            Console.ForegroundColor = ConsoleColor.Magenta; Write("  / _ \\  / |/ / / ___/  / __/  /  |/  /  / _ |  / __/  / // /  / __/  / _ \\");
            Console.ForegroundColor = ConsoleColor.Red; Write(" / ___/ /    / / (_ /  _\\ \\   / /|_/ /  / __ | _\\ \\   / _  /  / _/   / , _/");
            Console.ForegroundColor = ConsoleColor.DarkRed; Write("/_/    /_/|_/  \\___/  /___/  /_/  /_/  /_/ |_|/___/  /_//_/  /___/  /_/|_| ");
            Console.ForegroundColor = ConsoleColor.Gray; Write("--- by andreweathan --- (and a little help from Marioalexsan)\n");

            List<string> files = new List<string>();
            if (File.Exists(options.input))
            {
                Write(options.input + " is a file");
                files.Add(options.input);
            }
            else if (Directory.Exists(options.input))
            {
                Write(options.input + " is a directory");
                var subfiles = Directory.EnumerateFiles(options.input);
                files = subfiles.ToList();
            }
            else
            {
                WError("Couldn't find file " + options.input + "!");
                return;
            }

            var timeTotal = new Stopwatch();
            var settings = new MagickReadSettings();

            settings.ColorType = ColorType.TrueColorAlpha;
            SeedRand srand = new SeedRand(options.seed);

            Write("Using seed " + options.seed);

            int filesdone = 0;

        timeTotal.Start();
            foreach (string file in files) {
                using (var img = new MagickImage(file, settings))
                {
                    var timeThis = new Stopwatch();
                    img.ColorType = ColorType.TrueColorAlpha;

                    var pixels = img.GetPixels();
                    var bytes = pixels.ToByteArray(0, 0, img.Width, img.Height, PixelMapping.RGBA);
                    if (bytes == null)
                    {
                        WError("Failed to corrupt image " + file + ": Image byte data was null!");
                        continue;
                    }
                    
                    timeThis.Start();
                    var (rgba, imgwidth, imgheight) = OldStyleCorruptImage(bytes, options, srand, img.Width, img.Height);
                    bytes = rgba;
                    timeThis.Stop();

                    img.Crop(imgwidth, imgheight);
                    pixels = img.GetPixels();
                    pixels.SetPixels(bytes);
                    filesdone++;

                    //Console.WriteLine(Path.GetExtension(output));

                    string output = options.output;

                    if (files.Count > 1)
                    {
                        string noext = Path.GetFileNameWithoutExtension(output);
                        string ext = Path.GetExtension(output);

                        if (ext == "")
                        {
                            output = Path.Combine(noext, Path.GetFileName(file));
                        }
                        else 
                        {
                            output = noext + "_" + filesdone + ext;
                        }
                    }

                    if (!options.overwrite)
                    {
                        string original = output;
                        bool ow_warned = false;
                        int ow_warns = 0;
                        while (File.Exists(output))
                        {
                            ow_warns++;

                            string noext = Path.GetFileNameWithoutExtension(original);
                            string ext = Path.GetExtension(original);
                            output = noext + "_" + filesdone + "_" + ow_warns + ext;

                            ow_warned = true;
                        }

                        if (ow_warned)
                        {
                            WWarn("WARN: Output file already exists [" + original + "], writing to [" + output + "] instead!");
                            WWarn("WARN: To ignore this warning and overwrite the existing file, pass the argument -o");
                        }
                    }

                    string dir = Path.GetDirectoryName(output);
                    if (dir != "") Directory.CreateDirectory(dir);
                    img.Write(output);
                    Write(filesdone + ": " + file + " => " + output + " (" + timeThis.ElapsedMilliseconds + " ms)");
                }
            }
        timeTotal.Stop();

            Write("Done! (" + timeTotal.ElapsedMilliseconds + " ms total)");
        }

        public static (byte[] rgbaout, int imagewidth, int imageheight) OldStyleCorruptImage(byte[] rgba, CLIOptions options, SeedRand srand, int width, int height)
        {
            byte[] rgba_out = rgba.ToArray(); // clone
            int imgwidth = width;
            int imgheight = height;

            if (options.sizeMul / options.sizeDiv != 1 || (options.sizeMul != 1 && options.sizeDiv != 1))
            {
                // image size multiplier and divider
                Size calc = Corruption.CalculateModifiedWH(width, height, options);
                rgba_out = Corruption.CrunchImage(rgba_out, width, height, calc.Width, calc.Height);
                imgwidth = calc.Width;
                imgheight = calc.Height;

                Write("\tResized image from " + width + "x" + height + " to " + imgwidth + "x" + imgheight);
            }

            // contrast
            if (options.contrast != 0)
            {
                Corruption.ContrastImage(rgba_out, rgba_out, options.contrast);
                Write("\tContrasted the image by " + (options.contrast < 0 ? "" : "+") + Convert.ToString(options.contrast));
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

                Write("\tCrunched the image from " + imgwidth + "x" + imgheight + " to " + (int)cwidth + "x" + (int)cheight);

                imgwidth = (int)cwidth;
                imgheight = (int)cheight;

                rgba_out = Corruption.CrunchImage(rgba_out, width, height, imgwidth, imgheight);
            }

            if (options.bufferShiftBits != 0)
            {
                Corruption.BitShift(rgba_out, rgba_out, options.bufferShiftBits);
                Write("\tBitshifted the image " + options.bufferShiftBits + " bits to the " + (options.bufferShiftBits > 0 ? "right" : "left"));
            }

            if (options.corruptRegions > 0)
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

                // to keep compatibility with nodejs png****er's outputs these need to be generated separately
                for (int i = 0; i < regionArray.Count; i++)
                {
                    Region lol = regionArray[i];
                    lol.BitshiftAmount = -PFFloor(1, 32, srand);
                    regionArray[i] = lol;
                }

                Corruption.RegionalCorrupt(ref rgba_out, regionArray);
                Write("\tApplied " + options.corruptRegions + " corrupted regions to the image");
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

                Corruption.ImageSplitCorrupt(ref rgba_out, splits, imgwidth, imgheight);
                Write("\tCreated " + options.corruptRegions + " corrupted splits");
            }

            if (options.clamp)
            {
                Corruption.ClampTransparency(rgba_out, rgba_out, rgba);
                Write("\tClamped image corruption to opaque pixels");
            }

            if (options.blackBackground)
            {
                Corruption.UnderlayBlack(rgba_out, rgba_out);
                Write("\tUnderlayed a black background");
            }

            // resize to normal after crunching
            if (cwidth != -1 && cheight != -1)
            {
                rgba_out = Corruption.CrunchImage(rgba_out, imgwidth, imgheight, width, height);
                imgwidth = width;
                imgheight = height;
                Write("\tCrunched image from " + imgwidth + "x" + imgheight + " back to " + width + "x" + height);
            }

            return (rgba_out, imgwidth, imgheight);
        }
    }

    public class CLIOptions : PFOptions
    {
        [CLIValue("v")]
        public bool verbose { get; set; } = false;

        [CLIValue("s")]
        public bool silent { get; set; } = false;

        [CLIValue("o")]
        public bool overwrite { get; set; } = false;

        [CLIValue("input")]
        public string input { get; set; } = "input.png";

        [CLIValue("output")]
        public string output { get; set; } = "output.png";

        // Shifts the entire image by this many bits (negative allowed)
        [CLIValue("shift")]
        public new int bufferShiftBits { get; set; } = 0;

        // Corrupts regions of an image
        [CLIValue("regions")]
        public new int corruptRegions { get; set; } = 0;

        // Min region size, negative values means the height is divided by the absolute value
        [CLIValue("rmin")]
        public new int regionMinSize { get; set; } = -25; // regionMinSize = height / 25
                                                          // Max region size, negative values means the height is divided by the absolute value

        [CLIValue("rmax")]
        public new int regionMaxSize { get; set; } = -10; // regionMaxSize = height / 10

        // Image splits count, displaces image and subcorrupts it
        [CLIValue("splits")]
        public new int imageSplits { get; set; } = 0;

        [CLIValue("splitmin")]
        public new int splitsMin { get; set; } = 0;

        [CLIValue("splitmax")]
        public new int splitsMax { get; set; } = 100;

        // Contrast image (0 is neutral, -1 is lowest, 1 is highest)
        [CLIValue("contrast")]
        public new float contrast { get; set; } = 0f;

        // Multiply image size, 1 = ignore
        [CLIValue("mul")]
        public new float sizeMul { get; set; } = 1;

        // Divide image size, 1 = ignore
        [CLIValue("div")]
        public new float sizeDiv { get; set; } = 1;

        // Percentage (0-100), it resizes the image to this percentage in pre-processing and resizes it back to normal in post-processing
        [CLIValue("crunch")]
        public new int crunchPercent { get; set; } = 100; // width * 0.75, height * 0.75

        // Crunch the image to this width/height (ignores percentage if both crunch width and height aren't 0)
        // Like region size, setting a negative value acts as a divisor to the width/height
        [CLIValue("crwidth")]
        public new int crunchWidth { get; set; } = 0;

        [CLIValue("crheight")]
        public new int crunchHeight { get; set; } = 0;

        // Seed to use when corrupting this image
        [CLIValue("seed")]
        public new int seed { get; set; } = 0; // this is set to something random if unspecified

        // If the image is transparent, pngsmasher tries to localise the corruption to just solid pixels, but allows corruption if its pixel delta > 64
        [CLIValue("clamp")]
        public new bool clamp { get; set; } = false;

        // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
        [CLIValue("blackbg")]
        public new bool blackBackground { get; set; } = false;
    }
}