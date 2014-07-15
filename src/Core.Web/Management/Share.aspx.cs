using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Configuration;
using Core;
using Core.IO;
using Core.Web;
using System.Collections;

public partial class Lesktop_Management_Share : System.Web.UI.Page
{
	static string RowFormat =
	@"
	<tr>
		<td class='filename'>{0}</td>
		<td class='size'>{3}</td>
		<td class='operation'>
			<a href='../Download.ashx?FileName={4}'>下载</a>
			{2}
		</td>
	</tr>
	";

	CommandCtrl _cmdCtrl;

	protected void Page_Load(object sender, EventArgs e)
	{
		_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
		_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

		//FindControl("PlaceHolder1").Visible = CurrentUser.IsAdmin;
		_cmdCtrl.State["Action"] = null;
	}

	AccountInfo GroupInfo
	{
		get { return AccountImpl.Instance.GetUserInfo(Convert.ToInt32(Request.QueryString["Group"])); }
	}

	AccountInfo CurrentUser
	{
		get { return ServerImpl.Instance.GetCurrentUser(Context); }
	}

	private void cmdCtrl_OnCommand(string command, object data)
	{
		try
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			if (command == "Upload")
			{
				if (Request.Files["upload_file"] != null && Request.Files["upload_file"].InputStream.Length > 0)
				{
					String filename = String.Format("/{0}/Home/{1}", GroupInfo.ID, System.IO.Path.GetFileName(Request.Files["upload_file"].FileName));
					String dir = Path.GetDirectoryName(filename);
					if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
					using (System.IO.Stream stream = File.Open(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
					{
						byte[] buffer = new byte[4 * 1024];
						while (true)
						{
							int c = Request.Files["upload_file"].InputStream.Read(buffer, 0, buffer.Length);
							if (c == 0) break;
							stream.Write(buffer, 0, c);
						}
					}

					String msg = String.Format(
						"<span style='color: Red;'>\"{0}\"共享了文件：</span><br/><br/><div class=\"message_file\" contentEditable=\"false\">[FILE:{1}]</div>",
						CurrentUser.Nickname, Microsoft.JScript.GlobalObject.escape(filename)
					);

					MessageImpl.Instance.NewMessage(GroupInfo.ID, CurrentUser.ID, msg, null, false);
				}
			}
			else if (command == "Delete")
			{
				Hashtable data_hash = data as Hashtable;
				string filename = String.Format("/{0}/Home/{1}", GroupInfo.ID, data_hash["FileName"]);
				File.Delete(filename);
			}
			Response.Redirect(Request.Url.ToString());
		}
		catch (Exception ex)
		{
			_cmdCtrl.State["Action"] = "Error";
			_cmdCtrl.State["Exception"] = ex;
		}
	}

	protected String RenderList()
	{
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

		StringBuilder builder = new StringBuilder();
		string root = String.Format("/{0}/Home", GroupInfo.ID);
		if (!Directory.Exists(root)) Directory.CreateDirectory(root);
		DirectoryInfo di = new DirectoryInfo(root);
		foreach (FileSystemInfo fsi in di.GetFileSystemInfos())
		{
			if ((fsi.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
			{
				FileInfo fi = fsi as FileInfo;
				string temp = ""; 
				long size = fi.Length;
				if (size > 1024 * 1024 * 1024) temp = String.Format("{0:#######0.00}GB", (double)size / (1024 * 1024 * 1024));
				else if (size > 1024 * 1024) temp = String.Format("{0:#######0.00}MB", (double)size / (1024 * 1024));
				else if (size > 1024) temp = String.Format("{0:#######0.00}KB", (double)size / 1024);
				else temp = size.ToString() + "B";
				builder.AppendFormat(
					RowFormat,
					fi.Name, fi.FullName,
					(GroupInfo.SubType == 1 || (GroupInfo.SubType == 0 && GroupInfo.Creator == cu.ID)) ? String.Format("<a href='javascript:Delete({0})'>删除</a>", Utility.RenderJson(fi.Name)) : "",
					temp, Microsoft.JScript.GlobalObject.escape(fi.FullName)
				);
			}
		}
		return builder.ToString();
	}
}

public class GroupShare_CH : Core.CommandHandler
{
	public GroupShare_CH(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{
	}

	public override string Process()
	{
		Hashtable ps = Core.Utility.ParseJson(Data as string) as Hashtable;
		string action = ps["Action"] as string;
		int groupId = Convert.ToInt32(ps["GroupID"]);
		AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (!groupInfo.ContainsMember(cu.ID)) throw new Exception("权限不足！");
		string root = String.Format("/{0}/Home", groupInfo.ID);
		if (!Directory.Exists(root)) Directory.CreateDirectory(root);

		if (action == "GetFileList")
		{
			List<FileInfo> files = GetFileList(root);
			return Utility.RenderHashJson("Files", files, "GroupInfo", groupInfo.DetailsJson);
		}
		else if (action == "Delete")
		{
			string filename = String.Format("/{0}/Home/{1}", groupInfo.ID, ps["FileName"]);
			File.Delete(filename);
			List<FileInfo> files = GetFileList(root);
			return Utility.RenderHashJson("Files", files, "GroupInfo", groupInfo.DetailsJson);
		}
		else if (action == "Upload")
		{
			string src = ps["FileName"].ToString();
			string name = Path.GetFileNameWithoutExtension(src);
			string ext = Path.GetExtension(src);
			string filename = String.Format("/{0}/Home/{1}{2}", groupInfo.ID, name, ext);
			for (int i = 1; File.Exists(filename); i++)
			{
				filename = String.Format("/{0}/Home/{1}({2}){3}", groupInfo.ID, name, i, ext);
			}
			File.Copy(src, filename);

			String msg = String.Format(
				"<span>\"{0}\"共享了文件：</span><br/><br/><div class=\"message_file\" contentEditable=\"false\">[FILE:{1}]</div>",
				cu.Nickname, filename
			);

			MessageImpl.Instance.NewMessage(groupInfo.ID, cu.ID, msg, null, false);

			List<FileInfo> files = GetFileList(root);
			return Utility.RenderHashJson("Files", files, "GroupInfo", groupInfo.DetailsJson);
		}
		throw new NotImplementedException();
	}

	private static List<FileInfo> GetFileList(string root)
	{
		List<FileInfo> files = new List<FileInfo>();
		DirectoryInfo di = new DirectoryInfo(root);
		foreach (FileSystemInfo fsi in di.GetFileSystemInfos())
		{
			if ((fsi.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
			{
				FileInfo fi = fsi as FileInfo;
				files.Add(fi);
			}
		}
		return files;
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}