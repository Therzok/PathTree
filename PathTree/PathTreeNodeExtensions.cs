using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PathTree
{
	static class PathTreeNodeExtensions
	{
		[Conditional("DEBUG")]
		public static void Dump(this PathTreeNode rootNode)
		{
			var firstStack = new List<PathTreeNode> { rootNode };
			var childListStack = new List<List<PathTreeNode>> { firstStack };

			while (childListStack.Count > 0)
			{
				var childStack = childListStack[childListStack.Count - 1];

				if (childStack.Count == 0)
				{
					childListStack.RemoveAt(childListStack.Count - 1);
				}
				else
				{
					var tree = childStack[0];
					childStack.RemoveAt(0);

					string indent = "";
					for (int i = 0; i < childListStack.Count - 1; i++)
					{
						indent += (childListStack[i].Count > 0) ? "|  " : "   ";
					}

					Console.WriteLine(indent + "+- " + tree.Segment);

					if (tree.FirstChild != null)
					{
						var treeChildren = new List<PathTreeNode>();
						var child = tree.FirstChild;
						while (child != null)
						{
							treeChildren.Add(child);
							child = child.Next;
						}
						childListStack.Add(treeChildren);
					}
				}
			}
		}
	}
}
