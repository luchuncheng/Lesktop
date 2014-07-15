using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Web;
using System.Xml;
using System.Threading;

namespace Core
{
	public class ReceiveResponsesHandler : IHttpAsyncHandler
	{
		public ReceiveResponsesHandler()
		{
		}

		HttpContext m_Context = null;

		IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
		{
			m_Context = context;
			string sessionId = context.Request.Params["SessionID"];
			string clientVersion = context.Request.Params["ClientVersion"];
			string serverVersion = context.Request.Params["ServerVersion"];

			ResponsesListener asyncResult = new ResponsesListener(sessionId, cb, extraData);

			try
			{
				if (serverVersion != ServerImpl.Instance.Version) throw new IncompatibleException();
				if (!String.IsNullOrEmpty(clientVersion) && clientVersion != Custom.ApplicationInfo.ReleaseVersion) throw new IncompatibleException();

				int userid = ServerImpl.Instance.GetUserID(context);
				if (userid == 0) throw new UnauthorizedException();

				try
				{
					if (context.Request.Params["State"] != null)
					{
						Hashtable hash_state = Utility.ParseJson(context.Request.Params["State"]) as Hashtable;
						if (hash_state != null && hash_state.ContainsKey("Status"))
						{
							SessionManagement.Instance.MarkStatus(userid, hash_state["Status"].ToString());
						}
					}
				}
				catch
				{
				}

				if (SessionManagement.Instance.Receive(userid, sessionId, asyncResult))
				{
					ThreadPool.QueueUserWorkItem(asyncResult.Complete);
				}
			}
			catch (Exception ex)
			{
				asyncResult.Cache(Utility.RenderHashJson("IsSucceed", false, "Exception", ex));
				ThreadPool.QueueUserWorkItem(asyncResult.Complete);
			}
			return asyncResult;
		}

		void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
		{
			//将消息发送到客户端
			ResponsesListener asyncResult = result as ResponsesListener;
			asyncResult.Send(m_Context);
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}

	class UnauthorizedException : Exception
	{
	}

	class IncompatibleException : Exception
	{
	}
}
