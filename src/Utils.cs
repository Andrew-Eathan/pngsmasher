using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// code credits
/// https://tousu.in/qa/?qa=1156416/
/// has amazing speed, thank you random stranger
/// </summary>

namespace pngsmasher
{
    public static class Utils
    {
        public struct Offset
        {
            public int Start;
            public int End;

            public Offset(int Start, int End)
            {
                this.Start = Start;
                this.End = End;
            }
        }

        public struct Size
        {
            public int Width;
            public int Height;
        }

        public static void BlitBuffer(this byte[] target, byte[] data, int offset, int add = 0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                target[i + offset] = (byte)(data[i] + add);
            }
        }

        public static int PFFloor(float min, float max, Types.SeedRand rand)
        {
            var v = (int)Math.Floor(rand.Generate(min, max));
            return v;
        }

        public static int PFCeil(float min, float max, Types.SeedRand rand)
        {
            var v = (int)Math.Ceiling(rand.Generate(min, max));
            return v;
        }

        public static int SnapNumber(float num, float step)
        {
            return (int)(Math.Round(num / step) * step);
        }

        public static byte[] Combine(IEnumerable<byte[]> arrays)
        {
            byte[] bytes = new byte[arrays.Select(x => x.Length).Sum()];

            int offset = 0;

            foreach (byte[] b in arrays)
            {
                Array.Copy(b, 0, bytes, offset, b.Length);
                offset += b.Length;
            }

            return bytes;
        }

        public static void Shift(Span<byte> input, Span<byte> output, int direction)
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
    }
}
