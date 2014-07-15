using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Diagnostics;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Security.Cryptography;
using System.Text;
using System.Data.OleDb;

public partial class _lesktop_install : System.Web.UI.Page
{
	string _basePath = "";
	string _connectionString = "";
	string _host = "";
	string _version = "";
	string _fileRoot = "";

	protected void Page_Load(object sender, EventArgs e)
	{
		_basePath = Server.MapPath("~");

		_host = Request.Url.Host;
		if (Request.Url.Port != 80) _host += ":" + Request.Url.Port.ToString();
		if (Request.ApplicationPath != "/") _host += "/" + Request.ApplicationPath;

		while (_basePath.EndsWith("\\")) _basePath = _basePath.Substring(0, _basePath.Length - 1);

		_version = FileVersionInfo.GetVersionInfo(_basePath + @"/Bin/Core.dll").FileVersion.ToString();

		PlaceHolder1.Visible = File.Exists(_basePath + @"\App_Data\ip.mdb");
		cbImportIP.Checked = File.Exists(_basePath + @"\App_Data\ip.mdb");
	}

	public static void Compress(string[] paths, string target, int level)
	{
		using (ZipOutputStream outputStream = new ZipOutputStream(File.Create(target)))
		{
			try
			{
				outputStream.SetLevel(level);
				foreach (string path in paths)
				{
					Compress(path, outputStream);
				}
			}
			finally
			{
				outputStream.Finish();
				outputStream.Close();
			}
		}
	}

	public static void Compress(string path, string target, int level)
	{
		using (ZipOutputStream outputStream = new ZipOutputStream(File.Create(target)))
		{
			try
			{
				outputStream.SetLevel(level);
				Compress(path, outputStream);
			}
			finally
			{
				outputStream.Finish();
				outputStream.Close();
			}
		}
	}

	public static void Compress(string path, ZipOutputStream output)
	{
		Compress(path, output, "");
	}

	public static string MD5(string str)
	{
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		byte[] data = md5.ComputeHash(UTF8Encoding.Default.GetBytes(str));
		StringBuilder sBuilder = new StringBuilder();
		for (int i = 0; i < data.Length; i++)
		{
			sBuilder.Append(data[i].ToString("X2"));
		}
		return sBuilder.ToString();
	}

