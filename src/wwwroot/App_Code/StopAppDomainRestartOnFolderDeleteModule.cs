using System;
using System.Web;
using System.Reflection;

/// <summary> 
/// Stops the ASP.NET AppDomain being restarted (which clears 
/// Session state, Cache etc.) whenever a folder is deleted. 
/// </summary> 
public class StopAppDomainRestartOnFolderDeleteModule : System.Web.IHttpModule
{
	private static bool DisableFCNs = false;
	public void Init(HttpApplication context)
	{
		if (DisableFCNs) return;
		PropertyInfo p = typeof(HttpRuntime).GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		object o = p.GetValue(null, null);
		FieldInfo f = o.GetType().GetField("_dirMonSubdirs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
		object monitor = f.GetValue(o);
		MethodInfo m = monitor.GetType().GetMethod("StopMonitoring", BindingFlags.Instance | BindingFlags.NonPublic);
		m.Invoke(monitor, new object[] { });
		DisableFCNs = true;
	}
	public void Dispose() { }
}
