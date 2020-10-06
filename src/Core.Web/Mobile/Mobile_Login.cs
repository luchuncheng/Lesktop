using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public class Mobile_Login : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo current_user = null;

			if (Request.Params["login"] != null)
			{
				string user = Request.Params["user"];
				if (user != null && user != "")
				{
					string pwd = Request.Params["pwd"];
#if DEBUG
					if (true)
#else
					if (Core.AccountImpl.Instance.Validate(user, pwd))		
#endif
					{
						int userid = AccountImpl.Instance.GetUserID(user);
						// 仅验证不启动回话，重定向到default.aspx再启动回话
						ServerImpl.Instance.Login("", Context, userid, false, null, false, 2);
						current_user = AccountImpl.Instance.GetUserInfo(userid);
					}
				}
			}
			else if (Request.Params["visitor"] != null)
			{
#if DEBUG
				string ip = "117.136.10.171";
#else
				string ip = Context.Request.ServerVariables["REMOTE_ADDR"];
#endif
				int id = AccountImpl.Instance.CreateTempUser(ip);
				if (id != 0)
				{
					ServerImpl.Instance.Login("", Context, id, false, DateTime.Now.AddDays(7), false, 2);
					current_user = AccountImpl.Instance.GetUserInfo(id);
					int lucc_id = AccountImpl.Instance.GetUserID("lucc");
					if (lucc_id > 0)
					{
						AccountImpl.Instance.AddFriend(id, lucc_id);
						SessionManagement.Instance.Send(lucc_id, "GLOBAL:ADD_COMM_FRIEND", Utility.RenderHashJson("CommFriend", current_user.DetailsJson));
					}
				}
			}
			else
			{
				current_user = ServerImpl.Instance.GetCurrentUser(this.Context);
			}

			if (current_user != null)
			{
				Response.Redirect("default.aspx");
			}
		}
	}
}