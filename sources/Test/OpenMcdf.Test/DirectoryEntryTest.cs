using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenMcdf.Test
{
    [TestClass]
    public class DirectoryEntryTest
    {
        [TestMethod]
        public void Test_Name()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry = DirectoryEntry.New("Test", StgType.StgInvalid, dirRepository);
            Assert.AreEqual("Test", entry.Name);
        }
        
        [TestMethod]
        public void Test_long_Name()
        {
            const string name = "abcdefghijklmnopqrstuvwxyzabcde";
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry = DirectoryEntry.New(name, StgType.StgInvalid, dirRepository);
            Assert.AreEqual(name, entry.Name);
        }
        
        [TestMethod]
        public void Test_GetEntryName()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry = DirectoryEntry.New("Test", StgType.StgInvalid, dirRepository);
            Assert.AreEqual("Test", entry.GetEntryName());
        }
        
        [TestMethod]
        public void Test_SetEntryName()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry = DirectoryEntry.New("Test", StgType.StgInvalid, dirRepository);
            entry.SetEntryName("NewName");
            Assert.AreEqual("NewName", entry.Name);
        }
        
        [TestMethod]
        public void Test_SetEntryName_with_empty_name()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry = DirectoryEntry.New("Test", StgType.StgInvalid, dirRepository);
            entry.SetEntryName("");
            Assert.AreEqual("", entry.Name);
        }

        [DataTestMethod]
        [DataRow("aa", "a", 1)]
        [DataRow("a", "aa", -1)]
        [DataRow("a", "a", 0)]
        [DataRow("a", "b", -1)]
        [DataRow("a", "B", -1)]
        [DataRow("b", "a", 1)]
        [DataRow("B", "a", 1)]
        [DataRow("aa", "ab", -1)]
        public void Test_CompareTo(string first, string second, int expected)
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry entry1 = DirectoryEntry.New(first, StgType.StgInvalid, dirRepository);
            IDirectoryEntry entry2 = DirectoryEntry.New(second, StgType.StgInvalid, dirRepository);

            int actual = entry1.CompareTo(entry2);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test_New_adds_multiple_invalid()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            DirectoryEntry.New("first", StgType.StgInvalid, dirRepository);
            DirectoryEntry.New("second", StgType.StgInvalid, dirRepository);

            Assert.AreEqual(2, dirRepository.Count);
        }
        
        [TestMethod]
        public void Test_TryNew_overwrites_invalid()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            DirectoryEntry.TryNew("first", StgType.StgInvalid, dirRepository);
            DirectoryEntry.TryNew("second", StgType.StgProperty, dirRepository);

            Assert.AreEqual(1, dirRepository.Count);
            Assert.AreEqual("second", dirRepository[0].Name);
        }
                
        [TestMethod]
        public void Test_TryNew_adds_different_types()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            DirectoryEntry.TryNew("first", StgType.StgStream, dirRepository);
            DirectoryEntry.TryNew("second", StgType.StgProperty, dirRepository);

            Assert.AreEqual(2, dirRepository.Count);
        }

        [TestMethod]
        public void Test_Name_after_read()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry first = DirectoryEntry.New("first", StgType.StgInvalid, dirRepository);

            Stream memory = WriteToStream(first);

            IDirectoryEntry second = DirectoryEntry.New("", StgType.StgInvalid, dirRepository);
            second.Read(memory);
            
            Assert.AreEqual("first", second.Name);
        }
        
        [TestMethod]
        public void Test_cached_Name_after_read()
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            IDirectoryEntry first = DirectoryEntry.New("first", StgType.StgInvalid, dirRepository);

            Stream memory = WriteToStream(first);

            IDirectoryEntry second = DirectoryEntry.New("", StgType.StgInvalid, dirRepository);

            // Force caching
            _ = second.GetEntryName();
            
            second.Read(memory);
            
            Assert.AreEqual("first", second.Name);
        }
        
        [DataTestMethod]
        [DataRow("a\\b")]
        [DataRow("a/b")]
        [DataRow("a:b")]
        [DataRow("a!b")]
        [DataRow("abcdefghijklmnopqrstuvwxyzabcdef")]
        [ExpectedException(typeof(CFException))]
        public void Test_setting_invalid_name(string name)
        {
            IList<IDirectoryEntry> dirRepository = new List<IDirectoryEntry>();
            DirectoryEntry.New(name, StgType.StgInvalid, dirRepository);
        }
        
        private Stream WriteToStream(IDirectoryEntry entry)
        {
            MemoryStream memory = new MemoryStream();
            entry.Write(memory);
            memory.Position = 0;
            return memory;
        }
    }
}