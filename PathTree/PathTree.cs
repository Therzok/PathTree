//
// PathTree.cs
//
// Author:
//       Marius Ungureanu <maungu@microsoft.com>
//
// Copyright (c) 2018 Microsoft Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PathTree
{
	public class PathTree
	{
		internal readonly PathTreeNode rootNode = new PathTreeNode("");

		public PathTreeNode FindNode (string path)
		{
			TryFind(path, out var result, out _, out _, out _, out _);
			return result;
		}

		public IEnumerable<PathTreeNode> Normalize (int maxLeafs)
		{
			Queue<PathTreeNode> queue = new Queue<PathTreeNode>();

			int yielded = 0;
			var child = rootNode.FirstChild;
			while (child != null)
			{
				if (child.IsLive)
				{
					yielded++;
					yield return child;
				} else
					queue.Enqueue(child);

				child = child.Next;
			}
			if (queue.Count == 0)
				yield break;

			while (yielded <= maxLeafs && queue.Count != 0)
			{
				var node = queue.Dequeue();

				if (node.ChildrenCount + yielded - 1 < maxLeafs)
				{
					child = node.FirstChild;
					while (child != null)
					{
						if (child.IsLive)
						{
							yielded++;
							yield return child;
						}
						else
							queue.Enqueue(child);
						child = child.Next;
					}
				}
				else
				{
					yielded++;
					yield return node;
				}
			}
		}

		bool TryFind (string path, out PathTreeNode result, out PathTreeNode parent, out PathTreeNode previousNode, out string[] pathSegments, out int currentIndex)
		{
			currentIndex = 0;
			pathSegments = ToData(path);

			parent = rootNode;
			var currentNode = parent.FirstChild;
			previousNode = null;

			while (currentNode != null)
			{
				int comparisonResult = string.Compare(currentNode.Segment, pathSegments[currentIndex]);

				// We need to insert in this node's position.
				if (comparisonResult > 0)
					break;

				// Keep searching.
				if (comparisonResult < 0)
				{
					previousNode = currentNode;
					currentNode = currentNode.Next;
					continue;
				}

				// We found this segment in the tree.
				currentIndex++;

				// We found the node already, register the ID.
				if (currentIndex == pathSegments.Length)
				{
					result = currentNode;
					return true;
				}

				// We go to the first child of this segment and repeat the algorithm.
				parent = currentNode;
				previousNode = null;
				currentNode = parent.FirstChild;

			}
			result = null;
			return false;
		}

		public PathTreeNode AddNode (string path, object id)
		{
			if (TryFind(path, out var result, out var parent, out var previousNode, out var pathSegments, out var currentIndex))
			{
				result.RegisterId(id);
				return result;
			}

			// At this point, we need to create a new node.
			var (first, leaf) = PathTreeNode.CreateSubTree(pathSegments, currentIndex);
			if (id != null)
				leaf.RegisterId(id);

			InsertNode(first, parent, previousNode);

			return leaf;
		}

		public PathTreeNode RemoveNode (string path, object id)
		{
			if (TryFind(path, out var result, out var parent, out var previousNode, out var pathSegments, out var currentIndex)) {
				if (result.UnregisterId(id) && !result.IsLive)
				{
					if (parent.FirstChild == result)
						parent.FirstChild = result.Next;
					if (parent.LastChild == result)
						parent.LastChild = previousNode;
					parent.ChildrenCount -= 1;

					if (previousNode != null)
						previousNode.Next = result.Next;

					result.Next = null;
				}
			}
			return result;
		}

			// TODO: Optimize this to not allocate, but reuse a string and indices.
		string[] ToData(string path) => string.IsNullOrEmpty(path) ? Array.Empty<string>() : path.Split(Path.DirectorySeparatorChar);

		void InsertNode(PathTreeNode node, PathTreeNode parentNode, PathTreeNode previousNode)
		{
			parentNode.ChildrenCount += 1;
			if (previousNode == null)
			{
				// We're inserting at the beginning.
				node.Next = parentNode.FirstChild;
				parentNode.FirstChild = node;

				// We didn't have a child node, set it.
				if (node.Next == null)
					parentNode.LastChild = node;
				return;
			}

			// We are appending inbetween other nodes
			var next = previousNode.Next;
			previousNode.Next = node;
			node.Next = next;

			if (parentNode.LastChild == previousNode)
				parentNode.LastChild = node;
		}
	}
}
