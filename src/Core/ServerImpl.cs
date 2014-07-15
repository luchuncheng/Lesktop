using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Configuration;
using System.IO;
using System.Web.Security;
using System.Reflection;

namespace Core
{
	using IO;

	public class PathMap
	{
		private PathMap()
		{
		}

		Hashtable maps_ = new Hashtable();

		public void NewMap(string path, string real_path)
		{
			string[] nodes = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
			Hashtable cur = maps_;
			foreach (string node in nodes)
			{
				var key = node.ToLower();
				if (!cur.ContainsKey(key))
				{
					cur.Add(key, new Hashtable());
					(cur[key] as Hashtable).Add("*Name", node);
				}
				cur = cur[key] as Hashtable;
			}
			cur["*RealPath"] = real_path;
		}

		public String Map(string path)
		{
			if (maps_.Count == 0) return String.Empty;

			try
			{
				string[] nodes = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
				Hashtable cur = maps_;
				int i = 0;
				for (i = 0; i < nodes.Length; i++)
				{
					string node = nodes[i];
					string key = node.ToLower();
					if (!cur.ContainsKey(key)) break;
					cur = cur[key] as Hashtable;
				}

				if (!cur.ContainsKey("*RealPath") || cur["*RealPath"].GetType() != typeof(string)) return String.Empty;
				string real_path = cur["*RealPath"].ToString();
				if (i == nodes.Length) return real_path;
				return String.Format(@"{0}\{1}", real_path, Path.Join(@"\", nodes, i));
			}
			catch
			{
				return String.Empty;
			}
		}

		public string[] GetSubDirectories(string path)
		{
			List<String> sub_dirs = new List<string>();
			try
			{
				if (maps_.Count > 0)
				{
					string[] nodes = path.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
					Hashtable cur = maps_;
					int i = 0;
					for (i = 0; i < nodes.Length; i++)
					{
						string node = nodes[i];
						string key = node.ToLower();
						if (!cur.ContainsKey(key)) return sub_dirs.ToArray();
						cur = cur[key] as Hashtable;
					}

					if (!cur.ContainsKey("*RealPath") || cur["*RealPath"].GetType() != typeof(string))
					{
						foreach (DictionaryEntry de in cur)
						{
							if (!de.Key.ToString().StartsWith("*") && (de.Value as Hashtable).ContainsKey("*RealPath"))
							{
								sub_dirs.Add((de.Value as Hashtable)["*Name"].ToString());
							}
						}
					}
				}
			}
			catch
			{
			}
			return sub_dirs.ToArray();
		}

		static PathMap m_Instance = new PathMap();

		static public PathMap Instance
		{
			get
			{
				return m_Instance;
			}
		}
	}

	public class ServerImpl
	{
		static ServerImpl m_Instance = new ServerImpl();

		static public ServerImpl Instance
		{
			get 
			{
				m_Instance.Initialize(HttpContext.Current);
				return m_Instance; 
			}
		}

		bool enbale_dynamic_ = false;
		bool init_flag_ = false;
		object init_lock_ = new object();
		string base_dir_ = String.Empty;
		string file_root_;
		string app_path_ = "";

		public string BaseDirecotry
		{
			get { return base_dir_; }
		}

		public void Initialize(HttpContext context)
		{
			lock (init_lock_)
			{
				if (!init_flag_)
				{
					base_dir_ = context.Server.MapPath("~");

					file_root_ = GetFileRoot(context);
					enbale_dynamic_ = System.IO.File.Exists(String.Format("{0}\\Applications.js", base_dir_));
					try
					{
						if (System.IO.File.Exists(String.Format("{0}\\path_map.json", base_dir_)))
						{
							Hashtable path_map = Utility.ParseJson(System.IO.File.ReadAllText(String.Format("{0}\\path_map.json", base_dir_), Encoding.UTF8)) as Hashtable;

							string path_map_base = base_dir_;
							while (path_map_base.EndsWith("\\")) path_map_base = path_map_base.Substring(0, path_map_base.Length - 1);

							foreach (DictionaryEntry de in path_map)
							{
								Hashtable info = de.Value as Hashtable;
								string path = info["Path"].ToString();
								if (!System.IO.Path.IsPathRooted(path))
								{
									System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path_map_base + "\\" + path);
									path = di.FullName;
								}
								PathMap.Instance.NewMap(de.Key.ToString(), path);
							}
						}
					}
					catch
					{
					}

					app_path_ = context.Request.ApplicationPath;
					res_path_ = System.Web.Configuration.WebConfigurationManager.AppSettings["ResPath"];
					AccountImpl.Instance.Initialize(context);
					MessageImpl.Instance.Initialize(context);
					context.ApplicationInstance.Error += new EventHandler(HttpApplication_Error);
					init_flag_ = true;
					WriteLog("Service Initialize.");
				}
			}
		}

		void HttpApplication_Error(object sender, EventArgs e)
		{
			HttpApplication app = sender as HttpApplication;
			if (app.Context.Error != null)
			{
				ServerImpl.Instance.WriteLog(String.Format("{0}:\r\n{1}", app.Context.Error.GetType().Name, app.Context.Error.StackTrace));
			}
		}

		public void Dispose(HttpApplication app)
		{
		}

		public bool EnbaleDynamicApp
		{
			get { return enbale_dynamic_; }
		}

		public String GetFileRoot(HttpContext context)
		{
			string fileRoot = System.Web.Configuration.WebConfigurationManager.AppSettings["FileRoot"];

			if (String.IsNullOrEmpty(fileRoot))
			{
				string path = context.Server.MapPath("~");
				while (path.EndsWith("\\")) path = path.Substring(0, path.Length - 1);

				return System.IO.Path.GetDirectoryName(path) + @"\Files";
			}
			else
			{
				if (!System.IO.Path.IsPathRooted(fileRoot))
				{
					string path = context.Server.MapPath("~");
					System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, fileRoot));
					System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(System.IO.Path.Combine(path, fileRoot));
					return dir.FullName;
				}
				else
				{
					return fileRoot;
				}
			}
		}

