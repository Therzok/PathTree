using System;
using System.IO;
using NUnit.Framework;

namespace PathTree.Tests
{
	[TestFixture]
	public class PathTreeNodeTests
	{
		string[] seps = {
			"",
			Path.DirectorySeparatorChar.ToString(),
		};

		[TestCaseSource(nameof(seps))]
		public void CreateSubTree(string sep)
		{
			var path = Path.Combine("a", "b", "c") + sep;

			var (pathTreeNode, leaf) = PathTreeNode.CreateSubTree(path, 0);
			AssertPathTreeSubtree(pathTreeNode, "a", 2);
			Assert.AreEqual(1, pathTreeNode.ChildrenCount);

			pathTreeNode = pathTreeNode.FirstChild;
			AssertPathTreeSubtree(pathTreeNode, "b", 1);
			Assert.AreEqual(1, pathTreeNode.ChildrenCount);

			pathTreeNode = pathTreeNode.FirstChild;
			AssertPathTreeSubtree(pathTreeNode, "c", 0);
			Assert.AreEqual(0, pathTreeNode.ChildrenCount);
			Assert.AreSame(pathTreeNode, leaf);

			Assert.IsNull(pathTreeNode.FirstChild);

			void AssertPathTreeSubtree(PathTreeNode node, string segment, int childrenCount)
			{
				Assert.AreEqual(segment, node.Segment);
				Assert.IsNull(node.Next);
				Assert.AreSame(node.FirstChild, node.LastChild);
			}
		}

		[TestCase(0)]
		[TestCase(1)] // Should not crash
		public void EmptySubTrie(int startIndex)
		{
			var (node, leaf) = PathTreeNode.CreateSubTree(string.Empty, startIndex);
			Assert.IsNull(node);
			Assert.IsNull(leaf);
		}

		[Test]
		public void JustSlash()
		{
			var (node, leaf) = PathTreeNode.CreateSubTree(Path.DirectorySeparatorChar.ToString (), 0);
			Assert.IsNotNull(node);
			Assert.AreSame(node, leaf);
			Assert.AreEqual("", node.Segment);
			Assert.AreEqual(Path.DirectorySeparatorChar.ToString(), node.FullPath);
		}
	}
}
