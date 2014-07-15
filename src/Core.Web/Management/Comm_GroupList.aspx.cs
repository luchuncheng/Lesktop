using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;
using Core.Web;

public partial class Management_CommGroupList : System.Web.UI.Page
{
	static string RowFormat =
	@"
	<tr>
		<td class='headimg'>&nbsp;</td>
		<td class='name'>{1}</td>
		<td class='nickname'>{2}</td>
		<td class='creator'>{3}({4})</td>
		<td class='operation'>
			{10}
			<a href='javascript:Delete({5},{6},{7},{8})'>{9}</a>
		</td>
	</tr>
	";
	CommandCtrl _cmdCtrl;

	protected void Page_Load(object sender, EventArgs e)
	{
		_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
		_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
	}

	private void cmdCtrl_OnCommand(string command, object data)
	{
		try
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			if (command == "NewGroup")
			{
				Hashtable info = data as Hashtable;
				int id = AccountImpl.Instance.CreateGroup(cu.ID, info["Name"].ToString(), info["Nickname"].ToString(), -1, 0, "");
				AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(id);
				SessionManagement.Instance.Send(cu.ID, "GLOBAL:ADD_COMM_FRIEND", Utility.RenderHashJson("CommFriend", groupInfo.DetailsJson));
			}
			else if (command == "Exit")
			{
				int groupId = Convert.ToInt32(data);
				AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);
				AccountImpl.Instance.RemoveFromGroup(cu.ID, groupInfo.ID);

				string content = Utility.RenderHashJson(
					"Type", "ExitGroupNotify",
					"User", cu,
					"Group", groupInfo
				);

				MessageImpl.Instance.NewMessage(cu.ID, AccountImpl.AdminstratorID, content, null, false);
				MessageImpl.Instance.NewMessage(groupId, AccountImpl.AdminstratorID, content, null, false);

				SessionManagement.Instance.Send(cu.ID, "GLOBAL:REMOVE_COMM_FRIEND", Utility.RenderHashJson("CommFriend", groupInfo.DetailsJson));
			}
			else if (command == "Delete")
			{
				int id = Convert.ToInt32(data);
				AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(id);
				int[] members = groupInfo.Friends;
				AccountImpl.Instance.DeleteGroup(groupInfo.ID, cu.ID);

				foreach (int member in groupInfo.Friends)
				{
					if (member == cu.ID) continue;
					string content = Utility.RenderHashJson(
						"Type", "DeletetGroupNotify",
						"Group", groupInfo
					);
					MessageImpl.Instance.NewMessage(member, AccountImpl.AdminstratorID, content, null, false);
				}

				SessionManagement.Instance.SendToMultiUsers(members, "GLOBAL:REMOVE_COMM_FRIEND", Utility.RenderHashJson("CommFriend", groupInfo.DetailsJson));

			}
		}
		catch (Exception ex)
		{
			_cmdCtrl.State["Action"] = "Error";
			_cmdCtrl.State["Exception"] = ex;
		}
	}

	protected String RenderFriendList()
	{
		AccountInfo cu = AccountImpl.Instance.GetUserInfo(ServerImpl.Instance.GetUserID(Context));
		StringBuilder builder = new StringBuilder();
		foreach (int friend in cu.Friends)
		{
			AccountInfo fi = AccountImpl.Instance.GetUserInfo(friend);
			if (fi.Type == 1 && fi.SubType == 0 && !fi.IsTemp)
			{
				AccountInfo createor = AccountImpl.Instance.GetUserInfo(fi.Creator);
				builder.AppendFormat(
					RowFormat, "",
					HtmlUtil.ReplaceHtml(fi.Name),
					HtmlUtil.ReplaceHtml(fi.Nickname),
					HtmlUtil.ReplaceHtml(createor.Nickname),
					createor.Name,
					fi.ID,
					Core.Utility.RenderJson(fi.Nickname),
					Core.Utility.RenderJson(fi.Name),
					Core.Utility.RenderJson(cu.ID == createor.ID),
					cu.ID == createor.ID ? "解散群组" : "退出群组",
					cu.ID == createor.ID ? String.Format("<a href='javascript:Update(\"{0}\", {1})'>修改群资料</a>", HtmlUtil.ReplaceHtml(fi.Name), fi.ID) : ""
				);
			}
		}
		return builder.ToString();
	}
}
