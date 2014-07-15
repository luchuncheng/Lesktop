using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Client
{
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class WindowFrame : Form
	{
		String guid = Guid.NewGuid().ToString();

		WebBrowser browser_;

		string suspend_load_url_ = null, url_;
		object window_;

		int min_width_ = 200, min_height_ = 200;
		int border_width_ = 6, title_height_ = 18;
		bool resizable_ = true, has_min_button_ = true, has_max_button_ = true;

		int pre_cursor_x_, pre_cursor_y_;
		int pre_left_, pre_top_, pre_width_, pre_height_;
		int pos_;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				unchecked
				{
					cp.Style |= (int)(0x00080000 | 0x00010000 | 0x00020000);
				}
				return cp;
			}
		}

		public WindowFrame()
		{
			StartPosition = FormStartPosition.Manual;
			FormBorderStyle = FormBorderStyle.None;
			MaximumSize = new Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height);

			Resize += new EventHandler(WindowFrame_Resize);
			browser_ = new WebBrowser();
			browser_.Left = 0;
			browser_.Top = 0;
			browser_.Width = Width;
			browser_.Height = Height;
			browser_.ScrollBarsEnabled = false;
			browser_.IsWebBrowserContextMenuEnabled = false;
			browser_.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			browser_.Url = new Uri("about:blank");

			Controls.Add(browser_);
			browser_.ObjectForScripting = new ClientCall(browser_, this);
		}

		void WindowFrame_Resize(object sender, EventArgs e)
		{
			if (browser_.Document != null && browser_.Url != null && browser_.Url.ToString().ToLower() != "about:blank")
			{
				Utility.InvokeMethod(browser_.Document.Window.DomWindow, "OnWindowResize", Width, Height);
			}
		}

		public Int32 TitleHeight
		{
			get { return title_height_; }
		}

		public Int32 BorderWidth
		{
			get { return border_width_; }
		}

		virtual public void Init(
			int left, int top, int width, int height,
			int borderWidth, int titleHeight,
			bool isResizable, bool hasMin, bool hasMax,
			int minWidth, int minHeight,
			string text
		)
		{
			Bounds = new Rectangle(left, top, width, height);

			border_width_ = borderWidth;
			title_height_ = titleHeight;
			resizable_ = isResizable;
			has_min_button_ = hasMin;
			has_max_button_ = hasMax;
			min_width_ = minWidth;
			min_height_ = minHeight;
			Text = text;

			this.MinimumSize = new Size(minWidth, minHeight);

			browser_.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser1_DocumentCompleted);
			browser_.Url = new Uri(String.Format("{0}/{1}/Client/WindowFrame.htm", Global.ServiceUrl, Global.ResPath));
		}

		private void browser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (browser_.Url == e.Url && browser_.Url.ToString().ToLower() != "about:blank")
			{
				window_ = Utility.InvokeMethod(
					browser_.Document.Window.DomWindow, "Create",
					Width, Height, border_width_, title_height_, resizable_, has_min_button_, has_max_button_, Text
				);
				if (suspend_load_url_ != null)
				{
					url_ = suspend_load_url_;
					Utility.InvokeMethod(window_, "Load", suspend_load_url_);
				}
			}
		}

		public void js_onmousedown(int pos)
		{
			pre_cursor_x_ = Cursor.Position.X;
			pre_cursor_y_ = Cursor.Position.Y;
			pre_left_ = Left;
			pre_top_ = Top;
			pre_width_ = Width;
			pre_height_ = Height;
			pos_ = pos;
			SetCursor();
			Capture = true;

		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Capture)
			{
				int difX = Cursor.Position.X - pre_cursor_x_;
				int difY = Cursor.Position.Y - pre_cursor_y_;

				int newLeft = Left, newTop = Top, newWidth = Width, newHeight = Height;

				if (pos_ == 4)
				{
					Location = new Point(pre_left_ + difX, pre_top_ + difY);
				}
				else
				{
					if (resizable_)
					{
						switch (pos_)
						{
						case 0:
							newLeft = pre_left_ + difX;
							newTop = pre_top_ + difY;
							newWidth = pre_width_ - difX;
							newHeight = pre_height_ - difY;
							if (newWidth < min_width_)
							{
								newLeft -= min_width_ - newWidth;
								newWidth = min_width_;
							}
							if (newHeight < min_height_)
							{
								newTop -= min_height_ - newHeight;
								newHeight = min_height_;
							}
							break;
						case 1:
							newTop = pre_top_ + difY;
							newHeight = pre_height_ - difY;
							if (newHeight < min_height_)
							{
								newTop -= min_height_ - newHeight;
								newHeight = min_height_;
							}
							break;
						case 2:
							newTop = pre_top_ + difY;
							newWidth = pre_width_ + difX;
							newHeight = pre_height_ - difY;
							if (newWidth < min_width_)
							{
								newWidth = min_width_;
							}
							if (newHeight < min_height_)
							{
								newTop -= min_height_ - newHeight;
								newHeight = min_height_;
							}
							break;
						case 3:
							newLeft = pre_left_ + difX;
							newWidth = pre_width_ - difX;
							if (newWidth < min_width_)
							{
								newLeft -= min_width_ - newWidth;
								newWidth = min_width_;
							}
							break;
						case 5:
							newWidth = pre_width_ + difX;
							if (newWidth < min_width_)
							{
								newWidth = min_width_;
							}
							break;
						case 6:
							newLeft = pre_left_ + difX;
							newWidth = pre_width_ - difX;
							newHeight = pre_height_ + difY;
							if (newWidth < min_width_)
							{
								newLeft -= min_width_ - newWidth;
								newWidth = min_width_;
							}
							if (newHeight < min_height_)
							{
								newHeight = min_height_;
							}
							break;
						case 7:
							newHeight = pre_height_ + difY;
							if (newHeight < min_height_)
							{
								newHeight = min_height_;
							}
							break;
						case 8:
							newWidth = pre_width_ + difX;
							newHeight = pre_height_ + difY;
							if (newWidth < min_width_)
							{
								newWidth = min_width_;
							}
							if (newHeight < min_height_)
							{
								newHeight = min_height_;
							}
							break;
						}

						Bounds = new Rectangle(newLeft, newTop, newWidth, newHeight);
						Refresh();
					}
				}
			}
		}

		private void SetCursor()
		{
			if (pos_ == 4)
			{
				Cursor = Cursors.SizeAll;
			}
			else
			{
				if (resizable_)
				{
					switch (pos_)
					{
					case 0:
						Cursor = Cursors.SizeNWSE;
						break;
					case 1:
						Cursor = Cursors.SizeNS;
						break;
					case 2:
						Cursor = Cursors.SizeNESW;
						break;
					case 3:
						Cursor = Cursors.SizeWE;
						break;
					case 5:
						Cursor = Cursors.SizeWE;
						break;
					case 6:
						Cursor = Cursors.SizeNESW;
						break;
					case 7:
						Cursor = Cursors.SizeNS;
						break;
					case 8:
						Cursor = Cursors.SizeNWSE;
						break;
					default:
						Cursor = Cursors.Default;
						break;
					}
				}
				else
				{
					Cursor = Cursors.Default;
				}
			}
		}

		public ClientCoord GetClientCoord(int x, int y)
		{
			object mp = Utility.InvokeMethod(window_, "MapCoord", x, y);
			Point p = new Point(Convert.ToInt32(Utility.GetProperty(mp, "X")), Convert.ToInt32((Utility.GetProperty(mp, "Y"))));
			p = browser_.PointToScreen(p);
			return new ClientCoord(p.X, p.Y);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			Capture = false;
		}

		protected virtual void OnClose()
		{
		}

		protected virtual void OnWindowLoad(object window)
		{
		}

		protected void LoadWindow(string url)
		{
			if (window_ != null) { url_ = url; Utility.InvokeMethod(window_, "Load", url); }
			else suspend_load_url_ = url;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern Int32 ShowWindow(IntPtr handle, Int32 nCmdWindow);

		public void js_close()
		{
			OnClose();
		}

		public void js_minimum()
		{
			ShowWindow(Handle, 7);
			WindowState = FormWindowState.Minimized;
		}

		public void js_maximum()
		{
			ShowWindow(Handle, 3);
			WindowState = FormWindowState.Maximized;
			Refresh();
		}

		public void js_restore()
		{
			ShowWindow(Handle, 1);
			WindowState = FormWindowState.Normal;
			Refresh();
		}

		public void js_onload(object window)
		{
			OnWindowLoad(window);
		}

		protected WebBrowser Browser
		{
			get { return browser_; }
		}

		protected String Url
		{
			get { return url_; }
		}

		protected object WindowFrameDom
		{
			get { return window_; }
		}

	}
}
