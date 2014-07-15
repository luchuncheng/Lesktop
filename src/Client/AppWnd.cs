using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Client
{
	class AppWnd: Form
	{
		WebBrowser browser_;

		public class PageLoadEventArgs : EventArgs
		{
			public bool Result;
			public String Url;
		}
		public delegate void PageLoadDelegate(object sender, PageLoadEventArgs e);
		public event PageLoadDelegate PageLoad;

		public AppWnd()
		{
			Width = 600;
			Height = 450;

			browser_ = new WebBrowser();
			browser_.Dock = DockStyle.Fill;
			browser_.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnDocumentCompleted);
			Controls.Add(browser_);

			browser_.ObjectForScripting = new ClientCall(browser_, this);
		}

		public void LoadPage(string url)
		{
			browser_.Url = new Uri(url);
		}

		void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			object result = Utility.InvokeMethod(HtmlWindow, "SetClientMode", true);
			PageLoadEventArgs args = new PageLoadEventArgs();
			args.Result = (result != null && result.GetType() == typeof(Boolean) && (bool)result);
			args.Url = browser_.Url.ToString();
			PageLoad(this, args);
		}

		public Object Core
		{
			get
			{
				return Utility.GetProperty(browser_.Document.Window.DomWindow, "Core");
			}
		}

		public Object HtmlWindow
		{
			get
			{
				return browser_.Document.Window.DomWindow;
			}
		}
	}
}
