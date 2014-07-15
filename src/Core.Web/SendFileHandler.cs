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
	class SendFileHandler : IHttpHandler
	{
		static Hashtable UnsupportExt = new Hashtable();

		public SendFileHandler()
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

			if (context.Request.Files.Count > 0)
			{
				HttpPostedFile file = context.Request.Files[0];
				String filename = Core.ServerImpl.Instance.GetFullPath(context, "Temp") + "/" + Guid.NewGuid().ToString();
				Core.IO.Directory.CreateDirectory(filename);
				filename += "/" + file.FileName;
				Exception error = null;
				try
				{
					string ext = System.IO.Path.GetExtension(file.FileName);
					if (UnsupportExt.ContainsKey(ext.ToUpper()))
					{
						throw new Exception(String.Format("不支持上传此类型(*{0})的文件！", ext));
					}
					file.SaveAs(ServerImpl.Instance.MapPath(filename));
				}
				catch (Exception e)
				{
					error = e;
				}
				if (error == null) context.Response.Write(Core.Utility.RenderHashJson("Result", true, "Path", filename));
				else context.Response.Write(Core.Utility.RenderHashJson("Result", false, "Exception", error));
			}
			else
			{
				String name = context.Request.Headers["Content-Disposition"];
				System.IO.Stream inputStream = context.Request.InputStream;
				String filename = Core.ServerImpl.Instance.GetFullPath(context, "Temp") + "/" + Guid.NewGuid().ToString();
				Core.IO.Directory.CreateDirectory(filename);
				filename += "/" + context.Request.Headers["Content-Disposition"];
				Exception error = null;
				try
				{
					string ext = System.IO.Path.GetExtension(name);
					if (UnsupportExt.ContainsKey(ext.ToUpper()))
					{
						throw new Exception(String.Format("不支持上传此类型(*{0})的文件！", ext));
					}
					using (System.IO.Stream stream = File.Create(filename))
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
				if (error == null) context.Response.Write(Core.Utility.RenderHashJson("Result", true, "Path", filename));
				else context.Response.Write(Core.Utility.RenderHashJson("Result", false, "Exception", error));
			}

		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}
