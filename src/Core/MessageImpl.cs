using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using Microsoft.JScript;
using System.Reflection;
using Core.IO;

namespace Core
{
	/// <summary>
	///MessageManagement 的摘要说明
	/// </summary>
	public class MessageImpl
	{
		static MessageImpl m_Instance = new MessageImpl();

		static public MessageImpl Instance
		{
			get { return m_Instance; }
		}

		private MessageImpl()
		{
			m_Timer = new Timer(this.TimerProc);
#		if DEBUG
			m_Timer.Change(0, 60 * 60 * 1000);
#		else
			m_Timer.Change(0, 10 * 60 * 1000);
#		endif
		}

		Timer m_Timer = null;

		private void TimerProc(object state)
		{
			try
			{
				WriteCache();
			}
			catch
			{
			}
		}

		Object m_Lock = new Object();
		int m_MaxKey = 1;
		DateTime m_MaxCreatedTime = DateTime.Now;

#		if DEBUG
		public const Int32 MAX_CACHE_COUNT = 100;
#		else
		public const Int32 MAX_CACHE_COUNT = 100;
#		endif

		IMessageStorage m_MessageStorageImpl = null;

		IMessageStorage MessageStorageImpl
		{
			get
			{
				lock (m_Lock)
				{
					if (m_MessageStorageImpl == null) Initialize(HttpContext.Current);
					return m_MessageStorageImpl;
				}
			}
		}

		public void Initialize(HttpContext context)
		{
			lock (m_Lock)
			{
				if (m_MessageStorageImpl == null)
				{
					string accStorage = System.Web.Configuration.WebConfigurationManager.AppSettings["MessageStorageImpl"];
					String[] accStorageInfo = accStorage.Split(new char[] { ' ' });
					Type type = Assembly.Load(accStorageInfo[0]).GetType(accStorageInfo[1]);
					ConstructorInfo ctor = type.GetConstructor(new Type[] { });
					m_MessageStorageImpl = ctor.Invoke(new object[] { }) as IMessageStorage;

					m_MaxKey = MessageStorageImpl.GetMaxKey();
					m_MaxCreatedTime = MessageStorageImpl.GetCreatedTime();
				}
			}
		}

		public void Dispose()
		{
		}

		Hashtable m_ChatRoom = new Hashtable();

		public void JoinChatRoom(int room_id, AccountInfo user)
		{
			lock (m_ChatRoom)
			{
				if (!m_ChatRoom.ContainsKey(room_id)) m_ChatRoom.Add(room_id, new Hashtable());
				(m_ChatRoom[room_id] as Hashtable)[user.ID] = user;
			}
		}

		public void LeaveChatRoom(int room_id, AccountInfo user)
		{
			lock (m_ChatRoom)
			{
				if (!m_ChatRoom.ContainsKey(room_id)) return;
				if ((m_ChatRoom[room_id] as Hashtable).ContainsKey(user.ID)) (m_ChatRoom[room_id] as Hashtable).Remove(user.ID);
			}
		}

		public AccountInfo[] GetChatRoomUsers(int room_id)
		{
			lock (m_ChatRoom)
			{
				if (!m_ChatRoom.ContainsKey(room_id)) return null;
				Hashtable room_users = m_ChatRoom[room_id] as Hashtable;
				List<AccountInfo> users = new List<AccountInfo>();
				foreach (DictionaryEntry de in room_users)
				{
					users.Add(de.Value as AccountInfo);
				}
				return users.ToArray();
			}
		}

		public List<Message> FindInDatabase(int receiver, int sender, Nullable<DateTime> from)
		{
			lock (m_Lock)
			{
				return MessageStorageImpl.Find(receiver == 0 ? 0 : receiver, sender == 0 ? 0 : sender, from);
			}
		}