		public int GetUserID(HttpContext context)
		{
			return Custom.CustomServerImpl.GetUserID(context);
		}

		public AccountSession Login(String sessionId, HttpContext context, int id, bool clientMode, Nullable<DateTime> expires)
		{
			return Login(sessionId, context, id, clientMode, expires, true);
		}

		public AccountSession Login(String sessionId, HttpContext context, int id, bool clientMode, Nullable<DateTime> expires, bool startSession)
		{
			FormsAuthentication.Initialize();
			string[] roles = new string[0];
			Custom.CustomServerImpl.AddCookie(context, sessionId, id, roles, expires, clientMode);
			return startSession ? SessionManagement.Instance.NewSession(id, sessionId) : null;
		}

		public void Logout(HttpContext context)
		{
			Custom.CustomServerImpl.RemoveCookie(context);
		}

		public bool IsPublic(string path)
		{
			string relative = Core.IO.Path.GetRelativePath(path);
			string root = Core.IO.Path.GetRoot(relative).ToLower();
			return root == "public";
		}

		public bool IsTemp(string path)
		{
			string relative = Core.IO.Path.GetRelativePath(path);
			string root = Core.IO.Path.GetRoot(relative).ToLower();
			return root == "temp";
		}

		public string MapPath(string path)
		{
			string relative = Core.IO.Path.GetRelativePath(path);
			string[] pns = relative.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			if (pns.Length > 0 && pns[0].ToLower() == "public")
			{
				return Path.Join(@"\", file_root_, "Public", Path.Join(@"\", pns, 1));
			}
			else if (pns.Length > 0 && pns[0].ToLower() == "temp")
			{
				return Path.Join(@"\", file_root_, "Temp", Core.IO.Path.GetUser(path).ToString(), Path.Join(@"\", pns, 1));
			}
			else if (pns.Length > 0 && pns[0].ToLower() == "wwwroot")
			{
				return Path.Join(@"\", base_dir_, Path.Join(@"\", pns, 1));
			}
			else
			{
				string real_path = PathMap.Instance.Map(path);
				if (String.IsNullOrEmpty(real_path))
				{
					int user = Core.IO.Path.GetUser(path);
					return String.Format(@"{0}\Users\{1}\{2}", file_root_, user.ToString(), Path.Join(@"\", pns, 0));
				}
				else
				{
					return real_path;
				}
			}
		}

		public string GetFullPath(HttpContext context, string path)
		{
			int user = Path.GetUser(path);
			return user == 0 ? String.Format("/{0}/{1}", GetUserID(context), path) : path;
		}

		int RootDirectoryPermission = IOPermission.Read;
		int PublicSubItemsPermission = IOPermission.Read;
		int RootSubItemsPermission = IOPermission.Read | IOPermission.Write;
		int RootPublicSubItemsPermission = IOPermission.Read;

		public void CheckPermission(HttpContext context, string path, int action)
		{
			string relative = Path.GetRelativePath(path);
			string relativeRoot = Path.GetRoot(relative).ToLower();
			bool isRootItem = String.Compare(relative, relativeRoot, true) == 0;

			if ((relativeRoot == "pub") && !isRootItem && action == IOPermission.Read) return;
			if ((relativeRoot == "public") && !isRootItem && action == IOPermission.Read) return;

			AccountInfo currentUser = ServerImpl.Instance.GetCurrentUser(context);
			if (currentUser != null && currentUser.ID == 3) return;

			int owner = Path.GetUser(path);
			if (owner == 0) owner = currentUser.ID;
			AccountInfo ownerInfo = AccountImpl.Instance.GetUserInfo(owner);

			if (ownerInfo.Type == 1 && (ownerInfo.ContainsMember(currentUser.ID) || ownerInfo.SubType == 3)) return;

			if (relativeRoot == "message" && ownerInfo.Type == 1)
			{
				try
				{
					string[] nodes = relative.Split(new char[] { '/' }, 3, StringSplitOptions.RemoveEmptyEntries);
					if (nodes.Length >= 2)
					{
						int msg_id = Convert.ToInt32(nodes[1].Substring(3));
						if (!MessageImpl.Instance.CheckPermission(currentUser.ID, msg_id))
						{
							throw new PermissionException();
						}
						if (action == IOPermission.Read) return;
					}
				}
				catch
				{
				}
			}

			if (currentUser == null || ownerInfo.ID != currentUser.ID) throw new PermissionException();

			if (String.IsNullOrEmpty(relative) && (RootDirectoryPermission & action) != action) throw new PermissionException();

			if (String.Compare(relative, relativeRoot, true) == 0)
			{
				if (relativeRoot == "public")
				{
					if ((RootPublicSubItemsPermission & action) != action) throw new PermissionException();
				}
				else
				{
					if ((RootSubItemsPermission & action) != action) throw new PermissionException();
				}
			}
			else
			{
				if (relativeRoot == "public")
				{
					if ((PublicSubItemsPermission & action) != action) throw new PermissionException();
				}
				else
				{
				}
			}
		}

		public String Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}

		String res_path_;

		public String ResPath
		{
			get { return res_path_; }
		}

		private object _logLock = new object();

		public void WriteLog(string text)
		{
			lock (_logLock)
			{
				string filename = file_root_ + @"\trace.txt";
				try
				{
					if (System.IO.File.Exists(filename))
					{
						System.IO.FileInfo info = new System.IO.FileInfo(filename);
						if (info.Length > 30 * 1024 * 1024) System.IO.File.Delete(filename);
					}
				}
				catch
				{
				}

				string line = String.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}\r\n", DateTime.Now, text);
				System.IO.File.AppendAllText(filename, line, Encoding.UTF8);
			}
		}

		public AccountInfo GetCurrentUser(HttpContext context)
		{
			return AccountImpl.Instance.GetUserInfo(GetUserID(context));
		}

		ICommonStorage m_ICommonStorage = null;
		object m_ICommonStorageLock = new object();

		public ICommonStorage CommonStorageImpl
		{
			get
			{
				lock (m_ICommonStorageLock)
				{
					if (m_ICommonStorage == null)
					{
						string accStorage = System.Web.Configuration.WebConfigurationManager.AppSettings["CommonStorageImpl"];
						String[] accStorageInfo = accStorage.Split(new char[] { ' ' });
						Type type = Assembly.Load(accStorageInfo[0]).GetType(accStorageInfo[1]);
						ConstructorInfo ctor = type.GetConstructor(new Type[] { });
						m_ICommonStorage = ctor.Invoke(new object[] { }) as ICommonStorage;
					}

					return m_ICommonStorage;
				}
			}
		}

		public String Debug
		{
			get
			{
#				if DEBUG
				return "true";
#				else
				return "false";
#				endif
			}
		}

		public String AppPath
		{
			get
			{
				return app_path_;
			}
		}
	}

	public class PermissionException : Exception
	{
		public PermissionException()
			: base("权限不足")
		{
		}
	}

	public class IOPermission
	{
		public const int Rename = 1;
		public const int Delete = 1 << 1;
		public const int Read = 1 << 2;
		public const int Write = 1 << 3;
	}
}
