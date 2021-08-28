using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenMcdf.Extensions.OLEProperties
{
    public class DictionaryEntry
    {
        private const int CP_WINUNICODE = 0x04B0;

        int codePage;

        public DictionaryEntry(int codePage)
        {
            this.codePage = codePage;
        }

        public uint PropertyIdentifier { get; set; }
        public int Length { get; set; }
        public String Name { get { return GetName(); } }

        private byte[] nameBytes;

        public void Read(BinaryReader br)
        {
            PropertyIdentifier = br.ReadUInt32();
            Length = br.ReadInt32();

            if (codePage != CP_WINUNICODE)
            {
                nameBytes = br.ReadBytes(Length - 1); // don't read the null terminator
                
                // skip the null terminator
                br.ReadByte();
            }
            else
            {
                int bytesLen = Length * 2;
                nameBytes = br.ReadBytes(bytesLen - 2); // don't read the null terminator
                
                // skip the null terminator
                br.ReadBytes(2);

                // skip padding, if any
                int m = 4 - bytesLen % 4;
                if (m > 0)
                    br.ReadBytes(m);
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(PropertyIdentifier);
            bw.Write(Length);
            bw.Write(nameBytes);

            //if (codePage == CP_WINUNICODE)
            //    int m = Length % 4;

            //if (m > 0)
            //    for (int i = 0; i < m; i++)
            //        bw.Write((byte)m);
        }

        private string GetName()
        {
            return Encoding.GetEncoding(this.codePage).GetString(nameBytes);
        }


    }
}
