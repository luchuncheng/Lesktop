try
{
	if (window.external.Version != undefined && window.external.Version != Core.Config.Version)
	{
		window.external.ShowError("\u5BA2\u6237\u7AEF\u7248\u672C\u4E0D\u517C\u5BB9\uFF0C\u8BF7\u5347\u7EA7\u81F3 " + Core.Config.Version + "！");
		window.external.ExitApplication();
	}
}
catch (ex)
{
}

function IsDebug()
{
	return window.external != undefined && window.external.Debug;
}

if (window.Core.PluginsNS == undefined) window.Core.PluginsNS = {};

try
{
	for (var i in Core.Plugins)
	{
		Core.Plugins[i].ID = 'PLUGIN' + (100000 + parseInt(i)).toString();
		if (Core.Plugins[i].Global != null)
		{
			var scriptHtml = null;

			if (window.ClientMode == true)
			{
				scriptHtml = String.format('<script src="Plugins/{0}" type="text/javascript"></script>', Core.Plugins[i].Global);
			}
			else
			{
				scriptHtml = String.format('<script src="{0}/{1}" type="text/javascript"></script>', Core.GetUrl("Plugins"), Core.Plugins[i].Global);
			}

			document.writeln(scriptHtml);
		}
	}
}
catch(ex)
{
}

Core.UnreadMsgBoxImpl = (function(){

var obj = {};

var m_UnreadMessages = {};
var m_RelativeUsers = [];
var m_Count = 0;

obj.Add = function(msg, users)
{
	var peer = (msg.Receiver.Type == 0 ? msg.Sender : msg.Receiver);
	if (m_UnreadMessages[peer.ID] == undefined)
	{
		m_UnreadMessages[peer.ID] = [];
		m_RelativeUsers.push(users[peer.ID]);
	}
	m_UnreadMessages[peer.ID].push(msg);
	m_Count++;
	if(obj.IsUnreadMsgBoxVisible())
	{
		try
		{
			m_MsgBoxForm.GetHtmlWindow().Refresh();
		}
		catch (ex)
		{
		}
	}
	window.external.RefreshTrayIcon();
}

obj.RefreshMsgBox = function()
{
	if(obj.IsUnreadMsgBoxVisible())
	{
		try
		{
			m_MsgBoxForm.GetHtmlWindow().Refresh();
		}
		catch (ex)
		{
		}
	}
}

obj.GetRelativeUsers = function()
{
	return m_RelativeUsers;
}

obj.Open = function(peer)
{
	var form = Core.Session.GetGlobal("ChatService").Open(peer, false, {});
}

obj.GetUnreadMessages = function(id)
{
	return m_UnreadMessages[id] == undefined ? [] : m_UnreadMessages[id];
}

obj.GetUnreadMsgCount = function()
{
	return m_Count;
}

obj.GetUnreadMsgCountOfUser = function(id)
{
	return  m_UnreadMessages[id].length;
}

obj.IsUnreadMsgBoxVisible = function()
{
	return m_MsgBoxForm != null && m_MsgBoxForm.IsVisible();
}

obj.Clear = function(id)
{
	if (id != undefined)
	{
		if (m_UnreadMessages[id] != undefined)
		{
			m_Count -= m_UnreadMessages[id].length;
			delete m_UnreadMessages[id];
			var i = 0;
			for(i = 0; i < m_RelativeUsers.length; i++)
			{
				if(m_RelativeUsers[i].ID == id) break;
			}
			if(i < m_RelativeUsers.length) m_RelativeUsers.splice(i, 1);
		}
	}
	else
	{
		m_UnreadMessages = {};
		m_RelativeUsers = [];
		m_Count = 0;
	}
}

var m_MsgBoxForm = null;
var m_Created = false;

function CreateUnreadMsgBox(callback)
{
	if (m_MsgBoxForm == null)
	{
		m_MsgBoxForm = Core.CreateWindow(
			{
				Left: 0, Top: 0, Width: 120, Height: 80,
				Title: { InnerHTML: "消息盒子" },
				HasMinButton: false, HasMaxButton: false,
				Resizable: false,
				MinHeight: 80,
				ShowInTaskbar: false,
				TopMost: true,
				OnClose: function(f)
				{
					f.Hide();
					window.external.RefreshTrayIcon();
				}
			}
		);
		m_MsgBoxForm.Load(
			Core.GetUrl("UnreadMsgBox.htm"), 
			function()
			{
				m_Created = true;
				callback();
			}
		);
	}
	else
	{
		callback();
	}
}

obj.ShowUnreadMsgBox = function(right)
{
	if(right == undefined) right = 0;
	if(m_MsgBoxForm == null)
	{
		CreateUnreadMsgBox(
			function()
			{
				m_MsgBoxForm.GetHtmlWindow().Refresh();
				window.external.RefreshTrayIcon();
				m_MsgBoxForm.MoveEx("RIGHT|BOTTOM", right, 0, true);
				m_MsgBoxForm.ShowWindow(4);
			}
		);
	}
	else if(m_Created)
	{
		try
		{
			m_MsgBoxForm.GetHtmlWindow().Refresh();
			m_MsgBoxForm.MoveEx("RIGHT|BOTTOM", right, 0, true);
			m_MsgBoxForm.ShowWindow(4);
			window.external.RefreshTrayIcon();
		}
		catch(ex)
		{
		}
	}
}

obj.HideUnreadMsgBox = function()
{
	if(m_MsgBoxForm != null)
	{
		m_MsgBoxForm.Hide();
		window.external.RefreshTrayIcon();
	}
}

obj.OpenFirst = function()
{
	if(obj.GetUnreadMsgCount() > 0) 
	{	
		CreateUnreadMsgBox(
			function()
			{
				m_MsgBoxForm.GetHtmlWindow().Refresh();
				m_MsgBoxForm.GetHtmlWindow().OpenFirst();
			}
		);
	}
}

return obj;

})();

Core.PluginsUtil = {}

Core.PluginsUtil.Call = function(method)
{
	var args = [];
	for (var i = 1; i < arguments.length; i++) args.push(arguments[i]);
	for (var i in Core.Plugins)
	{
		try
		{
			Core.Plugins[i][method].apply(Core.Plugins[i], args);
		}
		catch (ex)
		{
		}
	}
}

