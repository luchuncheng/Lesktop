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
	public partial class Management_AllUsers : System.Web.UI.Page
	{
		static string RowFormat =
		@"
		<tr>
			<td class='headimg'>&nbsp;</td>
			<td class='name'>{1}</td>
			<td class='nickname'>{2}</td>
			<td class='email'>{3}</td>
			<td class='dept'>{7}</td>
			<td class='operation'><a href='javascript:ResetUserDepts({4},{5},{6})'>修改所属部门</a><a href='javascript:Delete({4},{5},{6})'>删除</a></td>
		</tr>
		";
		//<a href='javascript:Remove({4},{5},{6},{8},{9})'>移除</a>

		CommandCtrl _cmdCtrl;
		int _deptID;
		static Hashtable m_PredefineUsers = new Hashtable();
		protected DataRow DeptInfo;

		static Management_AllUsers()
		{
			m_PredefineUsers[AccountImpl.AdminID] = AccountImpl.AdminID;
			m_PredefineUsers[AccountImpl.AdminstratorID] = AccountImpl.AdminstratorID;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			_deptID = Convert.ToInt32(Request.QueryString["DeptID"]);
			DeptInfo = AccountImpl.Instance.GetDeptInfo(_deptID);

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

			_cmdCtrl.State["Action"] = null;
			_cmdCtrl.State["CheckLoginName"] = Custom.ApplicationInfo.CheckLoginName;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			try
			{
				if (command == "Delete")
				{
					int id = Convert.ToInt32(data);
					AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(id);
					if (m_PredefineUsers.ContainsKey(userInfo.ID))
					{
						throw new Exception("不能删除该用户！");
					}

					int[] related_users = AccountImpl.Instance.GetRelatedUsers(userInfo.ID);
					string[] comm_friends = GetCommFriends(id);
					AccountImpl.Instance.DeleteUser(userInfo.ID);
					AccountImpl.Instance.DeleteUserInfo(userInfo.ID);
					_cmdCtrl.State["Action"] = "ClearDeptData";
					_cmdCtrl.State["DeptID"] = _deptID;
					_cmdCtrl.State["DeptInfo"] = AccountImpl.Instance.GetDeptInfo(_deptID);
				}
				else if (command == "ResetUserDepts")
				{
					Hashtable hash_data = data as Hashtable;
					AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(hash_data["UserID"]));
					AccountImpl.Instance.ResetUserDepts(userInfo.ID, hash_data["NewDepts"].ToString());
					_cmdCtrl.State["Action"] = "ResetDeptData";
					_cmdCtrl.State["DeptID"] = 0;
				}
				else if (command == "CreateUser")
				{
					Hashtable info = data as Hashtable;
					int id = AccountImpl.Instance.CreateUser(info["Name"].ToString(), info["Nickname"].ToString(), "", info["EMail"].ToString(), _deptID, 1);
					AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(id);
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

		string[] GetCommFriends(int id)
		{
			DataRowCollection comm_friends = ServerImpl.Instance.CommonStorageImpl.GetCommFriends(id);
			List<string> friends = new List<string>();
			foreach (DataRow friend_row in comm_friends)
			{
				friends.Add(friend_row["CommFriendName"].ToString());
			}
			return friends.ToArray();
		}

		protected String RenderAllUsersList()
		{
			StringBuilder builder = new StringBuilder();
			foreach (DataRow row in AccountImpl.Instance.GetAllUsers(_deptID))
			{
				if (Convert.ToInt32(row["ID"]) > 3)
				{
					string depts = HtmlUtil.ReplaceHtml(row["Depts"].ToString()).Replace(",", "<br/>");
					builder.AppendFormat(
						RowFormat,
						"",
						HtmlUtil.ReplaceHtml(row["Name"].ToString()),
						HtmlUtil.ReplaceHtml(row["Nickname"].ToString()),
						HtmlUtil.ReplaceHtml(row["EMail"].ToString()),
						Convert.ToInt32(row["ID"]),
						Core.Utility.RenderJson(row["Nickname"].ToString()),
						Core.Utility.RenderJson(row["Name"].ToString()),
						String.IsNullOrEmpty(depts) ? "&nbsp" : depts,
						0,
						Core.Utility.RenderJson(row["Depts"])
					);
				}
			}
			return builder.ToString();
		}
	}
}