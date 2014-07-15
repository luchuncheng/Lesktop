using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Net.Json;

namespace Client
{
	public class CommandHandler
	{
		public String CommandID;
		public object Hanlder;

		public CommandHandler(String cid, object handler)
		{
			CommandID = cid;
			Hanlder = handler;
		}
	}

	delegate void CommandHandlerCallbackDelegate(CommandHandler ch, object data);

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class ResponsesHandler
	{
		public ResponsesHandler()
		{
		}

		public void Start()
		{
			lock (m_Lock)
			{
				if (m_Thread == null)
				{
					m_Thread = new ReceiveResponseThread();
					m_Thread.Start();
				}
			}
		}

		public void Stop()
		{
			lock (m_Lock)
			{
				if (m_Thread != null)
				{
					m_Thread.Stop();
					m_Thread = null;
				}
			}
		}

		public bool IsRunning()
		{
			return m_Thread != null && m_Thread.IsRunning;
		}

		static object m_Lock = new object();
		static Hashtable m_Handlers = new Hashtable();
		static ReceiveResponseThread m_Thread = null;
		static Hashtable m_StateData = new Hashtable();

		public void SetState(string key, object data)
		{
			lock (m_StateData)
			{
				m_StateData[key] = data;
			}
		}

		public object GetState(string key)
		{
			lock (m_StateData)
			{
				return m_StateData[key];
			}
		}

		public Hashtable GetState()
		{
			return m_StateData;
		}

		public void NewCommandHandler(string commandId, object handler)
		{
			lock (m_Handlers)
			{
				m_Handlers[commandId] = new CommandHandler(commandId, handler);
			}
		}

		public void RemoveCommandHandler(string commandId)
		{
			lock (m_Handlers)
			{
				m_Handlers.Remove(commandId);
			}
		}

		public void InvokeCallback(string commandId, object data)
		{
			lock (m_Handlers)
			{
				if (m_Handlers.ContainsKey(commandId))
				{
					CommandHandler ch = m_Handlers[commandId] as CommandHandler;
					if (Client.Global.Desktop.IsHandleCreated && !Client.Global.Desktop.IsDisposed)
					{
						Client.Global.Desktop.BeginInvoke(new CommandHandlerCallbackDelegate(InvokeHandlerCallback), ch, data);
					}
					m_Handlers.Remove(commandId);
				}
			}
		}

		public void InvokeErrorCallback(string commandId, object data)
		{
			lock (m_Handlers)
			{
				if (commandId == "all")
				{
					foreach (DictionaryEntry ent in m_Handlers)
					{
						CommandHandler ch = ent.Value as CommandHandler;

						if (Client.Global.Desktop.IsHandleCreated && !Client.Global.Desktop.IsDisposed)
						{
							Client.Global.Desktop.BeginInvoke(new CommandHandlerCallbackDelegate(InvokeHandlerErrorCallback), ch, data);
						}
					}
					m_Handlers = new Hashtable();
				}
				else
				{
					if (m_Handlers.ContainsKey(commandId))
					{
						CommandHandler ch = m_Handlers[commandId] as CommandHandler;
						if (Client.Global.Desktop.IsHandleCreated && !Client.Global.Desktop.IsDisposed)
						{
							Client.Global.Desktop.BeginInvoke(new CommandHandlerCallbackDelegate(InvokeHandlerErrorCallback), ch, data);
						}
						m_Handlers.Remove(commandId);
					}
				}
			}
		}

		static void InvokeHandlerCallback(CommandHandler ch, object data)
		{
			if (data.GetType().FullName == "System.__ComObject")
			{
				Utility.InvokeMethod(ch.Hanlder, "Callback", data, "data");
			}
			else
			{
				Utility.InvokeMethod(ch.Hanlder, "Callback", Utility.RenderJson(data), "json");
			}
		}


		static void InvokeHandlerErrorCallback(CommandHandler ch, object data)
		{
			if (data.GetType().FullName == "System.__ComObject")
			{
				Utility.InvokeMethod(ch.Hanlder, "ErrorCallback", data, "data");
			}
			else
			{
				Utility.InvokeMethod(ch.Hanlder, "ErrorCallback", Utility.RenderJson(data), "json");
			}
		}

		static ResponsesHandler kInstance = new ResponsesHandler();

		public static ResponsesHandler Instance
		{
			get { return ResponsesHandler.kInstance; }
		}
	}

	public class ReceiveResponseThread
	{
		Thread m_Thread = null;
		Boolean m_Stop = true;
		AutoResetEvent m_Event = null;

