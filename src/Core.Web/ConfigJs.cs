using System;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using System.Xml;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Core.IO;
using System.Threading;

namespace Core.Web
{
	class ConfigJs : IHttpHandler
	{
		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			String js = String.Format(
				@"
				if(window.Core == undefined) window.Core = {{}};
				Core.Config = {{
					Version: '{0}',
					AppPath: '{1}',
					ResPath: '{2}'
				}};",
				ServerImpl.Instance.Version,
				ServerImpl.Instance.AppPath,
				ServerImpl.Instance.ResPath
			);

			context.Response.ContentType = "application/x-javascript";
			context.Response.Write(js);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}
