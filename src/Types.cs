using System;

namespace pngsmasher.Core;

public class Types
{
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

        public SeedRand(int seed) => currentSeed = originalSeed = seed;

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

    public class PFOptions
    {
        // Shifts the entire image by this many bits (negative allowed)
        public int bufferShiftBits = 0;

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

        // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
        public bool blackBackground = false;
    }
}
