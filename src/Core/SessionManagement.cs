using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using System.Data;

namespace Core
{
	public class ResponsesListener : IAsyncResult
	{
		AsyncCallback m_AsyncCallback = null;
		object m_Data = null;
		bool m_IsCompleted = false;

		String _sessionId;
		DateTime _createTime;

		public DateTime CreateTime
		{
			get { return _createTime; }
		}

		public String SessionID
		{
			get { return _sessionId; }
		}

		public ResponsesListener(String sessionId, AsyncCallback callback, Object extraData)
		{
			m_Data = extraData;
			m_AsyncCallback = callback;
			_createTime = DateTime.Now;
			_sessionId = sessionId;
		}

		bool IAsyncResult.IsCompleted { get { return m_IsCompleted; } }

		bool IAsyncResult.CompletedSynchronously { get { return false; } }

		WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }

		Object IAsyncResult.AsyncState { get { return m_Data; } }

		String m_Cache = "";

		public void Cache(object content)
		{
			m_Cache = content.ToString();
		}

		public void Send(HttpContext context)
		{
			context.Response.Write(m_Cache.ToString());
		}

		public void Complete(object data)
		{
			m_AsyncCallback(this);
			m_IsCompleted = true;
		}
	}

	public class CommandResponse : IRenderJson
	{
		public String CommandID;
		public String Data;

		public CommandResponse(String cmd, String data)
		{
			CommandID = cmd;
			Data = data;
		}

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(builder, "CommandID", CommandID, "Data", new JsonText(Data));
		}
	}

	public interface IAccountSessionEventHandler
	{
		void OnRemove();
	}

	public class AccountSession
	{
		object _lock = new object();

		DateTime _createdTime = DateTime.Now;

		public DateTime LatestAccessTime
		{
			get { return _createdTime; }
		}

		ResponsesListener _listener = null;

		List<CommandResponse> _cache = new List<CommandResponse>();

		string _sessionId = String.Empty;

		public string SessionID
		{
			get { return _sessionId; }
		}

		LinkedListNode<ClearSessionNode> _node = null;

		public LinkedListNode<ClearSessionNode> ListNode
		{
			get { return _node; }
		}

		public AccountSession(int user, string sessionid)
		{
			_sessionId = sessionid;
			_node = new LinkedListNode<ClearSessionNode>(new ClearSessionNode(user, sessionid, this));
		}

		public bool Receive(ResponsesListener listener)
		{
			lock (_lock)
			{
				_createdTime = DateTime.Now;

				if (_cache.Count > 0)
				{
					String json = Utility.RenderHashJson("IsSucceed", true, "Responses", _cache);
					listener.Cache(json);
					_cache.Clear();
					return true;
				}
				else
				{
					_listener = listener;
					return false;
				}
			}
		}

		public void Send(String commandId, String data)
		{
			lock (_lock)
			{
				_cache.Add(new CommandResponse(commandId, data));
				SendCache();
			}
		}

		public void SendCache()
		{
			lock (_lock)
			{
				if (_listener != null)
				{
					try
					{
						String json = Utility.RenderHashJson("IsSucceed", true, "Responses", _cache);
						_listener.Cache(json);
						_cache.Clear();
					}
					finally
					{
						ThreadPool.QueueUserWorkItem(_listener.Complete);
						_listener = null;
					}
				}
			}
		}

		IAccountSessionEventHandler event_handler_ = null;

		public IAccountSessionEventHandler SessionEventHandler
		{
			get { return event_handler_; }
		}

		public void SetEventHandler(IAccountSessionEventHandler event_handler)
		{
			event_handler_ = event_handler;
		}
	}

	public class AccountState
	{
		int m_UserID;

		public int UserID
		{
			get { return m_UserID; }
		}

		Dictionary<String, AccountSession> m_Sessions = new Dictionary<String, AccountSession>();

		//客户端最后一次连接到服务器的时间(保存到数据库中的值)
		DateTime m_LastAccessTimeS;

		//客户端最后一次连接到服务器的时间(实时)，每次客户端链接到服务器时，会立即更新该变量的值，为防止频繁写数据库，
		//不会立刻将m_LastAccessTime存储到数据库，只有当m_LastAccessTime - m_LastAccessTimeS > 2分钟时，才会将LastAccessTime存储
		//到数据库，m_LastAccessTimeS始终与数据库中Users表的LastAccessTime列的值相同
		DateTime m_LastAccessTime;

		public String Status = "Online";

		public AccountState(int id)
		{
			AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(id);
			m_UserID = userInfo.ID;
			m_LastAccessTime = AccountImpl.Instance.GetLastAccessTime(userInfo.ID);
			m_LastAccessTimeS = m_LastAccessTime;
		}

		private void SendUnreadMessage(AccountSession session)
		{
			try
			{
				if (m_UserID == AccountImpl.AdminID) return;

				DateTime from;
				lock (this)
				{
					from = m_LastAccessTime;
				}

				List<Message> msgs = MessageImpl.Instance.Find(m_UserID, 0, from);
				string data = String.Empty;

				if (msgs.Count > 0)
				{
					data = Utility.RenderHashJson(
						"Peer", 0,
						"Messages", msgs,
						"Users", Utility.GetRelUsers(msgs)
					);
					AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(m_UserID);
				}
				else
				{
					data = Utility.RenderHashJson(
						"Peer", 0,
						"Messages", JsonText.EmptyArray,
						"Users", JsonText.EmptyObject
					);
				}

				session.Send("GLOBAL:IM_MESSAGE_NOTIFY", data);
			}
			catch
			{
			}
		}

		public AccountSession NewSession(string sessionId)
		{
			Status = "Online";
			AccountSession session = null;
			lock (this)
			{
				string remove_session = null;

				foreach (KeyValuePair<String, AccountSession> ent in m_Sessions)
				{
					if ((ent.Value as AccountSession).SessionID != sessionId)
					{
						(ent.Value as AccountSession).Send("GLOBAL:OFFLINE", null);
						remove_session = (ent.Value as AccountSession).SessionID;
					}
				}

				if (!String.IsNullOrEmpty(remove_session))
				{
					m_Sessions.Remove(remove_session);
				}

				if (!m_Sessions.ContainsKey(sessionId))
				{
					m_Sessions[sessionId] = (session = new AccountSession(m_UserID, sessionId));
				}
				session = m_Sessions[sessionId] as AccountSession;
#				if TRACE
				ServerImpl.Instance.WriteLog(String.Format("New Session:SessionID = \"{0}\", UserID=\'{1}\'", sessionId, m_UserID));
#				endif
			}
			if (session != null)
			{
				SendUnreadMessage(session);
				SessionManagement.Instance.Insert(session);
			}

			lock (this)
			{
				m_LastAccessTime = DateTime.Now;
				UpdateLastAccessTime();
			}

			AccountInfo info = AccountImpl.Instance.GetUserInfo(UserID);
			SessionManagement.Instance.Send("UserStateChanged", Utility.RenderHashJson("User", info.ID, "State", "Online", "Details", info.DetailsJson));

			return session;
		}

		public bool Receive(string sessionId, ResponsesListener listener)
		{
			bool sendOnline = (m_Sessions.Count == 0);

			AccountSession session = null;
			bool reset = false;

			lock (this)
			{
				if (!m_Sessions.ContainsKey(sessionId))
				{
					m_Sessions[sessionId] = new AccountSession(m_UserID, sessionId);
					reset = true;
				}

				session = m_Sessions[sessionId] as AccountSession;
			}

			if (session != null)
			{
				SessionManagement.Instance.Insert(session);
			}

			if (reset)
			{
#				if TRACE
				ServerImpl.Instance.WriteLog(String.Format("Reset Session:SessionID = \"{0}\", UserID=\'{1}\'", sessionId, m_UserID));
#				endif

				if (session != null)
				{
					session.Send("GLOBAL:SessionReset", "null");
					SendUnreadMessage(session);
				}
			}

			lock (this)
			{
				m_LastAccessTime = DateTime.Now;

				if (reset || (m_LastAccessTime - m_LastAccessTimeS).TotalSeconds > 120)
				{
					UpdateLastAccessTime();
				}
			}

			if (sendOnline)
			{
				AccountInfo info = AccountImpl.Instance.GetUserInfo(UserID);
				SessionManagement.Instance.Send("UserStateChanged", Utility.RenderHashJson("User", info.ID, "State", "Online", "Details", info.DetailsJson));
			}

			return session.Receive(listener);
		}

		public void UpdateLastAccessTime()
		{
			lock (this)
			{
				m_LastAccessTimeS = m_LastAccessTime;
				AccountImpl.Instance.UpdateLastAccessTime(m_UserID, m_LastAccessTimeS);
			}
			ServerImpl.Instance.WriteLog(String.Format("UpdateLastAccessTime: UserID = {0}, LastAccessTime = {1:yyyy-MM-dd HH:mm:ss}", m_UserID, m_LastAccessTimeS));
		}

		public void Timeout(String sessionId)
		{
			lock (this)
			{
				AccountSession session = m_Sessions[sessionId] as AccountSession;
				session.SendCache();
			}
		}

		public void Remove(String sessionId)
		{
			AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(m_UserID);
			lock (this)
			{
				bool update_last_accss_time = m_Sessions.Count > 0;

				if (m_Sessions.ContainsKey(sessionId))
				{
					AccountSession session = m_Sessions[sessionId] as AccountSession;
					session.SendCache();
					m_Sessions.Remove(sessionId);
					if (session.SessionEventHandler != null) session.SessionEventHandler.OnRemove();

					if (update_last_accss_time && m_Sessions.Count == 0)
					{
						UpdateLastAccessTime();
					}
				}
			}
		}

		public int SessionCount
		{
			get { return m_Sessions.Count; }
		}

		public void Send(String commandId, String data)
		{
			List<String> ol = new List<string>();

			lock (this)
			{
				foreach (KeyValuePair<String, AccountSession> ent in m_Sessions)
				{
					(ent.Value as AccountSession).Send(commandId, data);
				}
			}
		}

		public Boolean IsOnline
		{
			get
			{
				return m_Sessions.Count > 0;
			}
		}

		public DateTime LastAccessTime
		{
			get { return m_LastAccessTime; }
		}

		public AccountSession FindSession(string session_id)
		{
			lock (this)
			{
				foreach (KeyValuePair<String, AccountSession> ent in m_Sessions)
				{
					AccountSession session = ent.Value as AccountSession;
					if(session.SessionID==session_id) return session;
				}
				return null;
			}
		}
	}

	public class ClearSessionNode
	{
		public DateTime InsertTime;

		int _userID;
		String _sessionID;

		AccountSession _session;

		public AccountSession Session
		{
			get { return _session; }
		}

		public String SessionID
		{
			get { return _sessionID; }
		}

		public int UserID
		{
			get { return _userID; }
		}

		public ClearSessionNode(int user, string sessionid, AccountSession session)
		{
			_userID = user;
			_sessionID = sessionid;
			_session = session;
		}
	}

	public class SessionManagement
	{
		static SessionManagement m_Instance = new SessionManagement();

		static public SessionManagement Instance
		{
			get { return m_Instance; }
		}

		public const Int32 TIMER_PERIOD = 20 * 1000;
		public const Int32 SESSION_ONLINE_TIMEOUT = 120 * 1000;
		public const Int32 SESSION_TIMEOUT = 55 * 1000;
		LinkedList<ClearSessionNode> m_ClearSessionList = new LinkedList<ClearSessionNode>();

		int m_StatCount = 0;

		Timer m_Timer = null;

		private void TimerProc(object state)
		{
			try
			{
				List<LinkedListNode<ClearSessionNode>> removeNodes = new List<LinkedListNode<ClearSessionNode>>();
				List<ClearSessionNode> timeoutNodes = new List<ClearSessionNode>();

#				if TRACE
				StringBuilder sessions = new StringBuilder();
#				endif

				lock (m_ClearSessionList)
				{
					DateTime now = DateTime.Now;

					LinkedListNode<ClearSessionNode> n = m_ClearSessionList.First;
					while (n != null)
					{
						double diff = (now - n.Value.InsertTime).TotalMilliseconds;
						if (diff > SESSION_ONLINE_TIMEOUT) removeNodes.Add(n);
						else if (diff > SESSION_TIMEOUT) timeoutNodes.Add(n.Value);
						else break;
						n = n.Next;
					}

					foreach (LinkedListNode<ClearSessionNode> rn in removeNodes)
					{
						m_ClearSessionList.Remove(rn);

#						if TRACE
						if (sessions.Length > 0) sessions.Append(",");
						sessions.AppendFormat("({0},{1})", rn.Value.UserID, rn.Value.SessionID);
#						endif
					}
				}

#				if TRACE
				if (sessions.Length > 0)
				{
					ServerImpl.Instance.WriteLog(String.Format("Clear Sessions Timer:Clear = \"{0}\"", sessions));
				}
#				endif

				foreach (ClearSessionNode tn in timeoutNodes)
				{
					try
					{
						AccountState s = GetAccountState(tn.UserID);
						if (s != null) s.Timeout(tn.SessionID);
					}
					catch
					{
					}
				}

				Hashtable offlineNotifyUsers = new Hashtable();

				foreach (LinkedListNode<ClearSessionNode> rn in removeNodes)
				{
					try
					{
						AccountState s = GetAccountState(rn.Value.UserID);
						offlineNotifyUsers[rn.Value.UserID] = rn.Value.UserID;
						if (s != null) s.Remove(rn.Value.SessionID);
						if (!s.IsOnline)
						{
							lock (m_Accounts)
							{
								m_Accounts.Remove(s.UserID);
							}
						}
					}
					catch
					{
					}
				}

				foreach (DictionaryEntry ent in offlineNotifyUsers)
				{
					if (!SessionManagement.Instance.IsOnline((int)ent.Value))
					{
						AccountInfo info = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(ent.Key));
						Send("UserStateChanged", Utility.RenderHashJson("User", ent.Key, "State", "Offline", "Details", info.DetailsJson));
					}
				}

#				if TRACE
				if ((++m_StatCount) > 6)
				{
					int account_count = 0, session_count = 0;
					lock (m_Accounts)
					{
						foreach (KeyValuePair<int, AccountState> ent in m_Accounts)
						{
							AccountState accstate = ent.Value;
							int sc = accstate.SessionCount;
							if (sc > 0)
							{
								account_count++;
								session_count += sc;
							}

						}
					}
					m_StatCount = 0;
					ServerImpl.Instance.WriteLog(String.Format("All Sessions: Account Count = {0}, Session Count = {1}", account_count, session_count));
				}
#				endif
			}
			catch
			{
			}
		}

		public void Insert(AccountSession session)
		{
			lock (m_ClearSessionList)
			{
				if (m_ClearSessionList != session.ListNode.List)
				{
					m_ClearSessionList.AddLast(session.ListNode);
				}
				else
				{
					m_ClearSessionList.Remove(session.ListNode);
					m_ClearSessionList.AddLast(session.ListNode);
				}
				session.ListNode.Value.InsertTime = DateTime.Now;
			}
		}

		private SessionManagement()
		{
			m_Timer = new Timer(this.TimerProc);
			m_Timer.Change(0, TIMER_PERIOD);
		}

		Dictionary<int, AccountState> m_Accounts = new Dictionary<int, AccountState>();

		private AccountState GetAccountState(int userid)
		{
			lock (m_Accounts)
			{
				if (!m_Accounts.ContainsKey(userid))
				{
					m_Accounts[userid] = new AccountState(userid);
				}
				return m_Accounts[userid] as AccountState;
			}
		}

		public bool IsOnline(int userid)
		{
			lock (m_Accounts)
			{
				if (!m_Accounts.ContainsKey(userid)) return false;
				else return (m_Accounts[userid] as AccountState).IsOnline;
			}
		}

		public void MarkStatus(int user, string status)
		{
			if (IsOnline(user))
			{
				AccountInfo ai = AccountImpl.Instance.GetUserInfo(user);
				AccountState state = GetAccountState(user);
				if (state.Status != status)
				{
					SessionManagement.Instance.Send(
						"UserStateChanged",
						Utility.RenderHashJson("User", ai.ID, "State", status, "Details", ai.DetailsJson)
					);
					state.Status = status;
				}
			}
		}

		public string GetStatus(int user)
		{
			lock (m_Accounts)
			{
				if(!IsOnline(user)) return "Offline";
				return GetAccountState(user).Status;
			}
		}

		public void Send(int user, string command, string data)
		{
			if (AccountImpl.Instance.GetUserInfo(user) == null) return;
			if(IsOnline(user))
			{
				GetAccountState(user).Send(command, data);
			}
		}

		public void Send(string command, string data)
		{
			List<AccountState> accs = new List<AccountState>();
			lock (m_Accounts)
			{
				foreach (KeyValuePair<int, AccountState> ent in m_Accounts)
				{
					accs.Add(ent.Value as AccountState);
				}
			}

			foreach (AccountState state in accs)
			{
				state.Send(command, data);
			}
		}

		public void NotifyDeptDataChanged(int dept_id)
		{
			NotifyDeptDataChanged(dept_id, false);
		}

		public void NotifyDeptDataChanged(int dept_id, bool send_admin)
		{
			DataRow dept_Info = AccountImpl.Instance.GetDeptInfo(dept_id);
			List<AccountState> accs = new List<AccountState>();
			lock (m_Accounts)
			{
				foreach (KeyValuePair<int, AccountState> ent in m_Accounts)
				{
					AccountState state = ent.Value as AccountState;
					if (state.IsOnline)
					{
						AccountInfo u = AccountImpl.Instance.GetUserInfo(state.UserID);
						if (u != null && u.SubType == 1 && (u.ID != AccountImpl.AdminID || send_admin))
						{
							accs.Add(ent.Value as AccountState);
						}
					}
				}
			}

			String notify_json = Utility.RenderHashJson("DeptID", dept_id, "DeptInfo", dept_Info);
			foreach (AccountState state in accs)
			{
				state.Send("GLOBAL:CLEAR_DEPT_DATA", notify_json);
			}
		}

		public void SendToMultiUsers(int[] users, string command, string data)
		{
			foreach (int u in users)
			{
				SessionManagement.Instance.Send(u, command, data);
			}
		}

		public void SendToRelatedUsers(int user, string command, string data)
		{
			int[] relatedUsers = AccountImpl.Instance.GetRelatedUsers(user);
			SessionManagement.Instance.SendToMultiUsers(relatedUsers, command, data);
		}

		public void SendToGroupMembers(int group, string command, string data)
		{
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(group);
			foreach (int member in groupInfo.Friends)
			{
				AccountInfo memberInfo = AccountImpl.Instance.GetUserInfo(member);
				SessionManagement.Instance.Send(memberInfo.ID, command, data);
			}
		}

		public void NotifyResetListCache(int[] users, bool send_dept_user, bool send_dept, AccountInfo[] update_accs)
		{
			string notify_json = CreateResetListCacheJson(send_dept_user, send_dept, update_accs);
			SendToMultiUsers(users, "GLOBAL:RESET_LIST_CACHE", notify_json);
		}

		public void NotifyResetListCache(int user, bool send_dept_user, bool send_dept, AccountInfo[] update_accs)
		{
			string notify_json = CreateResetListCacheJson(send_dept_user, send_dept, update_accs);
			SendToRelatedUsers(user, "GLOBAL:RESET_LIST_CACHE", notify_json);
			Send(user, "GLOBAL:RESET_LIST_CACHE", notify_json);
		}

		private static string CreateResetListCacheJson(bool send_dept_user, bool send_dept, AccountInfo[] update_accs)
		{
			List<AccountInfo.Details> update_accs_info = null;
			DataRowCollection depts = null;
			Hashtable dept_user = null;
			if (send_dept_user)
			{
				dept_user = AccountImpl.Instance.GetDeptUser();
			}
			if (send_dept)
			{
				depts = AccountImpl.Instance.GetAllDepts(-1);
			}
			if (update_accs != null)
			{
				update_accs_info = new List<AccountInfo.Details>();
				foreach (AccountInfo acc in update_accs)
				{
					update_accs_info.Add(acc.DetailsJson);
				}
			}
			string notify_json = Utility.RenderHashJson(
				"Update", update_accs_info,
				"Depts", depts,
				"DeptUser", dept_user
			);
			return notify_json;
		}

		public void UpdateLastAccessTimeAll()
		{
			lock (m_Accounts)
			{
				foreach (KeyValuePair<int, AccountState> ent in m_Accounts)
				{
					AccountState state = ent.Value;
					if(state.IsOnline) state.UpdateLastAccessTime();
				}
			}
		}

		public AccountSession FindSession(int user, string session_id)
		{
			if (!IsOnline(user)) return null;
			AccountState state = GetAccountState(user);
			return state.FindSession(session_id);
		}

		public bool Receive(int user, string sessionId, ResponsesListener listener)
		{
			return GetAccountState(user).Receive(sessionId, listener);
		}

		public AccountSession NewSession(int user, string sessionId)
		{
			return GetAccountState(user).NewSession(sessionId);
		}

		public void RemoveSession(int user, string session_id)
		{
			GetAccountState(user).Remove(session_id);
		}

	}
}
