using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using Microsoft.Win32;


namespace Client
{
	using Properties;

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class ClientCall
	{
		static byte[] EncryptPriKey = null;
		static byte[] EncryptPubKey = null;
	
		Form _window_frame = null;
		WebBrowser _browser = null;

		public ClientCall(WebBrowser b, Form windor_frame)
		{
			_browser = b;
			_window_frame = windor_frame;
		}

		public object WindowFrameCtrl
		{
			get { return _window_frame; }
		}

		public void InitClient(bool success)
		{
			Client.Program.InitClient(success);
		}

		public object CreateWindow(object config)
		{
			Window win = null;
			win = new Window();
			win.Init(config);
			return win.IWindow;
		}

		public void ShowError(string message)
		{
			MessageBox.Show(_browser, message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void ShowWarning(string message)
		{
			MessageBox.Show(_browser, message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public String Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}

		public void RefreshTrayIcon()
		{
			Program.RefreshTrayIcon();
		}

		public SessionImpl Session
		{
			get { return SessionImpl.Instance; }
		}

		public String Cookie
		{
			get { return SessionImpl.Instance.GetCookie(); }
		}

		public Window.IWindowImpl OutputPanel
		{
			get { return null; }
		}

		public object Desktop
		{
			get { return Global.Desktop.HtmlWindow; }
		}

		public String ServiceUrl
		{
			get { return Global.ServiceUrl; }
		}

		public String AppPath
		{
			get { return Global.AppPath; }
		}

		public String ResPath
		{
			get { return Global.ResPath; }
		}

		public Boolean IsLocal
		{
			get
			{
				return _browser.Url.Scheme.ToLower() == "file";
			}
		}

		public String OpenFile(String filter)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = filter;
			if (ofd.ShowDialog(_browser) == DialogResult.OK)
			{
				return ofd.FileName;
			}
			else
			{
				return String.Empty;
			}

		}

		public void ExitApplication()
		{
			Global.Desktop.Close();
			ResponsesHandler.Instance.Stop();
			Global.TrayIcon.Visible = false;
			Application.Exit();
		}

		public void RestartApplication()
		{
			Global.Desktop.Close();
			ResponsesHandler.Instance.Stop();
			Global.TrayIcon.Visible = false;
			System.Diagnostics.Process.Start(AppDomain.CurrentDomain.BaseDirectory + "Client.exe");
			Application.Exit();
		}

		public object OpenWebImg()
		{
			return null;
		}

		public String GradScreen()
		{
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			Bitmap bmp = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));
			}
			GradScreenForm screen = new GradScreenForm(bmp);
			if (screen.ShowDialog() == DialogResult.OK)
			{
				string tempFile = String.Format(@"{0}\{1}.jpg", Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
				screen.ResultBitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Jpeg);
				return tempFile;
			}
			else
			{
				return String.Empty;
			}
		}

		public String GetClipboardImage()
		{
			Image image = Clipboard.GetImage();
			if (image == null) return String.Empty;
			string tempFile = String.Format(@"{0}\{1}.jpg", Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
			image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Jpeg);
			return tempFile;
		}

		public void PlayMsgNotifySound()
		{
			if (!Program.Silence && System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\data\msg.wav"))
			{
				SoundPlayer sp = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + @"\data\msg.wav");
				sp.Play();
			}
		}

		public Config LocalSetting
		{
			get { return Config.Instance; }
		}

		public bool RegisterHotKey(int action, string saveKey, bool ctrlKey, bool shiftKey, bool altkey, uint key)
		{
			HotKey hk = new HotKey(ctrlKey, shiftKey, altkey, key);
			if (!HotKeyUtil.RegisterHotKeyAction(action, hk)) return false;
			Config.Instance.SetValue(saveKey, Utility.RenderJson(hk));
			return true;
		}

		public string GetAutoLoginData()
		{
			if (IsRunning()) return "";
			string content = Config.Instance.GetValue("AutoLogin");
			if (content == "") return "";
			byte[] keys = GetMacBytes();
			if (keys == null) return "";
			string json = MyEncrypt.DecryptDES(keys, content);
			return json == content ? "" : json;
		}

