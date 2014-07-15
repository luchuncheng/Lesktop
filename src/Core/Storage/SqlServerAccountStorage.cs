using System;
using System.Collections;
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
	class SqlServerAccountStorage : IAccountStorage
	{
		string m_ConnectionString = "";

		public SqlServerAccountStorage()
		{
			m_ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["IMDB"].ConnectionString;
		}

		DataRow IAccountStorage.GetUserInfo(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetUserInfo";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add("id", DbType.Int32).Value = id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows.Count > 0 ? dt.Rows[0] : null;
		}

		int IAccountStorage.GetUserID(string name)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "GetUserID";

				cmd.Parameters.Add("name", DbType.String).Value = name;
				object id = cmd.ExecuteScalar();

				return id != DBNull.Value ? Convert.ToInt32(id) : 0;
			}
			finally
			{
				conn.Close();
			}
		}

		DataRowCollection IAccountStorage.GetAllUsers(int deptID)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllUsers";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("deptID", DbType.Int32).Value = deptID;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetAllCompanyUsers()
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllCompanyUsers";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetAllGroups(int deptID)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllGroups";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("deptID", DbType.Int32).Value = deptID;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetAllRegisterUsers()
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllRegisterUsers";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetAllRegisterGroups()
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllRegisterGroups";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetAllDepts(int deptID)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllDepts";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("deptID", DbType.Int32).Value = deptID;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows;
		}

		DataRowCollection IAccountStorage.GetVisibleUsers(int id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetVisibleUsers";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("id", DbType.String).Value = id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataRowCollection IAccountStorage.GetVisibleUsersDetails(string name)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetVisibleUsersDetails";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("name", DbType.String).Value = name;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataRowCollection IAccountStorage.GetFriends(string name)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetFriends";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("name", DbType.String).Value = name;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		int IAccountStorage.GetRelationship(int account1, int account2)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetRelationship";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("account1", DbType.Int32).Value = account1;
			cmd.Parameters.Add("account2", DbType.Int32).Value = account2;

			DataTable result = new DataTable();

			SqlDataAdapter ada = new SqlDataAdapter();

			ada.SelectCommand = cmd;
			ada.Fill(result);
			ada.Dispose();

			return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["Relationship"]) : -1;
		}

		bool IAccountStorage.Validate(string name, string password)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "Validate";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("name", DbType.String).Value = name;
			cmd.Parameters.Add("password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(password);

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			return dt.Rows.Count > 0;
		
		}

		String[] IAccountStorage.GetUserRoles(string name)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = "GetUserRoles";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("name", DbType.String).Value = name;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			DataTable dt = new DataTable();
			ada.Fill(dt);
			ada.Dispose();

			List<string> names = new List<string>();
			foreach (DataRow row in dt.Rows) names.Add(row["RoleName"] as string);

			return names.ToArray();
		}

		void IAccountStorage.AddFriend(int user, int friend)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"AddFriend";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user", DbType.Int32).Value = user;
			cmd.Parameters.Add("friend", DbType.Int32).Value = friend;

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

		void IAccountStorage.DeleteFriend(int userId, int friendId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"DeleteFriend";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("user", DbType.Int32).Value = userId;
			cmd.Parameters.Add("friend", DbType.Int32).Value = friendId;

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

		void IAccountStorage.DeleteUser(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"DeleteUser", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("id", DbType.Int32).Value = id;
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


		void IAccountStorage.RemoveFromDept(int userId, int deptId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"RemoveFromDept", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("userId", DbType.Int32).Value = userId;
					cmd.Parameters.Add("deptId", DbType.Int32).Value = deptId;
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


		void IAccountStorage.ResetUserDepts(int userId, string ids)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"ResetUserDepts", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("userId", DbType.Int32).Value = userId;
					cmd.Parameters.Add("depts", DbType.String).Value = ids;
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

		void IAccountStorage.AddUsersToDept(int[] ids, int deptId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"AddUsersToDept", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("ids", SqlDbType.Image).Value = Utility.ToBytes(ids);
					cmd.Parameters.Add("deptId", DbType.Int32).Value = deptId;
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

		void IAccountStorage.AddUsersToGroup(int[] ids, int groupId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"AddUsersToGroup", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("ids", SqlDbType.Image).Value = Utility.ToBytes(ids);
					cmd.Parameters.Add("groupId", DbType.Int32).Value = groupId;
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

		void IAccountStorage.RemoveFromGroup(int[] ids, int groupId)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"RemoveFromGroup", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("ids", SqlDbType.Image).Value = Utility.ToBytes(ids);
					cmd.Parameters.Add("groupId", DbType.Int32).Value = groupId;
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

		void IAccountStorage.DeleteGroup(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand(@"DeleteGroup", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("id", DbType.Int32).Value = id;
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

		int IAccountStorage.CreateUser(String name, String nickname, String password, String email, int deptId, int subType)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				object ret = null;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand insertUser = new SqlCommand("CreateUser", conn, trans);
					insertUser.CommandType = CommandType.StoredProcedure;

					insertUser.Parameters.Add("name", DbType.String).Value = name;
					insertUser.Parameters.Add("password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(password);
					insertUser.Parameters.Add("nickname", DbType.String).Value = nickname;
					insertUser.Parameters.Add("email", DbType.String).Value = email;
					insertUser.Parameters.Add("deptId", DbType.Int32).Value = deptId;
					insertUser.Parameters.Add("subType", DbType.Int32).Value = subType;

					ret = insertUser.ExecuteScalar();
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return ret != null && ret != DBNull.Value ? Convert.ToInt32(ret) : 0;
			}
			finally
			{
				conn.Close();
			}
		}

		int IAccountStorage.CreateGroup(int creator, String name, String nickname, int deptId, int subType, string remark)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				int id = 0;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("CreateGroup", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("creator", DbType.Int32).Value = creator;
					cmd.Parameters.Add("name", DbType.String).Value = name;
					cmd.Parameters.Add("nickname", DbType.String).Value = nickname;
					cmd.Parameters.Add("deptId", DbType.Int32).Value = deptId;
					cmd.Parameters.Add("subType", DbType.Int32).Value = subType;
					cmd.Parameters.Add("remark", DbType.String).Value = remark;

					object val = cmd.ExecuteScalar();
					id = (val == null || val == DBNull.Value ? 0 : Convert.ToInt32(val));
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return id;
			}
			finally
			{
				conn.Close();
			}
		}

		int IAccountStorage.CreateTempUser(String ip)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				int id = 0;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("CreateTempUser", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("ip", DbType.String).Value = ip;

					id = Convert.ToInt32(cmd.ExecuteScalar());
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return id;
			}
			finally
			{
				conn.Close();
			}
		}

		int IAccountStorage.CreateTempGroup(int creator, string members)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				int id = 0;
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("CreateTempGroup", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("creator", DbType.Int32).Value = creator;
					cmd.Parameters.Add("members", DbType.String).Value = members;

					id = Convert.ToInt32(cmd.ExecuteScalar());
				}
				catch
				{
					trans.Rollback();
					throw;
				}
				trans.Commit();
				return id;
			}
			finally
			{
				conn.Close();
			}
		}

		void IAccountStorage.ResetPassword(int id, string password)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("ResetPassword", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("id", DbType.Int32).Value = id;
				cmd.Parameters.Add("password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(password);

				cmd.ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		void IAccountStorage.UpdateUserInfo(int id, Hashtable values)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);

			conn.Open();
			try
			{
				if (values.ContainsKey("ResetTempUserPassword"))
				{

					SqlCommand checkPwdCmd = new SqlCommand();
					checkPwdCmd.Connection = conn;
					checkPwdCmd.CommandText = "update Users set [Password] = @Password where ID = @ID and IsTemp = 1";

					checkPwdCmd.Parameters.Add("ID", DbType.Int32).Value = id;
					checkPwdCmd.Parameters.Add("Password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(values["ResetTempUserPassword"].ToString());

					object val = checkPwdCmd.ExecuteScalar();

					return;
				}

				if (values.ContainsKey("Password"))
				{
					if (!values.ContainsKey("PreviousPassword")) throw new Exception("原密码错误！");

					SqlCommand checkPwdCmd = new SqlCommand();
					checkPwdCmd.Connection = conn;
					checkPwdCmd.CommandText = "select [ID] from Users where ID = @ID and Password = @Password";

					checkPwdCmd.Parameters.Add("ID", DbType.Int32).Value = id;
					checkPwdCmd.Parameters.Add("Password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(values["PreviousPassword"].ToString());

					object val = checkPwdCmd.ExecuteScalar();

					if (val == null) throw new Exception("原密码错误！");
				}

				StringBuilder cmdText = new StringBuilder();
				cmdText.Append("update Users set Name = Name");
				if (values.ContainsKey("Nickname")) cmdText.Append(",Nickname = @Nickname");
				if (values.ContainsKey("Password")) cmdText.Append(",Password = @Password");
				if (values.ContainsKey("EMail")) cmdText.Append(",EMail = @EMail");
				if (values.ContainsKey("AcceptStrangerIM")) cmdText.Append(",AcceptStrangerIM = @AcceptStrangerIM");
				if (values.ContainsKey("MsgFileLimit")) cmdText.Append(",MsgFileLimit = @MsgFileLimit");
				if (values.ContainsKey("MsgImageLimit")) cmdText.Append(",MsgImageLimit = @MsgImageLimit");
				if (values.ContainsKey("HomePage")) cmdText.Append(",HomePage = @HomePage");
				if (values.ContainsKey("HeadIMG")) cmdText.Append(",HeadIMG = @HeadIMG");
				if (values.ContainsKey("Tel")) cmdText.Append(",Tel = @Tel");
				if (values.ContainsKey("Mobile")) cmdText.Append(",Mobile = @Mobile");
				if (values.ContainsKey("Remark")) cmdText.Append(",Remark = @Remark");
				cmdText.Append(" where ID=@ID");
				if (values.ContainsKey("PreviousPassword")) cmdText.Append(" and Password = @PreviousPassword");
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = cmdText.ToString();

				if (values.ContainsKey("Nickname")) cmd.Parameters.Add("Nickname", DbType.String).Value = values["Nickname"];
				if (values.ContainsKey("Password")) cmd.Parameters.Add("Password", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(values["Password"] as string);
				if (values.ContainsKey("EMail")) cmd.Parameters.Add("EMail", DbType.String).Value = values["EMail"];
				if (values.ContainsKey("InviteCode")) cmd.Parameters.Add("InviteCode", DbType.String).Value = values["InviteCode"];

				if (values.ContainsKey("AcceptStrangerIM")) cmd.Parameters.Add("AcceptStrangerIM", DbType.Int32).Value = ((bool)values["AcceptStrangerIM"]) ? 1 : 0;
				if (values.ContainsKey("MsgFileLimit")) cmd.Parameters.Add("MsgFileLimit", DbType.Int32).Value = Convert.ToInt32((Double)values["MsgFileLimit"]);
				if (values.ContainsKey("MsgImageLimit")) cmd.Parameters.Add("MsgImageLimit", DbType.Int32).Value = Convert.ToInt32((Double)values["MsgImageLimit"]);

				if (values.ContainsKey("HomePage")) cmd.Parameters.Add("HomePage", DbType.String).Value = values["HomePage"];
				if (values.ContainsKey("HeadIMG")) cmd.Parameters.Add("HeadIMG", DbType.String).Value = values["HeadIMG"];
				if (values.ContainsKey("Tel")) cmd.Parameters.Add("Tel", DbType.String).Value = values["Tel"];
				if (values.ContainsKey("Mobile")) cmd.Parameters.Add("Mobile", DbType.String).Value = values["Mobile"];
				if (values.ContainsKey("Remark")) cmd.Parameters.Add("Remark", DbType.String).Value = values["Remark"];

				cmd.Parameters.Add("ID", DbType.Int32).Value = id;
				if (values.ContainsKey("PreviousPassword")) cmd.Parameters.Add("PreviousPassword", DbType.String).Value = Custom.CustomServerImpl.EncryptPassword(values["PreviousPassword"] as string);

				cmd.ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		String[] IAccountStorage.GetGroupManagers(string name)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText =@"GetGroupManagers";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("name", DbType.String).Value = name;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			List<String> names = new List<string>();
			foreach (DataRow row in result.Rows) names.Add(row["Name"] as string);

			return names.ToArray();
		}

		void IAccountStorage.CreateDept(String nickname, int parent)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("CreateDept", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("name", DbType.String).Value = nickname;
					cmd.Parameters.Add("parentID", DbType.Int32).Value = parent;
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

		void IAccountStorage.DeleteDept(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("DeleteDept", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add("id", DbType.Int32).Value = id;
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

		Hashtable IAccountStorage.GetDeptUser()
		{
			Hashtable ht = new Hashtable();
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetDeptUser";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			foreach(DataRow row in result.Rows)
			{
				int deptID = Convert.ToInt32(row["DeptID"]);
				if(!ht.ContainsKey(deptID))
				{
					ht[deptID] = new Hashtable();
				}

				(ht[deptID] as Hashtable)[Convert.ToInt32(row["UserID"])] = row["UserID"];
			}

			return ht;
		}

		String IAccountStorage.GetUserDepts(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("select dbo.GetUserDepts(@id)", conn);
				cmd.Parameters.Add("id", DbType.Int32).Value = id;
				object ret = cmd.ExecuteScalar();
				return ret != null && ret != DBNull.Value ? ret.ToString() : "";
			}
			finally
			{
				conn.Close();
			}
		}

		DataRow IAccountStorage.GetDeptInfo(int id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetDeptInfo";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("id", DbType.Int32).Value = id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows.Count > 0 ? result.Rows[0] : null;
		}

		void IAccountStorage.UpdateDeptInfo(int id, string name)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("UpdateDeptInfo", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("id", DbType.Int32).Value = id;
					cmd.Parameters.Add("name", DbType.String).Value = name;

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

		DataRowCollection IAccountStorage.SearchUsers(string keyword, string kw_type)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"SearchUsers";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("keyword", DbType.String).Value = keyword;
			cmd.Parameters.Add("kw_type", DbType.String).Value = kw_type;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows;
		}

		DataTable IAccountStorage.GetAllEmbedCode()
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetAllEmbedCode";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result;
		}

		DataRow IAccountStorage.GetEmbedCode(int id)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetEmbedCode";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("id", DbType.Int32).Value = id;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result.Rows.Count > 0 ? result.Rows[0] : null;
		}

		int IAccountStorage.CreateEmbedCode(string users, string config)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("CreateEmbedCode", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("config", DbType.String).Value = config;
				cmd.Parameters.Add("Users", DbType.String).Value = users;

				return Convert.ToInt32(cmd.ExecuteScalar());
			}
			finally
			{
				conn.Close();
			}
		}

		void IAccountStorage.UpdateEmbedCode(int id, string users, string config)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("UpdateEmbedCode", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("id", DbType.Int32).Value = id;
				cmd.Parameters.Add("config", DbType.String).Value = config;
				cmd.Parameters.Add("Users", DbType.String).Value = users;

				cmd.ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		void IAccountStorage.DeleteEmbedCode(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("DeleteEmbedCode", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("id", DbType.Int32).Value = id;

				cmd.ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		DataTable IAccountStorage.GetServiceEmbedData(string ids)
		{
			DataTable result = new DataTable();

			SqlConnection conn = new SqlConnection(m_ConnectionString);
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = conn;
			cmd.CommandText = @"GetServiceEmbedData";
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("ids", DbType.String).Value = ids;

			SqlDataAdapter ada = new SqlDataAdapter();
			ada.SelectCommand = cmd;

			ada.Fill(result);
			ada.Dispose();

			return result;
		}

		void IAccountStorage.UpdateLastAccessTime(int id, DateTime last_access_time)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlTransaction trans = conn.BeginTransaction();
				try
				{
					SqlCommand cmd = new SqlCommand("UpdateLastAccessTime", conn, trans);
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.Parameters.Add("id", DbType.Int32).Value = id;
					cmd.Parameters.Add("last_access_time", DbType.DateTime).Value = last_access_time;

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

		DateTime IAccountStorage.GetLastAccessTime(int id)
		{
			SqlConnection conn = new SqlConnection(m_ConnectionString);
			conn.Open();
			try
			{
				SqlCommand cmd = new SqlCommand("GetLastAccessTime", conn);
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add("id", DbType.Int32).Value = id;

				return Convert.ToDateTime(cmd.ExecuteScalar());
			}
			finally
			{
				conn.Close();
			}
		}
	}
}
#pragma warning restore 618