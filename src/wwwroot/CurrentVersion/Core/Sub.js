
Core.main = (parent == window || (window.external != undefined && window.external.Desktop) != undefined) ? window.external.Desktop : parent;

Core.Config = {
	Version: Core.main.Core.Config.Version,
	AppPath: Core.main.Core.Config.AppPath,
	ResPath: Core.main.Core.Config.ResPath
};
		
Core.Taskbar = Core.main.Core.Taskbar;
Core.Desktop = Core.main.Core.Desktop;
Core.Login = Core.main.Core.Login;
Core.Plugins = Core.main.Core.Plugins;
Core.PluginsUtil = Core.main.Core.PluginsUtil;
Core.PluginsNS = Core.main.Core.PluginsNS;
Core.CreateMainMenu = Core.main.Core.CreateMainMenu;
Core.UnreadMsgBoxImpl = Core.main.Core.UnreadMsgBoxImpl;
Core.CategoryData = Core.main.Core.CategoryData;
Core.AccountData = Core.main.Core.AccountData;

if(Core.main.ClientMode != undefined) window.ClientMode = Core.main.ClientMode;

function SetClientMode(cm, win)
{	
	ClientMode = cm;
	CurrentWindow = win;

	document.oncontextmenu = function () { return !ClientMode; }

	if (Core.GetBrowser() == "IE")
	{
		try
		{
			document.execCommand("BackgroundImageCache", false, true);
		}
		catch (ex)
		{
		}
	}

	var enableSelTag = {
		"TEXTAREA": "",
		"INPUT": ""
	};

	document.onselectstart = function(evt)
	{
		var e = new Core.Event(evt, window);
		return (e.GetTarget().tagName != undefined && enableSelTag[e.GetTarget().tagName.toUpperCase()] != undefined)
	}

	Core.Utility.AttachEvent(
		document, "keydown",
		function()
		{
			if (event.keyCode == 116 || (event.ctrlKey && event.keyCode == 82))
			{
				event.keyCode = 0;
				event.returnValue = false;
				return false;
			}
			if (event.keyCode == 70 && event.ctrlKey && !event.altKey && !event.shiftKey)
			{
				event.keyCode = 0;
				event.returnValue = false;
				return false;
			}
		}
	);

	if (!ClientMode)
	{
		Core.Utility.AttachEvent(
			document, "mousedown",
			function()
			{
				CurrentWindow.BringToTop();
			}
		);
	}

	if (ClientMode)
	{
		Core.CreateWindow = function(config)
		{
			var _config = {};
			_config.Left = Core.Utility.IsNull(config.Left, 100);
			_config.Top = Core.Utility.IsNull(config.Top, 100);
			_config.Width = Core.Utility.IsNull(config.Width, 400);
			_config.Height = Core.Utility.IsNull(config.Height, 300);
			_config.MinWidth = Core.Utility.IsNull(config.MinWidth, Math.min(_config.Width, 400));
			_config.MinHeight = Core.Utility.IsNull(config.MinHeight, Math.min(_config.Height, 300));
			_config.HasMinButton = Core.Utility.IsNull(config.HasMinButton, true);
			_config.HasMaxButton = Core.Utility.IsNull(config.HasMaxButton, true);
			_config.Resizable = Core.Utility.IsNull(config.Resizable, true);
			_config.Css = Core.Utility.IsNull(config.Css, "window");
			_config.BorderWidth = Core.Utility.IsNull(config.BorderWidth, 6);
			_config.ShowInTaskbar = Core.Utility.IsNull(config.ShowInTaskbar, _config.HasMinButton);
			_config.Tag = Core.Utility.IsNull(config.Tag, null);
			_config.AllowDrop = Core.Utility.IsNull(config.AllowDrop, false);
			_config.TopMost = Core.Utility.IsNull(config.TopMost, false);
			_config.TranslateAccelerator = Core.Utility.IsNull(config.TranslateAccelerator, null);

			if (config.Title == undefined)
			{
				_config.Title = {
					Height: 18,
					InnerHTML: ""
				};
			}
			else
			{
				_config.Title = {};
				_config.Title.Height = Core.Utility.IsNull(config.Title.Height, 18);
				_config.Title.InnerHTML = Core.Utility.IsNull(config.Title.InnerHTML, "");
			}

			_config.OnClose = Core.Utility.IsNull(config.OnClose, null);

			var win = window.external.CreateWindow(_config);
			Core.Session.GetGlobal("WindowManagement").Add(win);
			win.OnClosed.Attach(function(w){Core.Session.GetGlobal("WindowManagement").Remove(w);});
			return win;
		}
		
		Core.CreateMenu = function(config)
		{
			return window.external.CreateMenu(config);
		}

		Core.Session = Core.main.Core.Session;
		Core.OutputPanel = window.external.Desktop.Core.OutputPanel;
	}
	else
	{
		Core.CreateWindow = parent.Core.CreateWindow;
		Core.CreateMenu = parent.Core.CreateMenu;
		Core.Session = parent.Core.Session;
		Core.OutputPanel = parent.Core.OutputPanel;
	}

	if (window.init != undefined) window.init();
	return true;
}