		/// <summary>
		/// 插入新的消息，插入消息后将查询m_Listeners中是否有符合条件的监听器，如存在，同时将消息发送出去
		/// </summary>
		public Message NewMessage(int receiver, int sender, String content, Hashtable data, bool isTemp)
		{
			AccountInfo senderInfo = AccountImpl.Instance.GetUserInfo(sender);
			AccountInfo receiverInfo = AccountImpl.Instance.GetUserInfo(receiver);
			if (senderInfo.ID == AccountImpl.AdminID || receiverInfo.ID == AccountImpl.AdminID) throw new Exception("系统管理员(admin) 不能发送或接受即时消息！");
			if (senderInfo == null || receiverInfo == null) throw new Exception("无效的消息接收者，该用户可能已被删除！");
			int key = 0;
			lock (m_Lock)
			{
				key = ++m_MaxKey;
			}
			if ((senderInfo.IsTemp || receiverInfo.IsTemp) || Custom.ApplicationInfo.FilterHtml) content = HtmlUtil.ReplaceHtml(content);
			MsgAccessoryEval eval = new MsgAccessoryEval(key, receiverInfo.ID, senderInfo.ID, data);
			Regex reg = new Regex("{Accessory [^\f\n\r\t\v<>]+}");
			content = reg.Replace(content, eval.Replace);
			lock (m_Lock)
			{
				Message message = new Message(
					senderInfo, receiverInfo,
					content, new DateTime((DateTime.Now.Ticks / 10000) * 10000), key
				);

				List<Message> messages = new List<Message>();
				messages.Add(message);

				if (isTemp)
				{
					message.IsTemp = isTemp;
				}

				if (receiverInfo.Type == 1 && receiverInfo.SubType == 3)
				{
					string cmdData = Utility.RenderHashJson(
						"Peer", senderInfo.ID,
						"Message", message,
						"Users", Utility.GetRelUsers(message)
					);

					AccountInfo[] users = GetChatRoomUsers(receiverInfo.ID);

					if (senderInfo.ID != receiverInfo.Creator)
					{
						AccountInfo creatorInfo = AccountImpl.Instance.GetUserInfo(receiverInfo.Creator);
						SessionManagement.Instance.Send(creatorInfo.ID, "GLOBAL:IM_MESSAGE_NOTIFY", cmdData);
					}

					foreach (AccountInfo acc in users)
					{
						if (acc.ID != senderInfo.ID) SessionManagement.Instance.Send(acc.ID, "GLOBAL:IM_MESSAGE_NOTIFY", cmdData);
					}
				}
				else
				{
					if (receiverInfo.Type == 0)
					{
						if (SessionManagement.Instance.IsOnline(receiverInfo.ID))
						{
							try
							{
								string cmdData = Utility.RenderHashJson(
									"Peer", senderInfo.ID,
									"Message", message,
									"Users", Utility.GetRelUsers(message)
								);

								SessionManagement.Instance.Send(receiverInfo.ID, "GLOBAL:IM_MESSAGE_NOTIFY", cmdData);
							}
							catch
							{
							}
						}
					}
					else
					{
						AccountInfo groupInfo = receiverInfo;
						foreach (int member in groupInfo.Friends)
						{
							try
							{
								AccountInfo memberInfo = AccountImpl.Instance.GetUserInfo(member);
								if (senderInfo.ID != AccountImpl.AdminstratorID && memberInfo.ID == senderInfo.ID) continue;
								if (memberInfo.ID != 3)
								{
									if (SessionManagement.Instance.IsOnline(memberInfo.ID))
									{
										string cmdData = Utility.RenderHashJson(
											"Peer", groupInfo.ID,
											"Message", message,
											"Users", Utility.GetRelUsers(message)
										);

										SessionManagement.Instance.Send(memberInfo.ID, "GLOBAL:IM_MESSAGE_NOTIFY", cmdData);
									}
								}
							}
							catch
							{
							}
						}
					}

					if (!isTemp)
					{
						MessageCacheManagement.Instance.Insert(receiver, message);

						if (MessageCacheManagement.Instance.Count >= MAX_CACHE_COUNT)
						{
							WriteCache();
						}
					}
				}
				return message;
			}
		}