Core.PluginsUtil.GetDependentModules = function(injectModule)
{
	var modules = [];
	for (var i in Core.Plugins)
	{
		try
		{
			if(Core.Plugins[i].InjectModules[injectModule].DependentModules != undefined)
			{
				for(var j in Core.Plugins[i].InjectModules[injectModule].DependentModules)
				{
					modules.push(Core.Plugins[i].InjectModules[injectModule].DependentModules[i]);
				}
			}
		}
		catch (ex)
		{
		}
	}
	return modules;
}

Core.PluginsUtil.GetPlugins = function(injectModule)
{
	var plugins = [];
	for (var i in Core.Plugins)
	{
		try
		{
			if(Core.Plugins[i].InjectModules[injectModule] != undefined)
			{
				plugins.push(Core.Plugins[i]);
			}
		}
		catch (ex)
		{
		}
	}
	return plugins;
}

Core.PluginsUtil.ExistPlugin = function(name)
{
	for (var i in Core.Plugins)
	{
		if(Core.Plugins[i].Name == name) return true;
	}
	return false;
}

Core.main = window;

function InitGlobal()
{
	var SingletonForm = (function()
	{

		var obj = {};

		var m_MainForm = null;

		obj.ShowMainForm = function()
		{
			if (m_MainForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 300,
					Height: 600,
					Title: {
						InnerHTML: String.format("{0}", Core.Session.GetUserInfo().Nickname)
					},
					MinWidth: 300,
					HasMaxButton: false,
					HasMinButton: true,
					OnClose: function(form)
					{
						if(ClientMode)
						{
							form.CompleteAll();
							form.Hide();
						}
						else
						{
							form.Close();
						}
					},
					AnchorStyle: Core.WindowAnchorStyle.Right | Core.WindowAnchorStyle.Bottom
				};
				m_MainForm = Core.CreateWindow(config);
				m_MainForm.OnClosed.Attach(
					function()
					{
						m_MainForm = null;
					}
				);
				m_MainForm.MoveEx("RIGHT|Bottom", -16, -32, true);
				m_MainForm.Show();
				m_MainForm.Load(Core.GetUrl("MainForm.htm"), null);
			}
			else
			{
				m_MainForm.Show();
			}

			return m_MainForm;
		}
		
		var m_OrgForm = null;

		obj.ShowOrgForm = function()
		{
			if (m_OrgForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 850,
					Height: 600,
					MinWidth: 850,
					MinHeight: 600,
					Title: {
						InnerHTML: "公司部门、人员和群组管理"
					},
					OnClose: function(form)
					{
						if(ClientMode)
						{
							form.CompleteAll();
							form.Hide();
						}
						else
						{
							form.Close();
						}
					},
					Resizable: true,
					HasMaxButton: ClientMode,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
				}
				m_OrgForm = Core.CreateWindow(config);
				m_OrgForm.OnClosed.Attach(function() { m_OrgForm = null; });
				m_OrgForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
				setTimeout(function() { m_OrgForm.Show(); }, 10);
				m_OrgForm.Load(Core.GetUrl("OrgForm.htm"), null);
			}
			else
			{
				setTimeout(function() { m_OrgForm.Show(); }, 10);
			}

			return m_OrgForm;
		}

		var m_MsgManagerForm = null;
		var m_MsgPanel = null;

		obj.ShowMsgManagerForm = function(peer)
		{
			if (m_MsgManagerForm == null)
			{
				var tag = {
					MsgHistoryPanel: null,
					OnCreated: function(p)
					{
						if(peer != undefined) p.Select(peer.ID);
						else p.Load();
						m_MsgPanel = p;
					}
				};
				
				var config = {
					Left: 0,
					Top: 0,
					Width: 720,
					Height: 550,
					MinWidth: 700,
					MinHeight: 550,
					Title: {
						InnerHTML: "\u6D88\u606F\u7BA1\u7406"
					},
					OnClose: function(form)
					{
						if(ClientMode)
						{
							form.CompleteAll();
							form.Hide();
						}
						else
						{
							form.Close();
						}
					},
					Resizable: true,
					HasMaxButton: ClientMode,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
				}
				m_MsgManagerForm = Core.CreateWindow(config);
				m_MsgManagerForm.SetTag(tag);
				m_MsgManagerForm.OnClosed.Attach(function() { m_MsgManagerForm = null; });
				m_MsgManagerForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
				setTimeout(function() { m_MsgManagerForm.Show(); }, 10);
				var url = Core.GetUrl("MsgManagerForm.htm");
				m_MsgManagerForm.Load(url, null);
			}
			else
			{
				setTimeout(
					function() 
					{ 
						if(m_MsgPanel != null)
						{
							if(peer != undefined) m_MsgPanel.Select(peer.ID);
							else m_MsgPanel.Load();
						}
						m_MsgManagerForm.Show(); 
					}, 
					10
				);
			}

			return m_MsgManagerForm;
		}

		var m_AdminMsgManagerForm = null;

		obj.ShowAdminMsgManagerForm = function()
		{
			if (m_AdminMsgManagerForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 720,
					Height: 550,
					MinWidth: 700,
					MinHeight: 550,
					Title: {
						InnerHTML: "\u6D88\u606F\u7BA1\u7406"
					},
					Resizable: true,
					HasMaxButton: ClientMode,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
				}
				m_AdminMsgManagerForm = Core.CreateWindow(config);
				m_AdminMsgManagerForm.OnClosed.Attach(function() { m_AdminMsgManagerForm = null; });
				m_AdminMsgManagerForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
				setTimeout(function() { m_AdminMsgManagerForm.Show(); }, 10);
				var url = Core.GetUrl("AdminMsgManagerForm.htm");
				url += "?random=" + (new Date()).getTime();
				m_AdminMsgManagerForm.Load(url, null);
			}
			else
			{
				setTimeout(function() { m_AdminMsgManagerForm.Show(); }, 10);
			}

			return m_AdminMsgManagerForm;
		}
		
		var m_UpdateSelfInfoForm = null;

		obj.ShowUpdateSelfInfoForm = function()
		{
			if (m_UpdateSelfInfoForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 632,
					Height: 570,
					MinWidth: 632,
					MinHeight: 570,
					Title: {
						InnerHTML: "\u4FEE\u6539\u4E2A\u4EBA\u8D44\u6599"
					},
					Resizable: false,
					HasMaxButton: false,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
				}
				m_UpdateSelfInfoForm = Core.CreateWindow(config);
				m_UpdateSelfInfoForm.OnClosed.Attach(function() { m_UpdateSelfInfoForm = null; });
				m_UpdateSelfInfoForm.MoveEx('CENTER', 0, 0, true);
				setTimeout(function() { m_UpdateSelfInfoForm.Show(); }, 10);
				var url = Core.GetUrl("UpdateSelfInfoForm.htm");
				url += "?random=" + (new Date()).getTime();
				m_UpdateSelfInfoForm.Load(url, null);
			}
			else
			{
				setTimeout(function() { m_UpdateSelfInfoForm.Show(); }, 10);
			}

			return m_UpdateSelfInfoForm;
		}
		
		var m_CreateTempGroupForm = null;

		obj.ShowCreateTempGroupForm = function()
		{
			if (m_CreateTempGroupForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 450,
					Height: 500,
					MinWidth: 370,
					MinHeight: 500,
					Title: {
						InnerHTML: "创建多人会话"
					},
					Resizable: false,
					HasMaxButton: false,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
				}
				m_CreateTempGroupForm = Core.CreateWindow(config);
				m_CreateTempGroupForm.SetTag({ IDS: "" });
				m_CreateTempGroupForm.OnClosed.Attach(
					function() 
					{ 
						m_CreateTempGroupForm = null; 
					}
				);
				m_CreateTempGroupForm.MoveEx('CENTER', 0, 0, true);
				setTimeout(function() { m_CreateTempGroupForm.Show(); }, 10);
				var url = Core.GetUrl("CreateTempGroupForm.htm");
				m_CreateTempGroupForm.Load(url, null);
			}
			else
			{
				setTimeout(function() { m_CreateTempGroupForm.Show(); }, 10);
			}

			return m_CreateTempGroupForm;
		}

		var m_AddFriendForm = null;

		obj.ShowAddFriendForm = function(friendName)
		{
			if (m_AddFriendForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 400,
					Height: 300,
					Title: {
						InnerHTML: "添加好友"
					},
					Resizable: false,
					HasMaxButton: false,
					HasMinButton: false
				}
				m_AddFriendForm = Core.CreateWindow(config);
				m_AddFriendForm.OnClosed.Attach(
					function() 
					{ 
						m_AddFriendForm = null; 
					}
				);
				m_AddFriendForm.MoveEx('center', 0, -20, true);
				setTimeout(function() { m_AddFriendForm.Show(); }, 10);
				var url = Core.GetUrl("AddFriendForm.htm?random=" + (new Date()).getTime());
				if (friendName != undefined) url += "&Name=" + friendName;
				m_AddFriendForm.Load(url, null);
			}
			else
			{
				setTimeout(function() { m_AddFriendForm.Show(); }, 10);
			}

			return m_AddFriendForm;
		}

		
		var m_LocalSettingForm = null;

		obj.ShowLocalSettingForm = function()
		{
			if (m_LocalSettingForm == null)
			{
				var config = {
					Left: 0,
					Top: 0,
					Width: 370,
					Height: 400,
					MinWidth: 370,
					MinHeight: 400,
					Title: {
						InnerHTML: "本地设置"
					},
					Resizable: false,
					HasMaxButton: false,
					HasMinButton: true,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
				}
				m_LocalSettingForm = Core.CreateWindow(config);
				m_LocalSettingForm.OnClosed.Attach(
					function() 
					{ 
						m_LocalSettingForm = null; 
					}
				);
				m_LocalSettingForm.MoveEx('CENTER', 0, 0, true);
				setTimeout(function() { m_LocalSettingForm.Show(); }, 10);
				var url = Core.GetUrl("LocalSettingForm.htm");
				m_LocalSettingForm.Load(url, null);
			}
			else
			{
				setTimeout(function() { m_LocalSettingForm.Show(); }, 10);
			}

			return m_LocalSettingForm;
		}

		return obj;
	})();

	Core.Session.RegisterGlobal("SingletonForm", SingletonForm);

	function ChatFormTag(cf, slient)
	{
		var This = this;
		var m_Msgs = [];
		var m_IsCreated = false;
		var m_ChatPanel = null;
		
		This.Slient = slient;

		This.OnFormCreated = new Core.Delegate();

		This.OnFormCreated.Attach(
			function(cp)
			{
				m_ChatPanel = cp;
				m_IsCreated = true;
				for (var i in m_Msgs)
				{
					m_ChatPanel.AddMessage(m_Msgs[i]);
				}
			}
		);

		This.AddMessage = function(msg)
		{
			if (m_IsCreated) m_ChatPanel.AddMessage(msg);
			else m_Msgs.push(msg);
		}
		
		This.IsCreated = function()
		{
			return m_IsCreated;
		}
	}

	var ChatService = (function()
	{
		var obj = {};
		var m_ChatForms = {};
		var m_IdleForms = [];
		
		function CollectForm(f)
		{
			return false;
		}
		
		obj.Exists = function(peer)
		{
			return m_ChatForms[peer] != undefined;
		}

		obj.Open = function(peer, slient, users)
		{
			try
			{
				if (slient == undefined) slient = false;
				if (users == undefined) users = {};
				var peerInfo = Core.AccountData.GetAccountInfo(peer);

				if (m_ChatForms[peer] == undefined)
				{					
					var form = null;
					
					if(m_IdleForms.length > 0) 
					{
						form = m_IdleForms[0];
						Core.Session.GetGlobal("WindowManagement").Add(form);
						m_IdleForms.splice(0, 1);
						form.SetTag(new ChatFormTag(form, slient));
						form.GetTag().Peer = peer;
						form.GetTag().PeerInfo = peerInfo;
						m_ChatForms[peer] = form;
						form.SetTitle(peerInfo == null ? peer.toString() : String.format("{0}({1})", peerInfo.Nickname, peerInfo.Name));
						form.GetHtmlWindow().Create(peer, peerInfo);
						var r = Math.round(Math.random() * 20);
						form.Resize(670, 500);
						if (!ClientMode) form.MoveEx("LEFT|BOTTOM", 16 + r, -32 - r, true);
						else form.MoveEx("MIDDLE|BOTTOM", 0 + r, -30 - r, true);
						if (ClientMode)
						{
							if (slient) form.ShowWindow(7);
							else form.ShowWindow(4);
						}
						else
						{
							if (slient) form.Minimum();
							else form.Show();
						}
					}
					else
					{
						form = Core.CreateWindow(
							{
								Left: 0, Top: 0, Width: 670, Height: 500, MinWidth: 670, MinHeight: 500,
								Title: { 
									InnerHTML: (peerInfo == null ? peer.toString() : String.format("{0}({1})", peerInfo.Nickname, peerInfo.Name))
								},
								AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom,
								AllowDrop: true,
								TranslateAccelerator: function(f, message, lParam, wParam)
								{
									if(message == 256 && wParam == 27)
									{
										setTimeout(function () { if (!CollectForm(f)) f.Close(); }, 10);
										return true;
									}
									return false;
								},
								OnClose: function(f)
								{
									if(ClientMode)
									{
										if(!CollectForm(f)) f.Close();
									}
									else
									{
										f.Close();
									}
								}
							}
						);
						form.SetTag(new ChatFormTag(form, slient));
						form.GetTag().Peer = peer;
						form.GetTag().PeerInfo = peerInfo;
						form.OnClosed.Attach(function() { delete m_ChatForms[peer]; });
						if (slient)
						{
							form.MoveEx("", 10000, 10000, true);
						}
						else
						{
							var r = Math.round(Math.random() * 40);
							if (!ClientMode) form.MoveEx("LEFT|BOTTOM", 16 + r, -32 - r, true);
							else form.MoveEx("MIDDLE|BOTTOM", 0 + r, -30 - r, true);
						}
						form.ShowWindow(4);
						m_ChatForms[peer] = form;
						form.Load(
							Core.GetUrl("ChatForm.htm"),
							function()
							{
								if (slient)
								{
									form.Minimum();
									if (!ClientMode)
									{
										var r = Math.round(Math.random() * 40);
										form.MoveEx("LEFT|BOTTOM", 16 + r, -32 - r, true);
									}
									else
									{
										form.MoveEx("MIDDLE|BOTTOM", 0, -30, true);
									}
								}
							}
						);
					}
					
					if(ClientMode)
					{
						var msgs = Core.UnreadMsgBoxImpl.GetUnreadMessages(peer);
						if(msgs.length > 0)
						{
							for(var k in msgs)
							{
								m_ChatForms[peer].GetTag().AddMessage(msgs[k]);
							}
							Core.UnreadMsgBoxImpl.Clear(peer);
							Core.UnreadMsgBoxImpl.RefreshMsgBox();
							if (Core.UnreadMsgBoxImpl.GetUnreadMsgCount() == 0)
							{
								Core.UnreadMsgBoxImpl.HideUnreadMsgBox();
							}
							window.external.RefreshTrayIcon();
						}
					}
				}
				else
				{
					if (!slient)
					{
						m_ChatForms[peer].Show();
					}
				}
				
				return m_ChatForms[peer];
			}
			catch(ex)
			{
				alert(ex);
			}
		}

		return obj;

	})();

	Core.Session.RegisterGlobal("ChatService", ChatService);

	var WindowManagement = (function()
	{
		var m_All = [];
		
		var obj = {};
		
		obj.Add = function(win)
		{
			m_All.push(win);
		}
		
		obj.Remove = function(win)
		{
			var i = 0;
			for(;i<m_All.length && m_All[i] != win;i++);
			if(i<m_All.length) m_All.splice(i,1);
		}
		
		obj.Notify = function(cmd, data)
		{
			for(var i in m_All)
			{
				try
				{
					m_All[i].OnNotify.Call(cmd,data);
				}
				catch(ex)
				{
				}
			}
		}
		
		return obj;
	})();
	
	Core.Session.RegisterGlobal("WindowManagement",WindowManagement);

	var ReponsesProcess = (function()
	{

		var obj = {};

		function Msg_Cort(m1, m2)
		{
			if (m1.CreatedTime > m2.CreatedTime) return 1;
			if (m1.CreatedTime < m2.CreatedTime) return -1;
			return 0;
		}

		var m_GlobalHandler = {
			"GLOBAL:IM_MESSAGE_NOTIFY": function(data)
			{
				try
				{
					if(data != undefined && data.Users != undefined) Core.AccountData.UpdateMultiAccountInfo(data.Users);
				}
				catch(ex)
				{
				}
				
				if (data.Peer == 0)
				{
					data.Messages.sort(Msg_Cort);
					for (var i in data.Messages)
					{
						var msg = data.Messages[i];
						if (msg.Sender != null && msg.Receiver != null)
						{
							if (msg.Sender.ID == Core.AccountData.AdminstratorID)
							{
								Core.Utility.ShowFloatForm(msg.Content, "json");
							}
							else
							{
								var peer = (msg.Receiver.Type == 0 ? msg.Sender.ID : msg.Receiver.ID);
								if(ClientMode && window.external.UseMsgBox && !Core.Session.GetGlobal("ChatService").Exists(peer))
								{
									Core.UnreadMsgBoxImpl.Add(msg, data.Users);
								}
								else
								{
									(function(msg)
									{
										var form = Core.Session.GetGlobal("ChatService").Open(peer, true, data.Users);
										form.GetTag().AddMessage(msg);
									})(msg);
								}
							}
						}
					}
					if(ClientMode && data.Messages.length > 0)
					{
						window.external.PlayMsgNotifySound();
					}
				}
				else
				{
					if (data.Message.Sender != null && data.Message.Receiver != null)
					{
						if (data.Message.Sender.ID == Core.AccountData.AdminstratorID)
						{
							Core.Utility.ShowFloatForm(data.Message.Content, "json");
						}
						else
						{
							if (data.Message.Receiver.Type == 0)
							{
								(function(msg)
								{
									if(ClientMode && window.external.UseMsgBox && !Core.Session.GetGlobal("ChatService").Exists(msg.Sender.ID))
									{
										Core.UnreadMsgBoxImpl.Add(msg, data.Users);
									}
									else
									{
										var form = Core.Session.GetGlobal("ChatService").Open(msg.Sender.ID, true, data.Users);
										form.GetTag().AddMessage(msg);
									}

									var senderInfo = data.Users[msg.Sender.ID];
									var curState = Core.AccountData.GetState(senderInfo.ID);
									if (curState != "" && curState != senderInfo.State)
									{
										Core.AccountData.ResetState(senderInfo.ID, senderInfo.State);
										Core.Session.GetGlobal("WindowManagement").Notify(
											"UserStateChanged",
											{
												User: msg.Sender.ID,
												State: senderInfo.State,
												Details: senderInfo
											}
										);
									}
								})(data.Message);
							}
							else
							{
								(function(msg)
								{
									if(ClientMode && window.external.UseMsgBox && !Core.Session.GetGlobal("ChatService").Exists(msg.Receiver.ID))
									{
										Core.UnreadMsgBoxImpl.Add(msg, data.Users);
									}
									else
									{
										var form = Core.Session.GetGlobal("ChatService").Open(msg.Receiver.ID, true, data.Users);
										form.GetTag().AddMessage(msg);
									}
								})(data.Message);
							}
						}
					}
					if(ClientMode)
					{
						window.external.PlayMsgNotifySound();
					}
				}
			},
			"GLOBAL:ADD_COMM_FRIEND": function(data)
			{
				Core.AccountData.AddCommFriend(data.CommFriend);
				Core.AccountData.FireDataChangedEvent("AddCommFriend", { CommFriend: data.CommFriend });
			},
			"GLOBAL:REMOVE_COMM_FRIEND": function(data)
			{
				Core.AccountData.RemoveCommFriend(data.CommFriend);
				Core.AccountData.FireDataChangedEvent("RemoveCommFriend", { CommFriend: data.CommFriend });
			},
			"GLOBAL:ACCOUNTINFO_CHANGED": function(data)
			{
				Core.AccountData.UpdateAccountInfo(data.Details);
				if(data.Details.ID == Core.Session.GetUserID())
				{
					Core.Session.ResetUserInfo(data.Details);
				}
			},
			"UserStateChanged": function(data)
			{
				Core.AccountData.ResetState(data.User, data.State);
				Core.Session.GetGlobal("WindowManagement").Notify("UserStateChanged", data);
			},
			"GLOBAL:CREATE_TEMP_GROUP": function(data)
			{
				Core.AccountData.UpdateTempGroupInfo(data.GroupInfo);
				Core.AccountData.FireDataChangedEvent("AddTempGroup", { GroupInfo: data.GroupInfo });
			},
			"GLOBAL:EXIT_TEMP_GROUP": function(data)
			{
				if(data.User.ID == Core.Session.GetUserID())
				{
					Core.AccountData.RemoveTempGroup(data.Group);
					Core.AccountData.FireDataChangedEvent("RemoveTempGroup", { GroupInfo: data.Group });
				}
				Core.Session.GetGlobal("WindowManagement").Notify("ExitTempGroup", data);
			},
			"GLOBAL:ADD_TO_GROUP": function(data)
			{
				Core.Session.GetGlobal("WindowManagement").Notify("AddToGroup", data);
			},
			"GLOBAL:REMOVE_FROM_GROUP": function(data)
			{
				Core.Session.GetGlobal("WindowManagement").Notify("RemoveFromGroup", data);
			},
			"GLOBAL:ADD_TEMP_GROUP": function(data)
			{
				for(var i in data.Users)
				{
					if(data.Users[i].ID == Core.Session.GetUserInfo().ID)
					{
						Core.AccountData.UpdateTempGroupInfo(data.GroupInfo);
						Core.AccountData.FireDataChangedEvent("AddTempGroup", { GroupInfo: data.GroupInfo });
						break;
					}
				}
				Core.Session.GetGlobal("WindowManagement").Notify("AddTempGroup", data);
			},
			"GLOBAL:OFFLINE": function(data)
			{
				Core.Session.ResponsesHandler.Stop();
				Core.Utility.ShowFloatForm("{\"Type\":\"Offline\"}", "json");
				Core.Session.WriteLog(
					String.format("Offline : SessionID = {0}", Core.Session.GetSessionID())
				);
			}
		}

		if(window.CustomServiceMode)
		{
			//客服模式下
			m_GlobalHandler["GLOBAL:IM_MESSAGE_NOTIFY"] = function(data)
			{
				try
				{
					if(data != undefined && data.Users != undefined) Core.AccountData.UpdateMultiAccountInfo(data.Users);
				}
				catch(ex)
				{
				}
				
				if (data.Peer == 0)
				{
					data.Messages.sort(Msg_Cort);
					for (var i in data.Messages)
					{
						var msg = data.Messages[i];
						if (msg.Sender != null && msg.Receiver != null && msg.Receiver.ID == window.CSData.User.ID && 
							(msg.Sender.ID == window.CSData.Peer.ID || msg.Sender.ID == Core.AccountData.AdminstratorID))
						{
							var peer = (msg.Receiver.Type == 0 ? msg.Sender.ID : msg.Receiver.ID);
							window.CSChatPanel.AddMessage(msg);
						}
					}
					if(ClientMode && data.Messages.length > 0)
					{
						window.external.PlayMsgNotifySound();
					}
				}
				else
				{
					var msg = data.Message;
					if((msg.Sender.ID == window.CSData.Peer.ID || msg.Sender.ID == Core.AccountData.AdminstratorID) && 
						msg.Receiver.ID == window.CSData.User.ID)
					{
						window.CSChatPanel.AddMessage(msg);
						try
						{
							var senderInfo = data.Users[msg.Sender.ID];
							var curState = Core.AccountData.GetState(senderInfo.ID);
							if (curState != "" && curState != senderInfo.State)
							{
								Core.AccountData.ResetState(senderInfo.ID, senderInfo.State);
								Core.Session.GetGlobal("WindowManagement").Notify(
									"UserStateChanged",
									{
										User: msg.Sender.ID,
										State: senderInfo.State,
										Details: senderInfo
									}
								);
							}
						}
						catch(ex)
						{
						}
					}
					if(ClientMode)
					{
						window.external.PlayMsgNotifySound();
					}
				}
			}
		}

		obj.Process = function(responseText)
		{
			var ret = Core.Utility.ParseJson(responseText);
			//Core.Session.WriteLog(responseText);
			if (ret.IsSucceed)
			{
				var responses = ret.Responses;

				for (var i in responses)
				{
					var cr = responses[i];

					if (cr.CommandID == "GLOBAL:SessionReset")
					{
						Core.Session.ResponsesHandler.InvokeErrorCallback("all", new Core.Exception("Server Error", "\u670D\u52A1\u5668\u9519\u8BEF!"));

					}
					else
					{
						if (m_GlobalHandler[cr.CommandID] != undefined)
						{
							m_GlobalHandler[cr.CommandID](cr.Data);
						}
						else
						{
							Core.Session.ResponsesHandler.InvokeCallback(cr.CommandID, cr.Data);
						}
					}
				}
			}
			else
			{
				if(ret.Exception.Name == "UnauthorizedException")
				{
					if(Core.Session.ResponsesHandler.IsRunning())
					{
						Core.Session.ResponsesHandler.Stop();
						Core.Utility.ShowFloatForm("{\"Type\":\"UnauthorizedException\"}", "json");
					}
				}
				else if(ret.Exception.Name == "IncompatibleException")
				{
					if(Core.Session.ResponsesHandler.IsRunning())
					{
						Core.Session.ResponsesHandler.Stop();
						Core.Utility.ShowFloatForm("{\"Type\":\"IncompatibleException\"}", "json");
					}
				}
			}
		}

		return obj;
	})();

	Core.Session.RegisterGlobal("ReponsesProcess", ReponsesProcess);
	
	function MarkStatus(status)
	{
		var data = {
			Action: "MarkStatus",
			Status: status
		};
		
		Core.SendCommand(
			function(ret)
			{
			},
			function(ex)
			{
			},
			Core.Utility.RenderJson(data), "Core.Web Common_CH", false
		);
	}

	Core.Session.RegisterGlobal("MarkStatus", MarkStatus);
}

