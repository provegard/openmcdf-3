using System;
using System.IO;

namespace OpenMcdf.Extensions
{
    internal static class BinaryReaderExtensions
    {
        internal static byte[] ReadUnicodeStringData(this BinaryReader reader, int nChars)
        {
            if (nChars == 0)
                return Array.Empty<byte>();
                
            int bytesLen = nChars * 2;
            var nameBytes = reader.ReadBytes(bytesLen - 2); // don't read the null terminator
                
            // skip the null terminator
            reader.ReadBytes(2);
                
            // skip padding, if any
            int m = 4 - bytesLen % 4;
            if (m > 0)
                reader.ReadBytes(m);

            return nameBytes;
        }
    }
}