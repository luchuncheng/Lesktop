using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public class Mobile_Logout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu != null)
			{
				ServerImpl.Instance.Logout(Context);
				String sessionId = Request.QueryString["SessionID"];
				if (sessionId != null)
				{
					SessionManagement.Instance.RemoveSession(cu.ID, sessionId);
					string data = Utility.RenderHashJson(
						"User", cu.ID,
						"State", SessionManagement.Instance.IsOnline(cu.ID) ? "Online" : "Offline",
						"Details", cu.DetailsJson
					);
					SessionManagement.Instance.Send("UserStateChanged", data);
				}
			}
			Response.Redirect("login.aspx");
		}
	}
}