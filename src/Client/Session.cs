using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Client
{
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class SessionImpl
	{
		object lock_ = new object();

		ResponsesHandler responses_ = null;
		Delegate after_init_service_ = new Delegate();
		string username_ = String.Empty, cookie_ = String.Empty, sessionid_ = String.Empty;
		object userinfo_ = null;
		int userid_ = 0;

		private SessionImpl()
		{
			responses_ = new ResponsesHandler();
		}

		public void InitService(string username, object userinfo, string cookie, string sessionId)
		{
			lock (lock_)
			{
				username_ = username;
				userinfo_ = userinfo;
				cookie_ = cookie;
				sessionid_ = sessionId;

				string nickname = Utility.GetProperty(userinfo_, "Nickname").ToString();
				string name = Utility.GetProperty(userinfo_, "Name").ToString();
				Client.Global.TrayIcon.Text = String.Format("{0}({1})", nickname, name);

				userid_ = Convert.ToInt32(Utility.GetProperty(userinfo_, "ID"));
				responses_.Start();
				after_init_service_.CallAll();
				Program.AfterInitService();
			}
		}

		public object AfterInitService
		{
			get { return after_init_service_; }
		}

		public int GetUserID()
		{
			lock (lock_)
			{
				return userid_;
			}
		}

		public object GetUserInfo()
		{
			lock (lock_)
			{
				return userinfo_;
			}
		}

		public void ResetUserInfo(object userInfo)
		{
			lock (lock_)
			{
				object wm = SessionImpl.Instance.GetGlobal("WindowManagement");
				Utility.InvokeMethod(wm, "Notify", "UserInfoChanged", userinfo_);
			}
		}

		public void Reset()
		{
			userinfo_ = null;
			username_ = String.Empty;
			sessionid_ = String.Empty;
			cookie_ = String.Empty;
		}

		public string GetCookie()
		{
			lock (lock_)
			{
				return cookie_;
			}
		}

		public string GetSessionID()
		{
			lock (lock_)
			{
				return sessionid_;
			}
        }

		public ResponsesHandler ResponsesHandler
		{
			get { return responses_; }
		}

        static object log_lock_ = new object();

        public void WriteLog(string text)
        {
			lock (log_lock_)
			{
				try
				{
					string line = String.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}\r\n", DateTime.Now, text);
					try
					{
						if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"applog.txt"))
						{
							System.IO.FileInfo info = new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"applog.txt");
							if (info.Length > 5 * 1024 * 1024) System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"applog.txt");
						}
					}
					catch
					{
					}
					System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"applog.txt", line, Encoding.UTF8);
				}
				catch
				{
				}
			}
        }

		static Hashtable global_objects_ = new Hashtable();

		public void RegisterGlobal(string key, object value)
		{
			lock (global_objects_)
			{
				global_objects_[key.ToUpper()] = value;
			}
		}

		public void RemoveGlobal(string key)
		{
			lock (global_objects_)
			{
				global_objects_.Remove(key.ToUpper());
			}
		}

		public object GetGlobal(string key)
		{
			lock (global_objects_)
			{
				return global_objects_[key.ToUpper()];
			}
		}

		static SessionImpl kInstance = new SessionImpl();

		static public SessionImpl Instance { get { return kInstance; } }
	}
}
