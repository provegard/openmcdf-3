/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * 
 * The Original Code is OpenMCDF - Compound Document Format library.
 * 
 * The Initial Developer of the Original Code is Federico Blaseotto.*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OpenMcdf
{
    public enum StgType : int
    {
        StgInvalid = 0,
        StgStorage = 1,
        StgStream = 2,
        StgLockbytes = 3,
        StgProperty = 4,
        StgRoot = 5
    }

    public enum StgColor : int
    {
        Red = 0,
        Black = 1
    }

    internal class DirectoryEntry : IDirectoryEntry
    {
        internal const int THIS_IS_GREATER = 1;
        internal const int OTHER_IS_GREATER = -1;
        private IList<IDirectoryEntry> dirRepository;

        private int sid = -1;
        public int SID
        {
            get => sid;
            set => sid = value;
        }

        internal static Int32 NOSTREAM
            = unchecked((int)0xFFFFFFFF);

        internal static Int32 ZERO
            = 0;

        private DirectoryEntry(String name, StgType stgType, IList<IDirectoryEntry> dirRepository)
        {
            this.dirRepository = dirRepository;

            this.stgType = stgType;

            if (stgType == StgType.StgStorage)
            {
                creationDate = BitConverter.GetBytes((DateTime.Now.ToFileTime()));
                StartSetc = ZERO;
            }

            if (stgType == StgType.StgInvalid)
            {
                StartSetc = ZERO;
            }

            if (name != String.Empty)
            {
                SetEntryName(name);
            }
        }

        private byte[] entryName = new byte[64];

        public byte[] EntryName => entryName;

        // cached for increased CompareTo performance
        private string cachedEntryName;

        public String GetEntryName()
        {
            if (entryName != null && entryName.Length > 0 && nameLength > 0)
            {
                // Skip null terminator when reading
                return cachedEntryName =
                    (cachedEntryName ?? Encoding.Unicode.GetString(entryName, 0, nameLength - 2));
            }

            return String.Empty;
        }

        public void SetEntryName(String newEntryName)
        {
            if (newEntryName == String.Empty)
            {
                entryName = new byte[64];
                nameLength = 0;
            }
            else
            {
                if (
                    newEntryName.Contains(@"\") ||
                    newEntryName.Contains(@"/") ||
                    newEntryName.Contains(@":") ||
                    newEntryName.Contains(@"!")

                    )
                    throw new CFException("Invalid character in entry: the characters '\\', '/', ':','!' cannot be used in entry name");

                if (newEntryName.Length > 31)
                    throw new CFException("Entry name MUST NOT exceed 31 characters");

                byte[] temp = Encoding.Unicode.GetBytes(newEntryName);
                byte[] newName = new byte[64];
                Buffer.BlockCopy(temp, 0, newName, 0, temp.Length);
                newName[temp.Length] = 0x00;
                newName[temp.Length + 1] = 0x00;

                entryName = newName;
                nameLength = (ushort)(temp.Length + 2);
            }

            cachedEntryName = newEntryName;
        }

        private ushort nameLength;
        public ushort NameLength
        {
            get => nameLength;
            set => throw new NotImplementedException();
        }

        private StgType stgType = StgType.StgInvalid;
        public StgType StgType
        {
            get => stgType;
            set => stgType = value;
        }
        private StgColor stgColor = StgColor.Red;

        public StgColor StgColor
        {
            get => stgColor;
            set => stgColor = value;
        }

        private Int32 leftSibling = NOSTREAM;
        public Int32 LeftSibling
        {
            get => leftSibling;
            set => leftSibling = value;
        }

        private Int32 rightSibling = NOSTREAM;
        public Int32 RightSibling
        {
            get => rightSibling;
            set => rightSibling = value;
        }

        private Int32 child = NOSTREAM;
        public Int32 Child
        {
            get => child;
            set => child = value;
        }

        private Guid storageCLSID
            = Guid.Empty;

        public Guid StorageCLSID
        {
            get => storageCLSID;
            set => storageCLSID = value;
        }


        private Int32 stateBits;

        public Int32 StateBits
        {
            get => stateBits;
            set => stateBits = value;
        }

        private byte[] creationDate = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public byte[] CreationDate
        {
            get => creationDate;
            set => creationDate = value;
        }

        private byte[] modifyDate = new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public byte[] ModifyDate
        {
            get => modifyDate;
            set => modifyDate = value;
        }

        private Int32 startSetc = Sector.ENDOFCHAIN;
        public Int32 StartSetc
        {
            get => startSetc;
            set => startSetc = value;
        }
        private long size;
        public long Size
        {
            get => size;
            set => size = value;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is IDirectoryEntry otherDir))
                throw new CFException("Invalid casting: compared object does not implement IDirectorEntry interface");

            if (NameLength > otherDir.NameLength)
            {
                return THIS_IS_GREATER;
            }
            if (NameLength < otherDir.NameLength)
            {
                return OTHER_IS_GREATER;
            }

            return string.Compare(GetEntryName(), otherDir.GetEntryName(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        /// <summary>
        /// FNV hash, short for Fowler/Noll/Vo
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>(not warranted) unique hash for byte array</returns>
        private static ulong fnv_hash(byte[] buffer)
        {

            ulong h = 2166136261;
            int i;

            for (i = 0; i < buffer.Length; i++)
                h = (h * 16777619) ^ buffer[i];

            return h;
        }

        public override int GetHashCode()
        {
            return (int)fnv_hash(entryName);
        }

        public void Write(Stream stream)
        {
            StreamRW rw = new StreamRW(stream);

            rw.Write(entryName);
            rw.Write(nameLength);
            rw.Write((byte)stgType);
            rw.Write((byte)stgColor);
            rw.Write(leftSibling);
            rw.Write(rightSibling);
            rw.Write(child);
            rw.Write(storageCLSID.ToByteArray());
            rw.Write(stateBits);
            rw.Write(creationDate);
            rw.Write(modifyDate);
            rw.Write(startSetc);
            rw.Write(size);

            rw.Close();
        }

        public void Read(Stream stream, CFSVersion ver = CFSVersion.Ver_3)
        {
            StreamRW rw = new StreamRW(stream);

            entryName = rw.ReadBytes(64);
            nameLength = rw.ReadUInt16();
            stgType = (StgType)rw.ReadByte();
            stgColor = (StgColor)rw.ReadByte();
            leftSibling = rw.ReadInt32();
            rightSibling = rw.ReadInt32();
            child = rw.ReadInt32();

            // Thanks to bugaccount (BugTrack id 3519554)
            if (stgType == StgType.StgInvalid)
            {
                leftSibling = NOSTREAM;
                rightSibling = NOSTREAM;
                child = NOSTREAM;
            }

            storageCLSID = new Guid(rw.ReadBytes(16));
            stateBits = rw.ReadInt32();
            creationDate = rw.ReadBytes(8);
            modifyDate = rw.ReadBytes(8);
            startSetc = rw.ReadInt32();

            if (ver == CFSVersion.Ver_3)
            {
                // avoid dirty read for version 3 files (max size: 32bit integer)
                // where most significant bits are not initialized to zero

                size = rw.ReadInt32();
                rw.ReadBytes(4); //discard most significant 4 (possibly) dirty bytes
            }
            else
            {
                size = rw.ReadInt64();
            }
        }

        public string Name => GetEntryName();


        public RedBlackTree.IRBNode Left
        {
            get
            {
                if (leftSibling == NOSTREAM)
                    return null;

                return dirRepository[leftSibling];
            }
            set
            {
                leftSibling = ((IDirectoryEntry)value)?.SID ?? NOSTREAM;

                if (leftSibling != NOSTREAM)
                    dirRepository[leftSibling].Parent = this;
            }
        }

        public RedBlackTree.IRBNode Right
        {
            get
            {
                if (rightSibling == NOSTREAM)
                    return null;

                return dirRepository[rightSibling];
            }
            set
            {

                rightSibling = ((IDirectoryEntry)value)?.SID ?? NOSTREAM;

                if (rightSibling != NOSTREAM)
                    dirRepository[rightSibling].Parent = this;

            }
        }

        public RedBlackTree.Color Color
        {
            get => (RedBlackTree.Color)StgColor;
            set => StgColor = (StgColor)value;
        }

        private IDirectoryEntry parent = null;

        public RedBlackTree.IRBNode Parent
        {
            get => parent;
            set => parent = value as IDirectoryEntry;
        }

        public RedBlackTree.IRBNode Grandparent()
        {
            return parent?.Parent;
        }

        public RedBlackTree.IRBNode Sibling()
        {
            return this == Parent?.Left ? Parent?.Right : Parent?.Left;
        }

        public RedBlackTree.IRBNode Uncle()
        {
            return Parent?.Sibling();
        }

        internal static IDirectoryEntry New(String name, StgType stgType, IList<IDirectoryEntry> dirRepository)
        {
            DirectoryEntry de;
            if (dirRepository != null)
            {
                de = new DirectoryEntry(name, stgType, dirRepository);
                // No invalid directory entry found
                dirRepository.Add(de);
                de.SID = dirRepository.Count - 1;
            }
            else
                throw new ArgumentNullException(nameof(dirRepository), "Directory repository cannot be null in New() method");

            return de;
        }

        internal static IDirectoryEntry Mock(String name, StgType stgType)
        {
            DirectoryEntry de = new DirectoryEntry(name, stgType, null);

            return de;
        }

        internal static IDirectoryEntry TryNew(String name, StgType stgType, IList<IDirectoryEntry> dirRepository)
        {
            DirectoryEntry de = new DirectoryEntry(name, stgType, dirRepository);

            // If we are not adding an invalid dirEntry as
            // in a normal loading from file (invalid dirs MAY pad a sector)
            // Find first available invalid slot (if any) to reuse it
            for (int i = 0; i < dirRepository.Count; i++)
            {
                if (dirRepository[i].StgType == StgType.StgInvalid)
                {
                    dirRepository[i] = de;
                    de.SID = i;
                    return de;
                }
            }

            // No invalid directory entry found
            dirRepository.Add(de);
            de.SID = dirRepository.Count - 1;

            return de;
        }

        public override string ToString()
        {
            return Name + " [" + sid + "]" + (stgType == StgType.StgStream ? "Stream" : "Storage");
        }

        public void AssignValueTo(RedBlackTree.IRBNode other)
        {
            DirectoryEntry d = other as DirectoryEntry;

            d.SetEntryName(GetEntryName());

            d.creationDate = new byte[creationDate.Length];
            creationDate.CopyTo(d.creationDate, 0);

            d.modifyDate = new byte[modifyDate.Length];
            modifyDate.CopyTo(d.modifyDate, 0);

            d.size = size;
            d.startSetc = startSetc;
            d.stateBits = stateBits;
            d.stgType = stgType;
            d.storageCLSID = new Guid(storageCLSID.ToByteArray());
            d.Child = Child;
        }
    }
}
