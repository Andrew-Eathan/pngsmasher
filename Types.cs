using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pngsmasher
{
    public class Types
    {

        public class PFOptions
        {
            // Shifts the entire image by this many bits (negative allowed)
            public int bufferShiftBits = 0;

            // Corrupts regions of an image
            public int bufferCorruptRegions = 0;
            // Min region size, negative values means the height is divided by the absolute value
            public int regionMinSize = -25; // regionMinSize = height / 25
            // Max region size, negative values means the height is divided by the absolute value
            public int regionMaxSize = -10; // regionMaxSize = height / 10

            // Image splits count, displaces image and subcorrupts it
            public int imageSplits = 0;
            public int splitsMin = 0;
            public int splitsMax = 100;

            // Contrast image (1 is neutral, 0 is low, 2 is high)
            public float contrast = 1f;

            // Multiply image size, 1 = ignore
            public int sizeMul = 1;

            // Divide image size, 1 = ignore
            public int sizeDiv = 1;

            // Percentage (0-100), it resizes the image to this percentage in pre-processing and resizes it back to normal in post-processing
            public int crunchPercent = 100; // width * 0.75, height * 0.75

            // Crunch the image to this width/height (ignores percentage if both crunch width and height aren't 0)
            // Like region size, setting a negative value acts as a divisor to the width/height
            public float crunchWidth = 0;
            public float crunchHeight = 0;

            // Seed to use when corrupting this image
            public int seed; // this is set to something random if unspecified

            // If the image is transparent, pngfucker tries to localise the corruption to just solid pixels, but allows corruption if its pixel delta > 64
            public bool clamp = false;

            // Export format
            public ExportFormat format = ExportFormat.Png;

            // Export quality (if format is jpg or jpg-based)
            public int ExportQuality = 100;

            // Corruption often mangles alpha bits with other components, resulting in the whole image having a ghost-ish transparency to it. This adds a black background behind the image.
            public bool blackBackground = false;
        }

        public class SeedRand
        {
            double originalSeed;
            double currentSeed;

            public SeedRand (string seed)
            {
                currentSeed = 0xFF;

                for (var i = 0; i < seed.Length; i++)
                    currentSeed = (int)currentSeed ^ seed[i];

                originalSeed = currentSeed;
            }

            public SeedRand (int seed) => currentSeed = originalSeed = seed;

            public double Generate(double min = 0, double max = 1, bool dontShuffleSeed = false)
            {
                double tmpSeed = 0;
                if (dontShuffleSeed) tmpSeed = currentSeed;

                currentSeed = (currentSeed * 9301 + 49297) % 233280;
                var ret = min + currentSeed / 233280 * (max - min);

                if (dontShuffleSeed) currentSeed = tmpSeed;
                Console.WriteLine(ret);
                return ret;
            }

            public void SetSeed(int seed) => currentSeed = seed;
            public void ResetSeed() => currentSeed = originalSeed;
        }

        public enum ExportFormat
        {
            Unknown = 0,
            //
            // Summary:
            //     Hasselblad CFV/H3D39II.
            ThreeFr = 1,
            //
            // Summary:
            //     Media Container.
            ThreeG2 = 2,
            //
            // Summary:
            //     Media Container.
            ThreeGp = 3,
            //
            // Summary:
            //     Raw alpha samples.
            A = 4,
            //
            // Summary:
            //     AAI Dune image.
            Aai = 5,
            //
            // Summary:
            //     Adobe Illustrator CS2.
            Ai = 6,
            //
            // Summary:
            //     Animated Portable Network Graphics.
            APng = 7,
            //
            // Summary:
            //     PFS: 1st Publisher Clip Art.
            Art = 8,
            //
            // Summary:
            //     Sony Alpha Raw Image Format.
            Arw = 9,
            //
            // Summary:
            //     Image sequence laid out in continuous irregular courses (Unknown).
            Ashlar = 10,
            //
            // Summary:
            //     Microsoft Audio/Visual Interleaved.
            Avi = 11,
            //
            // Summary:
            //     AV1 Image File Format (Heic).
            Avif = 12,
            //
            // Summary:
            //     AVS X image.
            Avs = 13,
            //
            // Summary:
            //     Raw blue samples.
            B = 14,
            //
            // Summary:
            //     Raw blue, green, and red samples.
            Bgr = 15,
            //
            // Summary:
            //     Raw blue, green, red, and alpha samples.
            Bgra = 16,
            //
            // Summary:
            //     Raw blue, green, red, and opacity samples.
            Bgro = 17,
            //
            // Summary:
            //     Microsoft Windows bitmap image.
            Bmp = 18,
            //
            // Summary:
            //     Microsoft Windows bitmap image (V2).
            Bmp2 = 19,
            //
            // Summary:
            //     Microsoft Windows bitmap image (V3).
            Bmp3 = 20,
            //
            // Summary:
            //     BRF ASCII Braille format.
            Brf = 21,
            //
            // Summary:
            //     Raw cyan samples.
            C = 22,
            //
            // Summary:
            //     Continuous Acquisition and Life-cycle Support Type 1.
            Cal = 23,
            //
            // Summary:
            //     Continuous Acquisition and Life-cycle Support Type 1.
            Cals = 24,
            //
            // Summary:
            //     Constant image uniform color.
            Canvas = 25,
            //
            // Summary:
            //     Caption.
            Caption = 26,
            //
            // Summary:
            //     Cineon Image File.
            Cin = 27,
            //
            // Summary:
            //     Cisco IP phone image format.
            Cip = 28,
            //
            // Summary:
            //     Image Clip Mask.
            Clip = 29,
            //
            // Summary:
            //     The system clipboard.
            Clipboard = 30,
            //
            // Summary:
            //     Raw cyan, magenta, yellow, and black samples.
            Cmyk = 31,
            //
            // Summary:
            //     Raw cyan, magenta, yellow, black, and alpha samples.
            Cmyka = 32,
            //
            // Summary:
            //     Canon Digital Camera Raw Image Format.
            Cr2 = 33,
            //
            // Summary:
            //     Canon Digital Camera Raw Image Format.
            Cr3 = 34,
            //
            // Summary:
            //     Canon Digital Camera Raw Image Format.
            Crw = 35,
            //
            // Summary:
            //     Cube color lookup table image.
            Cube = 36,
            //
            // Summary:
            //     Microsoft icon.
            Cur = 37,
            //
            // Summary:
            //     DR Halo.
            Cut = 38,
            //
            // Summary:
            //     Base64-encoded inline images.
            Data = 39,
            //
            // Summary:
            //     Digital Imaging and Communications in Medicine image.
            Dcm = 40,
            //
            // Summary:
            //     Kodak Digital Camera Raw Image File.
            Dcr = 41,
            //
            // Summary:
            //     Raw Photo Decoder (dcraw) (Dng).
            Dcraw = 42,
            //
            // Summary:
            //     ZSoft IBM PC multi-page Paintbrush.
            Dcx = 43,
            //
            // Summary:
            //     Microsoft DirectDraw Surface.
            Dds = 44,
            //
            // Summary:
            //     Multi-face font package.
            Dfont = 45,
            //
            // Summary:
            //     Microsoft Windows 3.X Packed Device-Independent Bitmap.
            Dib = 46,
            //
            // Summary:
            //     Digital Negative.
            Dng = 47,
            //
            // Summary:
            //     SMPTE 268M-2003 (DPX 2.0).
            Dpx = 48,
            //
            // Summary:
            //     Microsoft DirectDraw Surface.
            Dxt1 = 49,
            //
            // Summary:
            //     Microsoft DirectDraw Surface.
            Dxt5 = 50,
            //
            // Summary:
            //     Windows Enhanced Meta File.
            Emf = 51,
            //
            // Summary:
            //     Encapsulated Portable Document Format.
            Epdf = 52,
            //
            // Summary:
            //     Encapsulated PostScript Interchange format.
            Epi = 53,
            //
            // Summary:
            //     Encapsulated PostScript.
            Eps = 54,
            //
            // Summary:
            //     Level II Encapsulated PostScript.
            Eps2 = 55,
            //
            // Summary:
            //     Level III Encapsulated PostScript.
            Eps3 = 56,
            //
            // Summary:
            //     Encapsulated PostScript.
            Epsf = 57,
            //
            // Summary:
            //     Encapsulated PostScript Interchange format.
            Epsi = 58,
            //
            // Summary:
            //     Encapsulated PostScript with TIFF preview.
            Ept = 59,
            //
            // Summary:
            //     Encapsulated PostScript Level II with TIFF preview.
            Ept2 = 60,
            //
            // Summary:
            //     Encapsulated PostScript Level III with TIFF preview.
            Ept3 = 61,
            //
            // Summary:
            //     Epson RAW Format.
            Erf = 62,
            //
            // Summary:
            //     High Dynamic-range (HDR).
            Exr = 63,
            //
            // Summary:
            //     Farbfeld.
            Farbfeld = 64,
            //
            // Summary:
            //     Group 3 FAX.
            Fax = 65,
            //
            // Summary:
            //     Farbfeld.
            Ff = 66,
            //
            // Summary:
            //     Uniform Resource Locator (file://).
            File = 67,
            //
            // Summary:
            //     Flexible Image Transport System.
            Fits = 68,
            //
            // Summary:
            //     FilmLight.
            Fl32 = 69,
            //
            // Summary:
            //     Flash Video Stream.
            Flv = 70,
            //
            // Summary:
            //     Plasma fractal image.
            Fractal = 71,
            //
            // Summary:
            //     Uniform Resource Locator (ftp://).
            Ftp = 72,
            //
            // Summary:
            //     Flexible Image Transport System.
            Fts = 73,
            //
            // Summary:
            //     Formatted text image.
            Ftxt = 74,
            //
            // Summary:
            //     Raw green samples.
            G = 75,
            //
            // Summary:
            //     Group 3 FAX.
            G3 = 76,
            //
            // Summary:
            //     Group 4 FAX.
            G4 = 77,
            //
            // Summary:
            //     CompuServe graphics interchange format.
            Gif = 78,
            //
            // Summary:
            //     CompuServe graphics interchange format.
            Gif87 = 79,
            //
            // Summary:
            //     Gradual linear passing from one shade to another.
            Gradient = 80,
            //
            // Summary:
            //     Raw gray samples.
            Gray = 81,
            //
            // Summary:
            //     Raw gray and alpha samples.
            Graya = 82,
            //
            // Summary:
            //     Raw CCITT Group4.
            Group4 = 83,
            //
            // Summary:
            //     Identity Hald color lookup table image.
            Hald = 84,
            //
            // Summary:
            //     Radiance RGBE image format.
            Hdr = 85,
            //
            // Summary:
            //     High Efficiency Image Format.
            Heic = 86,
            //
            // Summary:
            //     High Efficiency Image Format.
            Heif = 87,
            //
            // Summary:
            //     Histogram of the image.
            Histogram = 88,
            //
            // Summary:
            //     Slow Scan TeleVision.
            Hrz = 89,
            //
            // Summary:
            //     Hypertext Markup Language and a client-side image map.
            Htm = 90,
            //
            // Summary:
            //     Hypertext Markup Language and a client-side image map.
            Html = 91,
            //
            // Summary:
            //     Uniform Resource Locator (http://).
            Http = 92,
            //
            // Summary:
            //     Uniform Resource Locator (https://).
            Https = 93,
            //
            // Summary:
            //     Truevision Targa image.
            Icb = 94,
            //
            // Summary:
            //     Microsoft icon.
            Ico = 95,
            //
            // Summary:
            //     Microsoft icon.
            Icon = 96,
            //
            // Summary:
            //     Phase One Raw Image Format.
            Iiq = 97,
            //
            // Summary:
            //     The image format and characteristics.
            Info = 98,
            //
            // Summary:
            //     Base64-encoded inline images.
            Inline = 99,
            //
            // Summary:
            //     IPL Image Sequence.
            Ipl = 100,
            //
            // Summary:
            //     ISO/TR 11548-1 format.
            Isobrl = 101,
            //
            // Summary:
            //     ISO/TR 11548-1 format 6dot.
            Isobrl6 = 102,
            //
            // Summary:
            //     JPEG-2000 Code Stream Syntax.
            J2c = 103,
            //
            // Summary:
            //     JPEG-2000 Code Stream Syntax.
            J2k = 104,
            //
            // Summary:
            //     JPEG Network Graphics.
            Jng = 105,
            //
            // Summary:
            //     Garmin tile format.
            Jnx = 106,
            //
            // Summary:
            //     JPEG-2000 File Format Syntax.
            Jp2 = 107,
            //
            // Summary:
            //     JPEG-2000 Code Stream Syntax.
            Jpc = 108,
            //
            // Summary:
            //     Joint Photographic Experts Group JFIF format.
            Jpe = 109,
            //
            // Summary:
            //     Joint Photographic Experts Group JFIF format.
            Jpeg = 110,
            //
            // Summary:
            //     Joint Photographic Experts Group JFIF format.
            Jpg = 111,
            //
            // Summary:
            //     JPEG-2000 File Format Syntax.
            Jpm = 112,
            //
            // Summary:
            //     Joint Photographic Experts Group JFIF format.
            Jps = 113,
            //
            // Summary:
            //     JPEG-2000 File Format Syntax.
            Jpt = 114,
            //
            // Summary:
            //     The image format and characteristics.
            Json = 115,
            //
            // Summary:
            //     JPEG XL Lossless JPEG1 Recompression.
            Jxl = 116,
            //
            // Summary:
            //     Raw black samples.
            K = 117,
            //
            // Summary:
            //     Kodak Digital Camera Raw Image Format.
            K25 = 118,
            //
            // Summary:
            //     Kodak Digital Camera Raw Image Format.
            Kdc = 119,
            //
            // Summary:
            //     Image label.
            Label = 120,
            //
            // Summary:
            //     Raw magenta samples.
            M = 121,
            //
            // Summary:
            //     MPEG Video Stream.
            M2v = 122,
            //
            // Summary:
            //     Raw MPEG-4 Video.
            M4v = 123,
            //
            // Summary:
            //     MAC Paint.
            Mac = 124,
            //
            // Summary:
            //     Colormap intensities and indices.
            Map = 125,
            //
            // Summary:
            //     Image Clip Mask.
            Mask = 126,
            //
            // Summary:
            //     MATLAB level 5 image format.
            Mat = 127,
            //
            // Summary:
            //     MATTE format.
            Matte = 128,
            //
            // Summary:
            //     Mamiya Raw Image File.
            Mef = 129,
            //
            // Summary:
            //     Magick Image File Format.
            Miff = 130,
            //
            // Summary:
            //     Multimedia Container.
            Mkv = 131,
            //
            // Summary:
            //     Multiple-image Network Graphics.
            Mng = 132,
            //
            // Summary:
            //     Raw bi-level bitmap.
            Mono = 133,
            //
            // Summary:
            //     MPEG Video Stream.
            Mov = 134,
            //
            // Summary:
            //     MPEG-4 Video Stream.
            Mp4 = 135,
            //
            // Summary:
            //     Magick Persistent Cache image format.
            Mpc = 136,
            //
            // Summary:
            //     MPEG Video Stream.
            Mpeg = 137,
            //
            // Summary:
            //     MPEG Video Stream.
            Mpg = 138,
            //
            // Summary:
            //     Sony (Minolta) Raw Image File.
            Mrw = 139,
            //
            // Summary:
            //     Magick Scripting Language.
            Msl = 140,
            //
            // Summary:
            //     ImageMagick's own SVG internal renderer.
            Msvg = 141,
            //
            // Summary:
            //     MTV Raytracing image format.
            Mtv = 142,
            //
            // Summary:
            //     Magick Vector Graphics.
            Mvg = 143,
            //
            // Summary:
            //     Nikon Digital SLR Camera Raw Image File.
            Nef = 144,
            //
            // Summary:
            //     Nikon Digital SLR Camera Raw Image File.
            Nrw = 145,
            //
            // Summary:
            //     Constant image of uniform color.
            Null = 146,
            //
            // Summary:
            //     Raw opacity samples.
            O = 147,
            //
            // Summary:
            //     OpenRaster format.
            Ora = 148,
            //
            // Summary:
            //     Olympus Digital Camera Raw Image File.
            Orf = 149,
            //
            // Summary:
            //     On-the-air bitmap.
            Otb = 150,
            //
            // Summary:
            //     Open Type font.
            Otf = 151,
            //
            // Summary:
            //     16bit/pixel interleaved YUV.
            Pal = 152,
            //
            // Summary:
            //     Palm pixmap.
            Palm = 153,
            //
            // Summary:
            //     Common 2-dimensional bitmap format.
            Pam = 154,
            //
            // Summary:
            //     Pango Markup Language.
            Pango = 155,
            //
            // Summary:
            //     Predefined pattern.
            Pattern = 156,
            //
            // Summary:
            //     Portable bitmap format (black and white).
            Pbm = 157,
            //
            // Summary:
            //     Photo CD.
            Pcd = 158,
            //
            // Summary:
            //     Photo CD.
            Pcds = 159,
            //
            // Summary:
            //     Printer Control Language.
            Pcl = 160,
            //
            // Summary:
            //     Apple Macintosh QuickDraw/PICT.
            Pct = 161,
            //
            // Summary:
            //     ZSoft IBM PC Paintbrush.
            Pcx = 162,
            //
            // Summary:
            //     Palm Database ImageViewer Format.
            Pdb = 163,
            //
            // Summary:
            //     Portable Document Format.
            Pdf = 164,
            //
            // Summary:
            //     Portable Document Archive Format.
            Pdfa = 165,
            //
            // Summary:
            //     Pentax Electronic File.
            Pef = 166,
            //
            // Summary:
            //     Embrid Embroidery Format.
            Pes = 167,
            //
            // Summary:
            //     Postscript Type 1 font (ASCII).
            Pfa = 168,
            //
            // Summary:
            //     Postscript Type 1 font (binary).
            Pfb = 169,
            //
            // Summary:
            //     Portable float format.
            Pfm = 170,
            //
            // Summary:
            //     Portable graymap format (gray scale).
            Pgm = 171,
            //
            // Summary:
            //     Portable half float format.
            Phm = 172,
            //
            // Summary:
            //     JPEG 2000 uncompressed format.
            Pgx = 173,
            //
            // Summary:
            //     Personal Icon.
            Picon = 174,
            //
            // Summary:
            //     Apple Macintosh QuickDraw/PICT.
            Pict = 175,
            //
            // Summary:
            //     Alias/Wavefront RLE image format.
            Pix = 176,
            //
            // Summary:
            //     Joint Photographic Experts Group JFIF format.
            Pjpeg = 177,
            //
            // Summary:
            //     Plasma fractal image.
            Plasma = 178,
            //
            // Summary:
            //     Portable Network Graphics.
            Png = 179,
            //
            // Summary:
            //     PNG inheriting bit-depth and color-type from original.
            Png00 = 180,
            //
            // Summary:
            //     opaque or binary transparent 24-bit RGB.
            Png24 = 181,
            //
            // Summary:
            //     opaque or transparent 32-bit RGBA.
            Png32 = 182,
            //
            // Summary:
            //     opaque or binary transparent 48-bit RGB.
            Png48 = 183,
            //
            // Summary:
            //     opaque or transparent 64-bit RGBA.
            Png64 = 184,
            //
            // Summary:
            //     8-bit indexed with optional binary transparency.
            Png8 = 185,
            //
            // Summary:
            //     Portable anymap.
            Pnm = 186,
            //
            // Summary:
            //     Pocketmod Personal Organizer (Pdf).
            Pocketmod = 187,
            //
            // Summary:
            //     Portable pixmap format (color).
            Ppm = 188,
            //
            // Summary:
            //     PostScript.
            Ps = 189,
            //
            // Summary:
            //     Level II PostScript.
            Ps2 = 190,
            //
            // Summary:
            //     Level III PostScript.
            Ps3 = 191,
            //
            // Summary:
            //     Adobe Large Document Format.
            Psb = 192,
            //
            // Summary:
            //     Adobe Photoshop bitmap.
            Psd = 193,
            //
            // Summary:
            //     Pyramid encoded TIFF.
            Ptif = 194,
            //
            // Summary:
            //     Seattle Film Works.
            Pwp = 195,
            //
            // Summary:
            //     Quite OK image format.
            Qoi = 196,
            //
            // Summary:
            //     Raw red samples.
            R = 197,
            //
            // Summary:
            //     Gradual radial passing from one shade to another.
            RadialGradient = 198,
            //
            // Summary:
            //     Fuji CCD-RAW Graphic File.
            Raf = 199,
            //
            // Summary:
            //     SUN Rasterfile.
            Ras = 200,
            //
            // Summary:
            //     Raw.
            Raw = 201,
            //
            // Summary:
            //     Raw red, green, and blue samples.
            Rgb = 202,
            //
            // Summary:
            //     Raw red, green, blue samples in 565 format.
            Rgb565 = 203,
            //
            // Summary:
            //     Raw red, green, blue, and alpha samples.
            Rgba = 204,
            //
            // Summary:
            //     Raw red, green, blue, and opacity samples.
            Rgbo = 205,
            //
            // Summary:
            //     LEGO Mindstorms EV3 Robot Graphic Format (black and white).
            Rgf = 206,
            //
            // Summary:
            //     Alias/Wavefront image.
            Rla = 207,
            //
            // Summary:
            //     Utah Run length encoded image.
            Rle = 208,
            //
            // Summary:
            //     Raw Media Format.
            Rmf = 209,
            //
            // Summary:
            //     Rsvg.
            Rsvg = 210,
            //
            // Summary:
            //     Panasonic Lumix Raw Image.
            Rw2 = 211,
            //
            // Summary:
            //     ZX-Spectrum SCREEN$.
            Scr = 212,
            //
            // Summary:
            //     Screen shot.
            Screenshot = 213,
            //
            // Summary:
            //     Scitex HandShake.
            Sct = 214,
            //
            // Summary:
            //     Seattle Film Works.
            Sfw = 215,
            //
            // Summary:
            //     Irix RGB image.
            Sgi = 216,
            //
            // Summary:
            //     Hypertext Markup Language and a client-side image map.
            Shtml = 217,
            //
            // Summary:
            //     DEC SIXEL Graphics Format.
            Six = 218,
            //
            // Summary:
            //     DEC SIXEL Graphics Format.
            Sixel = 219,
            //
            // Summary:
            //     Sparse Color.
            SparseColor = 220,
            //
            // Summary:
            //     Sony Raw Format 2.
            Sr2 = 221,
            //
            // Summary:
            //     Sony Raw Format.
            Srf = 222,
            //
            // Summary:
            //     Steganographic image.
            Stegano = 223,
            //
            // Summary:
            //     String to image and back.
            StrImg = 224,
            //
            // Summary:
            //     SUN Rasterfile.
            Sun = 225,
            //
            // Summary:
            //     Scalable Vector Graphics.
            Svg = 226,
            //
            // Summary:
            //     Compressed Scalable Vector Graphics.
            Svgz = 227,
            //
            // Summary:
            //     Text.
            Text = 228,
            //
            // Summary:
            //     Truevision Targa image.
            Tga = 229,
            //
            // Summary:
            //     EXIF Profile Thumbnail.
            Thumbnail = 230,
            //
            // Summary:
            //     Tagged Image File Format.
            Tif = 231,
            //
            // Summary:
            //     Tagged Image File Format.
            Tiff = 232,
            //
            // Summary:
            //     Tagged Image File Format (64-bit).
            Tiff64 = 233,
            //
            // Summary:
            //     Tile image with a texture.
            Tile = 234,
            //
            // Summary:
            //     PSX TIM.
            Tim = 235,
            //
            // Summary:
            //     PS2 TIM2.
            Tm2 = 236,
            //
            // Summary:
            //     TrueType font collection.
            Ttc = 237,
            //
            // Summary:
            //     TrueType font.
            Ttf = 238,
            //
            // Summary:
            //     Text.
            Txt = 239,
            //
            // Summary:
            //     Unicode Text format.
            Ubrl = 240,
            //
            // Summary:
            //     Unicode Text format 6dot.
            Ubrl6 = 241,
            //
            // Summary:
            //     X-Motif UIL table.
            Uil = 242,
            //
            // Summary:
            //     16bit/pixel interleaved YUV.
            Uyvy = 243,
            //
            // Summary:
            //     Truevision Targa image.
            Vda = 244,
            //
            // Summary:
            //     VICAR rasterfile format.
            Vicar = 245,
            //
            // Summary:
            //     Visual Image Directory.
            Vid = 246,
            //
            // Summary:
            //     Open Web Media.
            WebM = 247,
            //
            // Summary:
            //     Khoros Visualization image.
            Viff = 248,
            //
            // Summary:
            //     VIPS image.
            Vips = 249,
            //
            // Summary:
            //     Truevision Targa image.
            Vst = 250,
            //
            // Summary:
            //     WebP Image Format.
            WebP = 251,
            //
            // Summary:
            //     Wireless Bitmap (level 0) image.
            Wbmp = 252,
            //
            // Summary:
            //     Windows Meta File.
            Wmf = 253,
            //
            // Summary:
            //     Windows Media Video.
            Wmv = 254,
            //
            // Summary:
            //     Word Perfect Graphics.
            Wpg = 255,
            //
            // Summary:
            //     Sigma Camera RAW Picture File.
            X3f = 256,
            //
            // Summary:
            //     X Windows system bitmap (black and white).
            Xbm = 257,
            //
            // Summary:
            //     Constant image uniform color.
            Xc = 258,
            //
            // Summary:
            //     GIMP image.
            Xcf = 259,
            //
            // Summary:
            //     X Windows system pixmap (color).
            Xpm = 260,
            //
            // Summary:
            //     Microsoft XML Paper Specification.
            Xps = 261,
            //
            // Summary:
            //     Khoros Visualization image.
            Xv = 262,
            //
            // Summary:
            //     Raw yellow samples.
            Y = 263,
            //
            // Summary:
            //     The image format and characteristics.
            Yaml = 264,
            //
            // Summary:
            //     Raw Y, Cb, and Cr samples.
            Ycbcr = 265,
            //
            // Summary:
            //     Raw Y, Cb, Cr, and alpha samples.
            Ycbcra = 266,
            //
            // Summary:
            //     CCIR 601 4:1:1 or 4:2:2.
            Yuv = 267
        }
    }
}