		public ReceiveResponseThread()
		{
			m_Thread = new Thread(ThreadEntry);
		}

		public bool IsRunning
		{
			get
			{
				return !m_Stop;
			}
		}

		public void Start()
		{
			lock (this)
			{
				if (m_Stop)
				{
					m_Stop = false;
					m_Event = new AutoResetEvent(false);
					m_Thread.Start();
					Global.TrayIcon.Icon = Properties.Resources.Tray;
				}
			}
		}

		public void Stop()
		{
			lock (this)
			{
				if (!m_Stop)
				{
					m_Stop = true;
					if (m_WebRequest != null) m_WebRequest.Abort();
					if (!m_Event.WaitOne(10000, false))
					{
						m_Thread.Abort();
					}
					Global.TrayIcon.Icon = Properties.Resources.TrayGred;
				}
			}
		}

		HttpWebRequest m_WebRequest = null;

		void ThreadEntry()
		{
			int error_count = 0;
			while (!m_Stop)
			{
				if (error_count == 3)
				{
					ResponsesHandler.Instance.SetState("Status", "Offline");
					Global.TrayIcon.Icon = Properties.Resources.TrayGred;
				}

				Byte[] buffer;
				try
				{
					if (m_Stop) break;
					String reqid = DateTime.Now.Ticks.ToString();
					m_WebRequest = HttpWebRequest.Create(Global.ResUrl + "/Responses.ashx?RequestID=" + reqid) as HttpWebRequest;
					SessionImpl.Instance.WriteLog(String.Format("Post Begin: Url = Responses.ashx"));
					m_WebRequest.Method = "POST";
					//m_WebRequest.KeepAlive = true;
					//m_WebRequest.ServicePoint.Expect100Continue = true;
					//m_WebRequest.ServicePoint.SetTcpKeepAlive(true, 60 * 60 * 1000, 1000);
					m_WebRequest.Timeout = 2 * 60 * 1000;
					m_WebRequest.ContentType = "application/x-www-form-urlencoded";
					m_WebRequest.Headers.Add("Cookie", SessionImpl.Instance.GetCookie());
					string state = Utility.RenderJson(ResponsesHandler.Instance.GetState());
					string data = String.Format("RequestID={0}&SessionID={1}&ClientMode=true&ClientVersion={2}&ServerVersion={3}&State={4}", 
						reqid, 
						SessionImpl.Instance.GetSessionID(), 
						Assembly.GetExecutingAssembly().GetName().Version.ToString(), 
						Global.ServerVersion,
						state
					);
					buffer = Encoding.UTF8.GetBytes(data);
					m_WebRequest.GetRequestStream().Write(buffer, 0, buffer.Length);
					m_WebRequest.GetRequestStream().Close();
					HttpWebResponse response = m_WebRequest.GetResponse() as HttpWebResponse;
					buffer = new byte[response.ContentLength];

					int length = 0;
					while (true)
					{
						int count = response.GetResponseStream().Read(buffer, length, (int)buffer.Length - length);
						if (count > 0) length += count;
						if (length == buffer.Length || count < 0) break;
					}
					response.Close();
				}
				catch (Exception ex)
				{
					error_count++;
					SessionImpl.Instance.WriteLog(String.Format("Post Error: Url = Responses.ashx, Message = {1}", m_WebRequest.RequestUri.ToString(), ex.Message));
					ResponsesHandler.Instance.InvokeErrorCallback("all", new Exception("服务器错误!"));
					Thread.Sleep(3000);
					continue;
				}

				if (error_count >= 3) Global.TrayIcon.Icon = Properties.Resources.Tray;
				error_count = 0;
				ResponsesHandler.Instance.SetState("Status", Program.IsLeave ? "Leave" : "Online");
				try
				{
					String content = Encoding.UTF8.GetString(buffer);
					//SessionImpl.Global.WriteLog(String.Format("Post Success: Url = Responses.ashx, ResponseText = {1}", m_WebRequest.RequestUri.ToString(), content));
					if (m_Stop) break;
					if (!String.IsNullOrEmpty(content))
					{
						Global.Desktop.BeginInvoke(new ProcessDelegate(this.Process), content);
					}
					else
					{
					}
				}
				catch
				{
				}

				Thread.Sleep(100);
			}
			m_Event.Set();
		}

		private delegate void ProcessDelegate(string content);

		private void Process(string content)
		{
			try
			{
				Utility.InvokeMethod(SessionImpl.Instance.GetGlobal("ReponsesProcess"), "Process", content);
			}
			catch
			{
			}
		}
	}
}

