using System.Reflection;
using static pngsmasher.Core.Types;
using static pngsmasher.Core.Utils;

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

            Console.ForegroundColor = ConsoleColor.DarkMagenta;  Write("   ___    _  __  _____   ____   __  ___   ___    ____   __ __   ____   ___ ");
            Console.ForegroundColor = ConsoleColor.Magenta; Write("  / _ \\  / |/ / / ___/  / __/  /  |/  /  / _ |  / __/  / // /  / __/  / _ \\");
            Console.ForegroundColor = ConsoleColor.Red; Write(" / ___/ /    / / (_ /  _\\ \\   / /|_/ /  / __ | _\\ \\   / _  /  / _/   / , _/");
            Console.ForegroundColor = ConsoleColor.DarkRed; Write("/_/    /_/|_/  \\___/  /___/  /_/  /_/  /_/ |_|/___/  /_//_/  /___/  /_/|_| ");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public class CLIOptions : PFOptions
    {
        [CLIValue("v")]
        public bool verbose { get; set; } = false;

        [CLIValue("s")]
        public bool silent { get; set; } = false;

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