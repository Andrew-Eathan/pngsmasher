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

namespace pngfucker.NET
{
    public class Utils
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

        public static int PFFloor(float min, float max, Types.SeedRand rand)
        {
            return (int)Math.Floor(rand.Generate(min, max));
        }

        public static int PFCeil(float min, float max, Types.SeedRand rand)
        {
            return (int)Math.Ceiling(rand.Generate(min, max));
        }

        public static int SnapNumber(float num, float step)
        {
            return (int)(Math.Round(num / step) * step);
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
