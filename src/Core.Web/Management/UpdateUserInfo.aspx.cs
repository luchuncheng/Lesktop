using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;
using Core.IO;


namespace Core.Web
{
	public partial class Lesktop_Management_UpdateUserInfo : System.Web.UI.Page
	{
		protected AccountInfo Info = null;
		protected String Depts = "";

		protected void Page_Load(object sender, EventArgs e)
		{
			Info = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(Request.QueryString["ID"]));

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (!(cu != null && (cu.IsAdmin || (Info.Type == 0 && cu.ID == Info.ID) || (Info.Type == 1 && cu.ID == Info.Creator))))
			{
				throw new Exception("你没有权限访问该页面！");
			}
			if (cu.IsAdmin && Info.ID != AccountImpl.AdminID)
			{
				Depts = AccountImpl.Instance.GetUserDepts(Info.ID);
				Control depts_ph = FindControl("PlaceHolderDepts");
				if (depts_ph != null) depts_ph.Visible = true;
			}

			CommandCtrl cmdCtrl = FindControl("CommandCtrl1") as CommandCtrl;
			cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

			cmdCtrl.State["Info"] = Info.DetailsJson;

			Control prepwd_ph = FindControl("PrePasswordPH");
			if (prepwd_ph != null) prepwd_ph.Visible = Info.ID == ServerImpl.Instance.GetUserID(Context);

			Control resetpwd_ph = FindControl("PlaceHolderResetPwd");
			if (resetpwd_ph != null) resetpwd_ph.Visible = !Info.IsTemp;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			CommandCtrl cmdCtrl = FindControl("CommandCtrl1") as CommandCtrl;
			try
			{
				if (command == "Update")
				{
					Hashtable info = data as Hashtable;
					if (Request.Files["file_headimg"] != null && Request.Files["file_headimg"].InputStream.Length > 0)
					{
						String filename = String.Format("/{0}/pub/HeadIMG/{1}{2}", Info.ID, Guid.NewGuid().ToString().Replace("-", ""), Path.GetExtension(Request.Files["file_headimg"].FileName));
						info["HeadIMG"] = filename;
						string dirname = Path.GetDirectoryName(filename);
						Directory.CreateDirectory(dirname);
						File.SetAttributes(dirname, File.GetAttributes(dirname) | FileAttributes.Hidden);
						using (System.IO.Stream stream = File.Open(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
						{
							byte[] buffer = new byte[4 * 1024];
							while (true)
							{
								int c = Request.Files["file_headimg"].InputStream.Read(buffer, 0, buffer.Length);
								if (c == 0) break;
								stream.Write(buffer, 0, c);
							}
						}
					}

					Utility.FilterUserInfoHtml(info);
					AccountImpl.Instance.UpdateUserInfo(Info.ID, info);
					SessionManagement.Instance.SendToRelatedUsers(Info.ID, "GLOBAL:ACCOUNTINFO_CHANGED", Utility.RenderHashJson("Details", Info.DetailsJson));

					cmdCtrl.State["Info"] = Info.DetailsJson;
				}
				else if (command == "ResetPassword")
				{
					Hashtable info = data as Hashtable;
					AccountImpl.Instance.ResetPassword(Info.ID, info["Password"].ToString());
					cmdCtrl.State["Info"] = Info.DetailsJson;
				}
				else if (command == "ResetSelfPassword")
				{
					Hashtable info = data as Hashtable;
					AccountImpl.Instance.UpdateUserInfo(Info.ID, info);
				}
			}
			catch (Exception ex)
			{
				cmdCtrl.State["Action"] = "Error";
				cmdCtrl.State["Exception"] = ex;
			}
		}
	}
}