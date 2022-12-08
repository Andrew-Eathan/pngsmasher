namespace pngsmasher.tests;

public class CorruptionTests
{
    public class BitShift
    {
        public static byte[] GetData() => new byte[]
        {
            0b10001111, 0b11001101, 0b11011100, 0b11010110, 0b00011110
        };

        public static bool GetBit(byte[] bytes, int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;

            if (byteIndex < 0 || byteIndex >= bytes.Length)
                throw new Exception("Index is outside of array");

            byte byteValue = bytes[byteIndex];

            byte bitMask = (byte)(0b10000000 >> bitIndex);

            return (byteValue & bitMask) == bitMask;
        }

        [Fact(DisplayName = "Test shifts divisible by 8")]
        public void TestDivisibleBy8()
        {
            var data = GetData();
            var result = new byte[data.Length];

            for (int i = 0; i < 5; i++)
            {
                // Right shifts
                Corruption.BitShift(data, result, 8 * i);
                Assert.Equal(data[..^i], result[i..]);
            }

            for (int i = 0; i < 5; i++)
            {
                // Left shifts
                Corruption.BitShift(data, result, -8 * i);
                Assert.Equal(data[i..], result[..^i]);
            }
        }

        [Fact(DisplayName = "Test shifts not divisible by 8")]
        public void TestNotDivisibleBy8()
        {
            var data = GetData();
            var result = new byte[data.Length];

            for (int i = 1; i < 8; i++)
            {
                // Right shifts
                Corruption.BitShift(data, result, i);

                for (int bit = 0; bit < data.Length * 8 - i; bit++)
                    Assert.Equal(GetBit(data, bit), GetBit(result, i + bit));
            }

            for (int i = 1; i < 8; i++)
            {
                // Left shifts
                Corruption.BitShift(data, result, -i);

                for (int bit = 0; bit < data.Length * 8 - i; bit++)
                    Assert.Equal(GetBit(data, i + bit), GetBit(result, bit));
            }
        }

        [Fact(DisplayName = "Test shifting by 0")]
        public void TestNoShift()
        {
            var data = GetData();
            var result = new byte[data.Length];

            Corruption.BitShift(data, result, 0);

            Assert.Equal(data, result);
        }
    }
}