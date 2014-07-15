using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Core
{
	public interface IAccountStorage
	{
		DataRow GetUserInfo(int id);
		int GetUserID(string name);

		DataRowCollection GetAllUsers(int deptId);
		DataRowCollection GetAllCompanyUsers();
		DataRowCollection GetAllGroups(int deptId);
		DataRowCollection GetAllRegisterUsers();
		DataRowCollection GetAllRegisterGroups();
		DataRowCollection GetAllDepts(int deptId);
		DataRowCollection GetVisibleUsers(int id);
		DataRowCollection GetVisibleUsersDetails(string name);
		DataRowCollection SearchUsers(string keyword, string kw_type);
		String[] GetGroupManagers(string name);

		DataRowCollection GetFriends(string name);
		int GetRelationship(int account1, int account2);
		bool Validate(string userId, string password);
		String[] GetUserRoles(string userId);

		void AddFriend(int user, int friend);
		void DeleteFriend(int user, int friend);
		void DeleteUser(int name);
		void DeleteGroup(int id);
		void DeleteDept(int id);

		void RemoveFromDept(int userId, int deptId);
		void ResetUserDepts(int userId, string ids);
		void RemoveFromGroup(int[] ids, int deptId);
		void AddUsersToDept(int[] ids, int deptId);
		void AddUsersToGroup(int[] ids, int deptId);

		int CreateUser(String name, String nickname, String password, String email, int deptId, int subType);
		int CreateTempUser(String ip);
		int CreateGroup(int creator, String name, String nickname, int deptId, int subType, string remark);
		int CreateTempGroup(int creator, string members);
		void CreateDept(String nickname, int parent);

		void UpdateUserInfo(int name, Hashtable values);
		void ResetPassword(int id, string password);
		DataRow GetDeptInfo(int id);
		void UpdateDeptInfo(int id, string name);

		Hashtable GetDeptUser();
		String GetUserDepts(int id);

		DataTable GetAllEmbedCode();
		DataRow GetEmbedCode(int id);
		int CreateEmbedCode(string users, string config);
		void UpdateEmbedCode(int id, string users, string config);
		void DeleteEmbedCode(int id);
		DataTable GetServiceEmbedData(string ids);

		void UpdateLastAccessTime(int id, DateTime last_access_time);
		DateTime GetLastAccessTime(int id);
	}
}
