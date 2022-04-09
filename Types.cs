using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pngfucker.NET
{
    public class Types
    {
        public class PFOptions
        {
            // Shifts the entire image by this many bits (negative allowed)
            public int bufferShiftBits = 0;

            // Corrupts regions of an image
            public int bufferCorruptRegions = 3;
            // Min region size, negative values means the height is divided by the absolute value
            public int regionMinSize = -25; // regionMinSize = width / 25
            // Max region size, negative values means the height is divided by the absolute value
            public int regionMaxSize = -10; // regionMaxSize = width / 10

            // Image splits count, displaces image and subcorrupts it
            public int imageSplits = 2;

            // Contrast image
            public float contrast = 0.1f;

            // Multiply image size, 0 = ignore
            public int sizeMul = 0;

            // Divide image size, 0 = ignore
            public int sizeDiv = 0;

            // Percentage (0-100), it resizes the image to this percentage in pre-processing and resizes it back to normal in post-processing
            public int crunchPercent = 75; // width * 0.75, height * 0.75

            // Crunch the image to this width/height (ignores percentage if both crunch width and height aren't 0)
            // Like region size, setting a negative value acts as a divisor to the width/height
            public int crunchWidth = 0;
            public int crunchHeight = 0;

            // Seed to use when corrupting this image
            public int seed; // this is set to something random if unspecified

            // If the image is transparent, pngfucker tries to localise the corruption to just solid pixels, but allows corruption if its pixel delta > 64
            public bool clamp = false;

            // If true, exports the image with transparency.
            // This is by default off, because corruption mangles alpha bits, causing parts of the image to go partially transparent.
            public bool alpha = false;

            // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
            public bool blackBackground = false;
        }

        public class SeedRand
        {
            int originalSeed;
            int currentSeed;

            public SeedRand (string seed)
            {
                currentSeed = 0xFF;

                for (var i = 0; i < seed.Length; i++)
                    currentSeed ^= seed[i];

                originalSeed = currentSeed;
            }

            public SeedRand (int seed) => currentSeed = originalSeed = seed;

            public double Generate(double min = 0, double max = 1)
            {
                currentSeed = (currentSeed * 9301 + 49297) % 233280;
                var ret = min + ((double)currentSeed / 233280) * (max - min);
                return ret;
            }

            public void SetSeed(int seed) => currentSeed = seed;
            public void ResetSeed() => currentSeed = originalSeed;
        }
    }
}
