using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Core;

namespace Core.Web
{
	public partial class Lesktop_Management_EmbedCode : System.Web.UI.Page
	{
		CommandCtrl _cmdCtrl;
		PlaceHolder _ph = null;

		protected void Page_Load(object sender, EventArgs e)
		{
			_cmdCtrl = FindControl("CommandCtrl") as CommandCtrl;
			_ph = FindControl("PlaceHolder1") as PlaceHolder;
			_cmdCtrl.OnCommand += new CommandCtrl.OnCommandDelegate(cmdCtrl_OnCommand);
			_cmdCtrl.State["Action"] = null;
		}

		private void cmdCtrl_OnCommand(string command, object data)
		{
			if (command == "Create")
			{
				int id = AccountImpl.Instance.CreateEmbedCode("", Utility.RenderJson(data));
				_cmdCtrl.State["Action"] = "Edit";
				_cmdCtrl.State["ID"] = id;
			}
			else if (command == "Update")
			{
				Hashtable data_hash = data as Hashtable;
				AccountImpl.Instance.UpdateEmbedCode(
					Convert.ToInt32(data_hash["ID"]),
					(data_hash["EmbecConfig"] as Hashtable)["Users"].ToString(),
					Utility.RenderJson(data_hash["EmbecConfig"])
				);
				_cmdCtrl.State["Action"] = "Scroll";
				_cmdCtrl.State["ID"] = Convert.ToInt32(data_hash["ID"]);
			}
			else if (command == "Delete")
			{
				Hashtable data_hash = data as Hashtable;
				AccountImpl.Instance.DeleteEmbedCode(Convert.ToInt32(data_hash["ID"]));
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			DataTable dt = AccountImpl.Instance.GetAllEmbedCode();
			foreach (DataRow row in dt.Rows)
			{
				Lesktop_EmbedCode_EmbedCodeItem ctrl = LoadControl("EmbedCodeItem.ascx") as Lesktop_EmbedCode_EmbedCodeItem;
				ctrl.SetData(row);
				_ph.Controls.Add(ctrl);
			}
			base.OnPreRender(e);
		}
	}

	public class EmbedCodeConfig
	{
		private Hashtable _config = null;

		public EmbedCodeConfig(string json)
		{
			_config = Utility.ParseJson(json) as Hashtable;
		}

		public Boolean ShowWindow
		{
			get { return _config.ContainsKey("ShowWindow") ? Convert.ToBoolean(_config["ShowWindow"]) : true; }
		}

		public Boolean NewWindow
		{
			get { return _config.ContainsKey("NewWindow") ? Convert.ToBoolean(_config["NewWindow"]) : false; }
		}

		public int Width
		{
			get { return _config.ContainsKey("Width") ? Convert.ToInt32(_config["Width"]) : 120; }
		}

		public int Height
		{
			get { return _config.ContainsKey("Height") ? Convert.ToInt32(_config["Height"]) : 400; }
		}

		public String Name
		{
			get { return _config.ContainsKey("Name") ? _config["Name"].ToString() : ""; }
		}

		public String Users
		{
			get { return _config.ContainsKey("Users") ? _config["Users"].ToString() : ""; }
		}

		public String UsersPreview
		{
			get { return _config.ContainsKey("UsersPreview") ? _config["UsersPreview"].ToString() : ""; }
		}
	}
}