using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Core;

class Common_CH : Core.CommandHandler
{
	public Common_CH(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{
		
	}

	public override string Process()
	{
		Hashtable ps = Core.Utility.ParseJson(Data as string) as Hashtable;
		string action = ps["Action"] as string;
		if (action == "GetMessages")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			int page = Convert.ToInt32(ps["Page"]), pagecount = 0;
			if (ps.ContainsKey("Time"))
			{
				if (Convert.ToInt32(ps["Time"]) != 5)
				{
					DateTime now = DateTime.Now;
					DateTime to = new DateTime(now.Year, now.Month, now.Day);
					to = to.AddDays(1);
					ps["To"] = to;
					switch (Convert.ToInt32(ps["Time"]))
					{
					case 1:
						{
							ps["From"] = to.AddDays(-7);
							break;
						}
					case 2:
						{
							ps["From"] = to.AddMonths(-1);
							break;
						}
					case 3:
						{
							ps["From"] = to.AddMonths(-3);
							break;
						}
					case 4:
						{
							ps["From"] = to.AddYears(-100);
							break;
						}
					}
				}
			}
			List<Message> msgs = MessageImpl.Instance.GetMessages(
				cu.ID, Convert.ToInt32(ps["Peer"]),
				Convert.ToDateTime(ps["From"]),
				Convert.ToDateTime(ps["To"]),
				ref page, Convert.ToInt32(ps["PageSize"]), out pagecount,
				Convert.ToInt32(ps["MsgID"]),
				ps.ContainsKey("Content") ? Convert.ToString(ps["Content"]) : ""
			);
			return Utility.RenderHashJson(
				"Result", true,
				"Messages", msgs,
				"Page", page,
				"PageCount", pagecount
			);
		}
		else if (action == "GetGroupMembers")
		{
			int groupId = Convert.ToInt32(ps["ID"]);
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(groupId);
			AccountInfo creatorInfo = AccountImpl.Instance.GetUserInfo(groupInfo.Creator);
			List<AccountInfo.Details> members = new List<AccountInfo.Details>();
			Hashtable managers = new Hashtable();
			if (groupInfo.SubType == 3)
			{
				foreach (AccountInfo info in MessageImpl.Instance.GetChatRoomUsers(groupInfo.ID))
				{
					members.Add(info.DetailsJson);
				}
			}
			else
			{
				foreach (int member in groupInfo.Friends)
				{
					members.Add(AccountImpl.Instance.GetUserInfo(member).DetailsJson);
				}
				foreach (int member in groupInfo.GetGroupManagers())
				{
					AccountInfo u = AccountImpl.Instance.GetUserInfo(member);
					if (u.ID != creatorInfo.ID)
					{
						managers[u.ID] = u.DetailsJson;
					}
				}
			}
			return Utility.RenderHashJson(
				"Result", true,
				"Members", members,
				"Managers", managers,
				"GroupInfo", groupInfo,
				"GroupCreator", creatorInfo.DetailsJson
			);
		}
		else if (action == "Login")
		{
			String sessionId = Guid.NewGuid().ToString().ToUpper();
			AccountInfo current_user = null;
			if (!ps.ContainsKey("User"))
			{
				current_user = ServerImpl.Instance.GetCurrentUser(Context);
				if (current_user != null && !current_user.IsTemp)
				{
					Core.ServerImpl.Instance.Login(sessionId, Context, current_user.ID, Convert.ToBoolean(ps["ClientMode"]), null);
				}
				else
				{
					current_user = null;
				}
			}
			else
			{
				String name = ps["User"].ToString();
#if DEBUG
				if (true)
#else
				if (Core.AccountImpl.Instance.Validate(name, ps["Password"].ToString()))		
#endif
				{
					int id = AccountImpl.Instance.GetUserID(name);
					AccountInfo user_info = AccountImpl.Instance.GetUserInfo(id);
					if (user_info == null) throw new Exception("用户不存在或密码错误！");
					Core.ServerImpl.Instance.Login(sessionId, Context, user_info.ID, Convert.ToBoolean(ps["ClientMode"]), null);
					current_user = AccountImpl.Instance.GetUserInfo(id);
				}
				else
				{
					throw new Exception("用户不存在或密码错误！");
				}
			}

			int group_id = 0;

			if (current_user != null && ps.ContainsKey("Join"))
			{
				string groupName = ps["Join"] as string;
				group_id = AccountImpl.Instance.GetUserID(groupName);
				AccountInfo peerInfo = AccountImpl.Instance.GetUserInfo(group_id);
				if (peerInfo != null)
				{
					if (peerInfo.Type == 1 && !peerInfo.ContainsMember(current_user.ID))
					{
						AccountImpl.Instance.AddFriend(current_user.ID, peerInfo.ID);
						SessionManagement.Instance.SendToGroupMembers(peerInfo.ID, "GLOBAL:ACCOUNTINFO_CHANGED", Utility.RenderHashJson("Details", peerInfo.DetailsJson));
					}
					else if (peerInfo.Type == 0 && !peerInfo.ContainsFriend(current_user.ID))
					{
						AccountImpl.Instance.AddFriend(current_user.ID, peerInfo.ID);
					}
				}
			}

			if (current_user == null)
			{
				return Utility.RenderHashJson(
					"Result", true,
					"IsLogin", false
				);
			}
			else
			{
				if (current_user.IsTemp && ps.ContainsKey("EmbedCode") && Convert.ToInt32(ps["EmbedCode"]) != 0)
				{
					System.Data.DataRow row = AccountImpl.Instance.GetEmbedCode(Convert.ToInt32(ps["EmbedCode"]));
					string embedConfig = row["EmbedConfig"].ToString();
					Hashtable data = Utility.ParseJson(embedConfig) as Hashtable;
					System.Data.DataTable dt = AccountImpl.Instance.GetServiceEmbedData(data["Users"].ToString());
					Hashtable users = new Hashtable();
					foreach (DataRow user_row in dt.Rows)
					{
						AccountInfo info = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(user_row["ID"]));
						users[info.ID] = info.DetailsJson;
					}
					return Utility.RenderHashJson(
						"Result", true,
						"IsLogin", true,
						"UserInfo", current_user.DetailsJson,
						"SessionID", sessionId,
						"JoinGroup", group_id,
						"CompanyInfo", ServerImpl.Instance.CommonStorageImpl.GetCompanyInfo(),
						"CSDetails", dt.Rows,
						"CSUsers", users
					);
				}
				else
				{
					return Utility.RenderHashJson(
						"Result", true,
						"IsLogin", true,
						"UserInfo", current_user.DetailsJson,
						"SessionID", sessionId,
						"JoinGroup", group_id
					);
				}
			}
		}
		else if (action == "CreateTempGroup")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			int id = AccountImpl.Instance.CreateTempGroup(AccountImpl.AdminstratorID, ps["Members"].ToString() + "," + cu.ID.ToString());
			AccountInfo info = AccountImpl.Instance.GetUserInfo(id);
			List<int> notify_users = new List<int>();
			foreach (int friendid in info.Friends)
			{
				if (cu.ID != friendid && cu.ID != 2) notify_users.Add(friendid);
			}
			SessionManagement.Instance.SendToMultiUsers(notify_users.ToArray(), "GLOBAL:CREATE_TEMP_GROUP", Utility.RenderHashJson("GroupInfo", info.DetailsJson));
			return Utility.RenderHashJson("GroupInfo", info.DetailsJson);
		}
		else if (action == "ExitTempGroup")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(ps["GroupID"]));
			int[] members = groupInfo.Friends;

			AccountImpl.Instance.RemoveFromGroup(
				ServerImpl.Instance.GetCurrentUser(Context).ID,
				groupInfo.ID
			);
			if (groupInfo.Friends.Length == 1)
			{
				AccountImpl.Instance.DeleteGroup(groupInfo.ID, groupInfo.Creator);
			}
			foreach (int memberid in members)
			{
				if (memberid != AccountImpl.AdminstratorID && SessionManagement.Instance.IsOnline(memberid))
				{
					SessionManagement.Instance.Send(
						memberid, "GLOBAL:EXIT_TEMP_GROUP",
						Utility.RenderHashJson("User", cu, "GroupID", groupInfo.ID, "Group", groupInfo.DetailsJson)
					);
				}
			}
			return Utility.RenderHashJson("Result", true, "Group", groupInfo);
		}
		else if (action == "AddToTempGroup")
		{
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(ps["GroupID"]));
			List<AccountInfo> users = new List<AccountInfo>();
			foreach (string id in ps["IDS"].ToString().Split(new char[] { ',' }))
			{
				AccountInfo user = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(id));
				if (!groupInfo.ContainsMember(user.ID)) users.Add(user);
			}
			if (users.Count > 0)
			{
				AccountImpl.Instance.AddUsersToGroup(Utility.ParseIntArray(ps["IDS"].ToString()), groupInfo.ID);
				var notify_data = Utility.RenderHashJson("GroupID", groupInfo.ID, "GroupInfo", groupInfo, "Users", users);
				foreach (int friendid in groupInfo.Friends)
				{
					if (friendid != 2 && SessionManagement.Instance.IsOnline(friendid))
					{
						SessionManagement.Instance.Send(friendid, "GLOBAL:ADD_TEMP_GROUP", notify_data);
					}
				}
			}

			List<int> notify_users = new List<int>();
			foreach (AccountInfo u in users)
			{
				notify_users.Add(u.ID);
			}
			SessionManagement.Instance.NotifyResetListCache(notify_users.ToArray(), false, false, new AccountInfo[] { groupInfo });
			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "GetMultiUsersInfo")
		{
			Hashtable infos = new Hashtable();
			if (Convert.ToBoolean(ps["ByName"]))
			{
				foreach (string name in ps["Data"].ToString().Split(new char[] { ',' }))
				{
					int id = AccountImpl.Instance.GetUserID(name);
					infos[name] = AccountImpl.Instance.GetUserInfo(id).DetailsJson;
				}
			}
			else
			{
				foreach (string id in ps["Data"].ToString().Split(new char[] { ',' }))
				{
					infos[id] = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(id)).DetailsJson;
				}
			}
			return Utility.RenderHashJson("Infos", infos);
		}
		else if (action == "GetMessageRecordUsers")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			DataTable dt = MessageImpl.Instance.GetMessageRecordUsers(cu.ID);
			return Utility.RenderHashJson("Users", dt.Rows);
		}
		else if (action == "NewComment")
		{
			ServerImpl.Instance.CommonStorageImpl.NewComment(
				UserID,
				Convert.ToInt32(ps["ReceiverID"]),
				ps["Content"] as string,
				ps["Tel"] as string,
				ps["Mail"] as string,
				ps["Name"] as string
			);

			AccountImpl.Instance.RefreshUserInfo(UserID);

			return Utility.RenderHashJson(
				"Result", true
			);
		}
		else if (action == "GetUnreadComment")
		{
			AccountInfo info = AccountImpl.Instance.GetUserInfo(UserID);
			DataTable dtComment = ServerImpl.Instance.CommonStorageImpl.GetUnreadComment(info.IsAdmin ? 0 : info.ID, 0);
			return Utility.RenderHashJson("Comments", dtComment.Rows);
		}
		else if (action == "HasUnreadComment")
		{
			AccountInfo info = AccountImpl.Instance.GetUserInfo(UserID);
			int count = ServerImpl.Instance.CommonStorageImpl.HasUnreadComment(info.ID);
			return Utility.RenderHashJson("Count", count);
		}
		else if (action == "ReLogin")
		{
			SessionManagement.Instance.NewSession(UserID, ps["SessionID"].ToString());

			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "MarkStatus")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			SessionManagement.Instance.MarkStatus(cu.ID, ps["Status"].ToString());
			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "SendAddFriendRequest")
		{
			string peerName = ps["Peer"].ToString();
			int peerId = AccountImpl.Instance.GetUserID(peerName);

			if (peerId == UserID)
			{
				throw new Exception("不能添加自己为好友！");
			}

			AccountInfo peerInfo = AccountImpl.Instance.GetUserInfo(peerId);
			AccountInfo currentUser = AccountImpl.Instance.GetUserInfo(UserID);

			if (peerInfo == null || (peerInfo.Type == 1 && peerInfo.SubType == 1))
			{
				throw new Exception(String.Format("用户(或群组) \"{0}\" 不存在！", peerName));
			}

			if (peerInfo.Type == 0)
			{
				AccountInfo cu = currentUser;
				if ((peerInfo.SubType == 1 && cu.SubType == 1) || currentUser.ContainsFriend(peerInfo.ID))
				{
					throw new Exception(String.Format("用户 \"{0}({1})\" 已经是您的好友！", peerInfo.Nickname, peerName));
				}

				MessageImpl.Instance.NewMessage(
					peerInfo.ID, AccountImpl.AdminstratorID,
					Utility.RenderHashJson("Type", "AddFriendRequest", "Peer", currentUser, "Info", ps["Info"] as string),
					null, false
				);
			}
			else
			{
				if (currentUser.ContainsFriend(peerInfo.ID))
				{
					throw new Exception(String.Format("您已加入群 \"{0}({1})\"！", peerInfo.Nickname, peerName));
				}

				if (peerInfo.Creator != AccountImpl.AdminID)
				{
					MessageImpl.Instance.NewMessage(
						peerInfo.Creator, AccountImpl.AdminstratorID,
						Utility.RenderHashJson(
							"Type", "AddGroupRequest",
							"User", currentUser,
							"Group", peerInfo,
							"Info", ps["Info"] as string
						),
						null, false
					);
				}
			}

			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "RemoveFromGroup")
		{
			int user = Convert.ToInt32(ps["User"]);
			int group = Convert.ToInt32(ps["Group"]);

			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(group);
			AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);

			AccountImpl.Instance.RemoveFromGroup(user, group);

			SessionManagement.Instance.SendToGroupMembers(group, "GLOBAL:REMOVE_FROM_GROUP", Utility.RenderHashJson("GroupID", groupInfo.ID, "User", userInfo));
			SessionManagement.Instance.Send(user, "GLOBAL:REMOVE_COMM_FRIEND", Utility.RenderHashJson("CommFriend", groupInfo.DetailsJson));

			string content = Utility.RenderHashJson(
				"Type", "ExitGroupNotify",
				"User", userInfo,
				"Group", groupInfo
			);
			MessageImpl.Instance.NewMessage(groupInfo.ID, AccountImpl.AdminstratorID, content, null, false);

			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "AddFriend")
		{
			String peerName = ps["Peer"].ToString();
			int peerId = AccountImpl.Instance.GetUserID(peerName);

			AccountInfo peerInfo = AccountImpl.Instance.GetUserInfo(peerId);
			AccountInfo currentUser = AccountImpl.Instance.GetUserInfo(UserID);

			if (!currentUser.ContainsFriend(peerInfo.ID))
			{
				AccountImpl.Instance.AddFriend(UserID, peerInfo.ID);

				string content = Utility.RenderHashJson(
					"Type", "AddFriendNotify",
					"User", currentUser,
					"Peer", peerInfo,
					"Info", ps["Info"] as string
				);

				MessageImpl.Instance.NewMessage(peerInfo.ID, AccountImpl.AdminstratorID, content, null, false);
				MessageImpl.Instance.NewMessage(UserID, AccountImpl.AdminstratorID, content, null, false);

				SessionManagement.Instance.Send(peerInfo.ID, "GLOBAL:ADD_COMM_FRIEND", Utility.RenderHashJson("CommFriend", currentUser.DetailsJson));
				SessionManagement.Instance.Send(UserID, "GLOBAL:ADD_COMM_FRIEND", Utility.RenderHashJson("CommFriend", peerInfo.DetailsJson));
			}
			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "AddToGroup")
		{
			int user = Convert.ToInt32(ps["User"]);
			int group = Convert.ToInt32(ps["Group"]);
			AccountInfo userInfo = AccountImpl.Instance.GetUserInfo(user);
			AccountInfo groupInfo = AccountImpl.Instance.GetUserInfo(group);

			AccountImpl.Instance.AddFriend(user, group);

			SessionManagement.Instance.SendToGroupMembers(group, "GLOBAL:ADD_TO_GROUP", Utility.RenderHashJson("GroupID", group, "User", userInfo.DetailsJson));
			SessionManagement.Instance.Send(user, "GLOBAL:ADD_COMM_FRIEND", Utility.RenderHashJson("CommFriend", groupInfo.DetailsJson));

			string content = Utility.RenderHashJson(
				"Type", "AddToGroupNotify",
				"User", AccountImpl.Instance.GetUserInfo(user),
				"Group", AccountImpl.Instance.GetUserInfo(group)
			);
			MessageImpl.Instance.NewMessage(groupInfo.ID, AccountImpl.AdminstratorID, content, null, false);

			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "Register")
		{
			int id = AccountImpl.Instance.CreateUser(
				ps["Name"].ToString(),
				ps["Nickname"].ToString(),
				ps["Password"].ToString(),
				ps["EMail"].ToString(),
				-1, 0
			);

			AccountInfo newUser = AccountImpl.Instance.GetUserInfo(id);
			Custom.CustomServerImpl.AfterCreateUser(ServerImpl.Instance, AccountImpl.Instance, SessionManagement.Instance, newUser);

			return Utility.RenderHashJson("Info", newUser);
		}
		else if (action == "RemoveSession")
		{
			AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
			if (cu != null)
			{
				string sessionId = ps["SessionID"].ToString();
				SessionManagement.Instance.RemoveSession(cu.ID, sessionId);
				string data = Utility.RenderHashJson(
					"User", cu.ID,
					"State", SessionManagement.Instance.IsOnline(cu.ID) ? "Online" : "Offline",
					"Details", cu.DetailsJson
				);
				SessionManagement.Instance.Send("UserStateChanged", data);
			}
			return Utility.RenderHashJson("Result", true);
		}
		else if (action == "CreateTempUser")
		{
			String password = Guid.NewGuid().ToString().Replace("-", "").ToUpper();

#			if DEBUG
			string ip = "117.136.10.171";
#			else
			string ip = Context.Request.ServerVariables["REMOTE_ADDR"];
#			endif

			int id = AccountImpl.Instance.CreateTempUser(ip);
			AccountInfo cu = AccountImpl.Instance.GetUserInfo(id);
			Hashtable items = new Hashtable();
			items["ResetTempUserPassword"] = password;
			AccountImpl.Instance.UpdateUserInfo(cu.ID, items);

			return Utility.RenderHashJson("Info", cu.DetailsJson, "Password", password);
		}

		throw new NotImplementedException(String.Format("Command \"{0}\" isn't implemented", action));
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}