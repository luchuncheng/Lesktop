using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;
using Core.Web;

public partial class Management_CommAllUsers : System.Web.UI.Page
{
	static string RowFormat =
	@"
	<tr>
		<td class='headimg'>&nbsp;</td>
		<td class='name'>{1}</td>
		<td class='nickname'>{2}</td>
		<td class='email'>{3}</td>
		<td class='registerTime'>{7:yyyy-MM-dd HH:mm}</td>
		<td class='operation'><a href='javascript:Delete({4},{5},{6})'>删除用户</a></td>
	</tr>
	";

	protected void Page_Load(object sender, EventArgs e)
	{
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (cu == null || cu.ID != AccountImpl.AdminID) throw new Exception("你没有权限访问该页面！");

		CommandCtrl cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
		cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

		cmdCtrl.State["Action"] = null;
	}

	private void cmdCtrl_OnCommand(string command, object data)
	{
		string peer = Convert.ToString(data);
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

		if (command == "Delete")
		{
			int id = Convert.ToInt32(data);
			AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(id);
			AccountImpl.Instance.DeleteUser(userInfo.ID);
		}
	}

	protected String RenderAllUsersList()
	{
		StringBuilder builder = new StringBuilder();
		foreach (DataRow row in AccountImpl.Instance.GetAllRegisterUsers())
		{
			builder.AppendFormat(
				RowFormat, "",
				HtmlUtil.ReplaceHtml(row["Name"].ToString()),
				HtmlUtil.ReplaceHtml(row["Nickname"].ToString()),
				HtmlUtil.ReplaceHtml(row["EMail"].ToString()),
				Convert.ToInt32(row["ID"]),
				Core.Utility.RenderJson(row["Nickname"].ToString()),
				Core.Utility.RenderJson(row["Name"].ToString()),
				row["RegisterTime"]
			);
		}
		return builder.ToString();
	}
}
