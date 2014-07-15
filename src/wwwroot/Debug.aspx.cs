using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Debug : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (Request.QueryString["user"] != null)
		{
			String name = Request.QueryString["user"];
			Int32 id = Core.AccountImpl.Instance.GetUserID(name);
			Core.ServerImpl.Instance.Login("", Context, id, false, null, false);
		}
	}
}
