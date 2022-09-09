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
        public static string log = "";
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

        public class ArrayRange<T>
        {
            ArraySegment<T> ts;
            public ArrayRange(T[] arr, int start, int end)
            {
                var cstart = Math.Max(start, 0);
                var cend = Math.Min(end - start, arr.Length - start);
                ts = new ArraySegment<T>(arr, cstart, cend);
            }
            public T[] Array
            {
                get
                {
                    return ts.ToArray();
                }
            }
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
            log += v + "\n";
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

        public static byte[] Combine(byte[][] arrays)
        {
            byte[] bytes = new byte[0];

            foreach (byte[] b in arrays)
            {
                bytes = bytes.Concat(b).ToArray();
            }

            return bytes.ToArray();
        }

        public static byte[] ShiftLeft(byte[] value, int bitcount)
        {
            byte[] temp = new byte[value.Length];
            if (bitcount >= 8)
            {
                Array.Copy(value, bitcount / 8, temp, 0, temp.Length - (bitcount / 8));
            }
            else
            {
                Array.Copy(value, temp, temp.Length);
            }
            if (bitcount % 8 != 0)
            {
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] <<= bitcount % 8;
                    if (i < temp.Length - 1)
                    {
                        temp[i] |= (byte)(temp[i + 1] >> 8 - bitcount % 8);
                    }
                }
            }
            return temp;
        }

        public static byte[] ShiftRight(byte[] value, int bitcount)
        {
            byte[] temp = new byte[value.Length];
            if (bitcount >= 8)
            {
                Array.Copy(value, 0, temp, bitcount / 8, temp.Length - (bitcount / 8));
            }
            else
            {
                Array.Copy(value, temp, temp.Length);
            }
            if (bitcount % 8 != 0)
            {
                for (int i = temp.Length - 1; i >= 0; i--)
                {
                    temp[i] >>= bitcount % 8;
                    if (i > 0)
                    {
                        temp[i] |= (byte)(temp[i - 1] << 8 - bitcount % 8);
                    }
                }
            }
            return temp;
        }
    }
}
