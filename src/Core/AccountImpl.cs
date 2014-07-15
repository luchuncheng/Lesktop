using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Text;
using System.Reflection;

namespace Core
{
	public class AccountImpl
	{
		public const int AdminstratorID = 2;
		public const int AdminID = 3;

		static AccountImpl kInstance = new AccountImpl();

		public static AccountImpl Instance
		{
			get 
			{
				kInstance.Initialize(HttpContext.Current);
				return kInstance; 
			}
		}

		// 缓存中的用户信息的数量
#		if DEBUG
		const int MAX_CACHE_COUNT = 4000;
#		else
		const int MAX_CACHE_COUNT = 4000;
#		endif

		LinkedList<AccountInfo> list_ = new LinkedList<AccountInfo>();
		Dictionary<int, AccountInfo> cache_ = new Dictionary<int, AccountInfo>();
		object lock_ = new object();
		IAccountStorage storage_impl_ = null;

		IAccountStorage AccountStorageImpl
		{
			get
			{
				lock (lock_)
				{
					if (storage_impl_ == null) Initialize(HttpContext.Current);
					return storage_impl_;
				}
			}
		}

		public void Initialize(HttpContext context)
		{
			lock (lock_)
			{
				if (storage_impl_ == null)
				{
					string accStorage = System.Web.Configuration.WebConfigurationManager.AppSettings["AccountStorageImpl"];
					String[] accStorageInfo = accStorage.Split(new char[] { ' ' });
					Type type = Assembly.Load(accStorageInfo[0]).GetType(accStorageInfo[1]);
					ConstructorInfo ctor = type.GetConstructor(new Type[] { });
					storage_impl_ = ctor.Invoke(new object[] { }) as IAccountStorage;
				}
			}
		}

		public void DeleteUserInfo(int user_id)
		{
			lock (lock_)
			{
				if (cache_.ContainsKey(user_id)) cache_.Remove(user_id);
			}
		}