(function(){

var m_PluginsCommandHandlers = {};

Core.CreateMainMenu = function(ownerForm)
{
	if(ownerForm == undefined) ownerForm = null;
	var menuConfig = null;

	if (Core.Session.GetUserInfo().IsAdmin)
	{
		menuConfig = {
			Items: [
				{
					Text: "修改个人资料",
					ID: "UpdateSelfInfo"
				},
				{
					Text: "消息管理",
					ID: "MsgManagementForm"
				}
			],
			OwnerForm: ownerForm
		};
	}
	else
	{
		menuConfig = {
			Items: [
				{
					Text: "修改个人资料",
					ID: "UpdateSelfInfo"
				},
				{
					Text: "消息管理",
					ID: "MsgHistory"
				}
			],
			OwnerForm: ownerForm
		};
	}
	
	if (ClientMode)
	{
		menuConfig.Items.push({ Text: "本地设置", ID: "LocalSetting" });
	}
	try
	{
		var Plugins= Core.PluginsUtil.GetPlugins("MainForm");
		
		for (var i in Plugins)
		{
			var plugin = Plugins[i];
			var imc = plugin.InjectModules["MainForm"];
			if (imc.MainMenuExtent != undefined)
			{
				if (imc.MainMenuExtent.length > 0)
				{
					var addSplitor = false;
					for (var j in imc.MainMenuExtent)
					{
						try
						{

							var me = imc.MainMenuExtent[j];
							if ((Core.Session.GetUserInfo().IsAdmin || !me.AdminPermission) && (me.ClientMode != true || ClientMode) && 
								(!Core.Session.GetUserInfo().IsTemp || me.AllowTempUser != false))
							{
								if (!addSplitor)
								{
									var smec = {
										Text: "",
										ID: ""
									};
									menuConfig.Items.push(smec);
									addSplitor = true;
								}
								var mec = {
									Text: me.Text,
									ID: plugin.ID + ":" + me.Command
								};
								m_PluginsCommandHandlers[mec.ID] = me.OnCommand;

								menuConfig.Items.push(mec);
							}
						}
						catch(ex)
						{
							//handle exception
						}
					}
				}
			}
		}
	}
	catch(ex)
	{

	}
	
	if(ClientMode)
	{
		menuConfig.Items.push({
			Text: "退出",
			ID: "Exit"
		});
	}

	var m_Menu = new Core.CreateMenu(menuConfig);
	
	if(m_Menu != null && m_Menu.OnCommand != undefined)
	{
		m_Menu.OnCommand.Attach(
			function(command)
			{
				if (command == "AddFriend")
				{
					Core.Session.GetGlobal("SingletonForm").ShowAddFriendForm();
				}
				else if (command == "Management")
				{
					Core.Session.GetGlobal("SingletonForm").ShowOrgForm();
				}
				else if (command == "MsgHistory")
				{
					Core.Session.GetGlobal("SingletonForm").ShowMsgManagerForm();
				}
				else if (command == "UpdateSelfInfo")
				{
					Core.Session.GetGlobal("SingletonForm").ShowUpdateSelfInfoForm();
				}
				else if (command == "MsgManagementForm")
				{
					Core.Session.GetGlobal("SingletonForm").ShowAdminMsgManagerForm();
				}
				else if (command == "LocalSetting")
				{
					Core.Session.GetGlobal("SingletonForm").ShowLocalSettingForm();
				}
				else if (command == "Exit")
				{
					Core.ExitApplication();
				}
				else if (m_PluginsCommandHandlers[command] != undefined)
				{
					m_PluginsCommandHandlers[command].call(window);
				}
			}
		);
	}
	
	return m_Menu;
}

})();

