using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenMcdf.Extensions.OLEProperties;
using OpenMcdf.Extensions.OLEProperties.Interfaces;

namespace OpenMcdf.Extensions.Test
{
    [TestClass]
    public class PropertyFactoryTest
    {
        [TestMethod]
        public void Test_read_LPWSTR_property_without_padding()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                2, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 0, 0,   // name: 'a' + null terminator
            });
            string result = ReadLPWSTR(reader);
            Assert.AreEqual("a", result);
        }
        
        [TestMethod]
        public void Test_read_LPWSTR_property_with_padding()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0     // null terminator + padding
            });
            string result = ReadLPWSTR(reader);
            Assert.AreEqual("ab", result);
        }

        [TestMethod]
        public void Test_padding_after_LPWSTR_data_is_skipped()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0,    // null terminator + padding
                99             // read check
            });
            _ = ReadLPWSTR(reader);

            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }
        
        [TestMethod]
        public void Test_read_empty_LPWSTR_property()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                0, 0, 0, 0     // length in 16-bit Unicode chars including null terminator
            });
            string result = ReadLPWSTR(reader);
            Assert.AreEqual("", result);
        }
        
        [DataTestMethod]
        [DataRow(CodePages.CP_ASCII)]
        [DataRow(CodePages.CP_WINUNICODE)]
        public void Test_read_empty_LPSTR_property(int codePage)
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                0, 0, 0, 0     // length in 16-bit Unicode chars including null terminator
            });
            string result = ReadLPSTR(reader, codePage);
            Assert.AreEqual("", result);
        }
        
        [TestMethod]
        public void Test_read_LPSTR_property_of_UNICODE_type()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0     // null terminator + padding
            });
            string result = ReadLPSTR(reader, CodePages.CP_WINUNICODE);
            Assert.AreEqual("ab", result);
        }
        
        [TestMethod]
        public void Test_padding_after_LPSTR_property_of_UNICODE_type_is_skipped()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 16-bit Unicode chars including null terminator
                97, 0, 98, 0,  // name: 'a', 'b'
                0, 0, 0, 0,    // null terminator + padding
                99             // read check
            });
            _ = ReadLPSTR(reader, CodePages.CP_WINUNICODE);

            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }
        
        [TestMethod]
        public void Test_read_LPSTR_property_of_ASCII_type()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 8-bit Unicode chars including null terminator
                97, 98, 0, 0   // name: 'a', 'b' + null terminator + padding
            });
            string result = ReadLPSTR(reader, CodePages.CP_ASCII);
            Assert.AreEqual("ab", result);
        }
                
        [TestMethod]
        public void Test_padding_after_LPSTR_property_of_ASCII_type_is_skipped()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                3, 0, 0, 0,    // length in 8-bit Unicode chars including null terminator
                97, 98, 0, 0,  // name: 'a', 'b' + null terminator + padding
                99             // read check
            });
            _ = ReadLPSTR(reader, CodePages.CP_ASCII);
            
            var next = reader.ReadByte();
            Assert.AreEqual((byte) 99, next);
        }
        
        [TestMethod]
        public void Test_reader_is_at_correct_position_after_reading_LPSTR_property_of_ASCII_type_without_padding()
        {
            BinaryReader reader = ReaderFor(new byte[]
            {
                4, 0, 0, 0,    // length in 8-bit Unicode chars including null terminator
                97, 98, 99, 0, // name: 'a', 'b', 'c' + null terminator, no padding
                100            // read check
            });
            _ = ReadLPSTR(reader, CodePages.CP_ASCII);
            
            var next = reader.ReadByte();
            Assert.AreEqual((byte) 100, next);
        }
        
        private string ReadLPSTR(BinaryReader reader, int codePage)
        {
            ITypedPropertyValue property = PropertyFactory.Instance.NewProperty(VTPropertyType.VT_LPSTR, codePage);
            property.Read(reader);
            return property.Value as string;
        }
        
        private string ReadLPWSTR(BinaryReader reader)
        {
            ITypedPropertyValue property = PropertyFactory.Instance.NewProperty(VTPropertyType.VT_LPWSTR, CodePages.CP_WINUNICODE);
            property.Read(reader);
            return property.Value as string;
        }

        private BinaryReader ReaderFor(byte[] data)
        {
            return new BinaryReader(new MemoryStream(data));
        }
    }
}