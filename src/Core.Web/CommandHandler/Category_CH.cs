using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Core;

class Category_CH : Core.CommandHandler
{
	public Category_CH(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{

	}

	public override string Process()
	{
		Hashtable ps = Core.Utility.ParseJson(Data as string) as Hashtable;
		string action = ps["Action"] as string;
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (action == "GetCategoryData")
		{
			return Utility.RenderHashJson(
				"CompanyInfo", ServerImpl.Instance.CommonStorageImpl.GetCompanyInfo()
			);
		}
		throw new NotImplementedException(String.Format("Command \"{0}\" isn't implemented", action));
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}