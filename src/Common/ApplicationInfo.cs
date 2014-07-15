using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Custom
{
	public class ApplicationInfo
	{
		static ApplicationInfo m_Instance = new ApplicationInfo();

		public static ApplicationInfo Instance
		{
			get { return m_Instance; }
		}

		private ApplicationInfo()
		{
		}

		public const String AssemblyTitle = "云骞即时通讯软件";

		public const String AssemblyCompany = "云骞";

		public const String AssemblyProduct = "云骞即时通讯软件";

		public const String AssemblyCopyright = "Copyright © 云骞 2013";

		public const String ReleaseVersion = "3.0.0.0";

		public const bool FilterHtml = true;

		public const bool CheckLoginName = false;
	}
}
