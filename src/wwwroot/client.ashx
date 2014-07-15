<%@ WebHandler Language="C#" Class="client" %>

using System;
using System.Web;
using System.IO;
using System.Text;

public class client : IHttpHandler
{
    byte[] command_line_ = new byte[2048];
	int command_line_flag_pos_ = Convert.ToInt32("{{COMMANDLINE_POS}}");

    static byte[] client_exe_ = null;
    static object client_exec_lock_ = new object();

    public void ProcessRequest(HttpContext context)
    {
		lock (client_exec_lock_)
		{
		    try
		    {
		        if (client_exe_ == null)
		        {
		            string path = Path.GetDirectoryName(context.Request.PhysicalPath) + "\\client.exe";
		            client_exe_ = File.ReadAllBytes(path);
		        }
		    }
		    catch
		    {
		        context.Response.StatusCode = 404;
		        context.Response.End();
		        return;
		    }
		}

        context.Response.ContentType = "application/octet-stream";
        context.Response.AppendHeader("Content-Disposition", "attachment;filename=client.exe");

		StringBuilder cmdline = new StringBuilder();
		foreach(string key in context.Request.QueryString.AllKeys)
		{
			if(cmdline.Length > 0) cmdline.Append(" ");
			string val = context.Request.QueryString[key];
			if (String.IsNullOrEmpty(val))
			{
				cmdline.AppendFormat("/{0}", key);
			}
			else
			{
				cmdline.AppendFormat("/{0} {1}", key, val);
			}
		}
		cmdline.Append("\0");

		int count = System.Text.Encoding.Unicode.GetBytes(cmdline.ToString(), 0, cmdline.Length, command_line_, 0);
        if (count > 0)
        {
            context.Response.AppendHeader("Content-Length", client_exe_.Length.ToString());
            context.Response.OutputStream.Write(client_exe_, 0, command_line_flag_pos_);
            context.Response.OutputStream.Write(command_line_, 0, command_line_.Length);
            context.Response.OutputStream.Write(client_exe_, command_line_flag_pos_ + 2048, client_exe_.Length - (command_line_flag_pos_ + 2048));
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}