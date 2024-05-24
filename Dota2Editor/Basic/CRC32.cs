namespace Dota2Editor.Basic
{
    public static class CRC32
    {
        private static readonly uint[] CRC_TABLE;
        private static readonly uint[] CRC_INVERSE;

        static CRC32()
        {
            CRC_TABLE = new uint[256];
            CRC_INVERSE = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                var a = i;
                var b = i << 24;
                for (var j = 0; j < 8; j++)
                {
                    a = (a >> 1) ^ ((a & 0x00000001) != 0 ? 0xEDB88320 : 0);
                    b = (b << 1) ^ ((b & 0x80000000) != 0 ? 0xDB710641 : 0);
                }
                CRC_TABLE[i] = a;
                CRC_INVERSE[i] = b;
            }
        }

        public static uint Compute(byte[] data)
        {
            uint rem = 0xFFFFFFFF;
            foreach (var b in data) rem = (rem >> 8) ^ CRC_TABLE[(rem ^ b) & 0xFF];
            return ~rem;
        }

        public static void Patch(byte[] data, uint targetCrc32, int length)
        {
            uint rem = 0xFFFFFFFF;
            var pos = data.Length - length;

            for (var i = 0; i < pos; i++) rem = (rem >> 8) ^ CRC_TABLE[(rem ^ data[i]) & 0xFF];
            for (var i = 0; i < length; i++) data[pos + i] = (byte)((rem >> (8 * i)) & 0xFF);

            rem = ~targetCrc32;

            for (var i = data.Length - 1; i >= pos; i--) rem = (rem << 8) ^ CRC_INVERSE[rem >> 24] ^ data[i];
            for (var i = 0; i < 4; i++) data[pos + i] = (byte)((rem >> (8 * i)) & 0xFF);
        }
    }
}
