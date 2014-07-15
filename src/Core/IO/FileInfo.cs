using System;
using System.Collections.Generic;
using System.Text;
using SIO = System.IO;
using Core;

namespace Core.IO
{
	public class FileInfo : FileSystemInfo, IRenderJson
	{
		SIO.FileInfo _info = null;
		string _fullName;
		string _path;
		string _name;

		public FileInfo(string fullName)
		{
			_fullName = fullName;
			_name = Path.GetFileName(fullName);
			_path = ServerImpl.Instance.MapPath(fullName);
			_info = new SIO.FileInfo(_path);
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
		public long Length
		{
			get { return _info.Length; }
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

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(
				builder,
				"FullName", FullName,
				"Name", Name,
				"Type", "F",
				"Size", Length,
				"LastModifiedTime", LastWriteTime,
				"Extension", _info.Extension
			);
		}
	}
}
