using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Core
{
	public class Message : IRenderJson
	{
		public String Content;
		public DateTime CreatedTime;
		public int ID;
		private int _senderId, _receiverId;

		public bool IsTemp = false;

		public Message(
			AccountInfo sender, AccountInfo receiver,
			String content, DateTime cteatedTime, int id
		)
		{
			_senderId = (sender == null ? 0 : sender.ID);
			_receiverId = (receiver == null ? 0 : receiver.ID);

			Content = content;
			CreatedTime = cteatedTime;
			ID = id;
		}

		public AccountInfo Sender
		{
			get { return AccountImpl.Instance.GetUserInfo(_senderId); }
		}

		public AccountInfo Receiver
		{
			get { return AccountImpl.Instance.GetUserInfo(_receiverId); }
		}

		public bool IsValid
		{
			get { return Sender != null && !Sender.IsDeleted && Receiver != null && !Receiver.IsDeleted; }
		}

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(
				builder,
				"Sender", Sender,
				"Receiver", Receiver,
				"CreatedTime", CreatedTime,
				"ID", ID,
				"Content", Content,
				"IsTemp", IsTemp
			);
		}
	}

	public interface IMessageStorage
	{
		int GetMaxKey();
		DateTime GetCreatedTime();
		List<Message> Find(int receiver, int sender, Nullable<DateTime> from);
		List<Message> GetMessages(int user, int peer, DateTime from, DateTime to, ref int page, int pagesize, out int pagecount, int msgid, string content);
		void Write(List<Message> messages);

		DataTable GetMessageList_Group(int isTemp, int pageSize, int pageIndex, out int pageCount);
		DataTable GetMessageList_User(int userId, int peerId, int pageSize, int pageIndex, out int pageCount);

		DataTable GetUserMessages(int userId, int peerId, DateTime from, DateTime to, int pageSize, ref int pageIndex, out int pageCount, int msgId);
		DataTable GetGroupMessages(int groupId, DateTime from, DateTime to, int pageSize, ref int pageIndex, out int pageCount, int msgId);

		void DeleteMessages(string ids);
		void DeleteMessages(int userId, int peerId);
		DataTable GetMessageRecordUsers(int id);

		DateTime GetRecvMsgMaxTime(int id);

		bool CheckPermission(int user_id, int msg_id);
	}
}
