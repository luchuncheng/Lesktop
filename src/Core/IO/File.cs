using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using SIO = System.IO;
using Core;

namespace Core.IO
{
	public static class File
	{

		public static void Delete(string path)
		{
			SIO.File.Delete(ServerImpl.Instance.MapPath(path));
		}

		public static bool Exists(string path)
		{
			return SIO.File.Exists(ServerImpl.Instance.MapPath(path));
		}

		public static FileAttributes GetAttributes(string path)
		{
			return (FileAttributes)SIO.File.GetAttributes(ServerImpl.Instance.MapPath(path));
		}

		public static void SetAttributes(string path,FileAttributes attrs)
		{
			SIO.File.SetAttributes(ServerImpl.Instance.MapPath(path), (SIO.FileAttributes)attrs);
		}

		public static SIO.Stream Create(string path)
		{
			return SIO.File.Create(ServerImpl.Instance.MapPath(path));
		}

		public static SIO.Stream Open(string path, SIO.FileMode mode,SIO.FileAccess access,SIO.FileShare share)
		{
			return SIO.File.Open(ServerImpl.Instance.MapPath(path), mode, access, share);
		}

		static public void Rename(string path, string name)
		{
			string rpath = ServerImpl.Instance.MapPath(path);
			SIO.File.Move(rpath, SIO.Path.GetDirectoryName(rpath) + @"\" + name);
		}

		static public void Move(string src, string des)
		{
			SIO.File.Move(ServerImpl.Instance.MapPath(src), ServerImpl.Instance.MapPath(des));
		}

		static public void Copy(string src, string des)
		{
			SIO.File.Copy(ServerImpl.Instance.MapPath(src), ServerImpl.Instance.MapPath(des));
		}
	}
}
