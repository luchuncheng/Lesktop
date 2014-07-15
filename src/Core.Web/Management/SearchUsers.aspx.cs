using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Core;


namespace Core.Web
{
	public partial class Management_SearchUsers : System.Web.UI.Page
	{
		static string RowFormat =
		@"
	<tr>
		<td class='headimg'><input type='checkbox' id='cb_{4}' value='{4}'></td>
		<td class='name'>{1}</td>
		<td class='nickname'>{2}</td>
		<td class='email'>{3}</td>
	</tr>
	";
		static string RowFormatRed =
		@"
	<tr>
		<td class='headimg' style='color:#AA0000;'><input checked type='checkbox' id='cb_{4}' value='{4}'></td>
		<td class='name' style='color:#AA0000;'>{1}</td>
		<td class='nickname' style='color:#AA0000;'>{2}</td>
		<td class='email' style='color:#AA0000;'>{3}</td>
	</tr>
	";

		CommandCtrl _cmdCtrl;

		protected DataRowCollection Rows = null;
		protected DataRowCollection PreSelRows = null;
		protected String IDS = "";

		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
			_cmdCtrl.State["Action"] = null;
			if (!IsPostBack)
			{
				_cmdCtrl.State["IDS"] = "";
				_cmdCtrl.State["Keyword"] = "";

				PlaceHolder ph1 = FindControl("PlaceHolder1") as PlaceHolder;
				ph1.Visible = false;
			}

			if (!String.IsNullOrEmpty(_cmdCtrl.State["IDS"].ToString()))
			{
				PreSelRows = AccountImpl.Instance.SearchUsers(_cmdCtrl.State["IDS"].ToString(), "MultiID");
			}

			(FindControl("PlaceHolder2") as PlaceHolder).Visible = PreSelRows != null && PreSelRows.Count > 0;
			(FindControl("PlaceHolder3") as PlaceHolder).Visible = !(PreSelRows != null && PreSelRows.Count > 0);
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			string peer = Convert.ToString(data);
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

			try
			{
				if (command == "Search")
				{
					Hashtable hash_data = data as Hashtable;
					Rows = AccountImpl.Instance.SearchUsers(hash_data["Keyword"].ToString(), "");

					PlaceHolder ph1 = FindControl("PlaceHolder1") as PlaceHolder;
					ph1.Visible = (Rows != null && Rows.Count > 0) || PreSelRows != null && PreSelRows.Count > 0;
					(FindControl("search_keyword") as HtmlInputText).Value = hash_data["Keyword"].ToString();
				}
			}
			catch (Exception ex)
			{
				_cmdCtrl.State["Action"] = "Error";
				_cmdCtrl.State["Exception"] = ex;
			}
		}

		protected String RenderLastSelList()
		{
			StringBuilder builder = new StringBuilder();
			if (PreSelRows != null)
			{
				foreach (DataRow row in PreSelRows)
				{
					builder.AppendFormat(
						RowFormatRed, "",
						HtmlUtil.ReplaceHtml(row["Name"].ToString()),
						HtmlUtil.ReplaceHtml(row["Nickname"].ToString()),
						HtmlUtil.ReplaceHtml(row["DeptName"].ToString()).Replace(",", "<br/>"),
						Convert.ToInt32(row["ID"]),
						Core.Utility.RenderJson(row["Nickname"].ToString()),
						Core.Utility.RenderJson(row["Name"].ToString()),
						row["RegisterTime"]
					);
				}
			}
			return builder.ToString();
		}

		protected String RenderAllUsersList()
		{
			StringBuilder builder = new StringBuilder();
			if (Rows != null)
			{
				foreach (DataRow row in Rows)
				{
					builder.AppendFormat(
						RowFormat, "",
						HtmlUtil.ReplaceHtml(row["Name"].ToString()),
						HtmlUtil.ReplaceHtml(row["Nickname"].ToString()),
						HtmlUtil.ReplaceHtml(row["DeptName"].ToString()).Replace(",", "<br/>"),
						Convert.ToInt32(row["ID"]),
						Core.Utility.RenderJson(row["Nickname"].ToString()),
						Core.Utility.RenderJson(row["Name"].ToString()),
						row["RegisterTime"]
					);
				}
			}
			return builder.ToString();
		}
	}
}