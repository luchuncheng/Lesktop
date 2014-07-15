using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Client
{
	class FileTransferServer
	{
		static short SERVER_PORT = 12345;
		static object GlobalLock = new object();

		static short GetServerPort()
		{
			lock (GlobalLock)
			{
				return SERVER_PORT++;
			}
		}

		public FileTransferServer()
		{
		}

		public short Receive(string file, object handler, System.Windows.Forms.Control ctrl)
		{
			Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			short port = GetServerPort();
			int count = 0;
			while (count < 20)
			{
				try
				{
					serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
					serverSocket.Listen(1);
				}
				catch
				{
					if (count < 20) { count++; throw; }
					port = GetServerPort();
				}
			}
			return port;
		}
	}
}
