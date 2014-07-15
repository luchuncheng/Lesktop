using System;
using System.Web;
using System.Collections;

namespace Core.Web
{
	using Core.Text;

	public class EmbedCS : IHttpHandler
	{
		static object EmbedJsTemp_Lock = new object();
		static TextTemplate EmbedJsTemp = null;
		static DateTime EmbedJsTempLWT;

		public void ProcessRequest(HttpContext context)
		{
			lock (EmbedJsTemp_Lock)
			{
				string tempPath = context.Server.MapPath(String.Format("~/{0}/Core/EmbedCS.js", ServerImpl.Instance.ResPath));
				System.IO.FileInfo info = new System.IO.FileInfo(tempPath);
				if (EmbedJsTemp == null || info.LastWriteTime > EmbedJsTempLWT)
				{
					EmbedJsTempLWT = info.LastWriteTime;
					EmbedJsTemp = new TextTemplate(tempPath, System.Text.Encoding.UTF8);
				}
			}

			context.Response.ContentType = "application/x-javascript";
			context.Response.AppendHeader("Content-Disposition", "filename=EmbedCS.js");

			Int32 user_id = AccountImpl.Instance.GetUserID(context.Request.QueryString["CSR"]);
			AccountInfo peerInfo = AccountImpl.Instance.GetUserInfo(user_id);

			string embed_config = String.Format(
				@"
				if(window.__embed_config == undefined) window.__embed_config = {{}};
				if(window.__embed_config.Users == undefined) window.__embed_config.Users = {{}};
				window.__embed_config.User = {1};
				window.__embed_config.Users[{0}] = window.__embed_config.User;
				window.__embed_config.DefaultCss = {2};
				",
				Core.Utility.RenderJson(peerInfo.Name.ToUpper()),
				Core.Utility.RenderJson(peerInfo.DetailsJson),
				Core.Utility.RenderJson(context.Request.QueryString["DefaultCss"] == null ? true : Boolean.Parse(context.Request.QueryString["DefaultCss"]))
			);
			context.Response.Write(embed_config);

			string host = context.Request.Url.Host;
			while (host.EndsWith("/")) host = host.Substring(0, host.Length - 1);
			if (context.Request.Url.Port != 80) host += String.Format(":{0}", context.Request.Url.Port);

			Hashtable values = new Hashtable();
			values["VERSION"] = ServerImpl.Instance.Version;
			values["CSURL"] = String.Format("http://{0}{1}/{2}/CustomService.aspx", host,
				ServerImpl.Instance.AppPath == "/" ? "" : "/", ServerImpl.Instance.ResPath);

			context.Response.Write(EmbedJsTemp.Render(values));
		}

		private string DecodeEmbedConfig(string str)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			for (int i = 0; i < str.Length; i += 4)
			{
				if (i + 4 <= str.Length)
				{
					builder.Append((Char)Convert.ToInt16(str.Substring(i, 4), 16));
				}
			}
			return builder.ToString();
		}

		public bool IsReusable
		{
			get { return false; }
		}

	}
}