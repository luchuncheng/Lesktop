using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Core;

namespace Core.Web
{
	public partial class Message_MessageCtrl : System.Web.UI.UserControl
	{
		public DataRow Data = null;

		protected void Page_Load(object sender, EventArgs e)
		{

		}

		public String SenderName
		{
			get
			{
				return Data["SenderName"].ToString();
			}
		}

		public String ReceiverName
		{
			get
			{
				return Data["ReceiverName"].ToString();
			}
		}

		public String SenderNickname
		{
			get
			{
				return Data["SenderNickname"].ToString();
			}
		}

		public String ReceiverNickname
		{
			get
			{
				return Data["ReceiverNickname"].ToString();
			}
		}

		public String CreatedTime
		{
			get
			{
				return String.Format("{0:yyyy-MM-dd HH:mm:ss}", Data["CreatedTime"]);
			}
		}

		public String Content
		{
			get { return Data["Content"].ToString(); }
		}

		public String MsgID
		{
			get { return Data["ID"].ToString(); }
		}

		static Regex m_RegTag = new Regex(@"<(\/|)([^ \f\n\r\t\v\<\>\/]+)(\s[^\<\>]*|)(\/|)>");
		static Regex m_RegFile = new Regex(@"\x5BFILE:([^ \f\n\r\t\v\x5B\x5D]+)\x5D");

		private String Replace(Match m)
		{
			if (m.Groups[2].Value.ToLower() == "img") return "<span style='color:blue;'>[图片]</span>";
			return String.Empty;
		}

		private String ReplaceFile(Match m)
		{
			return "<span style='color:red;'>[文件]</span>";
		}

		public String Summary
		{
			get
			{
				string summary = m_RegTag.Replace(Content, Replace);
				summary = m_RegFile.Replace(summary, ReplaceFile);
				return summary;
			}
		}
	}
}