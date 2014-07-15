using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Core;

public partial class Lesktop_CurrentVersion_UploadFile : System.Web.UI.Page
{
	static Hashtable UnsupportExt = new Hashtable();
	protected void Page_Load(object sender, EventArgs e)
	{
		lock (UnsupportExt)
		{
			if (UnsupportExt.Count == 0)
			{
				UnsupportExt.Add(".CONFIG", "CONFIG");
			}
		}
		if (IsPostBack)
		{
			HtmlInputHidden json = FindControl("json") as HtmlInputHidden;
			try
			{
				FileUpload fu = FindControl("file") as FileUpload;
				string name = System.IO.Path.GetFileName(fu.FileName);
				string ext = System.IO.Path.GetExtension(fu.FileName);

				if (UnsupportExt.ContainsKey(ext.ToUpper()))
				{
					throw new Exception(String.Format("不支持上传此类型(*{0})的文件！", ext));
				}

				String filename = Core.ServerImpl.Instance.GetFullPath(Context, "Temp") + "/" + Guid.NewGuid().ToString();
				Core.IO.Directory.CreateDirectory(filename);
				filename += "/" + name;

				using (System.IO.Stream stream = Core.IO.File.Create(filename))
				{
					try
					{
						byte[] buffer = new byte[16 * 1024];
						int count = 0;
						do
						{
							count = fu.FileContent.Read(buffer, 0, buffer.Length);
							stream.Write(buffer, 0, count);
						} while (count == buffer.Length);
					}
					finally
					{
						stream.Close();
					}
				}

				json.Value = Utility.RenderHashJson("Result", true, "Path", filename);
			}
			catch (System.Exception ex)
			{
				json.Value = Utility.RenderHashJson("Result", false, "Exception", ex);
			}
		}
		else
		{
			HtmlInputHidden fileid = FindControl("fileid") as HtmlInputHidden;
			fileid.Value = Guid.NewGuid().ToString().ToUpper();
		}
	}
}