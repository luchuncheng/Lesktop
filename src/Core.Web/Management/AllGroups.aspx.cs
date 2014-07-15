using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public partial class Management_AllGroups : System.Web.UI.Page
	{
		static string RowFormat =
		@"
		<tr>
			<td class='headimg'>&nbsp;</td>
			<td class='nickname'>{2}</td>
			<td class='creator'>{3}({4})</td>
			<td class='operation'><a href='javascript:Delete({5},{6},{7})'>删除群组</a></td>
		</tr>
		";
		CommandCtrl _cmdCtrl;
		int _deptID = -1;
		protected DataRow DeptInfo;

		protected void Page_Load(object sender, EventArgs e)
		{
			_deptID = Convert.ToInt32(Request.QueryString["DeptID"]);
			DeptInfo = AccountImpl.Instance.GetDeptInfo(_deptID);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			try
			{
				string peer = Convert.ToString(data);
				AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

				if (command == "Delete")
				{
					int groupId = Convert.ToInt32(data);
					AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);
					int[] members = groupInfo.Friends;
					AccountImpl.Instance.DeleteGroup(groupInfo.ID, groupInfo.Creator);
					
					string content = Utility.RenderHashJson(
						"Type", "DeleteGroupNotify",
						"Group", groupInfo
					);
					foreach (int member in members)
					{
						AccountInfo memberInfo = AccountImpl.Instance.GetUserInfo(member);
						if (memberInfo.ID != AccountImpl.AdminID)
						{
							MessageImpl.Instance.NewMessage(memberInfo.ID, AccountImpl.AdminstratorID, content, null, false);
						}
					}
				}
				else if (command == "NewGroup")
				{
					Hashtable info = data as Hashtable;
					int id = AccountImpl.Instance.CreateGroup(cu.ID, "", info["Nickname"].ToString(), _deptID, 1, "");
					_cmdCtrl.State["Action"] = "ClearDeptData";
					_cmdCtrl.State["DeptID"] = _deptID;
					_cmdCtrl.State["DeptInfo"] = AccountImpl.Instance.GetDeptInfo(_deptID);
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
			foreach (DataRow row in AccountImpl.Instance.GetAllGroups(_deptID))
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
}