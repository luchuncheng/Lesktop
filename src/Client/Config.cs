using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace Client
{
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class Config
	{
		static Config m_Instance = new Config();
		public static Config Instance { get { return m_Instance; } }

		string m_Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\user.config";
		XmlDocument m_Doc = new XmlDocument();
		bool m_IsLoad = false;

		private Config()
		{
		}

		public void Load()
		{
			if (m_Doc.DocumentElement == null)
			{
				if (!m_IsLoad)
				{
					try
					{
						m_Doc.Load(m_Path);
					}
					catch
					{
						m_Doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Config></Config>");
					}
					m_IsLoad = true;
				}
			}
		}

		public String GetValue(string name)
		{
			lock (m_Doc)
			{
				Load();
				XmlNodeList xnl = m_Doc.DocumentElement.GetElementsByTagName(name);
				if (xnl.Count == 0 || xnl[0].GetType() != typeof(XmlElement)) return String.Empty;
				return (xnl[0] as XmlElement).InnerText;
			}
		}

		public void SetValue(string name, string value)
		{
			lock (m_Doc)
			{
				Load();
				XmlNodeList xnl = m_Doc.DocumentElement.GetElementsByTagName(name);
				if (xnl.Count == 0 || xnl[0].GetType() != typeof(XmlElement))
				{
					XmlElement elem = m_Doc.CreateElement(name);
					m_Doc.DocumentElement.AppendChild(elem);
					elem.InnerText = value;
				}
				else
				{
					(xnl[0] as XmlElement).InnerText = value;
				}
				m_Doc.Save(m_Path);
			}
		}

		public Int32 AttachSize
		{
			get
			{
				try
				{
					string val = GetValue("AttachSize");
					return String.IsNullOrEmpty(val) ? 10 : Int32.Parse(val);
				}
				catch
				{
					return 10;
				}
			}
			set
			{
				SetValue("AttachSize", value.ToString());
			}
		}

		public Int32 LeavePeriod
		{
			get
			{
				try
				{
					string val = GetValue("LeavePeriod");
					return String.IsNullOrEmpty(val) ? 5 : Int32.Parse(val);
				}
				catch
				{
					return 5;
				}
			}
			set
			{
				SetValue("LeavePeriod", value.ToString());
			}
		}
	}
}
