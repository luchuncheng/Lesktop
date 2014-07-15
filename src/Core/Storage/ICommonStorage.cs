using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Core
{
	public interface ICommonStorage
	{
		DataRow GetCompanyInfo();

		DataRowCollection GetDeptItems(int user_id, int dept_id);
		DataRowCollection GetSubDepts(int dept_id);
		DataRowCollection GetTempGroups(int user_id);
		DataRowCollection GetCommFriends(int user_id);

		void NewComment(int senderId, int receiverId, string content, string tel, string mail, string name);
		DataTable GetAllComment(int receiverId);
		DataTable GetUnreadComment(int receiverId, int mark);
		DataTable GetImportantComment(int receiverId);
		int HasUnreadComment(int receiverId);
		int MarkAsRead(int id);
		int MarkAsUnread(int id);
		int MarkAsImportant(int id);
		int MarkAsUnimportant(int id);
		int DeleteComment(int id);
	}
}
