using System;
namespace PathTree
{
	class Platform
	{
		public static bool IsWindows = System.IO.Path.DirectorySeparatorChar == '\\';
	}
}
