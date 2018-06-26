using System;
using System.Collections.Generic;

namespace PathTree
{
	public class PathTreeNode
	{
		public PathTreeNode FirstChild { get; set; }
		public PathTreeNode LastChild { get; set; }
		public PathTreeNode Next { get; set; }

		//readonly List<object> ids = new List<object>();

		public string Segment { get; }

		public PathTreeNode(string segment)
		{
			Segment = segment;
		}

		internal static (PathTreeNode root, PathTreeNode leaf) CreateSubTree (string[] pathSegments, int startIndex)
		{
			PathTreeNode lastNode = null, rootNode = null;
			for (int i = startIndex; i < pathSegments.Length; ++i)
			{
				var node = new PathTreeNode(pathSegments[i]);

				if (lastNode != null)
					lastNode.FirstChild = lastNode.LastChild = node;
				else
					rootNode = node;

				lastNode = node;
			}
			return (rootNode, lastNode);
		}
	}
}
