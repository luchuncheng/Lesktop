using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

#pragma warning disable 618

namespace Core
{
	public class SqlServerMessageStorage : IMessageStorage
	{
		String m_ConnectionString = "";
		int m_MaxKey = 1;
		DateTime m_MaxCreatedTime = DateTime.Now;

		public SqlServerMessageStorage()
		{
			m_ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["IMDB"].ConnectionString;

			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand(
					"select max([ID]) as MaxKey, max(CreatedTime) as MaxCreatedTime from Message",
					conn
				);

				SqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					m_MaxKey = reader[0] == DBNull.Value ? 1 : Convert.ToInt32(reader[0]);
					m_MaxCreatedTime = reader[1] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader[1]);
				}
			}
			finally
			{
				conn.Close();
			}
		}

		private string ConnectionString
		{
			get
			{
				return m_ConnectionString;
			}
		}

		int IMessageStorage.GetMaxKey()
		{
			return m_MaxKey;
		}

		DateTime IMessageStorage.GetCreatedTime()
		{
			return m_MaxCreatedTime;
		}

		List<Message> IMessageStorage.GetMessages(int user, int peer, DateTime from, DateTime to, ref int page, int pagesize, out int pagecount, int msgid, string content)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();
			try
			{

				if (from == null) from = new DateTime(2000, 1, 1);
				SqlCommand cmd = new SqlCommand("GetMessages", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("user", DbType.Int32).Value = user;
				cmd.Parameters.Add("peer", DbType.Int32).Value = peer;
				cmd.Parameters.Add("from", DbType.DateTime).Value = from;
				cmd.Parameters.Add("to", DbType.DateTime).Value = to;
				cmd.Parameters.Add("pagesize", DbType.Int32).Value = pagesize;
				cmd.Parameters.Add("msgid", DbType.Int32).Value = msgid;
				cmd.Parameters.Add("content", SqlDbType.NVarChar, 1024).Value = content;

				SqlParameter param_page = cmd.Parameters.Add("page", DbType.Int32);
				param_page.Direction = ParameterDirection.InputOutput;
				param_page.Value = page;

				SqlParameter param_pagecount = cmd.Parameters.Add("pagecount", DbType.Int32);
				param_pagecount.Direction = ParameterDirection.Output;

				List<Message> messages = new List<Message>();
				SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
				try
				{
					while (reader.Read())
					{
						Message msg = new Message(
							AccountImpl.Instance.GetUserInfo(Convert.ToInt32(reader[2])),
							AccountImpl.Instance.GetUserInfo(Convert.ToInt32(reader[1])),
							reader.GetString(3), Convert.ToDateTime(reader[4]), Convert.ToInt32(reader[0])
						);
						messages.Add(msg);
					}
				}
				finally
				{
					reader.Close();
				}
				page = Convert.ToInt32(param_page.Value);
				pagecount = Convert.ToInt32(param_pagecount.Value);
				return messages;
			}
			finally
			{
				conn.Close();
			}
		}

		List<Message> IMessageStorage.Find(int receiver, int sender, Nullable<DateTime> from)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();
			try
			{

				if (from == null) from = new DateTime(2000, 1, 1);
				SqlCommand cmd = new SqlCommand("FindMessages", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("user", DbType.Int32).Value = receiver;
				cmd.Parameters.Add("peer", DbType.Int32).Value = sender;
				cmd.Parameters.Add("from", DbType.DateTime).Value = from.Value;

				List<Message> messages = new List<Message>();
				SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
				try
				{
					while (reader.Read())
					{
						Message msg = new Message(
							AccountImpl.Instance.GetUserInfo(Convert.ToInt32(reader[2])),
							AccountImpl.Instance.GetUserInfo(Convert.ToInt32(reader[1])),
							reader.GetString(3), Convert.ToDateTime(reader[4]), Convert.ToInt32(reader[0])
						);
						messages.Add(msg);
					}
				}
				finally
				{
					reader.Close();
				}
				return messages;
			}
			finally
			{
				conn.Close();
			}
		}

		void IMessageStorage.Write(List<Message> messages)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();
			try
			{
				//启动事务
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					foreach (Message msg in messages)
					{
						if (msg.IsValid)
						{
							//超过缓存的最大值，将缓存中的消息全部写入数据库
							SqlCommand cmd = new SqlCommand("NewMessage", conn);
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Parameters.Add("Receiver", DbType.Int32).Value = msg.Receiver.ID;
							cmd.Parameters.Add("Sender", DbType.Int32).Value = msg.Sender.ID;
							cmd.Parameters.Add("Content", DbType.String).Value = msg.Content;
							cmd.Parameters.Add("CreatedTime", DbType.DateTime).Value = msg.CreatedTime;
							cmd.Parameters.Add("ID", DbType.Int32).Value = msg.ID;
							cmd.Transaction = trans;
							cmd.ExecuteNonQuery();
						}
					}

					trans.Commit();
				}
				catch
				{
					trans.Rollback();
				}
			}
			finally
			{
				conn.Close();
			}
		}

		DataTable IMessageStorage.GetMessageList_Group(int isTemp, int pageSize, int pageIndex, out int pageCount)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "GetMessageList_Group";

			cmd.Parameters.Add("IsTemp", DbType.Int32).Value = isTemp;
			cmd.Parameters.Add("PageSize", DbType.Int32).Value = pageSize;
			cmd.Parameters.Add("CurrentPage", DbType.Int32).Value = pageIndex;
			cmd.Parameters.Add("PageCount", DbType.Int32).Direction = ParameterDirection.Output;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			pageCount = Convert.ToInt32(cmd.Parameters["PageCount"].Value);

			return dt;
		}

		DataTable IMessageStorage.GetMessageList_User(int userId, int peerId, int pageSize, int pageIndex, out int pageCount)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "GetMessageList_User";

			cmd.Parameters.Add("User", DbType.Int32).Value = userId;
			cmd.Parameters.Add("Peer", DbType.Int32).Value = peerId;
			cmd.Parameters.Add("PageSize", DbType.Int32).Value = pageSize;
			cmd.Parameters.Add("CurrentPage", DbType.Int32).Value = pageIndex;
			cmd.Parameters.Add("PageCount", DbType.Int32).Direction = ParameterDirection.Output;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			pageCount = Convert.ToInt32(cmd.Parameters["PageCount"].Value);

			return dt;
		}

		DataTable IMessageStorage.GetUserMessages(int userId, int peerId, DateTime from, DateTime to, int pageSize, ref int pageIndex, out int pageCount, int msgId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "GetMessages_User";

			cmd.Parameters.Add("User", DbType.Int32).Value = userId;
			cmd.Parameters.Add("Peer", DbType.Int32).Value = peerId;
			cmd.Parameters.Add("From", DbType.DateTime).Value = from;
			cmd.Parameters.Add("To", DbType.DateTime).Value = to;
			cmd.Parameters.Add("PageSize", DbType.Int32).Value = pageSize;
			cmd.Parameters.Add("CurrentPage", DbType.Int32).Value = pageIndex;
			cmd.Parameters["CurrentPage"].Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add("PageCount", DbType.Int32).Direction = ParameterDirection.Output;
			cmd.Parameters.Add("MsgID", DbType.Int32).Value = msgId;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			pageCount = Convert.ToInt32(cmd.Parameters["PageCount"].Value);
			pageIndex = Convert.ToInt32(cmd.Parameters["CurrentPage"].Value);

			return dt;
		}

		DataTable IMessageStorage.GetGroupMessages(int groupId, DateTime from, DateTime to, int pageSize, ref int pageIndex, out int pageCount, int msgId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "GetMessages_Group";

			cmd.Parameters.Add("Group", DbType.Int32).Value = groupId;
			cmd.Parameters.Add("From", DbType.DateTime).Value = from;
			cmd.Parameters.Add("To", DbType.DateTime).Value = to;
			cmd.Parameters.Add("PageSize", DbType.Int32).Value = pageSize;
			cmd.Parameters.Add("CurrentPage", DbType.Int32).Value = pageIndex;
			cmd.Parameters["CurrentPage"].Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add("PageCount", DbType.Int32).Direction = ParameterDirection.Output;
			cmd.Parameters.Add("MsgID", DbType.Int32).Value = msgId;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			pageCount = Convert.ToInt32(cmd.Parameters["PageCount"].Value);
			pageIndex = Convert.ToInt32(cmd.Parameters["CurrentPage"].Value);

			return dt;
		}

		void IMessageStorage.DeleteMessages(string ids)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"DeleteMessages";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("ids", DbType.String).Value = ids;

			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					cmd.Transaction = trans;
					cmd.ExecuteNonQuery();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
			}
			finally
			{
				conn.Close();
			}
		}

		void IMessageStorage.DeleteMessages(int userId, int peerId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"DeleteMessageGroup";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user", DbType.Int32).Value = userId;
			cmd.Parameters.Add("peer", DbType.Int32).Value = peerId;

			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					cmd.Transaction = trans;
					cmd.ExecuteNonQuery();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
			}
			finally
			{
				conn.Close();
			}
		}

		DataTable IMessageStorage.GetMessageRecordUsers(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = "GetMessageRecordUsers";

			cmd.Parameters.Add("id", DbType.Int32).Value = id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt;
		}

		DateTime IMessageStorage.GetRecvMsgMaxTime(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetRecvMsgMaxTime";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("id", DbType.Int32).Value = id;

			conn.Open();
			try
			{
				return Convert.ToDateTime(cmd.ExecuteScalar());
			}
			finally
			{
				conn.Close();
			}
		}

		bool IMessageStorage.CheckPermission(int user_id, int msg_id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"select MsgID from UserRecvMessage where MsgID = @MsgID and UserID = @UserID";

			cmd.Parameters.Add("MsgID", DbType.Int32).Value = msg_id;
			cmd.Parameters.Add("UserID", DbType.Int32).Value = user_id;

			conn.Open();
			try
			{
				object val = cmd.ExecuteScalar();
				return val != null && val != DBNull.Value;
			}
			finally
			{
				conn.Close();
			}
		}

	}
}

#pragma warning restore 618