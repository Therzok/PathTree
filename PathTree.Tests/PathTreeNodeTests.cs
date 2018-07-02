﻿using System;
using System.IO;
using NUnit.Framework;

namespace PathTree.Tests
{
	[TestFixture]
	public class PathTreeNodeTests
	{
		readonly string[] pathDatas = {
			Path.Combine ("a", "b", "c"),
			Path.Combine ("a", "b", "d"),
			Path.Combine ("b", "c", "d")
		};

		[Test]
		public void CreateSubTree()
		{
			var path = Path.Combine("a", "b", "c");

			var (pathTreeNode, leaf) = PathTreeNode.CreateSubTree(path.Split(Path.DirectorySeparatorChar), 0);
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
			var (node, leaf) = PathTreeNode.CreateSubTree(Array.Empty<string>(), startIndex);
			Assert.IsNull(node);
			Assert.IsNull(leaf);
		}
	}
}
