using System;
using System.Collections.Generic;
using System.Text;
using SIO = System.IO;
using Core;

namespace Core.IO
{
	public class DirectoryInfo : FileSystemInfo, IRenderJson
	{
		SIO.DirectoryInfo _info = null;
		string _fullName;
		string _path;
		string _name;

		public DirectoryInfo(string fullName)
		{
			_fullName = fullName;
			_name = Path.GetFileName(fullName);
			_path = ServerImpl.Instance.MapPath(fullName);
			_info = new SIO.DirectoryInfo(_path);
		}

		public override string Name
		{
			get { return _name; }
		}

		public override string FullName
		{
			get
			{
				return _fullName;
			}
		}

		public override FileAttributes Attributes
		{
			get { return (FileAttributes)_info.Attributes; }
		}

		public override DateTime CreationTimeUtc
		{
			get { return _info.CreationTimeUtc; }
		}

		public override DateTime CreationTime
		{
			get { return _info.CreationTime; }
		}

		public override DateTime LastAccessTime
		{
			get { return _info.LastAccessTime; }
		}

		public override DateTime LastAccessTimeUtc
		{
			get { return _info.LastAccessTimeUtc; }
		}

		public override DateTime LastWriteTime
		{
			get { return _info.LastWriteTime; }
		}

		public override DateTime LastWriteTimeUtc
		{
			get { return _info.LastWriteTimeUtc; }
		}

		public FileSystemInfo[] GetFileSystemInfos()
		{
			List<FileSystemInfo> fss = new List<FileSystemInfo>();
			foreach (SIO.DirectoryInfo dir in _info.GetDirectories())
			{
				fss.Add(new DirectoryInfo(FullName + "/" + dir.Name));
			}
			foreach (string sub_dir in PathMap.Instance.GetSubDirectories(FullName))
			{
				fss.Add(new DirectoryInfo(FullName + "/" + sub_dir));
			}
			if (Path.GetRelativePath(FullName) == "")
			{
				fss.Add(new DirectoryInfo(FullName + "/Public"));
			}
			foreach (SIO.FileInfo file in _info.GetFiles())
			{
				fss.Add(new FileInfo(FullName + "/" + file.Name));
			}
			return fss.ToArray();
		}

		public FileSystemInfo[] GetDirectoryInfos()
		{
			List<FileSystemInfo> fss = new List<FileSystemInfo>();

			foreach (SIO.DirectoryInfo dir in _info.GetDirectories())
			{
				fss.Add(new DirectoryInfo(FullName + "/" + dir.Name));
			}
			if (Path.GetRelativePath(FullName) == "")
			{
				fss.Add(new DirectoryInfo(FullName + "/Public"));
			}
			return fss.ToArray();
		}

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(
				builder,
				"FullName", FullName,
				"Name", Name,
				"Type", "D",
				"LastModifiedTime", LastWriteTime
			);
		}
	}
}
