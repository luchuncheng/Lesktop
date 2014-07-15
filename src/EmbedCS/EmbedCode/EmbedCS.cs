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

		static object EmbedNWJsTemp_Lock = new object();
		static TextTemplate EmbedNWJsTemp = null;
		static DateTime EmbedNWJsTempLWT;

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

			lock (EmbedNWJsTemp_Lock)
			{
				string tempPath = context.Server.MapPath(String.Format("~/{0}/Core/EmbedCS_NW.js", ServerImpl.Instance.ResPath));
				System.IO.FileInfo info = new System.IO.FileInfo(tempPath);
				if (EmbedNWJsTemp == null || info.LastWriteTime > EmbedNWJsTempLWT)
				{
					EmbedNWJsTempLWT = info.LastWriteTime;
					EmbedNWJsTemp = new TextTemplate(tempPath, System.Text.Encoding.UTF8);
				}
			}

			context.Response.ContentType = "application/x-javascript";
			context.Response.AppendHeader("Content-Disposition", "filename=EmbedCS_NW.js");

			Hashtable data = null;
			string embedConfig = null;
			var id = context.Request.QueryString["ID"] != null ? Convert.ToInt32(context.Request.QueryString["ID"]) : 0;
			if (id > 0)
			{
				System.Data.DataRow row = AccountImpl.Instance.GetEmbedCode(id);
				embedConfig = row["EmbedConfig"].ToString();
				data = Utility.ParseJson(embedConfig) as Hashtable;
			}
			else
			{
				embedConfig = DecodeEmbedConfig(context.Request.QueryString["EmbedConfig"]);
				data = Utility.ParseJson(embedConfig) as Hashtable;
			}

			System.Data.DataTable dt = AccountImpl.Instance.GetServiceEmbedData(data["Users"].ToString());
			dt.Columns.Add("IsOnline", typeof(Boolean));
			foreach (System.Data.DataRow r in dt.Rows)
			{
				r["IsOnline"] = SessionManagement.Instance.IsOnline(Convert.ToInt32(r["ID"]));
			}

			string embed_config = String.Format(
				"var __embed_config = {0};\r\n__embed_config.Details={1}\r\n\r\n",
				embedConfig,
				Utility.RenderJson(dt.Rows)
			);
			context.Response.Write(embed_config);

			string host = context.Request.Url.Host;
			while (host.EndsWith("/")) host = host.Substring(0, host.Length - 1);
			if (context.Request.Url.Port != 80) host += String.Format(":{0}", context.Request.Url.Port);

			Hashtable values = new Hashtable();
			values["VERSION"] = ServerImpl.Instance.Version;
			string serviceurl = "http://" + host + ServerImpl.Instance.AppPath;
			if (!serviceurl.EndsWith("/")) serviceurl += "/";
			serviceurl += ServerImpl.Instance.ResPath;
			values["SERVICEURL"] = serviceurl;

			if (Convert.ToBoolean(data["NewWindow"]) && !Convert.ToBoolean(data["ShowWindow"])) context.Response.Write(EmbedNWJsTemp.Render(values));
			else context.Response.Write(EmbedJsTemp.Render(values));
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