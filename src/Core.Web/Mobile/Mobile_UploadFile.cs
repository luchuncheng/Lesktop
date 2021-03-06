﻿using System;
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
	class Mobile_UploadFile : IHttpHandler
	{
		static Hashtable UnsupportExt = new Hashtable();

		public Mobile_UploadFile()
		{
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			lock (UnsupportExt)
			{
				if (UnsupportExt.Count == 0)
				{
					UnsupportExt.Add(".CONFIG", "CONFIG");
				}
			}
			string file_type = context.Request.QueryString["type"].ToLower();
			String filepath = "";
			Exception error = null;
			try
			{
				HttpPostedFile postfile = context.Request.Files["file"];
				String name = System.IO.Path.GetFileName(postfile.FileName);
				string ext = System.IO.Path.GetExtension(name);
				System.IO.Stream inputStream = postfile.InputStream;

				filepath = Core.ServerImpl.Instance.GetFullPath(context, "Temp") + "/Mobile/";

				if (file_type == "file")
				{
					filepath += Utility.GenUniqueFileName();
					if (!Core.IO.Directory.Exists(filepath))
					{
						Core.IO.Directory.CreateDirectory(filepath);
					}
					filepath += "/" + name;
				}
				else
				{
					filepath += "Images";
					if (!Core.IO.Directory.Exists(filepath))
					{
						Core.IO.Directory.CreateDirectory(filepath);
					}
					filepath += "/" + Utility.GenUniqueFileName() + ext;
				}

				if (UnsupportExt.ContainsKey(ext.ToUpper()))
				{
					throw new Exception(String.Format("不支持上传此类型(*{0})的文件！", ext));
				}
				using (System.IO.Stream stream = File.Create(filepath))
				{
					byte[] buffer = new byte[4 * 1024];
					while (true)
					{
						int c = inputStream.Read(buffer, 0, buffer.Length);
						if (c == 0) break;
						stream.Write(buffer, 0, c);
					}
				}
			}
			catch (Exception e)
			{
				error = e;
			}

			if (error == null)
			{
				string url = ServerImpl.Instance.AppPath;
				if (!url.EndsWith("/")) url += "/";
				url += String.Format("{0}/Download.ashx?FileName={1}", ServerImpl.Instance.ResPath, Utility.Escape(filepath));
				if (file_type == "image")
				{
					string maxwidth_qs = context.Request.QueryString["maxwidth"];
					string maxheight_qs = context.Request.QueryString["maxheight"];
					if (maxwidth_qs != null && maxheight_qs != null)
					{
						url += String.Format("&MaxWidth={0}&MaxHeight={1}", maxwidth_qs, maxheight_qs);
					}
				}
				Hashtable data = new Hashtable();
				data["src"] = url;
				context.Response.Write(Core.Utility.RenderHashJson("code", 0, "msg", "", "data", data));
			}
			else
			{
				context.Response.Write(Core.Utility.RenderHashJson("code", 1, "msg", error.Message));
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}
