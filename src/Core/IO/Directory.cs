using System;
using System.Collections.Generic;
using System.Text;
using SIO = System.IO;
using Core;

namespace Core.IO
{
	public static class Directory
	{
		public static void CreateDirectory(string path)
		{
			SIO.Directory.CreateDirectory(ServerImpl.Instance.MapPath(path));
		}

		public static void Delete(string path)
		{
			FileSystemInfo[] infos = new DirectoryInfo(path).GetFileSystemInfos();

			foreach(FileSystemInfo info in infos)
			{
				FileAttributes attrs = File.GetAttributes(info.FullName);
				if ((attrs & FileAttributes.Directory) == FileAttributes.Directory)
				{
					Directory.Delete(info.FullName);
				}
				else
				{
					File.Delete(info.FullName);
				}
			}
			SIO.Directory.Delete(ServerImpl.Instance.MapPath(path));
		}

		public static bool Exists(string path)
		{
			return SIO.Directory.Exists(ServerImpl.Instance.MapPath(path));
		}

		static public void Rename(string path, string name)
		{
			string rpath = ServerImpl.Instance.MapPath(path);
			SIO.Directory.Move(rpath, SIO.Path.GetDirectoryName(rpath) + @"\" + name);
		}

		static public void Move(string src, string des)
		{
			if (!Directory.Exists(des)) Directory.CreateDirectory(des);

			foreach (FileSystemInfo fsi in new DirectoryInfo(src).GetFileSystemInfos())
			{
				FileAttributes attrs = fsi.Attributes;
				if ((attrs & FileAttributes.Directory) == FileAttributes.Directory)
				{
					Directory.Move(fsi.FullName, des + "/" + fsi.Name);
				}
				else
				{
					File.Move(fsi.FullName, des + "/" + fsi.Name);
				}
			}
			Directory.Delete(src);
		}

		public static void Copy(string src, string des)
		{
			if (!Directory.Exists(des)) Directory.CreateDirectory(des);

			foreach (FileSystemInfo fsi in new DirectoryInfo(src).GetFileSystemInfos())
			{
				FileAttributes attrs = fsi.Attributes;
				if ((attrs & FileAttributes.Directory) == FileAttributes.Directory)
				{
					Directory.Copy(fsi.FullName, des + "/" + fsi.Name);
				}
				else
				{
					File.Copy(fsi.FullName, des + "/" + fsi.Name);
				}
			}
		}
	}
}
