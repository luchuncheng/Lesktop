using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using Microsoft.JScript;
using Convert = System.Convert;

/// <summary>
///DownloadHandler 的摘要说明
/// </summary>
namespace Core.Web
{
	public class DownloadHandler : IHttpHandler
	{
		static Hashtable ContentType = new Hashtable();

		static DownloadHandler()
		{
			ContentType[".CRX"] = "application/x-chrome-extension";
			ContentType[".JS"] = "application/x-javascript";
			ContentType[".XML"] = "text/xml";
			ContentType[".HTML"] = "text/html";
			ContentType[".HTM"] = "text/html";
			ContentType[".CSS"] = "text/css";
			ContentType[".TXT"] = "text";
			ContentType[".PNG"] = "image";
			ContentType[".BMP"] = "image";
			ContentType[".GIF"] = "image";
			ContentType[".JPG"] = "image";
			ContentType[".ICO"] = "image";
			ContentType[".CUR"] = "image";
		}

		public DownloadHandler()
		{
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			try
			{
				string fileName = "";
				string reqFilename = Path.GetFileName(context.Request.Url.AbsolutePath).ToLower();
				System.IO.Stream stream = null;
				if (reqFilename == "headimg.ashx")
				{
					int userid = Convert.ToInt32(context.Request.QueryString["user"]);
					int size = Convert.ToInt32(context.Request.QueryString["size"] ?? "0");
					bool gred = Convert.ToBoolean(context.Request.QueryString["gred"] ?? "false");

					AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(userid);

					string preFileName = "";
					if (String.IsNullOrEmpty(userInfo.HeadIMG))
					{
						preFileName = context.Server.MapPath("~/" + ServerImpl.Instance.ResPath + "/Themes/Default/HeadIMG/user");
					}
					else
					{
						preFileName = ServerImpl.Instance.MapPath(userInfo.HeadIMG);
						if (!System.IO.File.Exists(preFileName))
						{
							throw new Exception();
						}
						ServerImpl.Instance.CheckPermission(context, userInfo.HeadIMG, IOPermission.Read);
					}
					fileName = preFileName;
					if (gred) fileName += ".gred";
					if (size > 0) fileName += String.Format(".{0}", size);
					if (fileName != preFileName)
					{
						fileName += ".png";
						if (!File.Exists(fileName))
						{
							fileName = ZoomHeadImage(preFileName, fileName, size, size, gred);
						}
					}
					stream = System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
				}
				else
				{
					fileName = ServerImpl.Instance.GetFullPath(context, GlobalObject.unescape(context.Request["FileName"]));
					ServerImpl.Instance.CheckPermission(context, fileName, IOPermission.Read);
					stream = Core.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
				}
				if(stream == null) throw new Exception();
				try
				{
					string ext = Path.GetExtension(fileName).ToUpper();
					if (ContentType.ContainsKey(ext))
					{
						context.Response.ContentType = ContentType[ext] as string;
						context.Response.AppendHeader("Content-Disposition", string.Format("filename={0}{1}", UTF8_FileName(Path.GetFileNameWithoutExtension(fileName)), Path.GetExtension(fileName)));
					}
					else
					{
						context.Response.ContentType = "application/octet-stream";
						context.Response.AppendHeader("Content-Disposition", string.Format("attachment;filename={0}{1}", UTF8_FileName(Path.GetFileNameWithoutExtension(fileName)), Path.GetExtension(fileName)));
					}


					FileSystemInfo fileInfo = new FileInfo(fileName);
					if (fileInfo != null)
					{
						context.Response.AppendHeader("Last-Modified", String.Format("{0:R}", fileInfo.LastWriteTime));
					}

					context.Response.AppendHeader("Content-Length", stream.Length.ToString());

					byte[] buffer = new byte[4 * 1024];
					while (true)
					{
						int c = stream.Read(buffer, 0, buffer.Length);
						if (c == 0) break;
						context.Response.OutputStream.Write(buffer, 0, c);
						context.Response.Flush();
					}
				}
				finally
				{
					stream.Close();
				}
			}
			catch
			{
				context.Response.StatusCode = 404;
				context.Response.End();
			}
		}

		bool NeedEncode(string s)
		{
			foreach (char c in s)
			{
				if (!(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '.')) return true;
			}
			return false;
		}

		string UTF8_FileName(string fn)
		{
			if (!NeedEncode(fn)) return fn;
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(fn);
			System.Text.StringBuilder builder=new System.Text.StringBuilder();
			foreach(byte b in buffer)
			{
				builder.AppendFormat("%{0:X2}", b);
			}
			return builder.ToString();
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		private static string ZoomHeadImage(String headImg, String targetImg, int width, int height, bool gred)
		{
			Bitmap zoomImg = null;
			using (Stream stream = File.Open(headImg, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				Bitmap img = new Bitmap(stream);
				if (width > 0 && height > 0 && (img.Width > width || img.Height > height || stream.Length > 20 * 1024))
				{
					if (Path.GetExtension(targetImg).ToLower() != ".png") targetImg += ".png";

					int newWidth, newHeight;
					if (img.Width * height > img.Height * width)
					{
						newWidth = width;
						newHeight = img.Height * width / img.Width;
					}
					else
					{
						newHeight = height;
						newWidth = img.Width * height / img.Height;
					}
					zoomImg = new Bitmap(img, new Size(newWidth, newHeight));
					if (gred) zoomImg = Core.Utility.ToGray(zoomImg, 0);
				}
				else
				{
					if (gred)
					{
						zoomImg = Core.Utility.ToGray(img, 0);
					}
					else
					{
						zoomImg = img;
					}
				}
			}

			using (Stream target_stream = File.Open(targetImg, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Bitmap t_img = new Bitmap(width, height);
				Graphics graphics = Graphics.FromImage(t_img);
				graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));
				graphics.DrawImage(zoomImg, (width - zoomImg.Width) / 2, (height - zoomImg.Height) / 2);
				t_img.Save(target_stream, ImageFormat.Png);
			}

			return targetImg;
		}


	}
}