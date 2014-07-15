using System;
using System.Collections.Generic;
using System.Text;
using Core;

namespace Core.IO
{
	public static class Path
	{
		public static int GetUser(string path)
		{
			if (path.StartsWith("/"))
			{
				int index = path.IndexOf('/', 1);
				return Convert.ToInt32(path.Substring(1, index == -1 ? path.Length - 1 : index - 1));
			}
			else
			{
				return 0;
			}
		}

		public static string GetRelativePath(string path)
		{
			if (path.StartsWith("/"))
			{
				int index = path.IndexOf('/', 1);
				return index == -1 ? "" : path.Substring(index + 1);
			}
			else
			{
				return path;
			}
		}

		public static string GetDirectoryName(string path)
		{
			int index = path.LastIndexOf('/');
			return index <= 0 ? "" : path.Substring(0, index);
		}

		public static string GetFileName(string path)
		{
			int index = path.LastIndexOf('/');
			return index == 0 ? "" : path.Substring(index + 1);
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			string fileName = GetFileName(path);
			int index = fileName.LastIndexOf('.');
			return index == -1 ? fileName : fileName.Substring(0, index);
		}

		public static string GetExtension(string path)
		{
			string fileName = GetFileName(path);
			int index = fileName.LastIndexOf('.');
			return index == -1 ? "." : fileName.Substring(index);
		}

		public static string Join(string split, params string[] pns)
		{
			StringBuilder builder = new StringBuilder();
			foreach(string n in pns)
			{
				string s = n.Trim();
				if (!string.IsNullOrEmpty(s) && IsValidNode(s))
				{
					if (builder.Length > 0) builder.Append(split);
					builder.Append(s);
				}
			}
			return builder.ToString();
		}

		public static string Join(string split, string[] pns, int index)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = index; i < pns.Length; i++)
			{
				string n = pns[i].Trim();
				if (!string.IsNullOrEmpty(n) && IsValidNode(n))
				{
					if (builder.Length > 0) builder.Append(split);
					builder.Append(n);
				}
			}
			return builder.ToString();
		}

		private static bool IsValidNode(string n)
		{
			foreach (char c in n)
			{
				if (c != '.') return true;
			}
			return false;
		}

		public static string Format(string path, char split)
		{
			string[] ns = path.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder builder = new StringBuilder();
			if (path[0] == split) builder.Append(split);
			foreach(string n in ns)
			{
				string s = n.Trim();
				if (!string.IsNullOrEmpty(s) && IsValidNode(s))
				{
					if (builder.Length > 0) builder.Append(split);
					builder.Append(s);
				}
			}
			return builder.ToString();
		}

		public static bool IsEqual(string p1, string p2)
		{
			return String.Compare(p1, p2, true) == 0;
		}

		public static bool IsParent(string parent,string child)
		{
			return string.IsNullOrEmpty(parent) || parent.Length < child.Length && 
				child.StartsWith(parent, StringComparison.OrdinalIgnoreCase) && 
				child[parent.Length] == '/';
		}

		public static string GetRelativePath(string parent, string child)
		{
			if (string.IsNullOrEmpty(parent)) return child;
			else if (parent.Length == child.Length) return "";
			else return child.Substring(parent.Length + 1);
		}

		public static string GetRoot(string relativePath)
		{
			int index = relativePath.IndexOf("/");
			return index == -1 ? relativePath : relativePath.Substring(0, index);
		}
	}
}
