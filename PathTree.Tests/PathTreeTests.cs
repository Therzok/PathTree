using System;
using System.IO;
using NUnit.Framework;

namespace PathTree.Tests
{
	[TestFixture]
	public class PathTreeTests
	{
		[Test]
		public void CreateEmptyTree ()
		{
			var tree = new PathTree();
			var node = tree.rootNode;

			Assert.IsNull(node.FirstChild);
			Assert.IsNull(node.LastChild);
			Assert.IsNull(node.Next);
			Assert.IsNullOrEmpty(node.Segment);
		}

		static PathTree CreateTree()
		{
			var tree = new PathTree();

			// a
			// + b
			//   + c
			//   + d
			//   + e
			//   + f
			//     + f1
			//     + f2
			//   + g
			//     + g1
			//     + g2
			tree.AddNode(Path.Combine("a", "b", "g", "g1"));
			tree.AddNode(Path.Combine("a", "b", "g"));
			tree.AddNode(Path.Combine("a", "b", "c"));
			tree.AddNode(Path.Combine("a", "b", "e"));
			tree.AddNode(Path.Combine("a", "b", "d"));
			tree.AddNode(Path.Combine("a", "b", "f"));
			tree.AddNode(Path.Combine("a", "b", "f", "f1"));
			tree.AddNode(Path.Combine("a", "b", "f", "f2"));
			tree.AddNode(Path.Combine("a", "b", "g", "g2"));

			return tree;
		}

		[Test]
		public void CreateSimpleTree()
		{
			var tree = CreateTree();

			PathTreeNode root, a, b, c, d, e, f, f1, f2, g, g1, g2, x, y, z;

			root = tree.rootNode;
			a = root.FirstChild;
			b = a.FirstChild;
			c = b.FirstChild;
			d = c.Next;
			e = d.Next;
			f = e.Next;
			f1 = f.FirstChild;
			f2 = f1.Next;
			g = f.Next;
			g1 = g.FirstChild;
			g2 = g1.Next;

			// rootNode -> a, z
			Assert.AreEqual(nameof(a), a.Segment);
			Assert.IsNull(a.Next);
			Assert.AreSame(a, root.LastChild);

			// a -> b
			Assert.AreEqual(nameof(b), b.Segment);
			Assert.AreSame(a.LastChild, b);
			Assert.IsNull(b.Next);

			// b -> c, d, e, f, g
			Assert.AreEqual(nameof(c), c.Segment);
			Assert.AreEqual(nameof(d), d.Segment);
			Assert.AreEqual(nameof(e), e.Segment);
			Assert.AreEqual(nameof(f), f.Segment);
			Assert.AreEqual(nameof(g), g.Segment);

			Assert.AreSame(b.LastChild, g);

			// c, d, e
			Assert.IsNull(c.FirstChild);
			Assert.IsNull(d.FirstChild);
			Assert.IsNull(e.FirstChild);
			Assert.IsNull(g.Next);

			// f -> f1, f2
			Assert.AreSame(f2, f.LastChild);
			Assert.AreEqual(nameof(f1), f1.Segment);
			Assert.AreEqual(nameof(f2), f2.Segment);
			Assert.IsNull(f2.Next);

			// g -> g1, g2
			Assert.AreEqual(nameof(g1), g1.Segment);
			Assert.AreEqual(nameof(g2), g2.Segment);
			Assert.AreSame(g2, g.LastChild);
			Assert.IsNull(g2.Next);

			// a
			// ...
			// z
			// + y
			//   + x

			tree.AddNode(Path.Combine("z", "y", "x"));

			z = a.Next;
			y = z.FirstChild;
			x = y.FirstChild;

			// root -> z
			Assert.AreEqual(nameof(z), z.Segment);
			Assert.AreSame(z, root.LastChild);
			Assert.AreSame(a.Next, z);
			Assert.IsNull(z.Next);

			// z -> y
			Assert.AreEqual(nameof(z), z.Segment);
			Assert.AreSame(y, z.LastChild);
			Assert.IsNull(y.Next);

			// y -> x
			Assert.AreEqual(nameof(x), x.Segment);
			Assert.IsNull(x.FirstChild);
			Assert.IsNull(x.LastChild);
			Assert.IsNull(x.Next);
		}

		[Test]
		public void AssertSameNodeIsReturned()
		{
			var tree = new PathTree();

			var b = tree.AddNode(Path.Combine ("a", "b"));

			var firstA = tree.FindNode("a");
			var newA = tree.AddNode("a");

			Assert.AreSame(firstA, newA);
			Assert.AreSame(b, firstA.FirstChild);
			Assert.AreSame(b, firstA.LastChild);
		}

		[Test]
		public void AssertNodeRemoved()
		{
			var tree = CreateTree();

			var b = tree.FindNode(Path.Combine("a", "b"));
			Assert.AreEqual(nameof(b), b.Segment);

			// b -> c
			var c = b.FirstChild;
			Assert.AreEqual(nameof(c), c.Segment);

			// Remove first
			var c2 = tree.RemoveNode(Path.Combine("a", "b", "c"));
			Assert.AreSame(c, c2);

			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "c")));

			// b -> d
			var d = b.FirstChild;
			Assert.AreNotSame(c, d);
			Assert.AreEqual(nameof(d), d.Segment);

			// b -> g
			var g = b.LastChild;
			Assert.AreEqual(nameof(g), g.Segment);

			// Remove last
			var g2 = tree.RemoveNode(Path.Combine("a", "b", "g"));
			Assert.AreSame(g, g2);

			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "g")));
			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "g", "g1")));
			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "g", "g2")));

			// b -> f
			var f = b.LastChild;
			Assert.AreEqual(nameof(f), f.Segment);

			// Remove middle
			var e = tree.FindNode(Path.Combine("a", "b", "e"));
			Assert.IsNotNull(e);

			var e2 = tree.RemoveNode(Path.Combine("a", "b", "e"));
			Assert.AreSame(e, e2);

			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "e")));

			Assert.AreSame(d, b.FirstChild);
			Assert.AreSame(f, b.LastChild);
			Assert.AreSame(f, d.Next);
		}
	}
}
