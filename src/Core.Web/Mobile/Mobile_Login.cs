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
					String sessionId = Guid.NewGuid().ToString().ToUpper();
					ServerImpl.Instance.Login(sessionId, Context, userid, false, null, true, 2);
					current_user = AccountImpl.Instance.GetUserInfo(userid);
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