	public static void Compress(string path, ZipOutputStream output, string relativePath)
	{
		if (!string.IsNullOrEmpty(relativePath) && !relativePath.EndsWith("\\"))
		{
			relativePath += "\\";
		}

		if (Directory.Exists(path))
		{
			FileSystemInfo[] fsis = new DirectoryInfo(path).GetFileSystemInfos();
			ZipEntry entry = new ZipEntry(relativePath + Path.GetFileName(path) + "/");
			entry.DateTime = DateTime.Now;
			output.PutNextEntry(entry);
			foreach (FileSystemInfo fsi in fsis)
			{
				Compress(path + "\\" + fsi.Name, output, relativePath + Path.GetFileName(path));
			}
		}
		else
		{
			Crc32 crc = new Crc32();
			//打开压缩文件
			Stream fs = File.Open(path, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			ZipEntry entry = new ZipEntry(relativePath + Path.GetFileName(path));
			entry.DateTime = DateTime.Now;
			fs.Close();
			crc.Reset();
			crc.Update(buffer);
			entry.Crc = crc.Value;
			output.PutNextEntry(entry);
			output.Write(buffer, 0, buffer.Length);
		}
	}

	public static void CopyDirectory(string src, string target, bool overwrite)
	{
		DirectoryInfo di = new DirectoryInfo(src);
		Directory.CreateDirectory(target);
		foreach (FileSystemInfo fs in di.GetFileSystemInfos())
		{
			if ((fs.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				Directory.CreateDirectory(target + "\\" + fs.Name);
				CopyDirectory(src + "\\" + fs.Name, target + "\\" + fs.Name, overwrite);
			}
			else
			{
				File.Copy(src + "\\" + fs.Name, target + "\\" + fs.Name, overwrite);
			}
		}
	}

	public static void CopyDirectory(string src, string target)
	{
		CopyDirectory(src, target, false);
	}

	public static void DeleteDirectory(string dir)
	{
		if (Directory.Exists(dir))
		{
			DirectoryInfo di = new DirectoryInfo(dir);
			foreach (FileSystemInfo fs in di.GetFileSystemInfos())
			{
				if ((fs.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
				{
					if ((fs.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
					{
						File.SetAttributes(fs.FullName, FileAttributes.Normal);
						DeleteDirectory(fs.FullName);
					}
					else
					{
						File.SetAttributes(fs.FullName, FileAttributes.Normal);
						File.Delete(fs.FullName);
					}
				}
			}
			Directory.Delete(dir);
		}
	}

	void CreateDatabase(string connectionString)
	{
		String pwd = Custom.CustomServerImpl.EncryptPassword(tbAdminPwd.Text);

		SqlConnection conn = new SqlConnection(connectionString);
		conn.Open();
		try
		{
			SqlTransaction tran = conn.BeginTransaction();
			try
			{
				string text = File.ReadAllText(_basePath + @"\App_Data\Db.sql", Encoding.GetEncoding(936));
				string[] sqls = text.Split(new string[] { "GO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string sql in sqls)
				{
					SqlCommand cmd = new SqlCommand();
					cmd.Connection = conn;
					cmd.CommandText = sql;
					cmd.Transaction = tran;
					cmd.ExecuteNonQuery();
				}

				string dataSql = "UPDATE Users SET Password = @AdminPwd WHERE [ID] = 3";
				SqlCommand dataCmd = new SqlCommand();
				dataCmd.Connection = conn;
				dataCmd.CommandText = dataSql;
				dataCmd.Parameters.Add("AdminPwd", SqlDbType.NVarChar).Value = pwd;
				dataCmd.Transaction = tran;
				dataCmd.ExecuteNonQuery();

				tran.Commit();
			}
			catch
			{
				tran.Rollback();
				throw;
			}
		}
		finally
		{
			conn.Close();
		}

		XmlDocument webConfigXml = new XmlDocument();
		webConfigXml.Load(_basePath + @"\web.config");
		XmlElement connectionStringsElem = webConfigXml.DocumentElement.GetElementsByTagName("connectionStrings")[0] as XmlElement;
		foreach (XmlElement addElem in connectionStringsElem.GetElementsByTagName("add"))
		{
			if (addElem.GetAttribute("name") == "IMDB")
			{
				addElem.SetAttribute("connectionString", connectionString);
			}
		}
		webConfigXml.Save(_basePath + @"\web.config");
	}

	void RenderFiles()
	{
		if (_fileRoot.ToLower() != "app_data")
		{
			string absPath = "";
			if (!Path.IsPathRooted(_fileRoot))
			{
				DirectoryInfo info = new DirectoryInfo(_basePath + @"\" + _fileRoot);
				absPath = info.FullName;
			}
			else
			{
				absPath = _fileRoot;
			}
			CopyDirectory(_basePath + @"\App_Data", absPath, true);
		}

		XmlDocument webConfigXml = new XmlDocument();
		webConfigXml.Load(_basePath + @"\web.config");
		XmlElement appSettingsElem = webConfigXml.DocumentElement.GetElementsByTagName("appSettings")[0] as XmlElement;
		foreach (XmlElement addElem in appSettingsElem.GetElementsByTagName("add"))
		{
			if (addElem.GetAttribute("key") == "FileRoot")
			{
				addElem.SetAttribute("value", _fileRoot);
			}
		}
		webConfigXml.Save(_basePath + @"\web.config");
	}

	byte[] CreateSettingConf()
	{
		string setting = String.Format(
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
			"<Config>\r\n" +
			"    <ServiceUrl>http://{0}</ServiceUrl>\r\n" +
			"    <AppPath>/</AppPath>\r\n" +
			"    <ResPath>{1}</ResPath>\r\n" +
			"</Config>",
			_host, _version
		);

		return Encoding.UTF8.GetBytes(setting);
	}

	void CreateUpdateXml()
	{
		string updateXml = String.Format(
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
			"<Update \r\n" +
			"    Latest=\"{1}\" \r\n" +
			"    URL=\"http://{0}/Update/UPDATE-{1}.zip\"\r\n" +
			">\r\n" +
			"</Update>\r\n",
			_host, _version
		);

		File.WriteAllText(_basePath + @"\Update\latest.xml", updateXml);
	}

	void ImportIP()
	{
		string ip_path = _basePath + @"\App_Data\ip.mdb";
		if (File.Exists(ip_path))
		{
			String connectionString = String.Format(
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", 
				ip_path
			); 
			OleDbConnection conn = new OleDbConnection(connectionString);
			OleDbCommand cmd = new OleDbCommand("SELECT * FROM IP", conn);
			conn.Open();
			try
			{
				OleDbDataReader reader = cmd.ExecuteReader();

				SqlConnection importConn = new SqlConnection(_connectionString);
				SqlCommand importCmd = new SqlCommand("INSERT INTO IP (IP1,IP2,IP3,IP4) VALUES (@IP1,@IP2,@IP3,@IP4)", importConn);
				importCmd.Parameters.Add("IP1", SqlDbType.Float);
				importCmd.Parameters.Add("IP2", SqlDbType.Float);
				importCmd.Parameters.Add("IP3", SqlDbType.NVarChar);
				importCmd.Parameters.Add("IP4", SqlDbType.NVarChar);
				importConn.Open();
				try
				{
					SqlTransaction tran = importConn.BeginTransaction();
					try
					{
						while (reader.Read())
						{
							importCmd.Parameters["IP1"].Value = reader["IP1"];
							importCmd.Parameters["IP2"].Value = reader["IP2"];
							importCmd.Parameters["IP3"].Value = reader["IP3"];
							importCmd.Parameters["IP4"].Value = reader["IP4"];
							importCmd.Transaction = tran;
							importCmd.ExecuteNonQuery();
						}
						tran.Commit();
					}
					catch
					{
						tran.Rollback();
					}
				}
				finally
				{
					importConn.Close();
				}
			}
			finally
			{
				conn.Close();
			}
		}
	}

	void UpdateSettingConf(string path)
	{
		byte[] setting_buf = CreateSettingConf();

		using (ZipInputStream inputStream = new ZipInputStream(File.Open(path, FileMode.Open)))
		{
			using (ZipOutputStream outputStream = new ZipOutputStream(File.Create(path + ".temp")))
			{
				ZipEntry entry;
				try
				{
					while ((entry = inputStream.GetNextEntry()) != null)
					{
						outputStream.SetLevel(6);
						if (entry.Name.IndexOf("Setting.conf", StringComparison.CurrentCultureIgnoreCase) >= 0)
						{
							Crc32 crc = new Crc32();
							//打开压缩文件
							ZipEntry new_entry = new ZipEntry(entry.Name);
							new_entry.DateTime = DateTime.Now;
							crc.Reset();
							crc.Update(setting_buf);
							entry.Crc = crc.Value;
							outputStream.PutNextEntry(new_entry);
							outputStream.Write(setting_buf, 0, setting_buf.Length);
						}
						else
						{
							if (entry.IsDirectory)
							{
								ZipEntry new_entry = new ZipEntry(entry.Name);
								new_entry.DateTime = DateTime.Now;
								outputStream.PutNextEntry(entry);
							}
							else
							{
								byte[] buffer = new byte[entry.Size];
								inputStream.Read(buffer, 0, buffer.Length);

								Crc32 crc = new Crc32();
								//打开压缩文件
								ZipEntry new_entry = new ZipEntry(entry.Name);
								new_entry.DateTime = DateTime.Now;
								crc.Reset();
								crc.Update(buffer);
								entry.Crc = crc.Value;
								outputStream.PutNextEntry(new_entry);
								outputStream.Write(buffer, 0, buffer.Length);
							}
						}
					}
				}
				finally
				{
					outputStream.Finish();
					outputStream.Close();
				}
			}
		}

		File.Copy(path + ".temp", path, true);
		File.Delete(path + ".temp");
	}

	protected void btnInstall_Click(object sender, EventArgs e)
	{
		if (tbAdminPwd.Text != tbAdminPwdConfirm.Text)
		{
			throw new Exception("两次输入的密码不一致！");
		}

		_connectionString = String.Format(
			"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", 
			tbDbServer.Text, tbDbName.Text, tbDbUser.Text, tbDbPWD.Text
		);
		_fileRoot = tbFiles.Text;

		CreateDatabase(_connectionString);
		RenderFiles();
		CreateUpdateXml();
		UpdateSettingConf(String.Format(_basePath + @"\{0}\Client.zip", _version));
		UpdateSettingConf(String.Format(_basePath + @"\Update\UPDATE-{0}.zip", _version));

		try
		{
			if (cbImportIP.Checked) ImportIP();
		}
		catch
		{
		}

#		if !DEBUG
		try
		{
			File.Delete(_basePath + @"\install.aspx");
			File.Delete(_basePath + @"\install.aspx.cs");
			DeleteDirectory(_basePath + @"\Client");
		}
		catch
		{
		}
#		endif

		Response.Redirect("Default.aspx");
	}
}
