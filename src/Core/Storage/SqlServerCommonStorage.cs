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
	public class SqlServerCommonStorage : ICommonStorage
	{
		string m_SQLConnectionString;

		public SqlServerCommonStorage()
		{
			m_SQLConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["IMDB"].ConnectionString;
		}

		DataRow ICommonStorage.GetCompanyInfo()
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetCompanyInfo";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows.Count > 0 ? result.Rows[0] : null;
		}

		DataRowCollection ICommonStorage.GetDeptItems(int user_id, int dept_id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetDeptItems";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user_id", DbType.Int32).Value = user_id;
			cmd.Parameters.Add("dept_id", DbType.Int32).Value = dept_id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataRowCollection ICommonStorage.GetSubDepts(int dept_id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetSubDepts";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("dept_id", DbType.Int32).Value = dept_id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataRowCollection ICommonStorage.GetTempGroups(int user_id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetTempGroups";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user_id", DbType.Int32).Value = user_id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataRowCollection ICommonStorage.GetCommFriends(int user_id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetCommFriends";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user_id", DbType.Int32).Value = user_id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		void ICommonStorage.NewComment(int senderId, int receiverId, string content, string tel, string mail, string name)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("NewComment", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("SenderID", senderId).DbType = DbType.Int32;
					cmd.Parameters.AddWithValue("ReceiverID", receiverId).DbType = DbType.Int32;
					cmd.Parameters.AddWithValue("Content", content).DbType = DbType.String;
					cmd.Parameters.AddWithValue("Tel", tel).DbType = DbType.String;
					cmd.Parameters.AddWithValue("Mail", mail).DbType = DbType.String;
					cmd.Parameters.AddWithValue("Name", name).DbType = DbType.String;

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

		DataTable ICommonStorage.GetAllComment(int receiverId)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "GetAllComment";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("ReceiverID", receiverId).DbType = DbType.Int32;

				SqlDataAdapter ada = new SqlDataAdapter();
				ada.SelectCommand = cmd;

				DataTable dt = new DataTable();
				ada.Fill(dt);
				ada.Dispose();

				return dt;
			}
			finally
			{
				conn.Close();
			}
		}

		DataTable ICommonStorage.GetUnreadComment(int receiverId, int mark)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "GetUnreadComment";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("ReceiverID", receiverId).DbType = DbType.Int32;
				cmd.Parameters.AddWithValue("Mark", mark).DbType = DbType.Int32;

				SqlDataAdapter ada = new SqlDataAdapter();
				ada.SelectCommand = cmd;

				DataTable dt = new DataTable();
				ada.Fill(dt);
				ada.Dispose();

				return dt;
			}
			finally
			{
				conn.Close();
			}
		}

		DataTable ICommonStorage.GetImportantComment(int receiverId)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "GetImportantComment";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("ReceiverID", receiverId).DbType = DbType.Int32;

				SqlDataAdapter ada = new SqlDataAdapter();
				ada.SelectCommand = cmd;

				DataTable dt = new DataTable();
				ada.Fill(dt);
				ada.Dispose();

				return dt;
			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.HasUnreadComment(int receiverId)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "HasUnreadComment";
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.AddWithValue("id", receiverId).DbType = DbType.Int32;

				object ret = cmd.ExecuteScalar();

				return ret == null ? 0 : Convert.ToInt32(ret);
			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.MarkAsRead(int id)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("MarkAsRead", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("Id", id).DbType = DbType.Int32;

					ret = cmd.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret == null ? 0 : Convert.ToInt32(ret);

			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.MarkAsUnread(int id)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("MarkAsUnread", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("Id", id).DbType = DbType.Int32;

					ret = cmd.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret == null ? 0 : Convert.ToInt32(ret);
			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.MarkAsImportant(int id)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("MarkAsImportant", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("Id", id).DbType = DbType.Int32;

					ret = cmd.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret == null ? 0 : Convert.ToInt32(ret);
			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.MarkAsUnimportant(int id)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("MarkAsUnimportant", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("Id", id).DbType = DbType.Int32;

					ret = cmd.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret == null ? 0 : Convert.ToInt32(ret);
			}
			finally
			{
				conn.Close();
			}
		}

		int ICommonStorage.DeleteComment(int id)
		{
			SqlConnection conn = new SqlConnection(m_SQLConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("DeleteComment", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.AddWithValue("Id", id).DbType = DbType.Int32;

					ret = cmd.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret == null ? 0 : Convert.ToInt32(ret);
			}
			finally
			{
				conn.Close();
			}
		}
	}
}

#pragma warning restore 618