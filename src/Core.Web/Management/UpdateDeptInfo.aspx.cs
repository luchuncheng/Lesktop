using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Core;
using Core.IO;


namespace Core.Web
{
	public partial class Lesktop_Management_UpdateDeptInfo : System.Web.UI.Page
	{
		protected DataRow Data = null;

		protected int DeptID
		{
			get { return Convert.ToInt32(Data["ID"]); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			Data = AccountImpl.Instance.GetDeptInfo(Convert.ToInt32(Request.QueryString["DeptID"]));

			CommandCtrl cmdCtrl = FindControl("CommandCtrl1") as CommandCtrl;
			cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);

			cmdCtrl.State["Data"] = Data;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			CommandCtrl cmdCtrl = FindControl("CommandCtrl1") as CommandCtrl;
			try
			{
				if (command == "Update")
				{
					Hashtable dept_info = data as Hashtable;
					AccountImpl.Instance.UpdateDeptInfo(Convert.ToInt32(Data["ID"]), dept_info["Name"].ToString());
					SessionManagement.Instance.NotifyResetListCache(3, false, true, null);
					Data = AccountImpl.Instance.GetDeptInfo(Convert.ToInt32(Request.QueryString["DeptID"]));
					cmdCtrl.State["Action"] = "ResetDeptData";
					cmdCtrl.State["DeptID"] = DeptID;
					cmdCtrl.State["DeptInfo"] = AccountImpl.Instance.GetDeptInfo(DeptID);
				}
			}
			catch (Exception ex)
			{
				cmdCtrl.State["Action"] = "Alert";
				cmdCtrl.State["Message"] = ex.Message;
			}
		}
	}
}