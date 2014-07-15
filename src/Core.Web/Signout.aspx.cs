using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Configuration;

namespace Core.Web
{
	public partial class Signout : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Core.ServerImpl.Instance.Logout(Context);
			Response.Redirect(System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultPage"]);
		}
	}
}