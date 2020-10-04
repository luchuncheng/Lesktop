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
		protected void Page_Load(object sender, EventArgs e)
		{
			string relogin = Request.Params["relogin"];
			if(relogin != null)
			{
				Response.Redirect("login.aspx");
			}
		}
	}
}