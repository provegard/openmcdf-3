using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedBlackTree;

namespace OpenMcdf.Test
{
    /// <summary>
    /// Summary description for RBTreeTest
    /// </summary>
    [TestClass]
    public class RBTreeTest
    {
        internal IList<IDirectoryEntry> GetDirectoryRepository(int count)
        {
            List<IDirectoryEntry> repo = new List<IDirectoryEntry>(count);
            for (int i = 0; i < count; i++)
            {
                DirectoryEntry.New(i.ToString(), StgType.StgInvalid, repo);
            }

            return repo;
        }

        [TestMethod]
        public void Test_RBTREE_INSERT()
        {
            RBTree rbTree = new RBTree();
            IList<IDirectoryEntry> repo = GetDirectoryRepository(1000000);

            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            for (int i = 0; i < repo.Count; i++)
            {
                rbTree.TryLookup(DirectoryEntry.Mock(i.ToString(), StgType.StgInvalid), out var c);
                Assert.IsInstanceOfType(c, typeof(IDirectoryEntry));
                Assert.AreEqual(i.ToString(), ((IDirectoryEntry)c).Name);
            }
        }


        [TestMethod]
        public void Test_RBTREE_DELETE()
        {
            RBTree rbTree = new RBTree();
            IList<IDirectoryEntry> repo = GetDirectoryRepository(25);


            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            try
            {
                rbTree.Delete(DirectoryEntry.Mock("5", StgType.StgInvalid), out _);
                rbTree.Delete(DirectoryEntry.Mock("24", StgType.StgInvalid), out _);
                rbTree.Delete(DirectoryEntry.Mock("7", StgType.StgInvalid), out _);

                VerifyNodeDoesntExist(rbTree, "5");
                VerifyNodeDoesntExist(rbTree, "7");
                VerifyNodeDoesntExist(rbTree, "24");

                IRBNode c;
                Assert.IsTrue(rbTree.TryLookup(DirectoryEntry.Mock("6", StgType.StgStream), out c));
                Assert.IsInstanceOfType(c, typeof(IDirectoryEntry));
                Assert.IsTrue(rbTree.TryLookup(DirectoryEntry.Mock("12", StgType.StgStream), out c));
                Assert.AreEqual("12", ((IDirectoryEntry) c).Name);
            }
            catch (Exception ex)
            {
                Assert.Fail("Item removal failed: " + ex.Message);
            }
        }

        private static void VerifyNodeDoesntExist(RBTree rbTree, string value)
        {
            bool s = rbTree.TryLookup(DirectoryEntry.Mock(value, StgType.StgStream), out _);
            Assert.IsFalse(s);
        }

        private static void VerifyProperties(RBTree t)
        {
            VerifyProperty1(t.Root);
            VerifyProperty2(t.Root);
            // Property 3 is implicit
            VerifyProperty4(t.Root);
            VerifyProperty5(t.Root);
        }

        private static Color NodeColor(IRBNode n) 
        {
            return n?.Color ?? Color.BLACK;
        }

        private static void VerifyProperty1(IRBNode n)
        {

            Assert.IsTrue(NodeColor(n) == Color.RED || NodeColor(n) == Color.BLACK);

            if (n == null) return;
            VerifyProperty1(n.Left);
            VerifyProperty1(n.Right);
        }

        private static void VerifyProperty2(IRBNode root)
        {
            Assert.IsTrue(NodeColor(root) == Color.BLACK);
        }

        private static void VerifyProperty4(IRBNode n) 
        {

            if (NodeColor(n) == Color.RED)
            {
                Assert.IsTrue((NodeColor(n.Left) == Color.BLACK));
                Assert.IsTrue((NodeColor(n.Right) == Color.BLACK));
                Assert.IsTrue((NodeColor(n.Parent) == Color.BLACK));
            }

            if (n == null) return;
            VerifyProperty4(n.Left);
            VerifyProperty4(n.Right);
        }

        private static void VerifyProperty5(IRBNode root)
        {
            VerifyProperty5Helper(root, 0, -1);
        }

        private static int VerifyProperty5Helper(IRBNode n, int blackCount, int pathBlackCount)
        {
            if (NodeColor(n) == Color.BLACK)
            {
                blackCount++;
            }
            if (n == null)
            {
                if (pathBlackCount == -1)
                {
                    pathBlackCount = blackCount;
                }
                else
                {

                    Assert.IsTrue(blackCount == pathBlackCount);

                }
                return pathBlackCount;
            }

            pathBlackCount = VerifyProperty5Helper(n.Left, blackCount, pathBlackCount);
            pathBlackCount = VerifyProperty5Helper(n.Right, blackCount, pathBlackCount);

            return pathBlackCount;
        }



        [TestMethod]
        public void Test_RBTREE_ENUMERATE()
        {
            RBTree rbTree = new RBTree();
            IList<IDirectoryEntry> repo = GetDirectoryRepository(10000);

            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            VerifyProperties(rbTree);
        }
    }
}
