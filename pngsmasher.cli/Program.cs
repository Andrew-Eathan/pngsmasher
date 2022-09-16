using CommandLine;
using SixLabors.ImageSharp.Processing.Processors.Normalization;
using System.Reflection;
using static pngsmasher.Core.Types;
using static pngsmasher.Core.Utils;

namespace pngsmasher.CLI
{

    static class Program
    {
        static void Main()
        {

        }
    }

    public class CLIOptions : PFOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input image to read.")]
        public string input { get; set; }

        [Option('i', "input", Required = true, HelpText = "Output path.")]
        public string output { get; set; }

        // Shifts the entire image by this many bits (negative allowed)
        public int bufferShiftBits { get; set; }

        // Corrupts regions of an image
        public int corruptRegions = 0;
        // Min region size, negative values means the height is divided by the absolute value
        public int regionMinSize = -25; // regionMinSize = height / 25
                                        // Max region size, negative values means the height is divided by the absolute value
        public int regionMaxSize = -10; // regionMaxSize = height / 10

        // Image splits count, displaces image and subcorrupts it
        public int imageSplits = 0;
        public int splitsMin = 0;
        public int splitsMax = 100;

        // Contrast image (0 is neutral, -1 is lowest, 1 is highest)
        public float contrast = 0f;

        // Multiply image size, 1 = ignore
        public int sizeMul = 1;

        // Divide image size, 1 = ignore
        public int sizeDiv = 1;

        // Percentage (0-100), it resizes the image to this percentage in pre-processing and resizes it back to normal in post-processing
        public int crunchPercent = 25; // width * 0.75, height * 0.75

        // Crunch the image to this width/height (ignores percentage if both crunch width and height aren't 0)
        // Like region size, setting a negative value acts as a divisor to the width/height
        public float crunchWidth = 0;
        public float crunchHeight = 0;

        // Seed to use when corrupting this image
        public int seed; // this is set to something random if unspecified

        // If the image is transparent, pngfucker tries to localise the corruption to just solid pixels, but allows corruption if its pixel delta > 64
        public bool clamp = false;

        // Export quality (if format is jpg or jpg-based)
        public int ExportQuality = 100;

        // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
        public bool blackBackground = false;
    }
}