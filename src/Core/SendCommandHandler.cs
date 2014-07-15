using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Web;
using System.Xml;
using System.Threading;
using System.Web.SessionState;
using System.Reflection;

namespace Core
{
	public class SendCommandHandler : IHttpHandler
	{
		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			Exception error = null;
			String data = null;

			try
			{
				System.IO.Stream inputStream = context.Request.InputStream;
				Byte[] buffer = new Byte[inputStream.Length];
				inputStream.Read(buffer, 0, (int)inputStream.Length);
				string content = context.Request.ContentEncoding.GetString(buffer);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(content);

				String[] handlerInfo = doc.DocumentElement.GetAttribute("Handler").Split(new char[] { ' ' });
				String cmdId = doc.DocumentElement.GetAttribute("ID");
				String sessionId = doc.DocumentElement.GetAttribute("SessionID");
				bool isAsyn = Boolean.Parse(doc.DocumentElement.GetAttribute("IsAsyn"));

				Assembly assembly = Assembly.Load(handlerInfo[0]);
				Type handlerType = assembly.GetType(handlerInfo[1]);
				ConstructorInfo ctor = handlerType.GetConstructor(new Type[] { typeof(HttpContext), typeof(String), typeof(String), typeof(String) });
				CommandHandler handler = ctor.Invoke(new object[] { context, sessionId, cmdId, doc.DocumentElement.InnerXml }) as CommandHandler;

				if (isAsyn)
				{
					ThreadPool.QueueUserWorkItem(handler.Process);
				}
				else
				{
					data = handler.Process();
				}
			}
			catch (Exception ex)
			{
				ServerImpl.Instance.WriteLog(String.Format("{0}:{2}\r\n{1}", ex.GetType().Name, ex.StackTrace, ex.Message));
				error = ex;
			}

			if (error == null)
			{
				context.Response.Write(Utility.RenderHashJson("IsSucceed", true, "Data", new JsonText(data)));
			}
			else
			{
				context.Response.Write(Utility.RenderHashJson("IsSucceed", false, "Exception", error));
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}

	public abstract class CommandHandler
	{
		HttpContext _context = null;

		public HttpContext Context
		{
			get { return _context; }
		}

		String _data;

		public String Data
		{
			get { return _data; }
		}

		String _id;

		public String CommandID
		{
			get { return _id; }
		}

		String _sessionId;

		public String SessionID
		{
			get { return _sessionId; }
		}

		public int UserID
		{
			get { return ServerImpl.Instance.GetUserID(_context); }
		}

		public CommandHandler(HttpContext context, String sessionId, String id, String data)
		{
			_context = context;
			_data = data;
			_id = id;
			_sessionId = sessionId;
		}

		public abstract void Process(object data);
		public abstract String Process();
	}
}
