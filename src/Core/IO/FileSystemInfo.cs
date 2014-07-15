using System;
using System.Collections.Generic;
using System.Text;
using SIO = System.IO;
using Core;

namespace Core.IO
{
	public abstract class FileSystemInfo
	{
		public abstract DateTime CreationTime { get; }
		public abstract DateTime CreationTimeUtc { get; }
		public abstract DateTime LastAccessTime { get; }
		public abstract DateTime LastAccessTimeUtc { get; }
		public abstract DateTime LastWriteTime { get; }
		public abstract DateTime LastWriteTimeUtc { get; }
		public abstract FileAttributes Attributes { get; }
		public abstract string Name { get; }
		public abstract string FullName { get; }
	}

	public enum FileAttributes
	{
		ReadOnly = SIO.FileAttributes.ReadOnly,						//文件为只读。 
		Hidden = SIO.FileAttributes.Hidden,							//文件是隐藏的，因此没有包括在普通的目录列表中。 
		System = SIO.FileAttributes.System, 						//文件为系统文件。文件是操作系统的一部分或由操作系统以独占方式使用。 
		Directory = SIO.FileAttributes.Directory,					//文件为一个目录。 
		Archive = SIO.FileAttributes.Archive, 						//文件的存档状态。应用程序使用此属性为文件加上备份或移除标记。 
		Device = SIO.FileAttributes.Device, 						//保留供将来使用。 
		Normal = SIO.FileAttributes.Normal, 						//文件正常，没有设置其他的属性。此属性仅在单独使用时有效。 
		Temporary = SIO.FileAttributes.Temporary, 					//文件是临时文件。文件系统试图将所有数据保留在内存中以便更快地访问，而不是将数据刷新回大容量存储器中。不再需要临时文件时，应用程序会立即将其删除。 
		SparseFile = SIO.FileAttributes.SparseFile, 				//文件为稀疏文件。稀疏文件一般是数据通常为零的大文件。 
		ReparsePoint = SIO.FileAttributes.ReparsePoint, 			//文件包含一个重新分析点，它是一个与文件或目录关联的用户定义的数据块。 
		Compressed = SIO.FileAttributes.Compressed, 				//文件已压缩。 
		Offline = SIO.FileAttributes.Offline, 						//文件已脱机。文件数据不能立即供使用。 
		NotContentIndexed = SIO.FileAttributes.NotContentIndexed, 	//操作系统的内容索引服务不会创建此文件的索引。 
		Encrypted = SIO.FileAttributes.Encrypted					//该文件或目录是加密的。对于文件来说，表示文件中的所有数据都是加密的。对于目录来说，表示新创建的文件和目录在默认情况下是加密的
	}
}