Core.AccountData = (function() {

	var obj = {};

	var dept_data_ = {};
	
	obj.AdminstratorID = 2;
	obj.AdminID = 3;
		
	var temp_group_data = {};
	var has_fetch_temp_group_data_ = false;
	
	obj.GetTempGroups = function (callback)
	{
		var args = [];
		args.push(true);
		args.push({});
		for(var i = 1; i < arguments.length; i++) args.push(arguments[i]);
		
		if(has_fetch_temp_group_data_)
		{
			args[0] = true;
			args[1] = temp_group_data;
			callback.apply(Core.AccountData, args);
		}
		else
		{
			var data = {
				Action: "GetTempGroups"
			};
			
			Core.SendCommand(
				function(ret)
				{
					has_fetch_temp_group_data_ = true;
					temp_group_data = ret.TempGroups;
					Core.AccountData.UpdateMultiAccountInfo(ret.TempGroups);
					args[0] = true;
					args[1] = temp_group_data;
					callback.apply(Core.AccountData, args);
				},
				function(ex)
				{
					args[0] = true;
					args[1] = ex;
					callback.apply(Core.AccountData, args);
				},
				Core.Utility.RenderJson(data),"Core.Web Account_CH",false
			);
		}
	}
	
	obj.UpdateTempGroupInfo = function(group)
	{
		temp_group_data[group.ID] = group;
	}
	
	obj.RemoveTempGroup = function(group)
	{
		if(temp_group_data[group.ID] != undefined)
		{
			delete temp_group_data[group.ID];
		}
	}
	
	
	var comm_friends_ = new Core.Map();
	var has_fetch_comm_friends_ = false;
	
	obj.GetCommFriends = function (callback)
	{
		var args = [];
		args.push(true);
		args.push({});
		for(var i = 1; i < arguments.length; i++) args.push(arguments[i]);
		
		if(has_fetch_comm_friends_)
		{
			args[0] = true;
			args[1] = comm_friends_;
			callback.apply(Core.AccountData, args);
		}
		else
		{
			var data = {
				Action: "GetCommFriends"
			};
			
			Core.SendCommand(
				function(ret)
				{
					has_fetch_comm_friends_ = true;
					comm_friends_ = new Core.Map(ret.CommFriends);
					Core.AccountData.UpdateMultiAccountInfo(ret.CommFriends);
					args[0] = true;
					args[1] = comm_friends_;
					callback.apply(Core.AccountData, args);
				},
				function(ex)
				{
					args[0] = true;
					args[1] = ex;
					callback.apply(Core.AccountData, args);
				},
				Core.Utility.RenderJson(data),"Core.Web Account_CH",false
			);
		}
	}
	
	obj.AddCommFriend = function(comm_friend)
	{
		comm_friends_.Update(comm_friend, true);
	}
	
	obj.RemoveCommFriend = function(comm_friend)
	{
		comm_friends_.Remove(comm_friend);
	}
	
	var accounts_ = new Core.Map();
	
	obj.GetAccountInfo = function(key, callback)
	{
		try
		{
			var account = accounts_.Get(key);
			if(callback != undefined)
			{
				if(account == null)
				{
					var data = {
						Action: "GetMultiUsersInfo",
						ByName: typeof (key) != "number",
						Data: [key]
					};
					Core.SendCommand(
						function(ret)
						{
							account = (ret.Infos.length > 0 ? ret.Infos[0] : null);
							if(account != null)
							{
								obj.UpdateAccountInfo(account);
							}
							callback(account);
						},
						function(ex)
						{
							callback(null);
						},
						Core.Utility.RenderJson(data), "Core.Web Common_CH", false
					);
				}
				else
				{
					callback(account);
				}
			}
			return account;
		}
		catch(ex)
		{
			return null;
		}
	}
	
	obj.UpdateAccountInfo = function(account, fire_event)
	{
		try
		{
			if(fire_event == undefined) fire_event = true;
			if(accounts_.Update(account, true) && fire_event)
			{
				Core.Session.GetGlobal("WindowManagement").Notify("AccountInfoChanged", { Details: account });
			}
			return update;
		}
		catch(ex)
		{
			return false;
		}
	}
	
	obj.UpdateMultiAccountInfo = function(accounts, fire_event)
	{
		try
		{
			if(accounts != undefined && accounts != null)
			{
				for (var k in accounts)
				{
					try { obj.UpdateAccountInfo(accounts[k], fire_event); } catch (ex) { }
				}
			}
		}
		catch(ex)
		{
		}
	}
	
	obj.ResetState = function(user, state)
	{
		var account = obj.GetAccountInfo(user);
		if(account != null)
		{
			account.State = state;
		}
	}
	
	obj.GetState = function(user)
	{
		var account = obj.GetAccountInfo(user);
		return account == null ? "Offline" : account.State;
	}
	
	obj.OnDataChanged = new Core.Delegate();
	
	obj.FireDataChangedEvent = function(reason, data)
	{
		obj.OnDataChanged.Call(reason, data);
	}
	
	return obj;

})();

