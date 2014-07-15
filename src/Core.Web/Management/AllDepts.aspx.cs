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
	public partial class Management_AllDepts : System.Web.UI.Page
	{
		static string RowFormat =
		@"
		<tr>
			<td class='headimg'>&nbsp;</td>
			<td class='nickname'>{0}</td>
			<td class='registerTime'>&nbsp;</td>
			<td class='operation'><a href='javascript:Delete({2},{3})'>删除部门</a></td>
		</tr>
		";
		CommandCtrl _cmdCtrl;
		int _deptID = 0;
		protected DataRow DeptInfo;

		protected int DeptID
		{
			get { return Convert.ToInt32(DeptInfo["ID"]); }
		}


		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			_deptID = Convert.ToInt32(Request.QueryString["DeptID"]);
			DeptInfo = AccountImpl.Instance.GetDeptInfo(_deptID);

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
					int id = Convert.ToInt32(data);
					AccountImpl.Instance.DeleteDept(id);
					_cmdCtrl.State["Action"] = "ClearDeptData";
					_cmdCtrl.State["DeptID"] = _deptID;
					_cmdCtrl.State["DeptInfo"] = AccountImpl.Instance.GetDeptInfo(_deptID);
				}
				else if (command == "NewDept")
				{
					Hashtable info = data as Hashtable;
					AccountImpl.Instance.CreateDept(info["Nickname"].ToString(), _deptID);
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

		protected String RenderDeptList()
		{
			StringBuilder builder = new StringBuilder();
			foreach (DataRow row in AccountImpl.Instance.GetAllDepts(Convert.ToInt32(Request.QueryString["DeptID"])))
			{
				builder.AppendFormat(
					RowFormat,
					HtmlUtil.ReplaceHtml(row["Name"].ToString()),
					"",
					row["ID"],
					Utility.RenderJson(row["Name"].ToString())
				);
			}
			return builder.ToString();
		}
	}
}