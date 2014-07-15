using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


namespace Core.Web
{
	public partial class Message_ViewMessages : System.Web.UI.Page
	{
		Int32 _pageCount = 0;
		Int32 _currentPage = 0;

		public Int32 PageCount
		{
			get { return _pageCount; }
		}

		public Int32 CurrentPage
		{
			get { return _currentPage == -1 || _currentPage > PageCount ? PageCount : _currentPage; }
		}

		public String ReturnUrl
		{
			get { return Microsoft.JScript.GlobalObject.unescape(Request.QueryString["ReturnUrl"]); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu == null || !cu.IsAdmin) throw new Exception("你没有权限访问该页面！");

			if (IsPostBack)
			{
				switch (Request.Params["command"])
				{
				case "Delete":
					{
						Core.MessageImpl.Instance.DeleteMessages(Request.Params["data"]);
						break;
					}
				}
			}

			DateTime from, to, baseDate = new DateTime(1900, 1, 1);

			from = Request.QueryString["From"] == null ? new DateTime(1900, 1, 1) : baseDate.AddMilliseconds(Convert.ToInt32(Request.QueryString["From"]));
			to = Request.QueryString["To"] == null ? new DateTime(2100, 1, 1) : baseDate.AddMilliseconds(Convert.ToInt32(Request.QueryString["To"]));
			
			Int32 pageSize = 15;
			Int32 user = Request.QueryString["User"] == null ? 0 : Convert.ToInt32(Request.QueryString["User"]);
			Int32 peer = Request.QueryString["Peer"] == null ? 0 : Convert.ToInt32(Request.QueryString["Peer"]);
			int msgId = 0;
			if (Request.QueryString["CurrentPage"] != null)
			{
				_currentPage = Convert.ToInt32(Request.QueryString["CurrentPage"]);
				msgId = 0;
			}
			else
			{
				_currentPage = -1;
				msgId = Request.QueryString["MsgID"] == null ? 0 : Convert.ToInt32(Request.QueryString["MsgID"]);
			}

			Core.MessageImpl.Instance.WriteCache();

			DataTable msgs = null;
			if (peer > 0) msgs = Core.MessageImpl.Instance.GetUserMessages(user, peer, from, to, pageSize, ref _currentPage, out _pageCount, msgId);
			else msgs = Core.MessageImpl.Instance.GetGroupMessages(user, from, to, pageSize, ref _currentPage, out _pageCount, msgId);


			foreach (DataRow row in msgs.Rows)
			{
				Message_MessageCtrl ctrl = LoadControl("MessageCtrl.ascx") as Message_MessageCtrl;
				ctrl.Data = row;
				FindControl("MsgContainer").Controls.Add(ctrl);
			}
		}
	}
}