Core.CategoryData = (function()
{
	var obj = {};
	
	var category_data_ = null;
	
	obj.Fetch = function (callback)
	{
		var args = [];
		args.push(true);
		args.push({});
		for(var i = 1; i < arguments.length; i++) args.push(arguments[i]);
		
		if(category_data_ != null)
		{
			args[0] = true;
			args[1] = category_data_;
			callback.apply(Core.CategoryData, args);
		}
		else
		{
			var data = {
				Action: "GetCategoryData"
			};
			
			Core.SendCommand(
				function(ret)
				{
					category_data_ = ret;
					Core.AccountData.UpdateMultiAccountInfo(category_data_.Users);
					args[0] = true;
					args[1] = category_data_;
					callback.apply(Core.CategoryData, args);
				},
				function(ex)
				{
					args[0] = true;
					args[1] = ex;
					callback.apply(Core.CategoryData, args);
				},
				Core.Utility.RenderJson(data),"Core.Web Category_CH",false
			);
		}
	}
	
	obj.OnDataChanged = new Core.Delegate();
	
	obj.FireDataChangedEvent = function(reason, data)
	{
		obj.OnDataChanged.Call(reason, data);
	}
	
	obj.ResetCategories = function(categories)
	{
		category_data_.Categories = categories;
	}
	
	obj.ResetCategoryItems = function(citems)
	{
		category_data_.CategoryItems = citems;
	}
	
	obj.AddUserInfo = function(info)
	{
		category_data_.Users[info.ID] = info;
	}
	
	obj.AddDeptInfo = function(dept)
	{
		category_data_.Depts[dept.ID] = dept;
	}
	
	obj.GetCategories = function()
	{
		return category_data_.Categories;
	}
	
	return obj;
})();

