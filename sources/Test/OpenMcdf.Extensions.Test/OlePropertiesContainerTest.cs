using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenMcdf.Extensions.OLEProperties;

namespace OpenMcdf.Extensions.Test
{
    [TestClass]
    public class OlePropertiesContainerTest
    {
        [TestMethod]
        public void Test_read_Inventor_property_stream()
        {
            CompoundFile compoundFile = new CompoundFile();
            CFStream stream = compoundFile.RootStorage.AddStream("test");
            stream.SetData(ReadData("inventor_props_stream.cf"));

            OLEPropertiesContainer container = stream.AsOLEPropertiesContainer();

            IList<string> props =
                container.Properties.Select(prop => $"{prop.PropertyName} = {prop.Value}").ToList();
            
            // CollectionAssert.Contains fails with a useless message,
            // so provide the actual collection contents.
            string message = string.Join("; ", props);
            
            ICollection collection = (ICollection)props;
            
            CollectionAssert.Contains(collection, "Property Set Name = Inventor User Defined Properties", message);
        }

        private byte[] ReadData(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}