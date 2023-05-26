using System;

namespace pngsmasher.Core;

public class Types
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class CLIValueAttribute : System.Attribute
    {
        public string ParameterName;
        public string HelpString;
        public string Example;

        public CLIValueAttribute(string name, string helpString, string example)
        {
            ParameterName = name;
            HelpString = helpString;
            Example = example;
        }
    }

    public struct Logging
    {
        public Action<object> VerbWrite = null;
        public Action<object, bool> Write = null; // second bool should be optional but meh, dont wanna deal with delegates
        public Action<object> WWarn;
        public Action<object> WError;

        public Logging(Action<object> VerbWrite, Action<object, bool> Write, Action<object> WWarn, Action<object> WError)
        {
            this.VerbWrite = VerbWrite;
            this.Write = Write;
            this.WWarn = WWarn;
            this.WError = WError;
        }
    }

    public struct Region
    {
        public int Start;
        public int End;
        public int BitshiftAmount;
        public int AddValue;

        public Region(int Start, int End, int BitshiftAmount, int AddValue)
        {
            this.Start = Start;
            this.End = End;
            this.BitshiftAmount = BitshiftAmount;
            this.AddValue = AddValue;
        }
    }

    public struct Split
    {
        public int SplitBufferPos;
        public int BitshiftAmount;
        public int HorizontalShift;

        public Split(int SplitBufferPos, int BitshiftAmount, int HorizontalShift)
        {
            this.SplitBufferPos = SplitBufferPos;
            this.BitshiftAmount = BitshiftAmount;
            this.HorizontalShift = HorizontalShift;
        }
    }

    public struct Size
    {
        public int Width;
        public int Height;
    }

    public enum ColorSpace
    {
        RGBA1616168,
        RGB888,
        RGB161616,
        RG88,
        RB1616
    }

    public class SeedRand
    {
        double originalSeed;
        double currentSeed;

        public SeedRand(string seed)
        {
            currentSeed = 0xFF;

            for (var i = 0; i < seed.Length; i++)
                currentSeed = (int)currentSeed ^ seed[i];

            originalSeed = currentSeed;
        }

        public SeedRand(int seed) => currentSeed = originalSeed = Math.Abs(seed);

        public double Generate(double min = 0, double max = 1, bool dontShuffleSeed = false)
        {
            double tmpSeed = 0;
            if (dontShuffleSeed) tmpSeed = currentSeed;

            currentSeed = (currentSeed * 9301 + 49297) % 233280;
            var ret = min + currentSeed / 233280 * (max - min);

            if (dontShuffleSeed) currentSeed = tmpSeed;
            return ret;
        }

        public void SetSeed(int seed) => currentSeed = seed;
        public void ResetSeed() => currentSeed = originalSeed;
    }

    public class CLIOptions
    {
        [CLIValue("help", "Shows this help!", "+help")]
        public bool showHelp { get; set; } = false;

        [CLIValue("v", "Shows more helpful logs!", "+v")]
        public bool verbose { get; set; } = false;

        [CLIValue("s", "Runs completely silent. Shows absolutely nothing in console!", "+s")]
        public bool silent { get; set; } = false;

        [CLIValue("o", "Overwrites output image if one already exists with the given name.", "+o")]
        public bool overwrite { get; set; } = false;

        [CLIValue("input", "Input path. Can be any image file or a folder of images!", "-input shark.png | -input folderofsharks")]
        public string input { get; set; } = "input.png";

        [CLIValue("output", "Output path. If the output path is a folder, the input image(s) will be written to it. If the output is a file, pngsmasher tries to save the input file(s) with that name, appending a (1), (2), etc. if a file already exists.", "-output ketchup.png | -output mycoolfolder")]
        public string output { get; set; } = "output.png";

        // Shifts the entire image by this many bits (negative allowed)
        [CLIValue("shift", "Globally bit-shifts the image's RGBA buffer. This mostly looks good on high-contrast images.", "-shift 4 | -shift -5")]
        public int bufferShiftBits { get; set; } = 0;

        // Randomises the shift value by this amount positively and negatively each frame.
        [CLIValue("randshift", "Randomises the -shift value by this amount positively and negatively each frame.", "-shift 0 -randshift 4")]
        public int randomShiftAmount { get; set; } = 0;

        // Percentage (0-100) that decides the chance of pngsmasher not corrupting a frame. (taking a break)
        [CLIValue("breaks", "Percentage (0-100) that decides the chance of pngsmasher not corrupting a frame. (taking a break)", "-breaks 25 (25% chance that a frame will not be corrupted)")]
        public int breaks { get; set; } = 0;

        // When the input is a single-frame png image, this option will transform it into a corrupted APNG.
        [CLIValue("frames", "When the input is a single-frame png image, this option will transform it into a corrupted APNG.", "-frames 60 -fps 20")]
        public int frames { get; set; } = 0;

        // Sets APNG corruption FPS.
        [CLIValue("fps", "Sets APNG corruption FPS.", "-frames 60 -fps 20")]
        public int fps { get; set; } = -1;

        // Makes it so that the fps is default when "taking a break" (-breaks), but when not, the fps is set to -fps.
        [CLIValue("defaultfpsonbreak", "Makes it so that the fps is default when \"taking a break\" (-breaks), but when not, the fps is set to -fps.", "-breaks 50 -frames 30 -fps 10 +defaultfpsonbreak")]
        public bool defaultFPSOnBreak { get; set; } = false;

        // By default pngsmasher applies contrast, NTSC, and other non-corrupting effects during a break. This option makes frames "on break" to be identical to the input.
        [CLIValue("nofxonbreak", "Makes it so that the fps is default when \"taking a break\" (-breaks), but when not, the fps is set to -fps.", "-breaks 50 -fps 10 +nofxonbreak")]
        public bool noFXOnBreak { get; set; } = false;

        // Corrupts regions of an image
        [CLIValue("regions", "Creates glitched sub-regions in an image, which includes RGBA byte displacement and localised buffer bit-shifting.", "-regions 4")]
        public int corruptRegions { get; set; } = 0;

        // Min region size, negative values means the height is divided by the absolute value
        [CLIValue("rmin", "Minimum rows (height) of pixels for a region. Negative values act as a divider to the image height.", "-regions 3 -rmin 20 | -regions 2 -rmin -4 (divides height by 4)")]
        public int regionMinSize { get; set; } = -25; // regionMinSize = height / 25
                                                          // Max region size, negative values means the height is divided by the absolute value

        [CLIValue("rmax", "Maximum rows (height) of pixels for a region. Negative values act as a divider to the image height.", "-regions 2 -rmax 80 | -regions 4 -rmin -3 (divides height by 3)")]
        public int regionMaxSize { get; set; } = -10; // regionMaxSize = height / 10

        // Image splits count, displaces image and subcorrupts it
        [CLIValue("splits", "My realistic pride and joy: the splits effect bit-shifts the entire image below a certain random point. This creates a somewhat realistic corruption effect.", "-splits 3")]
        public int imageSplits { get; set; } = 0;

        [CLIValue("splitmin", "Minimum height at which an image split can start at.", "-splits 2 -splitmin 50  <-  The splits will only happen at 50% or greater of the image height (from top to bottom)")]
        public int splitsMin { get; set; } = 0;

        [CLIValue("splitmax", "Maximum height at which an image split can start at.", "-splits 3 -splitmax 40  <-  The splits will only happen at 50% or lower of the image height (from top to bottom)")]
        public int splitsMax { get; set; } = 100;

        // Contrast image (0 is neutral, -1 is lowest, 1 is highest)
        [CLIValue("contrast", "Contrasts the image pixels by this amount. 0 = no change, -1 = lowest contrast, 1 = highest!", "-contrast 0.4 | -contrast -0.2")]
        public float contrast { get; set; } = 0f;

        // Multiply image size, 1 = ignore
        [CLIValue("mul", "(BROKEN) Image size multiplier. This stacks with -div for precision, if you wish!", "-mul 1.5")]
        public float sizeMul { get; set; } = 1;

        // Divide image size, 1 = ignore
        [CLIValue("div", "(BROKEN) Image size divider. This stacks with -mul for precision, if you wish!", "-div 2")]
        public float sizeDiv { get; set; } = 1;

        // Percentage (0-100), it resizes the image to this percentage in pre-processing and resizes it back to normal in post-processing
        [CLIValue("crunch", "Creates a \"crunched\" effect in an image: The image is downscaled to this percentage, corrupted, and upscaled back to the original resolution.", "-crunch 60")]
        public int crunchPercent { get; set; } = 100; // width * 0.75, height * 0.75

        // Crunch the image to this width/height (ignores percentage if both crunch width and height aren't 0)
        // Like region size, setting a negative value acts as a divisor to the width/height
        [CLIValue("crwidth", "Optional override for the -crunch parameter width. Set both width and height percentages to above 0, otherwise pngsmasher will ignore these and use the percentage.", "-crwidth 70")]
        public int crunchWidth { get; set; } = 0;

        [CLIValue("crheight", "Optional override for the -crunch parameter height. Set both width and height percentages to above 0, otherwise pngsmasher will ignore these and use the percentage.", "-crheight 40")]
        public int crunchHeight { get; set; } = 0;

        // Seed to use when corrupting this image
        [CLIValue("seed", "Random seed for this image. Leave blank to generate a random seed between 1 and 2147483647.", "-seed 69420")]
        public int seed { get; set; } = -2147483647; // this is set to something random if unspecified

        // If the image is transparent, pngsmasher tries to localise the corruption to just solid pixels, but allows corruption if its pixel delta > 64
        [CLIValue("clamp", "This ignores corruption changes to a transparent image if they are outside of the opaque area.", "+clamp")]
        public bool clamp { get; set; } = false;

        // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
        [CLIValue("bg", "This underlays a background in a transparent image. This is useful, because corruption mangles alpha bits of an image, resulting in transparent pixels everywhere. Plus, this gives the image a nice dark gritty corruption if you use black! Use -bgred -bggreen -bgblue to control the color.", "+bg -bgred 64 -bggreen 0 -bgblue 255")]
        public bool bg { get; set; } = false;

        // Seed to use when corrupting this image
        [CLIValue("bgred", "Red value for background, default 0", "-bg -bgred 255 -bggreen 0 -bgblue 255")]
        public int bgRed { get; set; } = -2147483647;

        // Seed to use when corrupting this image
        [CLIValue("bggreen", "Green value for background, default 0", "-bg -bgred 255 -bggreen 255 -bgblue 255")]
        public int bgGreen { get; set; } = -2147483647;

        // Seed to use when corrupting this image
        [CLIValue("bgblue", "Blue value for background, default 0", "-bg -bgred 0 -bggreen 255 -bgblue 0")]
        public int bgBlue { get; set; } = -2147483647;

        // Simulates -clamp in -bg (tries not to underlay the background wherever nothing changed, useful to avoid flashing animated corruption)
        [CLIValue("bgclamp", "Simulates +clamp in +bg (tries not to underlay the background wherever nothing changed, useful to avoid flashing animated corruption but may cause issues sometimes)", "+bg +bgclamp")]
        public bool bgClamp { get; set; } = false;

        // Toggles my attempt at an NTSC filter
        [CLIValue("ntsc", "Toggles my attempt at an NTSC filter", "+ntsc / -ntsc")]
        public bool ntsc { get; set; } = false;

        // Controls the amount of fringing in the NTSC filter
        [CLIValue("fringe", "Amount of fringing in the NTSC filter", "+ntsc -fringe 1")]
        public int fringe { get; set; } = 1;

        // Controls the amount of horizontal blurring in the NTSC filter
        [CLIValue("xblur", "Amount of horizontal blurring in the NTSC filter", "+ntsc -xblur 4")]
        public int xBlur { get; set; } = 3;

        // How strongly the colors persist in the horizontal blur of the NTSC filter
        [CLIValue("xblurpower", "How strongly the colors persist in the horizontal blur of the NTSC filter", "-ntsc -xblur 4 -xblurpower 1.5")]
        public float xBlurPower { get; set; } = 1;

        // Applies a fancy blurrable grayscale abberation effect that i made by accident
        [CLIValue("grayabb", "Applies a fancy grayscale abberation effect that i made by accident", "+grayabb -grayabbsize 2 -grayabbpower 0.75")]
        public bool grayAbberation { get; set; } = false;

        [CLIValue("grayabbsize", "Grayscale abberation size", "+grayabb -grayabbsize 4")]
        public int grayAbberationWidth { get; set; } = 2;

        [CLIValue("grayabbpower", "Grayscale abberation power (how much color bleeding happens)", "+grayabb -grayabbsize 10 -grayabbpower 10 (makes the image look very faded)")]
        public float grayAbberationPower { get; set; } = 0.75f;

		// How far apart the color channels get "out of tune" in the grayscale abberation effect
		[CLIValue("grayabbdetune", "How far apart the color channels get \"out of tune\" in the grayscale abberation effect", "+grayabb -grayabbsize 2 -grayabbdetune 3")]
		public int grayAbberationDetune { get; set; } = 2;

		[CLIValue("rewidths", "test", "-rewidths")]
		public int rewidths { get; set; } = 0;
	}
}