Core.ShowMainForm = function()
{
	try
	{
		Core.Session.GetGlobal("SingletonForm").ShowMainForm();
	} 
	catch (ex)
	{
		window.external.ShowError(ex.toString());
	}
}

Core.ExitApplication = function()
{
	if(ClientMode)
	{
		Core.Session.ResponsesHandler.Stop();
		var data = {
			Action: "RemoveSession",
			SessionID: Core.Session.GetSessionID()
		};
		
		var exitapp_func = function()
		{
			window.external.ExitApplication();
		}
		var exitapp = function()
		{
			if(exitapp_func != null)
			{
				exitapp_func();
				exitapp_func = null;
			}
		}
		setTimeout(exitapp, 20000);
		Core.SendCommand(exitapp, exitapp, Core.Utility.RenderJson(data), "Core.Web Common_CH", false);
	}
}

function SetClientMode(cm)
{
	ClientMode = cm;

	var enableSelTag = {
		"TEXTAREA": "",
		"INPUT": ""
	};

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
			win.OnClosed.Attach(function(w) { Core.Session.GetGlobal("WindowManagement").Remove(w); });
			return win;
		}
		
		Core.CreateMenu = function(config)
		{
			return window.external.CreateMenu(config);
		}

		Core.Session = window.external.Session;
	}
	else
	{
		Core.Utility.AttachEvent(
			document, "mousemove",
			function()
			{
				Core.Utility.StopScrollTitle();
			}
		);
		Core.CreateWindow = function(config)
		{
			var win = new Window(config);
			Core.Session.GetGlobal("WindowManagement").Add(win);
			win.OnClosed.Attach(function(w) { Core.Session.GetGlobal("WindowManagement").Remove(w); });
			return win;
		}
		
		Core.CreateMenu = function(config)
		{
			var menu = new Menu(config);
			return menu;
		}

		Core.Session = new SessionConstructor();
	}

	InitGlobal();

	if(window.CustomServiceMode != true)
	{
		Desktop.Create();

		Core.Taskbar = Taskbar;
		Core.Desktop = Desktop;

		Core.OutputPanel = Core.CreateWindow(
			{
				Left: 200, Top: 150, Width: 600, Height: 450,
				Title: { InnerHTML: "\u8F93 \u51FA" },
				HasMinButton: false,
				OnClose: function(form)
				{
					form.Hide();
				}
			}
		);
	}

	if(IsDebug())
	{
		//Core.OutputPanel.Move(40, 40);
		//Core.OutputPanel.Show(true);
	}

	Core.PluginsUtil.Call("AfterInit");
	
	if(ClientMode)
	{
		if (window.init != undefined) window.init();
	}

	if(window.CustomServiceMode != true)
	{
		Core.Login(!ClientMode);
	}
			
	return true;
}