		public void WriteCache()
		{
			try
			{
				if (MessageCacheManagement.Instance.Count > 0)
				{
					List<Message> cacheMsgs = MessageCacheManagement.Instance.GetAll();
					MessageStorageImpl.Write(cacheMsgs);
					MessageCacheManagement.Instance.Clear();
					ServerImpl.Instance.WriteLog(String.Format("Write Cache: MaxCreatedTime = {0:yyyy-MM-dd HH:mm:ss}, Count = {1}", cacheMsgs[cacheMsgs.Count - 1].CreatedTime, cacheMsgs.Count));
				}
				else
				{
					ServerImpl.Instance.WriteLog("Write Cache: Count = 0");
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// 添加消息监听器，如果查找到符合监听器条件的消息，返回false，此时不会添加监听器
		/// 如果没有查找到符合监听器条件的消息，返回true，此时监听器将被添加到m_Listeners中
		/// </summary>
		public List<Message> Find(int receiver, int sender, Nullable<DateTime> from)
		{
			lock (m_Lock)
			{
				//获取用户receiver缓存的消息的最小发送时间
				Nullable<DateTime> min = MessageCacheManagement.Instance.GetMinCreatedTime(receiver);

				List<Message> messages = new List<Message>();

				//当from >= 缓存在内存中的消息的最小时间时，不必查询数据库
				if (min == null || from == null || from.Value < min.Value)
				{
					//查询数据库
					messages.AddRange(FindInDatabase(receiver, sender, from));
					if (AccountImpl.Instance.GetUserInfo(receiver).Type == 0)
					{
						messages.AddRange(FindInDatabase(sender, receiver, from));
					}
				}

				//在缓存中查询
				messages.AddRange(MessageCacheManagement.Instance.Find(receiver, sender, from.Value));
				if (AccountImpl.Instance.GetUserInfo(receiver).Type == 0)
				{
					messages.AddRange(MessageCacheManagement.Instance.Find(sender, receiver, from.Value));
				}

				return messages;
			}
		}

		public DataTable GetMessageList_Group(int isTemp, int pageSize, int pageIndex, out int pageCount)
		{
			WriteCache();
			return MessageStorageImpl.GetMessageList_Group(isTemp, pageSize, pageIndex, out pageCount);
		}

		public DataTable GetMessageList_User(int userId, int peerId, int pageSize, int pageIndex, out int pageCount)
		{
			WriteCache();
			return MessageStorageImpl.GetMessageList_User(userId, peerId, pageSize, pageIndex, out pageCount);
		}

		public DataTable GetUserMessages(int userId, int peerId, DateTime from, DateTime to, int pageSize, ref  int pageIndex, out int pageCount, int msgId)
		{
			WriteCache();
			return MessageStorageImpl.GetUserMessages(userId, peerId, from, to, pageSize, ref pageIndex, out pageCount, msgId);
		}

		public DataTable GetGroupMessages(int groupId, DateTime from, DateTime to, int pageSize, ref int pageIndex, out int pageCount, int msgId)
		{
			WriteCache();
			return MessageStorageImpl.GetGroupMessages(groupId, from, to, pageSize, ref pageIndex, out pageCount, msgId);
		}

		public void DeleteMessages(string ids)
		{
			MessageStorageImpl.DeleteMessages(ids);
		}

		public void DeleteMessages(int userId, int peerId)
		{
			MessageStorageImpl.DeleteMessages(userId, peerId);
		}

		public List<Message> GetMessages(int user, int peer, DateTime from, DateTime to, ref int page, int pagesize, out int pagecount, int msgid, string content)
		{
			WriteCache();
			return MessageStorageImpl.GetMessages(user, peer, from, to, ref page, pagesize, out pagecount, msgid, content);
		}

		public DataTable GetMessageRecordUsers(int id)
		{
			WriteCache();
			return MessageStorageImpl.GetMessageRecordUsers(id);
		}

		public DateTime GetRecvMsgMaxTime(int id)
		{
			return MessageStorageImpl.GetRecvMsgMaxTime(id);
		}

		public bool CheckPermission(int user_id, int msg_id)
		{
			return MessageStorageImpl.CheckPermission(user_id, msg_id);
		}
	}

	public class MessageCacheManagement
	{
		static MessageCacheManagement m_Instance = new MessageCacheManagement();

		static public MessageCacheManagement Instance
		{
			get { return m_Instance; }
		}

		private MessageCacheManagement()
		{
		}

		Int32 m_Count = 0;
		Dictionary<int, List<Message>> m_Cache = new Dictionary<int, List<Message>>();

		List<Message> GetUserMessageCache(int user)
		{
			if (!m_Cache.ContainsKey(user))
			{
				m_Cache.Add(user, new List<Message>());
			}

			return m_Cache[user] as List<Message>;
		}

		/// <summary>
		/// 清除缓存
		/// </summary>
		public void Clear()
		{
			lock (m_Cache)
			{
				List<Message> msgs = new List<Message>();
				foreach (KeyValuePair<int, List<Message>> ent in m_Cache)
				{
					(ent.Value as List<Message>).Clear();
				}
				m_Count = 0;
			}
		}

		/// <summary>
		/// 获取所有缓存的消息
		/// </summary>
		/// <returns></returns>
		public List<Message> GetAll()
		{
			lock (m_Cache)
			{
				List<Message> msgs = new List<Message>();
				foreach (KeyValuePair<int, List<Message>> ent in m_Cache)
				{
					foreach (Message msg in ent.Value as List<Message>)
					{
						msgs.Add(msg);
					}
				}
				return msgs;
			}
		}

		/// <summary>
		/// 获取某一用户缓存的消息的最小时间 
		/// </summary>
		public Nullable<DateTime> GetMinCreatedTime(int user)
		{
			lock (m_Cache)
			{
				List<Message> userMsgs = GetUserMessageCache(user);
				return userMsgs.Count == 0 ? null : new Nullable<DateTime>(userMsgs[0].CreatedTime);
			}
		}


		/// <summary>
		/// 在缓存中插入一条消息
		/// </summary>
		/// <param name="user"></param>
		/// <param name="msg"></param>
		public void Insert(int user, Message msg)
		{
			List<Message> userMsgs = null;

			lock (m_Cache)
			{
				userMsgs = GetUserMessageCache(user);
			}

			lock (userMsgs)
			{
				userMsgs.Add(msg);
				m_Count++;
			}
		}

		/// <summary>
		/// 查找缓存中接受者为user，发送时间大于from的消息
		/// </summary>
		public List<Message> Find(int user, int sender, DateTime from)
		{
			List<Message> msgs = new List<Message>();

			{
				List<Message> userMsgs = null;

				lock (m_Cache)
				{
					userMsgs = GetUserMessageCache(user);
				}

				lock (userMsgs)
				{
					int i = 0;
					while (i < userMsgs.Count && userMsgs[i].CreatedTime <= from) i++;

					while (i < userMsgs.Count)
					{
						if (sender == 0 || sender == userMsgs[i].Sender.ID)
						{
							msgs.Add(userMsgs[i]);
						}
						i++;
					}
				}
			}

			if (sender == 0)
			{
				//在缓冲中查找群消息
				AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);
				foreach (int groupId in userInfo.Groups)
				{
					AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);

					List<Message> groupMsgs = null;
					lock (m_Cache)
					{
						groupMsgs = GetUserMessageCache(groupInfo.ID);
					}

					lock (groupMsgs)
					{
						int i = 0;
						while (i < groupMsgs.Count && groupMsgs[i].CreatedTime <= from) i++;

						while (i < groupMsgs.Count)
						{
							if (groupMsgs[i].IsValid) msgs.Add(groupMsgs[i]);
							i++;
						}
					}
				}
			}
			return msgs;
		}

		/// <summary>
		/// 获取消息总量
		/// </summary>
		public Int32 Count
		{
			get { return m_Count; }
		}
	}

