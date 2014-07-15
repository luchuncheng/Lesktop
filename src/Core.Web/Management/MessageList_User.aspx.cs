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
	public partial class Lesktop_MsgHistory_MessageList_User : System.Web.UI.Page
	{
		static String MsgListTrFormat = @"
		<tr>
			<td class='checkbox'><input id='cb_{4}_{5}' value='{4},{5}' type='checkbox' /></td>
			<td class='user' style='color:#AA0000;'>{0} - {1}</td>
			<td class='msgcount'>{2}</td>
			<td class='renewtime'>{3: yyyy-MM-dd HH:mm:ss}</td>
			<td class='operation'><a href='javascript:ViewMessages({4}, {5}, {6})'>查看消息</a></td>
		</tr>
		";

		protected int _currentPage;
		protected int _pageCount;
		protected int _userId;
		protected int _peerId;
		DataTable _lists;
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

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			_currentPage = Request.QueryString["CurrentPage"] == null ? 1 : Convert.ToInt32(Request.QueryString["CurrentPage"]);

			_userId = Request.QueryString["UserID"] == null ? 0 : Convert.ToInt32(Request.QueryString["UserID"]);
			_peerId = Request.QueryString["PeerID"] == null ? 0 : Convert.ToInt32(Request.QueryString["PeerID"]);

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
			if (_lists == null)
			{
				_lists = MessageImpl.Instance.GetMessageList_User(_userId, _peerId, 15, _currentPage, out _pageCount);
				_cmdCtrl.State["PageCount"] = PageCount;
			}
			StringBuilder builder = new StringBuilder();
			foreach (DataRow row in _lists.Rows)
			{
				builder.AppendFormat(
					MsgListTrFormat,
					HtmlUtil.ReplaceHtml(row["UserNickname"].ToString()),
					HtmlUtil.ReplaceHtml(row["PeerNickname"].ToString()),
					row["MessagesCount"],
					row["LatestTime"],
					row["UserID"],
					row["PeerID"],
					Utility.RenderJson(Microsoft.JScript.GlobalObject.escape(Request.Url.ToString()))
				);
			}
			return builder.ToString();
		}
	}
}