		public void EnableAutoLogin(bool enable, string username, string pwd)
		{
			if (enable)
			{
				Hashtable data = new Hashtable();
				data["UserName"] = username ?? "";
				data["Password"] = pwd ?? "";
				byte[] keys = GetMacBytes();
				if (keys == null) return;
				string autologin_dat = MyEncrypt.EncryptDES(keys, Utility.RenderJson(data));
				Config.Instance.SetValue("AutoLogin", autologin_dat);
			}
			else
			{
				Config.Instance.SetValue("AutoLogin", "");
			}
		}

		public bool IsAutoStart()
		{
			string v = Config.Instance.GetValue("AutoStart");
			return string.Compare(v, "true", true) == 0;
		}

		public bool NeedCreateTempUser()
		{
			return Global.CreateTempAccount;
		}

		public bool UseMsgBox
		{
			get
			{
				return Config.Instance.GetValue("UseMsgBox") == "true";
			}
		}

		public void EnableAutoStart(bool enable)
		{
			if (enable)
			{
				string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

				IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
				IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(
					startupDir + "\\Client.lnk"
				);
				shortcut.TargetPath = AppDomain.CurrentDomain.BaseDirectory + @"Client.exe";
				shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				shortcut.WindowStyle = 1;
				shortcut.Description = "企业即时通讯软件";
				shortcut.Save();
			}
			else
			{
				string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
				if (File.Exists(startupDir + "\\Client.lnk")) File.Delete(startupDir + "\\Client.lnk");
			}
			Config.Instance.SetValue("AutoStart", enable ? "true" : "false");
		}

		bool IsRunning()
		{
			Process[] processes = Process.GetProcesses();
			int count = 0;
			foreach (Process process in processes)
			{
				try
				{
					if (String.Compare(process.MainModule.FileName, Assembly.GetExecutingAssembly().Location, true) == 0) count++;
				}
				catch
				{
				}
			}
			return count >= 2;
		}

        private static byte[] GetMacBytes()
        {
            byte[] bytes = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            string mac = GetMAC();
#			if DEBUG
			if (String.IsNullOrEmpty(mac)) return new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
#			else
            if (String.IsNullOrEmpty(mac)) return null;
#			endif
			string[] ns = mac.Split(new char[] { ':' });
            for (int i = 0; i < ns.Length && i < 8; i++) bytes[i] = Convert.ToByte(ns[i], 16);
            return bytes;
        }

		private static string GetMAC()
		{
			string MoAddress = "";
			try
			{
				ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
				ManagementObjectCollection moc = mc.GetInstances();
				foreach (ManagementObject mo in moc)
				{
					if (mo["MacAddress"] != null) return mo["MacAddress"].ToString();
					mo.Dispose();
				}
			}
			catch
			{
			}
			return MoAddress;
		}

		public String LoadLocalListCache()
		{
			string path = AppDomain.CurrentDomain.BaseDirectory + @"\data\list.cache";
			if (!File.Exists(path)) return "";

			Byte[] data = File.ReadAllBytes(path);
			string json = MyEncrypt.DecryptDES(EncryptPriKey, EncryptPubKey, data);
			return json;
		}

		public void SaveLocalList(string json)
		{
#			if !DEBUG
			string path = AppDomain.CurrentDomain.BaseDirectory + @"\data\list.cache";
			if (File.Exists(path)) File.Delete(path);

			Byte[] data = MyEncrypt.EncryptDES(EncryptPriKey, EncryptPubKey, json);
			File.WriteAllBytes(path, data);
			SessionImpl.Instance.WriteLog("SaveList");
#			endif
		}

		public String ToBase64String(String file)
		{
			Byte[] buffer = File.ReadAllBytes(Microsoft.JScript.GlobalObject.unescape(file));
			return Convert.ToBase64String(buffer);
		}

		public void Open(string cookie, string url, string name, object handler)
		{
			String dir = String.Format(@"{0}{1}", Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
			Directory.CreateDirectory(dir);

			OpenFileHandler dl = new OpenFileHandler(cookie, url, dir + "\\" + HttpUtility.UrlDecode(name), _browser, handler);
			dl.Download();
		}

		public void Upload(string cookie, string file, object handler)
		{
			UploadFileHandler dl = new UploadFileHandler(cookie, Global.ResUrl + "/SendFile.ashx", file, _browser, handler);
			dl.Upload();
		}

		public string SendFile(string file, object handler)
		{
			SendFileHandler dl = new SendFileHandler(-1, file, _browser, handler);
			dl.Start();
			return dl.Url;
		}

		public void Save(string cookie, string url, string name, object handler)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.FileName = HttpUtility.UrlDecode(name);
			if (sfd.ShowDialog(_browser) == DialogResult.OK)
			{
				SaveFileHandler dl = new SaveFileHandler(cookie, url, sfd.FileName, _browser, handler);
				dl.Download();
			}
		}

