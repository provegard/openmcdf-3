using System;
using System.Collections;
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
        internal IList<IRBNode> GetNodeList(int count)
        {
            List<IRBNode> repo = new List<IRBNode>(count);
            for (int i = 0; i < count; i++)
            {
                repo.Add(new TestNode(i));
            }

            return repo;
        }

        [TestMethod]
        public void Test_RBTREE_INSERT()
        {
            RBTree rbTree = new RBTree();
            IList<IRBNode> repo = GetNodeList(1000000);

            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            for (int i = 0; i < repo.Count; i++)
            {
                rbTree.TryLookup(new TestNode(i), out var c);
                Assert.IsInstanceOfType(c, typeof(TestNode));
                Assert.AreEqual(i, ((TestNode)c).Value);
            }
        }

        [TestMethod]
        public void Test_RBTREE_DELETE()
        {
            RBTree rbTree = new RBTree();
            IList<IRBNode> repo = GetNodeList(25);


            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            try
            {
                rbTree.Delete(new TestNode(5), out _);
                rbTree.Delete(new TestNode(24), out _);
                rbTree.Delete(new TestNode(7), out _);

                VerifyNodeDoesntExist(rbTree, 5);
                VerifyNodeDoesntExist(rbTree, 7);
                VerifyNodeDoesntExist(rbTree, 24);

                IRBNode c;
                Assert.IsTrue(rbTree.TryLookup(new TestNode(6), out c));
                Assert.IsInstanceOfType(c, typeof(TestNode));
                Assert.IsTrue(rbTree.TryLookup(new TestNode(12), out c));
                Assert.AreEqual(12, ((TestNode) c).Value);
            }
            catch (Exception ex)
            {
                Assert.Fail("Item removal failed: " + ex.Message);
            }
        }

        [TestMethod]
        public void Test_visit()
        {
            RBTree rbTree = new RBTree();
            IList<IRBNode> repo = GetNodeList(5);

            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            IList<int?> result = new List<int?>();
            rbTree.VisitTree(node => result.Add((node as TestNode)?.Value));

            var message = string.Join(", ", result);
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4 }, (ICollection) result, message);
        }

        private static void VerifyNodeDoesntExist(RBTree rbTree, int value)
        {
            bool s = rbTree.TryLookup(new TestNode(value), out _);
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
            IList<IRBNode> repo = GetNodeList(10000);

            foreach (var item in repo)
            {
                rbTree.Insert(item);
            }

            VerifyProperties(rbTree);
        }

        private class TestNode : IRBNode
        {
            public int Value { get; private set; }

            public TestNode(int value)
            {
                this.Value = value;
            }

            public int CompareTo(object obj)
            {
                if (obj is TestNode other) return Value.CompareTo(other.Value);
                return 0;
            }

            public IRBNode Left { get; set; }
            public IRBNode Right { get; set; }
            public Color Color { get; set; }
            public IRBNode Parent { get; set; }
            public IRBNode Grandparent() => Parent?.Parent;

            public IRBNode Sibling() => this == Parent?.Left ? Parent?.Right : Parent?.Left;

            public IRBNode Uncle() => Parent?.Sibling();

            public void AssignValueTo(IRBNode other)
            {
                if (other is TestNode node)
                {
                    node.Value = Value;
                }
            }
        }
    }
}
