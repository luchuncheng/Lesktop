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

public class WebIM : CommandHandler
{
	public WebIM(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{
	}

	public override string Process()
	{
		Hashtable param = Utility.ParseJson(Data) as Hashtable;
		String action = param["Action"] as String;
		if (action == "NewMessage")
		{
#			if DEBUG
			System.Threading.Thread.Sleep(1000);
#			endif
			int receiver = Convert.ToInt32(param["Receiver"]);
			int sender = Convert.ToInt32(param["Sender"]);
			String content = param["Content"] as String;
			bool isTemp = param.ContainsKey("IsTemp") ? Convert.ToBoolean(param["IsTemp"]) : false;

			Message msg = MessageImpl.Instance.NewMessage(receiver, sender, content, param, isTemp);
			return Utility.RenderJson(msg);
		}
		throw new NotImplementedException();
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}
