using System;
using System.Collections.Generic;

namespace PathTree
{
	public sealed class PathTreeNode
	{
		public PathTreeNode FirstChild { get; set; }
		public PathTreeNode LastChild { get; set; }
		public PathTreeNode Next { get; set; }
		public int ChildrenCount { get; set; }

		readonly List<object> ids = new List<object>();
		internal void RegisterId(object id) => ids.Add(id);
		internal bool UnregisterId(object id) => ids.Remove(id);
		public bool IsLive => ids.Count != 0;

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
				var node = new PathTreeNode (pathSegments[i]);

				if (lastNode != null) {
					lastNode.FirstChild = lastNode.LastChild = node;
					lastNode.ChildrenCount = 1;
				} else
					rootNode = node;

				lastNode = node;
			}
			return (rootNode, lastNode);
		}
	}
}
