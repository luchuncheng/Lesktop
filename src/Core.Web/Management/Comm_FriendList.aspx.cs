using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;
using Core.Web;

public partial class Management_CommFriendList : System.Web.UI.Page
{
	static string RowFormat =
	@"
	<tr>
		<td class='headimg'>&nbsp;</td>
		<td class='name'>{1}</td>
		<td class='nickname'>{2}</td>
		<td class='email'>{3}</td>
		<td class='operation'><a href='javascript:Delete({4},{5},{6})'>删除好友</a></td>
	</tr>
	";

	protected void Page_Load(object sender, EventArgs e)
	{
		CommandCtrl cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
		cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

		cmdCtrl.State["Action"] = null;
	}

	private void cmdCtrl_OnCommand(string command, object data)
	{
		int peer = Convert.ToInt32(data);
		AccountInfo peerInfo = AccountImpl.Instance.GetUserInfo(peer);
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

		if (command == "Delete")
		{
			AccountImpl.Instance.DeleteFriend(cu.ID, peer);

			string content = Utility.RenderHashJson(
				"Type", "DeleteFriendNotify",
				"User", cu,
				"Peer", peerInfo,
				"Info", ""
			);

			MessageImpl.Instance.NewMessage(peerInfo.ID, AccountImpl.AdminstratorID, content, null, false);
			MessageImpl.Instance.NewMessage(cu.ID, AccountImpl.AdminstratorID, content, null, false);

			SessionManagement.Instance.Send(peerInfo.ID, "GLOBAL:REMOVE_COMM_FRIEND", Utility.RenderHashJson("CommFriend", AccountImpl.Instance.GetUserInfo(cu.ID).DetailsJson));
			SessionManagement.Instance.Send(cu.ID, "GLOBAL:REMOVE_COMM_FRIEND", Utility.RenderHashJson("CommFriend", AccountImpl.Instance.GetUserInfo(peerInfo.ID).DetailsJson));
		}
	}

	protected String RenderFriendList()
	{
		AccountInfo cu = AccountImpl.Instance.GetUserInfo(ServerImpl.Instance.GetUserID(Context));
		StringBuilder builder = new StringBuilder();
		foreach (int userid in AccountImpl.Instance.GetUserInfo(cu.ID).Friends)
		{
			AccountInfo fi = AccountImpl.Instance.GetUserInfo(userid);
			if (fi.Type == 0 && (cu.SubType == 0 || fi.SubType == 0))
			{
				builder.AppendFormat(
					RowFormat, "",
					HtmlUtil.ReplaceHtml(fi.Name),
					HtmlUtil.ReplaceHtml(fi.Nickname),
					HtmlUtil.ReplaceHtml(fi.EMail),
					fi.ID,
					Core.Utility.RenderJson(fi.Nickname),
					Core.Utility.RenderJson(fi.Name)
				);
			}
		}
		return builder.ToString();
	}
}
