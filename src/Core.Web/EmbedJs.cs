using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace Core.Web
{
	public class EmbedJs : IHttpHandler
	{
		static String AppJs =
		"document.write(\'<script src=\"{1}/Applications.js?\' + (new Date()).getTime() + \'\" type=\"text/javascript\"></\' + \'script>\');\r\n";
		
		static string EmbedJsFormat =
		"document.write('<link href=\"{2}/Themes/Default/WebDesktop/Desktop.css\" rel=\"stylesheet\" type=\"text/css\" />');\r\n" +
		"document.write('<script src=\"{2}/Core/Config.ashx\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/Core/Common.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/Core/Extent.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		(ServerImpl.Instance.EnbaleDynamicApp ? AppJs : "") +
		"document.write('<script src=\"{2}/Core/Plugins.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/Core/Main.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/WebDesktop/Desktop.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/WebDesktop/Menu.js\" type=\"text/javascript\"><'+'/script>');\r\n" +
		"document.write('<script src=\"{2}/WebDesktop/Window.js\" type=\"text/javascript\"><'+'/script>');\r\n";
		
		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			var resRoot = ServerImpl.Instance.AppPath;
			if (!resRoot.EndsWith("/")) resRoot += "/";
			resRoot += ServerImpl.Instance.ResPath;
			string js = String.Format(EmbedJsFormat, ServerImpl.Instance.Version, ServerImpl.Instance.AppPath, resRoot, ServerImpl.Instance.ResPath);

			context.Response.ContentType = "application/x-javascript";
			context.Response.Write(js);
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}