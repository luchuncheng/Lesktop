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
using Core;

public partial class Comment_ImportantComment : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsPostBack)
		{
			switch (Request.Params["hidCommnad"])
			{
			case "MarkAsUnread":
				{
					ServerImpl.Instance.CommonStorageImpl.MarkAsUnread(Convert.ToInt32(Request.Params["hidData"]));
					break;
				}
			case "MarkAsImportant":
				{
					ServerImpl.Instance.CommonStorageImpl.MarkAsImportant(Convert.ToInt32(Request.Params["hidData"]));
					break;
				}
			case "MarkAsRead":
				{
					ServerImpl.Instance.CommonStorageImpl.MarkAsRead(Convert.ToInt32(Request.Params["hidData"]));
					break;
				}
			case "MarkAsUnimportant":
				{
					ServerImpl.Instance.CommonStorageImpl.MarkAsUnimportant(Convert.ToInt32(Request.Params["hidData"]));
					break;
				}
			case "Delete":
				{
					ServerImpl.Instance.CommonStorageImpl.DeleteComment(Convert.ToInt32(Request.Params["hidData"]));
					break;
				}
			}
		}
#		if DEBUG
		System.Threading.Thread.Sleep(500);
#		endif
		ShowImportantComment();
	}

	int _count = 0;

	public int Count
	{
		get { return _count; }
	}

	private void ShowImportantComment()
	{
		AccountInfo current_User = AccountImpl.Instance.GetUserInfo(UserID);

		DataTable dt = ServerImpl.Instance.CommonStorageImpl.GetImportantComment(current_User.IsAdmin ? 0 : current_User.ID);

		_count = dt.Rows.Count;

		foreach (DataRow row in dt.Rows)
		{
			Comment_CommenPanel cp = LoadControl("CommenPanel.ascx") as Comment_CommenPanel;
			cp.Data = row;
			Form.Controls.Add(cp);
		}
	}

	public int UserID
	{
		get { return ServerImpl.Instance.GetUserID(Context); }
	}
}
