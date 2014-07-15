using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;

namespace Core.Web
{
	public partial class CommandCtrl : System.Web.UI.UserControl
	{
		public delegate void OnCommandDelegate(string cmd, object data);

		public event OnCommandDelegate OnCommand;

		private Hashtable _state = new Hashtable();

		public Hashtable State
		{
			get { return _state; }
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			OnCommand += new OnCommandDelegate(CommandCtrl_OnCommand);

			if (!String.IsNullOrEmpty(Request.Params[ClientID + "_state_json"]))
			{
				_state = Core.Utility.ParseJson(Request.Params[ClientID + "_state_json"]) as Hashtable;
			}
		}

		private void CommandCtrl_OnCommand(string command, object data)
		{

		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(Request.Params[ClientID + "_command"]))
			{
				OnCommand(
					Request.Params[ClientID + "_command"],
					Core.Utility.ParseJson(Request.Params[ClientID + "_data"])
				);
			}
		}

		protected String StateJson
		{
			get { return Core.Utility.RenderJson(_state).Replace("\"", "&quot;").Replace("<", "&lt;"); }
		}

		protected String StateVarName
		{
			get { return "__" + ClientID + "_state"; }
		}

		static string ScriptFormat =
		"if(window.Core == undefined)\r\n" +
		"{{\r\n" +
		"    document.write('<script src=\"{0}/Core/Common.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"}}\r\n";

		protected String GetScriptJs()
		{
			var resRoot = ServerImpl.Instance.AppPath;
			if (!resRoot.EndsWith("/")) resRoot += "/";
			resRoot += ServerImpl.Instance.ResPath;
			return String.Format(ScriptFormat, resRoot);
		}
	}
}