using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenMcdf.Extensions.OLEProperties;

namespace OpenMcdf.Extensions.Test
{
    [TestClass]
    public class DictionaryEntryTest
    {
        [TestMethod]
        public void Test_read_WINUNICODE_name_without_padding()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                2, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 0, 0,   // name: 'a' + null terminator
            });
            DictionaryEntry entry = ReadEntry(reader, CodePages.CP_WINUNICODE);

            Assert.AreEqual("a", entry.Name);
        }

        [TestMethod]
        public void Test_read_WINUNICODE_name_with_padding()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0     // null terminator + padding
            });
            DictionaryEntry entry = ReadEntry(reader, CodePages.CP_WINUNICODE);

            Assert.AreEqual("ab", entry.Name);
        }
        
        [TestMethod]
        public void Test_padding_after_WINUNICODE_name_is_skipped()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0,    // null terminator + padding
                99             // read check
            });
            _ = ReadEntry(reader, CodePages.CP_WINUNICODE);

            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }
        
        [TestMethod]
        public void Test_reader_has_correct_position_after_WINUNICODE_unpadded_name_is_read()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                2, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 0, 0,   // name: 'a' + null terminator, no padding
                99             // read check
            });
            _ = ReadEntry(reader, CodePages.CP_WINUNICODE);

            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }
        
        [TestMethod]
        public void Test_read_ASCII_name()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                3, 0, 0, 0,    // length in 8-bit chars including null terminator
                97, 98, 0      // name: 'a', 'b' + null terminator
            });
            DictionaryEntry entry = ReadEntry(reader, CodePages.CP_ASCII);

            Assert.AreEqual("ab", entry.Name);
        }
        
        [TestMethod]
        public void Test_no_padding_is_skipped_when_reading_ASCII_name()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // property identifier
                3, 0, 0, 0,    // length in 8-bit chars including null terminator
                97, 98, 0,     // name: 'a', 'b' + null terminator
                99             // read check
            });
            _ = ReadEntry(reader, CodePages.CP_ASCII);

            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }

        private DictionaryEntry ReadEntry(BinaryReader reader, int codepage)
        {
            DictionaryEntry entry = new DictionaryEntry(codepage);
            entry.Read(reader);
            return entry;
        }

        private BinaryReader ReaderFor(byte[] data)
        {
            return new BinaryReader(new MemoryStream(data));
        }
    }
}