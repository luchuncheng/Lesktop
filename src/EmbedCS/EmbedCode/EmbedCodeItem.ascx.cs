using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Lesktop_EmbedCode_EmbedCodeItem : System.Web.UI.UserControl
{
	protected void Page_Load(object sender, EventArgs e)
	{

	}

	Core.Web.EmbedCodeConfig _config;
	int _id;
	string _usersPreview;

	public Core.Web.EmbedCodeConfig EmbedConfig
	{
		get { return _config; }
	}

	public string Url
	{
		get
		{
			string url = Request.Url.Host;
			while (url.EndsWith("/")) url = url.Substring(0, url.Length - 1);
			if (Request.Url.Port != 80) url += String.Format(":{0}", Request.Url.Port);
			if (Core.ServerImpl.Instance.AppPath != "/") url += Core.ServerImpl.Instance.AppPath;
			return url;
		}
	}

	public int EmbedCodeID
	{
		get { return _id; }
	}

	public String EmbecCode
	{
		get { return String.Format("<script language=\"javascript\" type=\"text/javascript\" >document.writeln('<script src=\"http://{0}/EmbedCS.ashx?ID={1}&' + (new Date()).getTime() + '\" language=\"javascript\" type=\"text/javascript\"></' + 'script>');</script>",Url, EmbedCodeID); }
	}

	public String UsersPreview
	{
		get { return _usersPreview; }
	}

	public void SetData(System.Data.DataRow row)
	{
		_id = Convert.ToInt32(row["ID"]);
		_usersPreview = row["UsersPreview"].ToString();
		_config = new Core.Web.EmbedCodeConfig(row["EmbedConfig"].ToString());
	}
}
