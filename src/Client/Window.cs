using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using MsHtmHstInterop;

namespace Client
{
	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public partial class Window : WindowFrame
	{
		object config_;
		object onclose_ = null;
		bool load_flag_ = false;
		object tag_ = null;
		object showdialog_callback_ = null;
		object translate_accelerator_handler_ = null;

		FormClosingEventHandler form_closing_event_handler_;
		IWindowImpl js_window_callback_ = null;

		public Window()
		{
			Icon = Properties.Resources.WindowIcon;
			js_window_callback_ = new IWindowImpl(this);
			form_closing_event_handler_ = new FormClosingEventHandler(this.Window_FormClosing);
			this.FormClosed += new FormClosedEventHandler(Window_FormClosed);
			this.FormClosing += form_closing_event_handler_;
		}

		public void Init(object config)
		{
			config_ = config;

			onclose_ = Utility.GetProperty(config, "OnClose");
			translate_accelerator_handler_ = Utility.GetProperty(config, "TranslateAccelerator");

			tag_ = Utility.GetProperty(config, "Tag");

			object title = Utility.GetProperty(config, "Title");

			base.Init(
				(int)Utility.GetProperty(config, "Left"),
				(int)Utility.GetProperty(config, "Top"),
				(int)Utility.GetProperty(config, "Width"),
				(int)Utility.GetProperty(config, "Height"),
				(int)Utility.GetProperty(config, "BorderWidth"),
				(int)Utility.GetProperty(title, "Height"),
				(bool)Utility.GetProperty(config, "Resizable"),
				(bool)Utility.GetProperty(config, "HasMinButton"),
				(bool)Utility.GetProperty(config, "HasMaxButton"),
				(int)Utility.GetProperty(config, "MinWidth"),
				(int)Utility.GetProperty(config, "MinHeight"),
				(string)Utility.GetProperty(title, "InnerHTML")
			);

			ShowInTaskbar = (bool)Utility.GetProperty(config, "ShowInTaskbar");

			TopMost = Convert.ToBoolean(Utility.GetProperty(config, "TopMost"));

			bool allowDrop = Convert.ToBoolean(Utility.GetProperty(config, "AllowDrop"));
			if (allowDrop)
			{
				_extent = new WebBrowserExtent(this, Browser);
				ICustomDoc icd = Browser.Document.DomDocument as ICustomDoc;
				icd.SetUIHandler(_extent);
			}
		}

		public override void Init(
			int left, int top, int width, int height, 
			int borderWidth, int titleHeight, 
			bool isResizable, bool hasMin, bool hasMax, 
			int minWidth, int minHeight, string text
			)
		{
			base.Init(left, top, width, height, borderWidth, titleHeight, isResizable, hasMin, hasMax, minWidth, minHeight, text);
		}

		public void Init(
			int left, int top, int width, int height, int borderWidth,
			int titleHeight, bool isResizable, bool hasMin, bool hasMax,
			int minWidth, int minHeight, string text,
			bool allowDrop
			)
		{
			Init(left, top, width, height, borderWidth, titleHeight, isResizable, hasMin, hasMax, minWidth, minHeight, text);

			if (allowDrop)
			{
				_extent = new WebBrowserExtent(this, Browser);
				ICustomDoc icd = Browser.Document.DomDocument as ICustomDoc;
				icd.SetUIHandler(_extent);
			}
		}

		public IWindowImpl IWindow
		{
			get { return js_window_callback_; }
		}

		public class PageLoadEventArgs : EventArgs
		{
			public bool Result;
			public String Url;
		}

		public delegate void PageLoadDelegate(object sender, PageLoadEventArgs e);

		public event PageLoadDelegate PageLoad;

		WebBrowserExtent _extent = null;

		protected override void OnWindowLoad(object window)
		{
			load_flag_ = true;
			object result = Utility.InvokeMethod(window, "SetClientMode", true, IWindow);
			if (result != null && result.GetType() == typeof(Boolean) && (bool)result)
			{
				if (IWindow.onload_callback_ != null)
				{
					Utility.CallFunc(IWindow.onload_callback_, IWindow);
				}
				js_window_callback_.OnLoad.Call(js_window_callback_);
				js_window_callback_.OnResize.Call(js_window_callback_);
			}

			PageLoadEventArgs args = new PageLoadEventArgs();
			args.Result = (result != null && result.GetType() == typeof(Boolean) && (bool)result);
			args.Url = Url.ToString();
			PageLoad(this, args);
		}

		[DllImport("kernel32.dll")]
		private static extern bool SetProcessWorkingSetSize(
			IntPtr process,
			int minSize,
			int maxSize
		);

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible == false)
			{
				IWindow.OnHidden.Call(IWindow);
			}
			base.OnVisibleChanged(e);
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsIconic(IntPtr hWnd);

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
		}

		protected override void OnResize(EventArgs e)
		{
			IWindow.OnResize.Call(IWindow);
			base.OnResize(e);
		}

		private void Window_FormClosed(object sender, FormClosedEventArgs e)
		{
			if ((Form)this == (Form)Global.Desktop)
			{
				HotKeyUtil.UnregisterAllHotKey();
			}
			if (showdialog_callback_ != null && !(showdialog_callback_ is DBNull))
			{
				Utility.CallFunc(showdialog_callback_, IWindow);
				showdialog_callback_ = null;
			}

			if (Owner != null && !Owner.Enabled) Owner.Enabled = true;

			js_window_callback_.OnClosed.Call(js_window_callback_);

			Browser.Dispose();
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
			}
		}

		public void DestoryWindow()
		{
			FormClosing -= form_closing_event_handler_;
			Close();
		}

		private void Window_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing && onclose_ != null && onclose_ != DBNull.Value)
			{
				Utility.CallFunc(onclose_, IWindow);
				e.Cancel = true;
			}
		}

		protected override void OnClose()
		{
			if (onclose_ != null && onclose_ != DBNull.Value)
			{
				Utility.CallFunc(onclose_, IWindow);
			}
			else
			{
				DestoryWindow();
			}
		}

		const UInt32 WM_USER = 0x0400;

		public Menu current_context_menu_ = null;

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x0006)
			{
				js_window_callback_.OnActivated.Call((Int32)m.WParam, (Int32)m.LParam);
			}
			else if (m.Msg == 0x011F)
			{
				if (current_context_menu_ != null && (((Int32)m.WParam >> 16) & 0xFFFF) == 0xFFFF && m.LParam == (IntPtr)0)
				{
					current_context_menu_.OnCommand.CallAll("", "");
					current_context_menu_ = null;
				}
				return;
			}
			try
			{
				if (!Global.Desktop.IsDisposed && m.HWnd == Global.Desktop.Handle)
				{
					if (m.Msg == 0x312)
					{
						HotKey hk = HotKeyUtil.FindHotKey(m.WParam.ToInt32());
						if (hk != null)
						{
							int action = HotKeyUtil.FindHotKeyAction(hk);
							if (action == 1)
							{
								if (SessionImpl.Instance.GetSessionID() != "")
								{
									object singletonForm = SessionImpl.Instance.GetGlobal("SingletonForm");
									Utility.InvokeMethod(singletonForm, "ShowMainForm");
								}
							}
							else if (action == 2)
							{
								object core = Global.Desktop.Core;
								object impl = Utility.GetProperty(core, "UnreadMsgBoxImpl");
								int count = (int)Utility.InvokeMethod(impl, "GetUnreadMsgCount", new object[] { });
								if (count > 0)
								{
									Utility.InvokeMethod(impl, "OpenFirst", new object[] { });
								}
								else
								{
									if (SessionImpl.Instance.GetSessionID() != "")
									{
										object singletonForm = SessionImpl.Instance.GetGlobal("SingletonForm");
										Utility.InvokeMethod(singletonForm, "ShowMainForm");
									}
								}
							}
						}
					}
				}
			}
			catch
			{
			}
			base.WndProc(ref m);
		}

		[System.Runtime.InteropServices.ComVisibleAttribute(true)]
		public class IWindowImpl
		{
			Window window_ = null;

			public object onload_callback_ = null;

			public IWindowImpl(Window b)
			{
				window_ = b;
			}

			public Window Form
			{
				get { return window_; }
			}

			public void ShowDialog(object parent, string pos, int left, int top, bool relativeParent, object callback)
			{
				window_.showdialog_callback_ = callback;
				if (parent != null)
				{
					window_.Owner = (parent as IWindowImpl).window_;
					MoveEx(pos, left, top, relativeParent);
					(parent as IWindowImpl).window_.Enabled = false;
					window_.Show();
				}
				else
				{
					MoveEx(pos, left, top, relativeParent);
					window_.Show();
				}
			}

			public void Load(String url, object onload)
			{
				if (url.StartsWith("/")) url = Global.ServiceUrl + "/" + url;
				onload_callback_ = onload;
				window_.LoadWindow(url);
			}

			public void Navigate(String url)
			{
				Waiting("正在载入窗口...");
				if (url.StartsWith("/")) url = Global.ServiceUrl + "/" + url;
				window_.LoadWindow(Global.ServiceUrl + "/" + url);
			}

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			static extern Int32 ShowWindow(IntPtr handle, Int32 nCmdWindow);

			[System.Runtime.InteropServices.DllImport("user32.dll ")]
			static extern bool SetForegroundWindow(IntPtr hWnd);

			public void Show()
			{
				ShowWindow(window_.Handle, 1);
				SetForegroundWindow(window_.Handle);
				window_.Refresh();
			}

			public void ShowWindow(int cmd)
			{
				ShowWindow(window_.Handle, cmd);
			}

			public void Hide()
			{
				window_.Hide();
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					Window.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
				}
			}

			public void Minimum()
			{
				if (window_.Visible)
				{
					window_.WindowState = FormWindowState.Minimized;
				}
				else
				{
					ShowWindow(window_.Handle, 7);
				}
			}

			public void Close()
			{
				window_.DestoryWindow();
			}

			public object GetTag()
			{
				return window_.tag_;
			}

			public void SetTag(object tag)
			{
				window_.tag_ = tag;
			}

			public void Waiting(string text)
			{
				Utility.InvokeMethod(window_.WindowFrameDom, "Waiting", text);
			}

			public void Completed()
			{
				Utility.InvokeMethod(window_.WindowFrameDom, "Completed");
			}

			public void CompleteAll()
			{
				Utility.InvokeMethod(window_.WindowFrameDom, "CompleteAll");
			}

			public void Reset()
			{
				window_.WindowState = FormWindowState.Normal;
				CompleteAll();
				Utility.InvokeMethod(window_.WindowFrameDom, "Reset");
			}

			public void Move(int left, int top)
			{
				window_.Left = left;
				window_.Top = top;
			}

			public void MoveEx(string pos, int x, int y, bool relativeParent)
			{
				pos = pos.ToUpper();
				if (pos == "") pos = "LEFT|TOP";
				else if (pos == "CENTER") pos = "MIDDLE|MIDDLE";

				string[] poss = pos.Split(new char[] { '|' });

				Rectangle rect = Rectangle.Empty;
				if (window_.Owner != null && relativeParent) rect = window_.Owner.Bounds;
				else rect = Screen.PrimaryScreen.WorkingArea;

				int left;
				if (poss[0] == "LEFT") left = rect.Left;
				else if (poss[0] == "RIGHT") left = rect.Right - GetWidth();
				else left = rect.Left + (rect.Width - GetWidth()) / 2;
				left += x;

				int top;
				if (poss[1] == "TOP") top = rect.Top;
				else if (poss[1] == "BOTTOM") top = rect.Bottom - GetHeight();
				else top = rect.Top + (rect.Height - GetHeight()) / 2;
				top += y;

				Move(left, top);
			}

			public void Resize(int width, int height)
			{
				window_.Width = width;
				window_.Height = height;
			}

			public object GetHtmlWindow()
			{
				return Utility.InvokeMethod(window_.WindowFrameDom, "GetHtmlWindow"); 
			}

			DropEvent drop_event_ = new DropEvent();
			NotifyEvent notify_event_ = new NotifyEvent();
			ActivateEvent activated_event_ = new ActivateEvent();
			DropFilesEvent dropfiles_event_ = new DropFilesEvent();
			CommonEvent load_event_ = new CommonEvent();
			CommonEvent hidden_event_ = new CommonEvent();
			CommonEvent closed_event_ = new CommonEvent();
			CommonEvent resized_event_ = new CommonEvent();

			public ActivateEvent OnActivated
			{
				get { return activated_event_; }
			}

			public CommonEvent OnLoad
			{
				get { return load_event_; }
			}

			public CommonEvent OnHidden
			{
				get { return hidden_event_; }
			}

			public CommonEvent OnClosed
			{
				get { return closed_event_; }
			}

			public CommonEvent OnResize
			{
				get { return resized_event_; }
			}

			public NotifyEvent OnNotify
			{
				get { return notify_event_; }
			}

			public DropFilesEvent OnDropFiles
			{
				get { return dropfiles_event_; }
			}

			public DropEvent OnDrop
			{
				get { return drop_event_; }
			}

			public bool IsLoad()
			{
				return window_.load_flag_;
			}

			public void SetTitle(string title)
			{
				window_.Text = title;
				Utility.InvokeMethod(window_.WindowFrameDom, "SetTitle", title); 
			}

			public String GetTitle()
			{
				return window_.Text;
			}

			public bool IsVisible()
			{
				return IsWindowVisible(window_.Handle);
			}

			public bool IsTop()
			{
				return window_.Visible;
			}

			public void BringToTop()
			{
				ShowWindow(window_.Handle, 1);
				SetForegroundWindow(window_.Handle);
				window_.Refresh();
			}

			public int GetLeft()
			{
				return window_.Left;
			}

			public int GetTop()
			{
				return window_.Top;
			}

			public int GetWidth()
			{
				if (window_.WindowState == FormWindowState.Minimized) return window_.RestoreBounds.Width;
				else return window_.Width;
			}

			public int GetHeight()
			{
				if (window_.WindowState == FormWindowState.Minimized) return window_.RestoreBounds.Height;
				else return window_.Height;
			}

			public int GetClientWidth()
			{
				return GetWidth() - window_.BorderWidth * 2;
			}

			public int GetClientHeight()
			{
				return GetHeight() - window_.BorderWidth * 2 - window_.TitleHeight;
			}

			public int GetMinClientWidth()
			{
				return GetClientWidth() - window_.BorderWidth * 2;
			}

			public int GetMinClientHeight()
			{
				return GetClientHeight() - window_.BorderWidth * 2 - window_.TitleHeight;
			}

			public ClientCoord GetClientCoord(int x, int y)
			{
				return window_.GetClientCoord(x, y);
			}

			[DllImport("user32.dll")]
			static extern Int32 FlashWindowEx(ref FLASHWINFO pwfi);

			[StructLayout(LayoutKind.Sequential)]
			public struct FLASHWINFO
			{
				public UInt32 cbSize;
				public IntPtr hwnd;
				public Int32 dwFlags;
				public UInt32 uCount;
				public Int32 dwTimeout;
			}

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			static extern bool IsWindowVisible(IntPtr hWnd);

			public void Notify()
			{
				if (!IsWindowVisible(window_.Handle))
				{
					ShowWindow(window_.Handle, 7);
				}
				
				FLASHWINFO fw = new FLASHWINFO();
				fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO)));
				fw.hwnd = window_.Handle;
				fw.dwFlags = 2;
				fw.uCount = 4;
				fw.dwTimeout = 400;
				FlashWindowEx(ref fw);
			}

			public void DoDragDrop(string type, string data)
			{
				Hashtable drop_data = new Hashtable();
				drop_data["__Type"] = type;
				drop_data["__Data"] = data;
				String drop_json = Utility.RenderJson(drop_data);
				window_.DoDragDrop(drop_json, DragDropEffects.Copy);
			}
		}

		class WebBrowserExtent : MsHtmHstInterop.IDropTarget, IDocHostUIHandler
		{
			MsHtmHstInterop.IDropTarget m_pPreDropTarget = null;

			WebBrowser _browser;
			Window _window;
			public WebBrowserExtent(Window win, WebBrowser browser)
			{
				_window = win;
				_browser = browser;
			}

			[PreserveSig]
			long MsHtmHstInterop.IDropTarget.DragEnter(
				[In]System.Runtime.InteropServices.ComTypes.IDataObject pDataObject,
				[In]int grfKeyState,
				[In] POINTL pt,
				[In, Out] ref uint pdwEffect
			)
			{
				uint effect = 0;
				if (m_pPreDropTarget != null) m_pPreDropTarget.DragEnter(pDataObject, grfKeyState, pt, ref effect);
				pdwEffect = effect | 1;
				return S_OK;
			}

			[PreserveSig]
			long MsHtmHstInterop.IDropTarget.DragOver(
				[In] int grfKeyState,
				[In] POINTL pt,
				[In, Out] ref uint pdwEffect)
			{
				uint effect = 0;
				if (m_pPreDropTarget != null) m_pPreDropTarget.DragOver(grfKeyState, pt, ref effect);
				pdwEffect = effect | 1;
				return S_OK;
			}

			[PreserveSig]
			long MsHtmHstInterop.IDropTarget.DragLeave()
			{
				if (m_pPreDropTarget != null) m_pPreDropTarget.DragLeave();
				return S_OK;
			}

			[PreserveSig]
			long MsHtmHstInterop.IDropTarget.Drop(
				[In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObject,
				[In] int grfKeyState,
				[In] POINTL pt,
				[In, Out] ref uint pdwEffect
			)
			{
				System.Runtime.InteropServices.ComTypes.IEnumFORMATETC pEnum = (System.Runtime.InteropServices.ComTypes.IEnumFORMATETC)pDataObject.EnumFormatEtc(DATADIR.DATADIR_GET);
				pEnum.Reset();
				FORMATETC[] etcs = new FORMATETC[1];
				int[] pceltFetched = new int[1];
				while (pEnum.Next(1, etcs, pceltFetched) == 0)
				{
					if (etcs[0].cfFormat == 15)
					{
						STGMEDIUM data;
						pDataObject.GetData(ref etcs[0], out data);
						if (data.tymed == TYMED.TYMED_HGLOBAL)
						{
							string[] items = null;
							IntPtr pData = GlobalLock(data.unionmember);
							try
							{
								int size = (int)GlobalSize(data.unionmember);
								int len = 0;
								unsafe
								{
									Int32* pOffset = (Int32*)pData;
									Int16* pFileNames = (Int16*)((Int32)pData + (*pOffset));
									size -= *pOffset;
									while (true)
									{
										if (len >= size || (pFileNames[len] == 0 && pFileNames[len + 1] == 0)) break;
										len += 2;
									}
									String s = Marshal.PtrToStringUni((IntPtr)pFileNames, len);
									items = s.Split(new char[] { (char)0 });
								}
							}
							finally
							{
								GlobalUnlock(data.unionmember);
							}
							StringBuilder files = new StringBuilder();
							foreach (string file in items)
							{
								if (System.IO.File.Exists(file))
								{
									if (files.Length > 0) files.Append('\0');
									files.Append(file);
								}
							}
							if (files.Length > 0)
							{
								_window.IWindow.OnDropFiles.Call(files.ToString());
								return S_OK;
							}
						}
					}
					else if (etcs[0].cfFormat == 13)
					{
						STGMEDIUM data;
						pDataObject.GetData(ref etcs[0], out data);
						String text = null;

						if (data.tymed == TYMED.TYMED_HGLOBAL)
						{
							IntPtr hData = GlobalLock(data.unionmember);
							try
							{
								int size = (int)GlobalSize(data.unionmember);
								text = Marshal.PtrToStringUni(hData);
							}
							finally
							{
								GlobalUnlock(data.unionmember);
							}
						}

						Hashtable data_json = null;
						try
						{
							data_json = Utility.ParseJson(text) as Hashtable;
						}
						catch
						{
							data_json = null;
						}

						if (data_json != null && data_json.ContainsKey("__Type") && data_json.ContainsKey("__Data"))
						{
							string drop_type = data_json["__Type"].ToString();
							string drop_data = data_json["__Data"].ToString();
							_window.IWindow.OnDrop.CallAll(drop_type, drop_data);
							return S_OK;
						}
					}
				}
				if (m_pPreDropTarget != null) m_pPreDropTarget.Drop(pDataObject, grfKeyState, pt, ref pdwEffect);
				return S_OK;
			}
			[DllImport("kernel32.dll")]
			static extern IntPtr GlobalLock(IntPtr hMem);

			[DllImport("kernel32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			static extern bool GlobalUnlock(IntPtr hMem);

			[DllImport("kernel32.dll")]
			static extern UIntPtr GlobalSize(IntPtr hMem);

			public static uint E_NOTIMPL = 0x80004001;
			public static uint S_OK = 0;
			public static uint S_FALSE = 1;

			[PreserveSig]
			uint IDocHostUIHandler.EnableModeless(int fEnable)
			{
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.FilterDataObject(MsHtmHstInterop.IDataObject pDO, out MsHtmHstInterop.IDataObject ppDORet)
			{
				ppDORet = null;
				return S_FALSE;
			}

			[PreserveSig]
			uint IDocHostUIHandler.GetDropTarget(MsHtmHstInterop.IDropTarget pDropTarget, out MsHtmHstInterop.IDropTarget ppDropTarget)
			{
				m_pPreDropTarget = pDropTarget;
				ppDropTarget = this;
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.GetExternal(out object ppDispatch)
			{
				ppDispatch = _browser.ObjectForScripting;
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.GetHostInfo(ref _DOCHOSTUIINFO pInfo)
			{
				pInfo.dwFlags |= 0x00000004;
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.GetOptionKeyPath(out string pchKey, uint dw)
			{
				pchKey = null;
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.HideUI()
			{
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.OnDocWindowActivate(int fActivate)
			{
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.OnFrameWindowActivate(int fActivate)
			{
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.ResizeBorder(ref MsHtmHstInterop.tagRECT prcBorder, IOleInPlaceUIWindow pUIWindow, int fRameWindow)
			{
				return S_OK;
			}

			[PreserveSig]
			uint IDocHostUIHandler.ShowContextMenu(uint dwID, ref MsHtmHstInterop.tagPOINT ppt, object pcmdtReserved, object pdispReserved)
			{
				return S_FALSE;
			}

			[PreserveSig]
			uint IDocHostUIHandler.ShowUI(uint dwID, IOleInPlaceActiveObject pActiveObject, IOleCommandTarget pCommandTarget, IOleInPlaceFrame pFrame, IOleInPlaceUIWindow pDoc)
			{
				return S_FALSE;
			}

			[PreserveSig]
			uint IDocHostUIHandler.TranslateAccelerator(ref tagMSG lpmsg, uint pguidCmdGroup, uint nCmdID)
			{
				try
				{
					if (_window.translate_accelerator_handler_ != null
						&& _window.translate_accelerator_handler_ != DBNull.Value
						)
					{
						object ret = Convert.ToBoolean(Utility.CallFunc(_window.translate_accelerator_handler_, _window.IWindow, lpmsg.message, lpmsg.lParam, lpmsg.wParam));
						if (Convert.ToBoolean(ret)) return S_OK;
					}
				}
				catch
				{
				}
				return E_NOTIMPL;
			}

			[PreserveSig]
			uint IDocHostUIHandler.TranslateUrl(uint dwTranslate, ref ushort pchURLIn, IntPtr ppchURLOut)
			{
				return S_FALSE;
			}

			[PreserveSig]
			uint IDocHostUIHandler.UpdateUI()
			{
				return S_OK;
			}
		}

		private void browser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
		{

		}

	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class ClientCoord
	{
		public int _x, _y;
		public int X { get { return _x; } }
		public int Y { get { return _y; } }

		public ClientCoord(int x, int y)
		{
			_x = x;
			_y = y;
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class CommonEvent : Delegate
	{
		public void Call(object val)
		{
			base.CallAll(val);
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class NotifyEvent : Delegate
	{
		public void Call(string command, object data)
		{
			base.CallAll(command, data);
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class DropFilesEvent : Delegate
	{
		public void Call(object val)
		{
			base.CallAll(val);
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class ActivateEvent : Delegate
	{
		public void Call(int wParam, int lParam)
		{
			base.CallAll(wParam, lParam);
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class DropEvent : Delegate
	{
		public void Call(string type, string data)
		{
			base.CallAll(type, data);
		}
	}

	class MenuTag
	{
		public string ID;
		public object Data;
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class Menu
	{
		CommandEvent comment_event_ = new CommandEvent();

		public CommandEvent OnCommand
		{
			get { return comment_event_; }
		}

		public void Popup(int x, int y)
		{
			if (form_ != null)
			{
				form_.current_context_menu_ = this;
				menu_.Show(form_, form_.PointToClient(new Point(x, y)));
			}
		}

		ContextMenu menu_;
		Window form_;

		public ContextMenu MenuCtrl
		{
			get { return menu_; }
		}

		public Menu(Control browser, object config)
		{
			object ownerform = Utility.GetProperty(config, "OwnerForm");
			form_ = (ownerform != null && ownerform.GetType() != typeof(DBNull)) ? (ownerform as Window.IWindowImpl).Form : null;

			menu_ = new ContextMenu();
			object items = Utility.GetProperty(config, "Items");
			int length = Convert.ToInt32(Utility.GetProperty(items, "length"));
			List<MenuItem> menuItems = new List<MenuItem>();
			for (int i = 0; i < length; i++)
			{
				MenuItem menuItem = new MenuItem();
				object item = Utility.GetProperty(items, i.ToString());
				string id = Utility.GetProperty(item, "ID").ToString();
				object data = Convert.ToBoolean(Utility.InvokeMethod(item, "hasOwnProperty", "Data")) ? Utility.GetProperty(item, "Data") : null;
				menuItem.Index = i;
				MenuTag tag = new MenuTag();
				tag.ID = id;
				tag.Data = data;
				if (String.IsNullOrEmpty(id))
				{
					menuItem.Text = "-";
					menuItem.Tag = tag;
				}
				else
				{
					string text = Utility.GetProperty(item, "Text").ToString();
					menuItem.Text = text;
					menuItem.Tag = tag;
				}
				menuItem.Click += new System.EventHandler(this.menuItem_Click);
				menuItems.Add(menuItem);
			}
			menu_.MenuItems.AddRange(menuItems.ToArray());
		}

		private void menuItem_Click(object sender, EventArgs e)
		{
			MenuItem menuItem = sender as MenuItem;
			MenuTag tag = menuItem.Tag as MenuTag;
			OnCommand.Call(tag.ID, tag.Data);
		}
	}

	[System.Runtime.InteropServices.ComVisibleAttribute(true)]
	public class CommandEvent : Delegate
	{
		public void Call(string cmd, object data)
		{
			base.CallAll(cmd, data);
		}
	}
}
