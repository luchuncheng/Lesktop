using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Core;


namespace Core.Web
{
	public partial class Lesktop_MsgHistory_MessageList_Group : System.Web.UI.Page
	{
		static String MsgListTrFormat = @"
		<tr>
			<td class='checkbox'><input id='cb_{0}' value='{0},0' type='checkbox' /></td>
			<td class='user' style='color:#AA0000;'>{1}</td>
			<td class='msgcount'>{2}</td>
			<td class='renewtime'>{3: yyyy-MM-dd HH:mm:ss}</td>
			<td class='operation'><a href='javascript:ViewMessages({0}, 0, {4})'>查看消息</a></td>
		</tr>
		";

		protected int _currentPage;
		protected int _pageCount;
		protected int _isTemp;
		protected DataTable _msgs = null;
		CommandCtrl _cmdCtrl;

		protected int CurrentPage
		{
			get { return _currentPage == -1 ? _pageCount : _currentPage; }
		}

		protected int PageCount
		{
			get { return _pageCount; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			_currentPage = Request.QueryString["CurrentPage"] == null ? 1 : Convert.ToInt32(Request.QueryString["CurrentPage"]);
			_isTemp = Request.QueryString["IsTemp"] == null ? 0 : Convert.ToInt32(Request.QueryString["IsTemp"]);

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
			_cmdCtrl.State["Action"] = null;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			try
			{
				if (command == "Delete")
				{
					string[] ids = (data as Hashtable)["Items"].ToString().Split(new char[] { ',' });
					for (int i = 0; i < ids.Length; i += 2)
					{
						MessageImpl.Instance.DeleteMessages(Convert.ToInt32(ids[i]), Convert.ToInt32(ids[i + 1]));
					}
				}
			}
			catch (Exception ex)
			{
				_cmdCtrl.State["Action"] = "Error";
				_cmdCtrl.State["Exception"] = ex;
			}
		}

		protected String RenderList()
		{
			if (_msgs == null)
			{
				_msgs = MessageImpl.Instance.GetMessageList_Group(_isTemp, 15, _currentPage, out _pageCount);
				_cmdCtrl.State["PageCount"] = PageCount;
			}

			StringBuilder builder = new StringBuilder();
			foreach (DataRow row in _msgs.Rows)
			{
				builder.AppendFormat(
					MsgListTrFormat,
					row["GroupID"],
					HtmlUtil.ReplaceHtml(row["GroupNickname"].ToString()),
					row["MessagesCount"],
					row["LatestTime"],
					Utility.RenderJson(Microsoft.JScript.GlobalObject.escape(Request.Url.ToString()))
				);
			}
			return builder.ToString();
		}
	}
}
