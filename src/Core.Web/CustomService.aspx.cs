using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using Core;

namespace Core.Web
{
	public partial class Lesktop_CustomService : System.Web.UI.Page
	{
		static Regex IPReg = new Regex(@"[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}");
		protected void Page_Load(object sender, EventArgs e)
		{
			try
			{
				String sessionId = Guid.NewGuid().ToString().ToUpper();
				AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);

				if (cu == null || !cu.IsTemp)
				{
#				if DEBUG
					string ip = "117.136.10.171";
#				else
				string ip = Context.Request.ServerVariables["REMOTE_ADDR"];
				if (!IPReg.IsMatch(ip)) ip = "";
#				endif

					int id = AccountImpl.Instance.CreateTempUser(ip);
					cu = AccountImpl.Instance.GetUserInfo(id);
				}

				HtmlInputHidden data_json = FindControl("data_json") as HtmlInputHidden;
				int peerId = AccountImpl.Instance.GetUserID(Request.QueryString["Peer"]);
				data_json.Value = Utility.RenderHashJson(
					"User", cu.DetailsJson,
					"Peer", AccountImpl.Instance.GetUserInfo(peerId).DetailsJson,
					"SessionID", sessionId
				);

				Core.ServerImpl.Instance.Login(sessionId, Context, cu.ID, false, DateTime.Now.AddYears(1));

				Response.AddHeader("P3P", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
			}
			catch (Exception ex)
			{
				ServerImpl.Instance.WriteLog(String.Format("{0}:\r\n{1}", ex.GetType().Name, ex.StackTrace));
				throw;
			}
		}
	}
}