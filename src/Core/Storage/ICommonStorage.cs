using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Core
{
	public interface ICommonStorage
	{
		DataRow GetCompanyInfo();
		DataRowCollection GetCategories(int user_id);
		DataRowCollection GetCategoryItems(int user_id);
		DataRowCollection GetCategoryDepts(int user_id);
		DataRowCollection ResetCategories(int user_id, int[] category_ids, int item_id, int type);
		DataRow CreateCategory(int user_id, string category_name, int parent_id);
		DataRow RenameCategory(int category_id, string category_name);
		void AddToCategory(int user_id, int category_id, int item_id);
		void AddItemsToCategory(int user_id, int category_id, int[] item_ids);
		void RemoveFromCategory(int user_id, int category_id, int item_id);
		void DeleteCategory(int category_id);

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