		public void Parse(string text)
		{
			Clipboard.SetText(text);
		}

		public object CreateMenu(object config)
		{
			return new Menu(_browser, config);
		}

		public void Login(string name, string password, object handler)
		{
			string data = String.Format(
				"<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
				"<Command ID=\"ID100000001-1091949377\" SessionID=\"\" Handler=\"Core.Web Common_CH\" IsAsyn=\"false\">" +
				"{{&#34;Action&#34;:&#34;Login&#34;,&#34;User&#34;:&#34;{0}&#34;,&#34;Password&#34;:&#34;{1}&#34;,&#34;ClientMode&#34;:true,&#34;EmbedCode&#34;:{2}}}" +
				"</Command>",
				name, password, Global.EmbedCode
			);
				
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] md5_data = md5.ComputeHash(UTF8Encoding.Default.GetBytes(String.Format("{0}:{1}", name, password)));
			EncryptPriKey = new byte[] { md5_data[0], md5_data[1], md5_data[2], md5_data[3], md5_data[4], md5_data[5], md5_data[6], md5_data[7] };
			EncryptPubKey = new byte[] { md5_data[8], md5_data[9], md5_data[10], md5_data[11], md5_data[12], md5_data[13], md5_data[14], md5_data[15] };

			LoginThreadEntryParams ps = new LoginThreadEntryParams();
			ps.Handler = handler;
			ps.Data = data;
			System.Threading.Thread thread = new System.Threading.Thread(LoginThreadEntry);
			thread.Start(ps);
		}

		class LoginThreadEntryParams
		{
			public String Data;
			public object Handler;
		}

		void LoginThreadEntry(object data)
		{
			LoginThreadEntryParams p = data as LoginThreadEntryParams;
			try
			{
				HttpWebRequest request = HttpWebRequest.Create(Global.ResUrl + "/Command.ashx") as HttpWebRequest;
				//request.KeepAlive = true;
				if (request.ServicePoint.ConnectionLimit < 100) request.ServicePoint.ConnectionLimit = 100;
				request.ContentType = "text/xml";
				request.Timeout = 2 * 60 * 1000;
				request.Method = "POST";

				byte[] buffer = Encoding.UTF8.GetBytes(p.Data);
				request.GetRequestStream().Write(buffer, 0, buffer.Length);
				request.GetRequestStream().Close();
				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				buffer = new byte[response.ContentLength];

				int length = 0;
				while (true)
				{
					int count = response.GetResponseStream().Read(buffer, length, (int)buffer.Length - length);
					if (count > 0) length += count;
					if (length == buffer.Length || count < 0) break;
				}
				response.Close();
				String content = Encoding.UTF8.GetString(buffer);

				Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onsuccess", new object[] { response.StatusCode.ToString(), content, response.GetResponseHeader("Set-Cookie") });
			}
			catch (Exception ex)
			{
				Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onerror", new object[] { "Server Error", ex.Message });
			}
		}

		public void Post(string url, string data, string type, int timeout, object handler)
		{
			PostThreadEntryParams ps = new PostThreadEntryParams();
			ps.Url = url;
			ps.Data = data;
			ps.Type = type;
			ps.Timeout = timeout;
			ps.Handler = handler;

			System.Threading.Thread thread = new System.Threading.Thread(PostThreadEntry);
			thread.Start(ps);
		}

		class PostThreadEntryParams
		{
			public String Url, Data, Type;
			public int Timeout;
			public object Handler;
		}

		void PostThreadEntry(object data)
		{
			try
			{
				PostThreadEntryParams p = data as PostThreadEntryParams;
				try
				{
					HttpWebRequest request = HttpWebRequest.Create(Global.ServiceUrl + "//" + p.Url) as HttpWebRequest;
					if (request.ServicePoint.ConnectionLimit < 100) request.ServicePoint.ConnectionLimit = 100;
					request.ContentType = p.Type;
					//request.KeepAlive = true;
					request.Timeout = p.Timeout;
					request.Method = "POST";
					request.Headers.Add("Cookie", SessionImpl.Instance.GetCookie());

					byte[] buffer = Encoding.UTF8.GetBytes(p.Data);
					request.GetRequestStream().Write(buffer, 0, buffer.Length);
					request.GetRequestStream().Close();
					HttpWebResponse response = request.GetResponse() as HttpWebResponse;
					buffer = new byte[response.ContentLength];

					int length = 0;
					while (true)
					{
						int count = response.GetResponseStream().Read(buffer, length, (int)buffer.Length - length);
						if (count > 0) length += count;
						if (length == buffer.Length || count < 0) break;
					}
					response.Close();
					String content = Encoding.UTF8.GetString(buffer);

					Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onsuccess", new object[] { response.StatusCode.ToString(), content });
				}
				catch (WebException ex)
				{
					if (ex.Status == WebExceptionStatus.RequestCanceled || ex.Status == WebExceptionStatus.Timeout)
					{
						Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onabort", new object[] { });
					}
					else
					{
						Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onerror", new object[] { "Server Error", ex.Message });
					}
				}
				catch (Exception ex)
				{
					Global.Desktop.BeginInvoke(new InvokeHandlerMethodDelegate(InvokeHandlerMethod), p.Handler, "onerror", new object[] { "Server Error", ex.Message });
				}
			}
			catch
			{
			}
		}

		delegate void InvokeHandlerMethodDelegate(object handler, string method, object[] ps);

		void InvokeHandlerMethod(object handler, string method, object[] ps)
		{
			try
			{
				Utility.InvokeMethod2(handler, method, ps);
			}
			catch
			{
			}
		}

		public Double GetTotalSize(string files)
		{
			Double total = 0;
			foreach (string file in files.Split(new Char[] { '\0' }))
			{
				total += (new FileInfo(file)).Length;
			}
			return total;
		}

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool InternetSetCookie(string lpszUrl, string lpszCookieName, string lpszCookieData);

		public void SetCookies(string url)
		{
			string cookie = SessionImpl.Instance.GetCookie();
			if (cookie != "")
			{
				string[] pairs = cookie.Split(new char[] { ';' });
				foreach (string p in pairs)
				{
					string[] ps = p.Split(new char[] { '=' });
					if (ps.Length == 2)
					{
						InternetSetCookie(url, ps[0], ps[1]);
					}
				}
			}
		}

		[System.Runtime.InteropServices.ComVisibleAttribute(true)]
		public class OpenFileHandler
		{
			string _url, _local, _cookie;
			WebBrowser _borwser;
			object _handler;
			WebClient _client;

			long _recvBytes = 0;
			long _ticks = 0;

			public OpenFileHandler(string cookie, string url, string local, WebBrowser browser, object handle)
			{
				if (url.StartsWith("/"))
				{
					_url = SettingConf.Instance.ServiceUrl + "/" + url;
				}
				else
				{
					_url = url;
				}
				_borwser = browser;
				_local = local;
				_handler = handle;
				_cookie = cookie;
			}

			public void Download()
			{
				_borwser.BeginInvoke(new HandlerBeforeDownloadDelegate(HandlerBeforeDownload), new object[] { this });

				try
				{
					_client = new WebClient();
					_client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
					_client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
					_client.Headers.Add("Cookie", _cookie);

					_ticks = DateTime.Now.Ticks;

					_client.DownloadFileAsync(new Uri(_url), _local);
				}
				catch (Exception e)
				{
					_borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Message);
					return;
				}
			}

			public void Cancel()
			{
				_client.CancelAsync();
			}

			void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
			{
				try
				{
					_borwser.BeginInvoke(new HandlerProcessingDelegate(HandlerProcessing), (long)e.BytesReceived, (long)e.TotalBytesToReceive);
				}
				catch
				{
					_client.CancelAsync();
				}
			}

			void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
			{
				try
				{
					if (e.Error == null) _borwser.BeginInvoke(new HandlerAfterDownloadDelegate(HandlerAfterDownload), e.Cancelled);
					else _borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Error.Message);
				}
				catch
				{
				}
			}

			delegate void HandlerBeforeDownloadDelegate(object ctr);
			void HandlerBeforeDownload(object ctr)
			{
				_handler.GetType().InvokeMember("BeforeDownload", BindingFlags.InvokeMethod, null, _handler, new object[] { ctr });
			}

			delegate void HandlerProcessingDelegate(long length, long size);
			void HandlerProcessing(long length, long size)
			{
				long temp = DateTime.Now.Ticks - _ticks;
				if (temp > 10000 * 1000 || length == size)
				{
					int ret = (int)_handler.GetType().InvokeMember("Processing", BindingFlags.InvokeMethod, null, _handler, new object[] { (double)length, (double)size, temp == 0 ? 0 : (double)((length - _recvBytes) * 1000 / (temp / 10000)) });
					if (ret == 0) _client.CancelAsync();
					_ticks += temp;
					_recvBytes = length;
				}
			}

			delegate void HandlerAfterDownloadDelegate(bool canceled);
			void HandlerAfterDownload(bool canceled)
			{
				if (!canceled)
				{
					System.Diagnostics.ProcessStartInfo psf = new System.Diagnostics.ProcessStartInfo();
					psf.FileName = _local;
					psf.Verb = "open";
					try
					{
						System.Diagnostics.Process.Start(psf);
					}
					catch
					{
					}
				}
				_handler.GetType().InvokeMember("AfterDownload", BindingFlags.InvokeMethod, null, _handler, new object[] { });
			}

			delegate void HandlerErrorDelegate(string msg);
			void HandlerError(string msg)
			{
				_handler.GetType().InvokeMember("HandleError", BindingFlags.InvokeMethod, null, _handler, new object[] { msg });
			}

		}

		[System.Runtime.InteropServices.ComVisibleAttribute(true)]
		public class SaveFileHandler
		{
			string _url, _local, _cookie;
			WebBrowser _borwser;
			object _handler;
			WebClient _client;

			long _recvBytes = 0;
			long _ticks = 0;

			public SaveFileHandler(string cookie, string url, string local, WebBrowser browser, object handle)
			{
				if (url.StartsWith("/"))
				{
					_url = SettingConf.Instance.ServiceUrl + url;
				}
				else
				{
					_url = url;
				}
				_borwser = browser;
				_local = local;
				_handler = handle;
				_cookie = cookie;
			}

			public void Download()
			{
				_borwser.BeginInvoke(new HandlerBeforeDownloadDelegate(HandlerBeforeDownload), new object[] { this });

				try
				{
					_client = new WebClient();
					_client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
					_client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
					_client.Headers.Add("Cookie", _cookie);

					_ticks = DateTime.Now.Ticks;

					_client.DownloadFileAsync(new Uri(_url), _local);
				}
				catch (Exception e)
				{
					_borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Message);
					return;
				}
			}

			public void Cancel()
			{
				_client.CancelAsync();
			}

			void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
			{
				try
				{
					_borwser.BeginInvoke(new HandlerProcessingDelegate(HandlerProcessing), (long)e.BytesReceived, (long)e.TotalBytesToReceive);
				}
				catch
				{
					_client.CancelAsync();
				}
			}

			void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
			{
				try
				{
					if (e.Error == null) _borwser.BeginInvoke(new HandlerAfterDownloadDelegate(HandlerAfterDownload), e.Cancelled);
					else _borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Error.Message);
				}
				catch
				{
					_client.CancelAsync();
				}
			}

			delegate void HandlerBeforeDownloadDelegate(object ctr);
			void HandlerBeforeDownload(object ctr)
			{
				_handler.GetType().InvokeMember("BeforeDownload", BindingFlags.InvokeMethod, null, _handler, new object[] { ctr });
			}

			delegate void HandlerProcessingDelegate(long length, long size);
			void HandlerProcessing(long length, long size)
			{
				long temp = DateTime.Now.Ticks - _ticks;
				if (temp > 10000 * 1000 || length == size)
				{
					int ret = (int)_handler.GetType().InvokeMember("Processing", BindingFlags.InvokeMethod, null, _handler, new object[] { (double)length, (double)size, temp == 0 ? 0 : (double)((length - _recvBytes) * 1000 / (temp / 10000)) });
					if (ret == 0) _client.CancelAsync();
					_ticks += temp;
					_recvBytes = length;
				}
			}

			delegate void HandlerAfterDownloadDelegate(bool canceled);
			void HandlerAfterDownload(bool canceled)
			{
				_handler.GetType().InvokeMember("AfterDownload", BindingFlags.InvokeMethod, null, _handler, new object[] { });
			}

			delegate void HandlerErrorDelegate(string msg);
			void HandlerError(string msg)
			{
				_handler.GetType().InvokeMember("HandleError", BindingFlags.InvokeMethod, null, _handler, new object[] { msg });
			}

		}

		[System.Runtime.InteropServices.ComVisibleAttribute(true)]
		public class UploadFileHandler
		{
			string _url, _local, _cookie;
			WebBrowser _borwser;
			object _handler;
			WebClient _client;

			long _recvBytes = 0;
			long _ticks = 0;

			public UploadFileHandler(string cookie, string url, string local, WebBrowser browser, object handle)
			{
				_url = url;
				_borwser = browser;
				_local = local;
				_handler = handle;
				_cookie = cookie;
			}

			public void Upload()
			{
				_borwser.BeginInvoke(new HandlerBeforeUploadDelegate(HandlerBeforeUpload), new object[] { this });

				try
				{
					_client = new WebClient();
					_client.UploadFileCompleted += new UploadFileCompletedEventHandler(client_UploadFileCompleted);
					_client.UploadProgressChanged += new UploadProgressChangedEventHandler(client_UploadProgressChanged);
					_client.Headers.Add("Cookie", _cookie);

					_ticks = DateTime.Now.Ticks;

					_client.UploadFileAsync(new Uri(_url), "POST", _local);
				}
				catch (Exception e)
				{
					_borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Message);
					return;
				}
			}

			public void Cancel()
			{
				_client.CancelAsync();
			}

			void client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
			{
				try
				{
					_borwser.BeginInvoke(new HandlerProcessingDelegate(HandlerProcessing), (int)e.BytesSent, (int)e.TotalBytesToSend);
				}
				catch
				{
					_client.CancelAsync();
				}
			}

			void client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
			{
				try
				{
					if (e.Error == null)
					{
						String content = Encoding.UTF8.GetString(e.Result);
						Hashtable data = Utility.ParseJson(content) as Hashtable;
						if ((bool)data["Result"])
						{
							_borwser.BeginInvoke(new HandlerAfterUploadDelegate(HandlerAfterUpload), e.Cancelled, data["Path"]);
						}
						else
						{
							Exception ex = data["Exception"] as Exception;
							_borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), ex.Message);
						}
					}
					else
					{
						_borwser.BeginInvoke(new HandlerErrorDelegate(HandlerError), e.Error.Message);
					}
				}
				catch
				{
				}
			}

			delegate void HandlerBeforeUploadDelegate(object ctr);
			void HandlerBeforeUpload(object ctr)
			{
				_handler.GetType().InvokeMember("BeforeUpload", BindingFlags.InvokeMethod, null, _handler, new object[] { ctr });
			}

			delegate void HandlerProcessingDelegate(long length, long size);
			void HandlerProcessing(long length, long size)
			{
				long temp = DateTime.Now.Ticks - _ticks;
				if (temp > 10000 * 1000 || length == size)
				{
					int ret = (int)_handler.GetType().InvokeMember("Processing", BindingFlags.InvokeMethod, null, _handler, new object[] { (double)length, (double)size, temp == 0 ? 0 : (double)((length - _recvBytes) * 1000 / (temp / 10000)) });
					if (ret == 0) _client.CancelAsync();
					_ticks += temp;
					_recvBytes = length;
				}
			}

			delegate void HandlerAfterUploadDelegate(bool canceled, string path);
			void HandlerAfterUpload(bool canceled, string path)
			{
				_handler.GetType().InvokeMember("AfterUpload", BindingFlags.InvokeMethod, null, _handler, new object[] { path });
			}

			delegate void HandlerErrorDelegate(string msg);
			void HandlerError(string msg)
			{
				_handler.GetType().InvokeMember("HandleError", BindingFlags.InvokeMethod, null, _handler, new object[] { msg });
			}

		}

		public class SendFileHandler
		{
			object m_Lock = new object();
			Socket _serverSocket = null;
			Socket _clientSocket = null;
			Int32 _acceptTimeout = 5 * 60 * 1000;
			String _filename = null;
			short _port = 0;
			object _handler;
			WebBrowser _browser;
			bool _isCancel = false;

			public SendFileHandler(Int32 acceptTimeout, String filename, WebBrowser browser, object handle)
			{
				if (acceptTimeout > 0) _acceptTimeout = acceptTimeout;
				_filename = filename;
				_handler = handle;
				_browser = browser;
			}

			public Int32 Port
			{
				get { return _port; }
			}

			public void Start()
			{
				_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_port = GetListenPort();
				int count = 0;
				while (count < 20)
				{
					try
					{
						_serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
					}
					catch
					{
						_port = GetListenPort();
						count++;
					}
					break;
				}
				if (count >= 20)
				{
					_serverSocket = null;
					throw new Exception("绑定端口失败！");
				}

				System.Threading.Thread thread = new System.Threading.Thread(SendThreadEntry);
				thread.Start();
			}

			public void CloseSocket(Socket s)
			{
				if (s != null)
				{
					try
					{
						s.Close();
					}
					catch
					{
					}
				}
			}

			public void Cancel()
			{
				lock (m_Lock)
				{
					_isCancel = true;
					CloseSocket(_clientSocket);
					CloseSocket(_serverSocket);
				}
			}

			public short GetListenPort()
			{
				return (short)(new Random()).Next(10000, 20000);
			}

			public void SendThreadEntry()
			{
				try
				{
					try
					{
						FileInfo fi = new FileInfo(_filename);
						_browser.BeginInvoke(new HandlerBeforeUploadDelegate(HandlerBeforeUpload), new object[] { (double)fi.Length });

						_serverSocket.Listen(1);

						_serverSocket.Blocking = false;
						while (true && _clientSocket == null)
						{
							List<Socket> list = new List<Socket>();
							list.Add(_serverSocket);
							Socket.Select(list, null, null, 500);
							Int32 acceptCount = 0;

							int ret = Convert.ToInt32(Utility.InvokeMethod(_handler, "QueryContinue"));
							if (ret == 0)
							{
								Cancel();
								throw new Exception("发送文件已取消！");
							}

							if (list.Count > 0)
							{
								if (acceptCount >= _acceptTimeout) throw new Exception();
								_clientSocket = _serverSocket.Accept();
								acceptCount += 500;
								break;
							}
						}

						String httpHeader = String.Empty;
						while (httpHeader.IndexOf("\r\n\r\n") < 0)
						{
							byte[] buffer = new byte[1024];
							_clientSocket.Blocking = true;
							int len = _clientSocket.Receive(buffer);
							httpHeader += Encoding.ASCII.GetString(buffer, 0, len);
						}

						HttpHeader header = new HttpHeader(httpHeader);

						if (String.Compare(Path.GetFileName(header.FileName), "cancel", true) == 0) throw new Exception("对方拒绝接受文件！");

						SendFile(_clientSocket);

						_browser.BeginInvoke(new HandlerAfterUploadDelegate(HandlerAfterUpload), _sendBytes != _size, "");
					}
					catch (Exception ex)
					{
						if (_isCancel) _browser.BeginInvoke(new HandlerAfterUploadDelegate(HandlerAfterUpload), true, "");
						else HandlerError(ex.Message);
					}

					lock (m_Lock)
					{
						CloseSocket(_clientSocket);
						CloseSocket(_serverSocket);
						_clientSocket = null;
						_serverSocket = null;
					}
				}
				catch
				{
				}
			}

			void SendFile(Socket client)
			{
				using (Stream stream = File.OpenRead(_filename))
				{
					_size = stream.Length;

					Hashtable headers = new Hashtable();

					FileInfo fi = new FileInfo(_filename);
					headers.Add("Content-Length", stream.Length);
					headers.Add("Content-Type", "application/octet-stream");
					headers.Add("Content-Disposition", String.Format("attachment;filename={0}", HttpUtility.UrlEncode(fi.Name)));
					headers.Add("Connection", "close");

					StringBuilder builder = new StringBuilder();
					builder.Append("HTTP/1.1 200 OK\r\n");
					foreach (DictionaryEntry ent in headers)
					{
						builder.AppendFormat("{0}: {1}\r\n", ent.Key, ent.Value);
					}
					builder.Append("\r\n");
					client.Send(Encoding.ASCII.GetBytes(builder.ToString()));

					byte[] buffer = new byte[4096];
					while (true)
					{
						int readCount = stream.Read(buffer, 0, 4096);
						int sendCount = client.Send(buffer, 0, readCount, SocketFlags.None);
						_sendBytes += sendCount;
						HandlerProcessing(_sendBytes, _size);
						if (readCount < 4096) break;
					}
				}
				client.Send(Encoding.ASCII.GetBytes("\r\n"));
			}

			IPAddress GetIP()
			{
				foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
					{
						IPInterfaceProperties IPInterfaceProperties = ni.GetIPProperties();
						UnicastIPAddressInformationCollection UnicastIPAddressInformationCollection = IPInterfaceProperties.UnicastAddresses;
						foreach (UnicastIPAddressInformation UnicastIPAddressInformation in UnicastIPAddressInformationCollection)
						{
							if (UnicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
							{
								return UnicastIPAddressInformation.Address;
							}
						}
					}
				}
				return null;
			}

			public String Url
			{
				get { return String.Format("http://{0}:{1}/{2}", GetIP().ToString(), Port, Path.GetFileName(_filename)); }
			}

			long _sendBytes = 0, _size, _sendBytesStat = 0;
			long _ticks = 0;

			delegate void HandlerBeforeUploadDelegate(double total);
			void HandlerBeforeUpload(double total)
			{
				_handler.GetType().InvokeMember("BeforeUpload", BindingFlags.InvokeMethod, null, _handler, new object[] { total });
			}

			delegate void HandlerProcessingDelegate(long length, long size);
			void HandlerProcessing(long length, long size)
			{
				long temp = DateTime.Now.Ticks - _ticks;
				if (temp > 10000 * 1000 || length == size)
				{
					int ret = (int)_handler.GetType().InvokeMember("Processing", BindingFlags.InvokeMethod, null, _handler, new object[] { (double)length, (double)size, temp == 0 ? 0 : (double)((length - _sendBytesStat) * 1000 / (temp / 10000)) });
					if (ret == 0) Cancel();
					_ticks += temp;
					_sendBytesStat = length;
				}
			}

			delegate void HandlerAfterUploadDelegate(bool canceled, string path);
			void HandlerAfterUpload(bool canceled, string path)
			{
				_handler.GetType().InvokeMember("AfterUpload", BindingFlags.InvokeMethod, null, _handler, new object[] { path });
			}

			delegate void HandlerErrorDelegate(string msg);
			void HandlerError(string msg)
			{
				_handler.GetType().InvokeMember("HandleError", BindingFlags.InvokeMethod, null, _handler, new object[] { msg });
			}

			class HttpHeader
			{
				string _fileName = null;

				public String FileName
				{
					get { return _fileName; }
				}

				public HttpHeader(string header)
				{
					Regex reg = new Regex(@"GET\s+([^\s\r\n]+)\s+HTTP/1.1\s*\r\n", RegexOptions.IgnoreCase);
					Match match = reg.Match(header);
					_fileName = match.Success ? Path.GetFileName(match.Groups[1].Value).ToLower() : String.Empty;
					_fileName = HttpUtility.UrlDecode(_fileName);
				}
			}
		}
	}

	public class MyEncrypt
	{
		private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
		/// <summary>  
		/// DES加密字符串  
		/// </summary>  
		/// <param name="encryptString">待加密的字符串</param>  
		/// <param name="encryptKey">加密密钥,要求为8位</param>  
		/// <returns>加密成功返回加密后的字符串，失败返回源串</returns>  
		public static string EncryptDES(byte[] rgbKey, string encryptString)
		{
			try
			{
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
				DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Convert.ToBase64String(mStream.ToArray());
			}
			catch
			{
				return encryptString;
			}
		}
		/// <summary>  
		/// DES解密字符串  
		/// </summary>  
		/// <param name="decryptString">待解密的字符串</param>  
		/// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>  
		/// <returns>解密成功返回解密后的字符串，失败返源串</returns>  
		public static string DecryptDES(byte[] rgbKey, string decryptString)
		{
			try
			{
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Convert.FromBase64String(decryptString);
				DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Encoding.UTF8.GetString(mStream.ToArray());
			}
			catch
			{
				return decryptString;
			}
		}

		/// <summary>  
		/// DES加密字符串  
		/// </summary>  
		/// <param name="encryptString">待加密的字符串</param>  
		/// <param name="encryptKey">加密密钥,要求为8位</param>  
		/// <returns>加密成功返回加密后的字符串，失败返回源串</returns>  
		public static byte[] EncryptDES(byte[] rgbKey, byte[] rgbIV, string encryptString)
		{
			try
			{
				byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
				DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return mStream.ToArray();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>  
		/// DES解密字符串  
		/// </summary>  
		/// <param name="decryptString">待解密的字符串</param>  
		/// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>  
		/// <returns>解密成功返回解密后的字符串，失败返源串</returns>  
		public static string DecryptDES(byte[] rgbKey, byte[] rgbIV, byte[] inputByteArray)
		{
			try
			{
				DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Encoding.UTF8.GetString(mStream.ToArray());
			}
			catch
			{
				return null;
			}
		}
	}
}