		public DataRowCollection GetAllUsers(int deptKey)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetAllUsers(deptKey);
			}
		}

		public DataRowCollection GetAllGroups(int deptKey)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetAllGroups(deptKey);
			}
		}

		public DataRowCollection GetAllRegisterUsers()
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetAllRegisterUsers();
			}
		}

		public DataRowCollection GetAllRegisterGroups()
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetAllRegisterGroups();
			}
		}

		public DataRowCollection GetAllDepts(int deptKey)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetAllDepts(deptKey);
			}
		}

		public DataRowCollection GetVisibleUsers(int id)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetVisibleUsers(id);
			}
		}

		public AccountInfo[] GetVisibleUsersDetails(string name)
		{
			lock (lock_)
			{
				List<AccountInfo> users = new List<AccountInfo>();
				DataRowCollection rows = AccountStorageImpl.GetVisibleUsersDetails(name);
				foreach (DataRow data in rows)
				{
					AccountInfo user_info = RefreshUserInfo(data);
					users.Add(user_info);
				}
				return users.ToArray();
			}
		}

		public void CacheAllCompanyUsers()
		{
			lock (lock_)
			{
				DataRowCollection rows = AccountStorageImpl.GetAllCompanyUsers();
				foreach (DataRow row in rows)
				{
					RefreshUserInfo(row);
				}
			}
		}

		public AccountInfo RefreshUserInfo(int id)
		{
			lock (lock_)
			{
				DataRow userInfo = AccountStorageImpl.GetUserInfo(id);
				return RefreshUserInfo(userInfo);
			}
		}

		private AccountInfo RefreshUserInfo(DataRow userInfo)
		{
			if (userInfo != null)
			{
				int userId = Convert.ToInt32(userInfo["ID"]);

				if (cache_.ContainsKey(userId))
				{
					AccountInfo info = cache_[userId] as AccountInfo;
					info.Reset(userInfo);
					return info;
				}
				else
				{
					AccountInfo info = new AccountInfo(userInfo);

					if (list_.Count >= MAX_CACHE_COUNT)
					{
						AccountInfo removeInfo = list_.First.Value;
						cache_.Remove(removeInfo.ID);
						list_.RemoveFirst();
					}

					cache_[info.ID] = info;
					list_.AddLast(info.ListNode);

					return info;
				}
			}
			else
			{
				return null;
			}
		}

		public int GetUserID(String name)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetUserID(name);
			}
		}

		public String[] GetGroupManagers(string name)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetGroupManagers(name);
			}
		}

		public bool Validate(string userId, string password)
		{
			lock (lock_)
			{
				return AccountStorageImpl.Validate(userId, password);
			}
		}

		public AccountInfo GetUserInfo(int userId)
		{
			lock (lock_)
			{
				AccountInfo ai = null;
				if (cache_.ContainsKey(userId))
				{
					ai = cache_[userId] as AccountInfo;
					list_.Remove(ai.ListNode);
					list_.AddLast(ai.ListNode);
				}
				else
				{
					ai = RefreshUserInfo(userId);
				}
				return ai;
			}
		}

		/// <summary>
		/// 更新用户信息
		/// </summary>
		/// <param name="name"></param>
		/// <param name="values"></param>
		public void UpdateUserInfo(int id, Hashtable values)
		{
			lock (lock_)
			{
				AccountStorageImpl.UpdateUserInfo(id, values);
				RefreshUserInfo(id);
			}
		}

		/// <summary>
		/// 添加好友
		/// </summary>
		/// <param name="user"></param>
		/// <param name="friend"></param>
		/// <param name="index"></param>
		public void AddFriend(int user, int friend, int index)
		{
			lock (lock_)
			{
				if (AccountStorageImpl.GetRelationship(user, friend) == -1)
				{
					AddFriend(user, friend);
				}
			}
		}

		public void AddFriend(int user, int friend)
		{
			lock (lock_)
			{
				AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);
				AccountInfo friendInfo = AccountImpl.Instance.GetUserInfo(friend);

				if (user != friend && !userInfo.ContainsFriend(friend))
				{
					AccountStorageImpl.AddFriend(user, friend);

					RefreshUserInfo(user);
					RefreshUserInfo(friend);

				}
			}
		}

		/// <summary>
		/// 删除好友
		/// </summary>
		/// <param name="user"></param>
		/// <param name="friend"></param>
		public void DeleteFriend(int user, int friend)
		{
			lock (lock_)
			{
				if (AccountStorageImpl.GetRelationship(user, friend) != -1)
				{
					AccountStorageImpl.DeleteFriend(user, friend);
					RefreshUserInfo(user);
					RefreshUserInfo(friend);

				}
			}
		}

		public DataRowCollection GetFriends(String name)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetFriends(name);
			}
		}

		public int[] GetRelatedUsers(int id)
		{
			lock (lock_)
			{
				List<int> users = new List<int>();
				DataRowCollection rows = AccountImpl.Instance.GetVisibleUsers(id);
				foreach (DataRow f in AccountImpl.Instance.GetVisibleUsers(id))
				{
					users.Add(Convert.ToInt32(f["ID"]));
				}

				return users.ToArray();
			}
		}

		public void RemoveFromDept(int userId, int deptId)
		{
			lock (lock_)
			{
				AccountStorageImpl.RemoveFromDept(userId, deptId);
			}
		}

		public void ResetUserDepts(int userId, string depts)
		{
			lock (lock_)
			{
				AccountStorageImpl.ResetUserDepts(userId, depts);
			}
		}

		public void AddUsersToDept(int[] ids, int deptId)
		{
			lock (lock_)
			{
				AccountStorageImpl.AddUsersToDept(ids, deptId);
			}
		}

		public void AddUsersToGroup(int[] ids, int groupId)
		{
			MessageImpl.Instance.WriteCache();

			lock (lock_)
			{
				AccountStorageImpl.AddUsersToGroup(ids, groupId);
				foreach (int id in ids)
				{
					RefreshUserInfo(id);
				}
				RefreshUserInfo(groupId);
			}
		}

		public void RemoveFromGroup(int[] ids, int groupId)
		{
			MessageImpl.Instance.WriteCache();

			lock (lock_)
			{
				AccountStorageImpl.RemoveFromGroup(ids, groupId);
				foreach (int id in ids)
				{
					RefreshUserInfo(id);
				}
				RefreshUserInfo(groupId);
			}
		}

		public void RemoveFromGroup(int id, int groupId)
		{
			MessageImpl.Instance.WriteCache();

			lock (lock_)
			{
				AccountStorageImpl.RemoveFromGroup(new int[] { id }, groupId);
				RefreshUserInfo(id);
				RefreshUserInfo(groupId);
			}
		}

		public void DeleteUser(int id)
		{
			AccountInfo info = GetUserInfo(id);
			List<int> friends = new List<int>();
			foreach (int s in info.Friends) friends.Add(s);

			lock (lock_)
			{
				AccountStorageImpl.DeleteUser(info.ID);
			}

			foreach (int friend in friends)
			{
				RefreshUserInfo(friend);
			}
		}

		/// <summary>
		/// 删除群
		/// </summary>
		/// <param name="name"></param>
		/// <param name="creator"></param>
		public void DeleteGroup(int groupId, int creator)
		{
			AccountInfo groupInfo = GetUserInfo(groupId);
			List<int> members = new List<int>();
			foreach (int s in groupInfo.Friends) members.Add(s);

			lock (lock_)
			{
				AccountStorageImpl.DeleteGroup(groupId);

				foreach (int member in members)
				{
					RefreshUserInfo(member);
				}
			}
		}

		public void DeleteDept(int id)
		{
			lock (lock_)
			{
				AccountStorageImpl.DeleteDept(id);
			}
		}

		public int CreateUser(String name, String nickname, String password, String email, int deptId, int subType)
		{
			lock (lock_)
			{
				int id = AccountStorageImpl.CreateUser(name, nickname, password, email, deptId, subType);
				RefreshUserInfo(AccountImpl.AdminID);
				return id;
			}
		}

		/// <summary>
		/// 创建群
		/// </summary>
		/// <param name="creator"></param>
		/// <param name="name"></param>
		/// <param name="nickname"></param>
		public int CreateGroup(int creator, String name, String nickname, int deptId, int subType, string remark)
		{
			lock (lock_)
			{
				int id = AccountStorageImpl.CreateGroup(creator, name, nickname, deptId, subType, remark);
				RefreshUserInfo(creator);
				RefreshUserInfo(id);
				return id;
			}
		}

		public int CreateTempGroup(int creator, String member)
		{
			lock (lock_)
			{
				int id = AccountStorageImpl.CreateTempGroup(creator, member);
				RefreshUserInfo(creator);
				RefreshUserInfo(id);
				return id;
			}
		}

		public int CreateTempUser(String ip)
		{
			lock (lock_)
			{
				int id = AccountStorageImpl.CreateTempUser(ip);
				RefreshUserInfo(id);
				return id;
			}
		}


		public void CreateDept(String nickname, int parentId)
		{
			lock (lock_)
			{
				AccountStorageImpl.CreateDept(nickname, parentId);
			}
		}

		public Hashtable GetDeptUser()
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetDeptUser();
			}
		}

		public String GetUserDepts(int id)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetUserDepts(id);
			}
		}

		public void ResetPassword(int id, string password)
		{
			lock (lock_)
			{
				AccountStorageImpl.ResetPassword(id, password);
			}
		}

		public DataRow GetDeptInfo(int id)
		{
			lock (lock_)
			{
				return AccountStorageImpl.GetDeptInfo(id);
			}
		}

		public void UpdateDeptInfo(int id, string name)
		{
			lock (lock_)
			{
				AccountStorageImpl.UpdateDeptInfo(id, name);
			}
		}

		public DataRowCollection SearchUsers(string keyword, string kw_type)
		{
			lock (lock_)
			{
				return AccountStorageImpl.SearchUsers(keyword, kw_type);
			}
		}

		public DataTable GetAllEmbedCode()
		{
			return AccountStorageImpl.GetAllEmbedCode();
		}

		public DataRow GetEmbedCode(int id)
		{
			return AccountStorageImpl.GetEmbedCode(id);
		}

		public int CreateEmbedCode(string users, string config)
		{
			return AccountStorageImpl.CreateEmbedCode(users, config);
		}

		public void UpdateEmbedCode(int id, string users, string config)
		{
			AccountStorageImpl.UpdateEmbedCode(id, users, config);
		}

		public void DeleteEmbedCode(int id)
		{
			AccountStorageImpl.DeleteEmbedCode(id);
		}

		public DataTable GetServiceEmbedData(string ids)
		{
			return AccountStorageImpl.GetServiceEmbedData(ids);
		}

		public void UpdateLastAccessTime(int id, DateTime last_access_time)
		{
			AccountStorageImpl.UpdateLastAccessTime(id, last_access_time);
		}

		public DateTime GetLastAccessTime(int id)
		{
			return AccountStorageImpl.GetLastAccessTime(id);
		}
	}

	public class AccountInfo : IRenderJson
	{
		object m_Lock = new object();

		private LinkedListNode<AccountInfo> m_ListNode;

		public LinkedListNode<AccountInfo> ListNode
		{
			get { return m_ListNode; }
		}

		int m_ID;
		int m_Type;
		int m_SubType;
		String m_Name;
		bool m_IsTemp;
		bool m_IsAdmin;
		bool m_IsDeleted;

		String m_NickName;
		String m_EMail;
		String m_Tel;
		String m_Mobile;
		String m_HeadIMG;
		String m_Remark;
		String m_HomePage;

		bool m_AcceptStrangerIM;
		int m_DiskSize;
		int m_IMFileLimit;
		int m_IMImageLimit;

		DateTime m_RegisterTime;
		FriendInfo m_Creator;

		object m_FriendsLock = new object();
		Dictionary<int, FriendInfo> m_FriendIndex = null;
		Hashtable m_Managers = null;
		FriendInfo[] m_Friends;

		Details _detailsJson = null;

		public AccountInfo(DataRow data)
		{
			_detailsJson = new Details(this);
			m_ListNode = new LinkedListNode<AccountInfo>(this);
			Reset(data);
		}

		public void Reset(DataRow data)
		{
			lock (m_Lock)
			{
				m_Name = Convert.ToString(data["Name"]);
				m_NickName = Convert.ToString(data["NickName"]);
				m_ID = Convert.ToInt32(data["ID"]);
				m_Type = Convert.ToInt32(data["Type"]);
				m_EMail = Convert.ToString(data["EMail"]);
				m_HeadIMG = Convert.ToString(data["HeadIMG"]);
				m_Remark = Convert.ToString(data["Remark"]);
				m_Tel = Convert.ToString(data["Tel"]);
				m_Mobile = Convert.ToString(data["Mobile"]);
				m_IsDeleted = (Convert.ToInt32(data["IsDeleted"]) == 1);
				m_SubType = Convert.ToInt32(data["SubType"]);
				m_AcceptStrangerIM = Convert.ToInt32(data["AcceptStrangerIM"]) != 0;
				m_IMFileLimit = Convert.ToInt32(data["MsgFileLimit"]);
				m_IMImageLimit = Convert.ToInt32(data["MsgImageLimit"]);
				m_IsTemp = (Convert.ToInt32(data["IsTemp"]) != 0);
				m_IsAdmin = (Convert.ToInt32(data["IsAdmin"]) != 0);
				m_HomePage = Convert.ToString(data["HomePage"]);
				m_DiskSize = Convert.ToInt32(data["DiskSize"]);
				m_RegisterTime = Convert.ToDateTime(data["RegisterTime"]);
				m_Creator = (m_Type == 0 ? null : new FriendInfo(Convert.ToString(data["Creator"]), Convert.ToInt32(data["CreatorID"]), 3, 0));
			}

			lock (m_FriendsLock)
			{
				m_FriendIndex = null;
				m_Managers = null;
				m_Friends = null;
			}
		}

		private void GetFriends()
		{
			lock (m_FriendsLock)
			{
				if (m_Friends == null)
				{
					List<FriendInfo> friends = new List<FriendInfo>(), managers = new List<FriendInfo>();
					FriendInfo creator = null;
					foreach (DataRow row in AccountImpl.Instance.GetFriends(m_Name))
					{
						string name = row["Name"] as string;
						DateTime renewTime = (DateTime)row["RenewTime"];
						FriendInfo fi = new FriendInfo(name, Convert.ToInt32(row["ID"]), Convert.ToInt32(row["Relationship"]), Convert.ToInt32(row["Type"]));
						friends.Add(fi);
						switch (Convert.ToInt32(row["Relationship"]))
						{
						case 2:
							managers.Add(fi);
							break;
						case 3:
							managers.Add(fi);
							creator = fi;
							break;
						}
					}

					m_Friends = friends.ToArray();

					m_FriendIndex = new Dictionary<int, FriendInfo>();
					foreach (FriendInfo friend in friends)
					{
						m_FriendIndex.Add(friend.ID, friend);
					}

					m_Managers = new Hashtable();
					if (m_Type != 1)
					{
						foreach (FriendInfo friend in managers)
						{
							m_Managers.Add(friend.Name.ToUpper(), friend);
						}
					}
				}
			}
		}

		public int[] Friends
		{
			get
			{
				lock (m_FriendsLock)
				{
					GetFriends();
					int[] array = new int[m_Friends.Length];
					for (int i = 0; i < m_Friends.Length; i++) array[i] = m_Friends[i].ID;
					return array;
				}
			}
		}

		public Boolean IsTemp
		{
			get
			{
				return m_IsTemp;
			}
		}

		public Boolean IsDeleted
		{
			get
			{
				return m_IsDeleted;
			}
		}

		public int SubType
		{
			get
			{
				return m_SubType;
			}
		}

		public int Creator
		{
			get
			{
				return (m_Creator == null ? 0 : m_Creator.ID);
			}
		}

		public String HeadIMG
		{
			get
			{
				return m_HeadIMG;
			}
		}

		public String Tel
		{
			get
			{
				return m_Tel;
			}
		}

		public String Mobile
		{
			get
			{
				return m_Mobile;
			}
		}

		public String Remark
		{
			get
			{
				return m_Remark;
			}
		}

		public int[] Groups
		{
			get
			{
				lock (m_FriendsLock)
				{
					GetFriends();
					List<int> groups = new List<int>();
					foreach (FriendInfo friend in m_Friends)
					{
						if (friend.PeerType == 1) groups.Add(friend.ID);
					}
					return groups.ToArray();
				}
			}
		}

		public int Type
		{
			get
			{
				return m_Type;
			}
		}

		public bool AcceptStrangerIM
		{
			get
			{
				return m_AcceptStrangerIM;
			}
		}

		public int MsgFileLimit
		{
			get
			{
				return m_IMFileLimit;
			}
		}

		public int MsgImageLimit
		{
			get
			{
				return m_IMImageLimit;
			}
		}

		public String Nickname
		{
			get
			{
				return m_NickName;
			}
		}

		public String EMail
		{
			get
			{
				return m_EMail;
			}
		}

		public String HomePage
		{
			get
			{
				return m_HomePage;
			}
		}

		public String Name
		{
		    get
		    {
		        return m_Name;
		    }
		}

		public int ID
		{
			get
			{
				return m_ID;
			}
		}

		public DateTime RegisterTime
		{
			get
			{
				return m_RegisterTime;
			}
		}

		public bool IsAdmin
		{
			get { return m_IsAdmin; }
		}

		public bool ContainsFriend(int userid)
		{
			lock (m_FriendsLock)
			{
				GetFriends();
				return m_FriendIndex.ContainsKey(userid);
			}
		}

		public bool ContainsMember(int userid)
		{
			lock (m_FriendsLock)
			{
				GetFriends();
				return m_FriendIndex.ContainsKey(userid);
			}
		}

		public bool IsManagedBy(string name)
		{
			lock (m_FriendsLock)
			{
				GetFriends();
				return m_Type == 1 && m_Managers.ContainsKey(name.ToUpper());
			}
		}

		public bool IsCreatedBy(string name)
		{
			lock (m_FriendsLock)
			{
				GetFriends();
				return m_Type == 1 && String.Compare(name, m_Creator.Name, true) == 0;
			}
		}

		public int[] GetGroupManagers()
		{
			List<int> managers = new List<int>();
			lock (m_Lock)
			{
				if (m_Type == 1)
				{
					foreach (DictionaryEntry ent in m_Managers)
					{
						FriendInfo fi = ent.Value as FriendInfo;
						managers.Add(fi.ID);
					}
				}
			}
			return managers.ToArray();
		}

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(
				builder,
				"ID", m_ID,
				"Name", m_Name,
				"Nickname", m_NickName,
				"Type", m_Type,
				"IsTemp", m_IsTemp,
				"SubType", m_SubType
			);
		}

		public class Details : IRenderJson
		{
			AccountInfo _info;
			public Details(AccountInfo info)
			{
				_info = info;
			}

			void IRenderJson.RenderJson(StringBuilder builder)
			{
				string status = SessionManagement.Instance.GetStatus(_info.ID);
				if (_info.Type == 0)
				{
					Utility.RenderHashJson(
						builder,
						"ID", _info.m_ID,
						"Name", _info.m_Name,
						"Nickname", _info.m_NickName,
						"Type", _info.m_Type,
						"EMail", _info.m_EMail,
						"HeadIMG", _info.m_HeadIMG,
						"Tel", _info.m_Tel,
						"Mobile", _info.m_Mobile,
						"HomePage", _info.m_HomePage,
						"Remark", _info.m_Remark,
						"IsAdmin", _info.m_IsAdmin,
						"State", status,
						"IsTemp", _info.m_IsTemp,
						"SubType", _info.m_SubType
					);
				}
				else
				{
					Utility.RenderHashJson(
						builder,
						"ID", _info.m_ID,
						"Name", _info.m_Name,
						"Nickname", _info.m_NickName,
						"Type", _info.m_Type,
						"EMail", _info.m_EMail,
						"HeadIMG", _info.m_HeadIMG,
						"Tel", _info.m_Tel,
						"Mobile", _info.m_Mobile,
						"HomePage", _info.m_HomePage,
						"Remark", _info.m_Remark,
						"IsAdmin", false,
						"IsTemp", _info.m_IsTemp,
						"SubType", _info.m_SubType,
						"Creator", _info.m_Creator.Name
					);
				}
			}
		}

		public Details DetailsJson
		{
			get { return _detailsJson; }
		}
	}

	public class FriendInfo
	{
		public String Name;
		public int ID;
		public int Relationthip;
		public int PeerType;

		public FriendInfo(string name, int id, int relationthip, int peerType)
		{
			ID = id;
			Name = name;
			Relationthip = relationthip;
			PeerType = peerType;
		}
	}
}
