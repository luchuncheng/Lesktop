using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public partial class Management_GroupMembers : System.Web.UI.Page
	{
		static string RowFormat =
		@"
		<tr>
			<td class='headimg'>&nbsp;</td>
			<td class='name'>{1}</td>
			<td class='nickname'>{2}</td>
			<td class='email'>{3}</td>
			<td class='registerTime'>{4:yyyy-MM-dd}</td>
			<td class='operation'>{5}</td>
		</tr>
		";

		CommandCtrl _cmdCtrl;
		int _groupID;

		protected void Page_Load(object sender, EventArgs e)
		{
			_groupID = Convert.ToInt32(Request.QueryString["GroupID"]);
			AccountInfo groupInfo = Core.AccountImpl.Instance.GetUserInfo(_groupID);

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (!(cu != null && (cu.IsAdmin || groupInfo.Type == 1 && cu.ID == groupInfo.Creator)))
			{
				throw new Exception("你没有权限访问该页面！");
			}

			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

			_cmdCtrl.State["Action"] = null;
			var bottom_ope = FindControl("BottomOpe");
			if (bottom_ope != null) bottom_ope.Visible = cu.IsAdmin;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			try
			{
				if (command == "Remove")
				{
					int id = Convert.ToInt32(data);
					AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(_groupID);
					int[] members = groupInfo.Friends;
					AccountImpl.Instance.RemoveFromGroup(id, _groupID);
					SessionManagement.Instance.NotifyResetListCache(groupInfo.Friends, false, false, new AccountInfo[] { groupInfo });
				}
				else if (command == "AddUsers")
				{
					string ids = (data as Hashtable)["IDS"].ToString();
					AccountImpl.Instance.AddUsersToGroup(Utility.ParseIntArray(ids), _groupID);
					AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(_groupID);
					SessionManagement.Instance.NotifyResetListCache(groupInfo.Friends, false, false, new AccountInfo[] { groupInfo });
				}
			}
			catch (Exception ex)
			{
				_cmdCtrl.State["Action"] = "Error";
				_cmdCtrl.State["Exception"] = ex;
			}
		}

		protected String RenderAllUsersList()
		{
			StringBuilder builder = new StringBuilder();
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(_groupID);
			foreach (int f in AccountImpl.Instance.GetUserInfo(_groupID).Friends)
			{
				AccountInfo memberInfo = AccountImpl.Instance.GetUserInfo(f);
				if (memberInfo.ID <= 3) continue;
				String ope = "";
				if(memberInfo.ID != groupInfo.Creator)
				{
					ope = String.Format(
						"<a href='javascript:Remove({0},{1},{2})'>移出本群</a>",
						Convert.ToInt32(memberInfo.ID),
						Core.Utility.RenderJson(memberInfo.Nickname),
						Core.Utility.RenderJson(memberInfo.Nickname)
					);
				}
				builder.AppendFormat(
					RowFormat, "",
					HtmlUtil.ReplaceHtml(memberInfo.Name),
					HtmlUtil.ReplaceHtml(memberInfo.Nickname),
					HtmlUtil.ReplaceHtml(memberInfo.EMail),
					memberInfo.RegisterTime,
					ope
				);
			}
			return builder.ToString();
		}
	}
}