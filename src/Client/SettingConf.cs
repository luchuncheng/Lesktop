using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace Client
{
	public class SettingConf
	{
		static SettingConf m_Instance = new SettingConf();

		public static SettingConf Instance { get { return m_Instance; } }

		string m_Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Setting.conf";
		string m_ServiceUrl = "", m_ResPath = "", m_AppPath = "";

		public String ServiceUrl { get { return m_ServiceUrl; } }
		public String ResPath { get { return m_ResPath; } }
		public String AppPath { get { return m_AppPath; } }
		
		private SettingConf()
		{
		}

		public void Load()
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(m_Path);

				m_ServiceUrl = (doc.DocumentElement.GetElementsByTagName("ServiceUrl")[0] as XmlElement).InnerText;
				m_ResPath = (doc.DocumentElement.GetElementsByTagName("ResPath")[0] as XmlElement).InnerText;
				m_AppPath = (doc.DocumentElement.GetElementsByTagName("AppPath")[0] as XmlElement).InnerText;
			}
			catch
			{
				throw new Exception("读取配置文件失败!");
			}
		}
	}
}