(function () {

var loginform_ = null;
var start_service_callback_ = null;

window.StartService = function(callback)
{
	Core.Utility.AttachEvent(
		window, "beforeunload",
		function()
		{
			if(Core.Session.ResponsesHandler.IsRunning())
			{
				Core.Session.ResponsesHandler.Stop();
				Core.Session.Logout();
			}
		}
	);

	if(window.CustomServiceMode == true)
	{
		window.CurrentWindow = window.CSWindow;
		SetClientMode(false);
		var csdata = Core.Utility.ParseJson(document.getElementById("data_json").value);
		Core.Session.InitService(csdata.User.Name, csdata.User, document.cookie, csdata.SessionID);
		if (callback != undefined && callback != null) callback();
	}
	else
	{
		start_service_callback_ = callback;
		if (loginform_ == null && (Core.Session == undefined || Core.Session.GetUserID() == 0))
		{
			SetClientMode(false, null);
		}
		else
		{
			if (start_service_callback_ != undefined && start_service_callback_ != null)
			{
				start_service_callback_();
				start_service_callback_ = null;
			}
		}
	}
}

Core.Login = function()
{
	if (loginform_ != null) return;
	loginform_ = Core.CreateWindow(
		{
			Left: 0, Top: 0, Width: 612, Height: 430,
			HasMinButton: ClientMode, HasMaxButton: false,
			Resizable: false,
			Title: { InnerHTML: "登录" }
		}
	);
	loginform_.SetTag({});
	loginform_.OnClosed.Attach(
		function(f)
		{
			loginform_ = null;
			if (Core.Session.GetUserID() != 0)
			{
				if (start_service_callback_ != undefined && start_service_callback_ != null)
				{
					start_service_callback_();
					start_service_callback_ = null;
				}
			}
			if (Core.Session.GetUserID() == 0 && ClientMode)
			{
				window.external.ExitApplication();
			}
		}
	);
	loginform_.MoveEx('center', 0, -20, true);
	loginform_.Show();
	if (Core.Session.GetUserID() != 0)
	{
		loginform_.GetTag().Name = Core.Session.GetUserInfo().Name;
	}
	Core.Session.Reset();
	loginform_.Load(Core.GetUrl("Login.htm"), function() { });
}

Core.ChatWith = function(peer)
{
	try
	{
		if(Core.Session.GetUserID() != 0 && Core.Session.GetUserID() != peer)
		{
			Core.Session.GetGlobal("ChatService").Open(parseInt(peer), false, {});
		}
	}
	catch(ex)
	{
	}
}

})();