using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Client
{
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class Delegate
	{
		List<object> m_All = new List<object>();

		public void CallAll(params object[] args)
		{
			foreach (object cb in m_All)
			{
				try
				{
					List<object> ps = new List<object>();
					ps.Add(cb);
					ps.AddRange(args);

					cb.GetType().InvokeMember("call", BindingFlags.InvokeMethod, null, cb, ps.ToArray());
				}
				catch
				{
				}
			}
		}

		public void Attach(object cb)
		{
			m_All.Add(cb);
		}

		public void Detach(object cb)
		{
			m_All.Remove(cb);
		}

		public void DetachAll()
		{
			m_All.Clear();
		}
	}
}
