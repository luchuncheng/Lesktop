using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public class Mobile_Offline : System.Web.UI.Page
	{
		string tip_;

		protected void Page_Load(object sender, EventArgs e)
		{
			string relogin = Request.Params["relogin"];
			if(relogin != null)
			{
				Response.Redirect("login.aspx");
			}

			tip_ = "您的账号在另一地点登录";

			string reason = Request.QueryString["reason"];
			if(reason != null)
			{
				if (String.Compare(reason,"Unauthorized", true) == 0)
				{
					tip_ = "服务器验证您的身份时发生错误";
				}
				else if (String.Compare(reason,"Incompatible", true) == 0)
				{
					tip_ = "服务器已升级";
				}
			}
		}

		public String Tip
		{
			get { return tip_; }
		}
	}
}