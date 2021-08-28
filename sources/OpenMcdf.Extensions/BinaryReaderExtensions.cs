using System;
using System.IO;

namespace OpenMcdf.Extensions
{
    internal static class BinaryReaderExtensions
    {
        private const int CP_WINUNICODE = 0x04B0;
        
        internal static byte[] ReadUnicodeStringData(this BinaryReader reader, int nChars)
        {
            if (nChars == 0)
                return Array.Empty<byte>();
                
            int bytesLen = nChars * 2;
            var nameBytes = reader.ReadBytes(bytesLen - 2); // don't read the null terminator
                
            // skip the null terminator
            reader.ReadBytes(2);
                
            // skip padding, if any
            int m = bytesLen % 4;
            if (m > 0)
                reader.ReadBytes(4 - m);

            return nameBytes;
        }

        internal static byte[] ReadCodePageStringData(this BinaryReader reader, int codePage, int nChars)
        {
            if (nChars == 0)
                return Array.Empty<byte>();
            
            if (codePage == CP_WINUNICODE)
                return reader.ReadUnicodeStringData(nChars);

            int bytesLen = nChars;
            var nameBytes = reader.ReadBytes(bytesLen - 1); // don't read the null terminator
                
            // skip the null terminator
            reader.ReadByte();
                
            // skip padding, if any
            int m = bytesLen % 4;
            if (m > 0)
                reader.ReadBytes(4 - m);

            return nameBytes;
        }
    }
}