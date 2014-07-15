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

class Account_CH : Core.CommandHandler
{
	public Account_CH(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{

	}

	public override string Process()
	{
		Hashtable ps = Core.Utility.ParseJson(Data as string) as Hashtable;
		string action = ps["Action"] as string;
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (action == "GetTempGroups")
		{
			DataRowCollection items = ServerImpl.Instance.CommonStorageImpl.GetTempGroups(cu.ID);
			Hashtable temp_groups = new Hashtable();
			foreach (DataRow item in items)
			{
				AccountInfo ginfo = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(item["ID"]));
				if (!temp_groups.ContainsKey(ginfo.ID))
				{
					temp_groups[ginfo.ID] = ginfo.DetailsJson;
				}
			}
			return Utility.RenderHashJson(
				"TempGroups", temp_groups
			);
		}
		else if (action == "GetCommFriends")
		{
			DataRowCollection items = ServerImpl.Instance.CommonStorageImpl.GetCommFriends(cu.ID);
			List<AccountInfo.Details> comm_friends = new List<AccountInfo.Details>();
			foreach (DataRow item in items)
			{
				comm_friends.Add(AccountImpl.Instance.GetUserInfo(Convert.ToInt32(item["CommFriendID"])).DetailsJson);
			}
			return Utility.RenderHashJson(
				"CommFriends", comm_friends
			);
		}
		else if (action == "GetDeptData")
		{
			int dept_id = Convert.ToInt32(ps["DeptID"]);
			DataRowCollection items = ServerImpl.Instance.CommonStorageImpl.GetDeptItems(cu.ID, dept_id);
			List<AccountInfo.Details> item_infos = new List<AccountInfo.Details>();
			foreach (DataRow item in items)
			{
				item_infos.Add(AccountImpl.Instance.GetUserInfo(Convert.ToInt32(item["UserID"])).DetailsJson);
			}
			return Utility.RenderHashJson(
				"Items", item_infos,
				"SubDepts", ServerImpl.Instance.CommonStorageImpl.GetSubDepts(dept_id)
			);
		}
		throw new NotImplementedException(String.Format("Command \"{0}\" isn't implemented", action));
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}