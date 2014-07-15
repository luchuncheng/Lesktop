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

public partial class Management_CommAllGroups : System.Web.UI.Page
{
	static string RowFormat =
	@"
	<tr>
		<td class='headimg'>&nbsp;</td>
		<td class='name'>{1}</td>
		<td class='nickname'>{2}</td>
		<td class='creator'>{3}({4})</td>
		<td class='registerTime'>{8:yyyy-MM-dd HH:mm}</td>
		<td class='operation'><a href='javascript:Delete({5},{6},{7})'>删除群组</a></td>
	</tr>
	";
	CommandCtrl _cmdCtrl;

	protected void Page_Load(object sender, EventArgs e)
	{
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (cu == null || cu.ID != AccountImpl.AdminID) throw new Exception("你没有权限访问该页面！");

		_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
		_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
	}

	private void cmdCtrl_OnCommand(string command, object data)
	{
		try
		{
			string peer = Convert.ToString(data);			
			if (command == "Delete")
			{
				int groupId = Convert.ToInt32(data);
				AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);
				int[] members = groupInfo.Friends;
				AccountImpl.Instance.DeleteGroup(groupId, groupInfo.Creator);

				foreach (int member in members)
				{
					AccountInfo memberInfo = AccountImpl.Instance.GetUserInfo(member);

					string content = Utility.RenderHashJson(
						"Type", "DeletetGroupNotify",
						"Group", groupInfo
					);
					MessageImpl.Instance.NewMessage(member, AccountImpl.AdminstratorID, content, null, false);
				}
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
		StringBuilder builder = new StringBuilder();
		foreach (DataRow row in AccountImpl.Instance.GetAllRegisterGroups())
		{
			AccountInfo createor = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(row["Creator"]));
			builder.AppendFormat(
				RowFormat, "",
				HtmlUtil.ReplaceHtml(row["Name"].ToString()),
				HtmlUtil.ReplaceHtml(row["Nickname"].ToString()),
				HtmlUtil.ReplaceHtml(createor.Nickname),
				createor.Name,
				row["ID"],
				Core.Utility.RenderJson(row["Nickname"].ToString()),
				Core.Utility.RenderJson(row["Name"].ToString()),
				row["RegisterTime"]
			);
		}
		return builder.ToString();
	}
}