	internal class MsgAccessoryEval
	{
		int m_Receiver, m_Sender;
		string m_ReceiverMsgDir, m_SenderMsgDir, m_MsgDir;
		Hashtable m_Data;

		public MsgAccessoryEval(int key, int receiver, int sender, Hashtable data)
		{
			m_Receiver = receiver;
			m_Sender = sender;

			m_Data = data;

			m_ReceiverMsgDir = string.Format("/{0}/Message/MSG{1:00000000}", receiver, key);
			m_SenderMsgDir = string.Format("/{0}/Message/MSG{1:00000000}", sender, key);
			m_MsgDir = AccountImpl.Instance.GetUserInfo(receiver).Type == 1 ? string.Format("/{1}/Message/MSG{0:00000000}", key, receiver) : string.Format("Message/MSG{0:00000000}", key);
		}

		public string Replace(Match match)
		{
			XmlDocument xml = new XmlDocument();
			string value = match.Value;
			xml.LoadXml(string.Format("<{0} />", value.Substring(1, value.Length - 2)));

			string src = GlobalObject.unescape(xml.DocumentElement.GetAttribute("src"));
			string type = xml.DocumentElement.GetAttribute("type").ToLower();
			string data = xml.DocumentElement.GetAttribute("data");

			bool isPublic = ServerImpl.Instance.IsPublic(src);

			if (isPublic)
			{
				//公共资源不拷贝
				return String.Format("{0}", Path.GetRelativePath(src));
			}
			else
			{
				if (!Directory.Exists(m_ReceiverMsgDir)) Directory.CreateDirectory(m_ReceiverMsgDir);
				if (AccountImpl.Instance.GetUserInfo(m_Receiver).Type == 0)
				{
					if (!Directory.Exists(m_SenderMsgDir)) Directory.CreateDirectory(m_SenderMsgDir);
				}

				int fileOwner = Path.GetUser(src);
				if (fileOwner == 0)
				{
					fileOwner = m_Sender;
					src = String.Format("/{0}/{1}", fileOwner, src);
				}

				bool allowRead = true;
				try
				{
					ServerImpl.Instance.CheckPermission(HttpContext.Current, src, IOPermission.Read);
				}
				catch
				{
					allowRead = false;
				}

				if (data != "" || allowRead)
				{
					Hashtable _files = new Hashtable();

					string fileName;
					if (!_files.ContainsKey(src))
					{
						fileName = Path.GetFileName(src);
						int i = 1;
						while (_files.ContainsValue(fileName))
						{
							fileName = string.Format("{0}({1}){2}", System.IO.Path.GetFileNameWithoutExtension(fileName), i.ToString(), System.IO.Path.GetExtension(fileName));
							i++;
						}

						_files.Add(src, fileName);

						try
						{
							if (AccountImpl.Instance.GetUserInfo(m_Receiver).Type == 0)
							{
								if (data == "")
								{
									File.Copy(src, m_SenderMsgDir + "/" + fileName);
								}
								else
								{
									String dataBase64 = m_Data[data] as String;
									Byte[] buffer = System.Convert.FromBase64String(dataBase64);

									using (System.IO.Stream stream = File.Open(m_SenderMsgDir + "/" + fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
									{
										try
										{
											stream.Write(buffer, 0, buffer.Length);
										}
										finally
										{
											stream.Close();
										}

									}
								}
							}
							if (data == "")
							{
								File.Copy(src, m_ReceiverMsgDir + "/" + fileName);
								if (ServerImpl.Instance.IsTemp(src))
								{
									File.Delete(src);
								}
							}
							else
							{
								String dataBase64 = m_Data[data] as String;
								Byte[] buffer = System.Convert.FromBase64String(dataBase64);

								using (System.IO.Stream stream = File.Open(m_ReceiverMsgDir + "/" + fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
								{
									try
									{
										stream.Write(buffer, 0, buffer.Length);
									}
									finally
									{
										stream.Close();
									}

								}
							}
						}
						catch
						{
						}

					}
					else
						fileName = _files[src] as string;

					string newUrl = GlobalObject.escape(string.Format("{0}/{1}", m_MsgDir, fileName)).Replace("+", "%2B");
					return newUrl;
				}
				else
				{
					return src;
				}
			}
		}
	}
}
