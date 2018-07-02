using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace PathTree.Tests
{
	[TestFixture]
	public class PathTreeTests
	{
		static object id = new object();

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
			tree.AddNode(Path.Combine("a", "b", "g", "g1"), id);
			tree.AddNode(Path.Combine("a", "b", "g"), id);
			tree.AddNode(Path.Combine("a", "b", "c"), id);
			tree.AddNode(Path.Combine("a", "b", "e"), id);
			tree.AddNode(Path.Combine("a", "b", "d"), id);
			tree.AddNode(Path.Combine("a", "b", "f"), id);
			tree.AddNode(Path.Combine("a", "b", "f", "f1"), id);
			tree.AddNode(Path.Combine("a", "b", "f", "f2"), id);
			tree.AddNode(Path.Combine("a", "b", "g", "g2"), id);

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
			Assert.AreEqual(1, root.ChildrenCount);

			// a -> b
			Assert.AreEqual(nameof(b), b.Segment);
			Assert.AreSame(a.LastChild, b);
			Assert.IsNull(b.Next);
			Assert.AreEqual(1, a.ChildrenCount);

			// b -> c, d, e, f, g
			Assert.AreEqual(nameof(c), c.Segment);
			Assert.AreEqual(nameof(d), d.Segment);
			Assert.AreEqual(nameof(e), e.Segment);
			Assert.AreEqual(nameof(f), f.Segment);
			Assert.AreEqual(nameof(g), g.Segment);
			Assert.AreEqual(5, b.ChildrenCount);

			Assert.AreSame(b.LastChild, g);

			// c, d, e
			Assert.IsNull(c.FirstChild);
			Assert.AreEqual(0, c.ChildrenCount);
			Assert.IsNull(d.FirstChild);
			Assert.AreEqual(0, d.ChildrenCount);
			Assert.IsNull(e.FirstChild);
			Assert.AreEqual(0, e.ChildrenCount);
			Assert.IsNull(g.Next);

			// f -> f1, f2
			Assert.AreSame(f2, f.LastChild);
			Assert.AreEqual(nameof(f1), f1.Segment);
			Assert.AreEqual(nameof(f2), f2.Segment);
			Assert.AreEqual(2, f.ChildrenCount);
			Assert.AreEqual(0, f1.ChildrenCount);
			Assert.AreEqual(0, f2.ChildrenCount);
			Assert.IsNull(f2.Next);

			// g -> g1, g2
			Assert.AreEqual(nameof(g1), g1.Segment);
			Assert.AreEqual(nameof(g2), g2.Segment);
			Assert.AreSame(g2, g.LastChild);
			Assert.AreEqual(2, g.ChildrenCount);
			Assert.AreEqual(0, g1.ChildrenCount);
			Assert.AreEqual(0, g2.ChildrenCount);
			Assert.IsNull(g2.Next);

			// a
			// ...
			// z
			// + y
			//   + x

			tree.AddNode(Path.Combine("z", "y", "x"), id);

			z = a.Next;
			y = z.FirstChild;
			x = y.FirstChild;

			// root -> z
			Assert.AreEqual(nameof(z), z.Segment);
			Assert.AreSame(z, root.LastChild);
			Assert.AreSame(a.Next, z);
			Assert.AreEqual(2, root.ChildrenCount);
			Assert.IsNull(z.Next);

			// z -> y
			Assert.AreEqual(nameof(z), z.Segment);
			Assert.AreSame(y, z.LastChild);
			Assert.AreEqual(1, z.ChildrenCount);
			Assert.IsNull(y.Next);

			// y -> x
			Assert.AreEqual(nameof(x), x.Segment);
			Assert.AreEqual(1, y.ChildrenCount);
			Assert.IsNull(x.FirstChild);
			Assert.IsNull(x.LastChild);
			Assert.IsNull(x.Next);
		}

		[Test]
		public void AssertSameNodeIsReturned()
		{
			var tree = new PathTree();

			var b = tree.AddNode(Path.Combine ("a", "b"), id);

			var firstA = tree.FindNode("a");
			var newA = tree.AddNode("a", id);

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
			var c2 = tree.RemoveNode(Path.Combine("a", "b", "c"), id);
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
			var g2 = tree.RemoveNode(Path.Combine("a", "b", "g"), id);
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

			var e2 = tree.RemoveNode(Path.Combine("a", "b", "e"), id);
			Assert.AreSame(e, e2);

			Assert.IsNull(tree.FindNode(Path.Combine("a", "b", "e")));

			Assert.AreSame(d, b.FirstChild);
			Assert.AreSame(f, b.LastChild);
			Assert.AreSame(f, d.Next);
		}

		[Test]
		public void AssertNodeNotRemovedWithMultipleRegistrations()
		{
			var tree = CreateTree();

			var b = tree.FindNode(Path.Combine("a", "b"));
			Assert.AreEqual(nameof(b), b.Segment);

			var newId = new object();

			var b2 = tree.AddNode(Path.Combine("a", "b"), newId);
			Assert.AreSame(b, b2);
			Assert.IsNotNull(b2);

			var b3 = tree.RemoveNode(Path.Combine("a", "b"), id);
			Assert.AreSame(b2, b3);
			Assert.IsNotNull(b3);

			var b4 = tree.FindNode(Path.Combine("a", "b"));
			Assert.AreSame(b3, b4);
			Assert.IsNotNull(b4);

			var b5 = tree.RemoveNode(Path.Combine("a", "b"), newId);
			Assert.AreSame(b4, b5);
			Assert.IsNotNull(b5);

			var bRemoved = tree.FindNode(Path.Combine("a", "b"));
			Assert.IsNull(bRemoved);
		}

		[Test]
		public void Normalize()
		{
			var tree = CreateTree();

			var nodes = tree.Normalize(1).ToArray();
			Assert.AreEqual(1, nodes.Length);
 			Assert.AreEqual("b", nodes[0].Segment);

			nodes = tree.Normalize(2).ToArray();
			Assert.AreEqual(1, nodes.Length);
			Assert.AreEqual("b", nodes[0].Segment);

			nodes = tree.Normalize(3).ToArray();
			Assert.AreEqual(1, nodes.Length);
			Assert.AreEqual("b", nodes[0].Segment);

			nodes = tree.Normalize(4).ToArray();
			Assert.AreEqual(1, nodes.Length);
			Assert.AreEqual("b", nodes[0].Segment);

			// b has 5 children
			nodes = tree.Normalize(5).ToArray();
			Assert.AreEqual(5, nodes.Length);
			Assert.AreEqual("c", nodes[0].Segment);
			Assert.AreEqual("d", nodes[1].Segment);
			Assert.AreEqual("e", nodes[2].Segment);
			Assert.AreEqual("f", nodes[3].Segment);
			Assert.AreEqual("g", nodes[4].Segment);

			// f has 2 children, but it is live
			nodes = tree.Normalize(6).ToArray();
			Assert.AreEqual(5, nodes.Length);
			Assert.AreEqual("c", nodes[0].Segment);
			Assert.AreEqual("d", nodes[1].Segment);
			Assert.AreEqual("e", nodes[2].Segment);
			Assert.AreEqual("f", nodes[3].Segment);
			Assert.AreEqual("g", nodes[4].Segment);

			// remove f's registration
			var node = tree.FindNode(Path.Combine("a", "b", "f"));
			node.UnregisterId(id);

			// f has 2 children which should be unrolled
			nodes = tree.Normalize(6).ToArray();
			Assert.AreEqual(6, nodes.Length);
			Assert.AreEqual("c", nodes[0].Segment);
			Assert.AreEqual("d", nodes[1].Segment);
			Assert.AreEqual("e", nodes[2].Segment);
			Assert.AreEqual("g", nodes[3].Segment);
			Assert.AreEqual("f1", nodes[4].Segment);
			Assert.AreEqual("f2", nodes[5].Segment);

			// g has 2 children, but it is live
			nodes = tree.Normalize(7).ToArray();
			Assert.AreEqual(6, nodes.Length);
			Assert.AreEqual("c", nodes[0].Segment);
			Assert.AreEqual("d", nodes[1].Segment);
			Assert.AreEqual("e", nodes[2].Segment);
			Assert.AreEqual("g", nodes[3].Segment);
			Assert.AreEqual("f1", nodes[4].Segment);
			Assert.AreEqual("f2", nodes[5].Segment);

			// remove f's registration
			node = tree.FindNode(Path.Combine("a", "b", "g"));
			node.UnregisterId(id);

			nodes = tree.Normalize(7).ToArray();
			Assert.AreEqual(7, nodes.Length);
			Assert.AreEqual("c", nodes[0].Segment);
			Assert.AreEqual("d", nodes[1].Segment);
			Assert.AreEqual("e", nodes[2].Segment);
			Assert.AreEqual("f1", nodes[3].Segment);
			Assert.AreEqual("f2", nodes[4].Segment);
			Assert.AreEqual("g1", nodes[5].Segment);
			Assert.AreEqual("g2", nodes[6].Segment);

			node = tree.FindNode("a");
			node.RegisterId(id);

			nodes = tree.Normalize(1).ToArray();
			Assert.AreEqual(1, nodes.Length);
			Assert.AreEqual("a", nodes[0].Segment);

			nodes = tree.Normalize(7).ToArray();
			Assert.AreEqual(1, nodes.Length);
			Assert.AreEqual("a", nodes[0].Segment);
		}
	}
}
