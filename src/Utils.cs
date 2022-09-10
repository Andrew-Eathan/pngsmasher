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
    }
}
