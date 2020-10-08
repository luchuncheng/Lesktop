
if (window.ClientMode == undefined) window.ClientMode = false;
if (window.Device == undefined) window.Device = 0;

var CurrentWindow = null;

Core.WindowAnchorStyle = {};
Core.WindowAnchorStyle.Left = 1;
Core.WindowAnchorStyle.Right = 1 << 1;
Core.WindowAnchorStyle.Top = 1 << 2;
Core.WindowAnchorStyle.Bottom = 1 << 3;
Core.WindowAnchorStyle.All = 15;

Core.Post = function(url, data, type, timeout, handler)
{
	try
	{
		var request = null;

		if (window.XMLHttpRequest)
		{
			request = new XMLHttpRequest();
		}
		else if (window.ActiveXObject)
		{
			request = new ActiveXObject("Microsoft.XMLHttp");
		}

		request.onreadystatechange = function()
		{
			if (request.readyState == 4)
			{
				try
				{
					switch (request.status)
					{
						case 200:
							{
								//Core.Session.WriteLog(String.format("Post Success: Url = {1}, status = {0}, ResponseText = '{2}'", request.status, url, request.responseText));
								if (request.responseText != "")
									handler.onsuccess(request.status, request.responseText);
								else
									handler.onerror("Server Error", "\u670D\u52A1\u5668\u9519\u8BEF!");
								break;
							}
						default:
							{
								handler.onerror(request.status, request.statusText);
								Core.Session.WriteLog(String.format("Post Error : status = {0}, statusText = {1}, Url = {2}", request.status, request.statusText, url));
								break;
							}
					}
				}
				catch (ex)
				{
					handler.onerror(ex.mame, ex.message);
				}
				if (timer != null) clearTimeout(timer);
				request = null;
				timer = null;
			}
		}
		var timer = null;
		if (timeout > 0)
		{
			timer = setTimeout(
				function()
				{
					if (request != null)
					{
						request.onreadystatechange = function() { };
						request.abort();
						request = null;
						handler.onabort();
					}
				},
				timeout
			);
		}

		request.open("POST", url, true);
		request.setRequestHeader("Content-Type", type);
		request.send(data);
		return {
			Abort: function()
			{
				if (timer != null) clearTimeout(timer);
				if (request != null)
				{
					Core.Session.WriteLog(String.format("Post Abort: Url = {0}", url));
					request.onreadystatechange = function() { };
					request.abort();
					request = null;
					handler.onabort();
				}
				timer = null;
			}
		}
	}
	catch (ex)
	{
		handler.onerror(new Core.Exception(ex.name, ex.message));
	}
}

Core.IWindow = function()
{

	this.ShowDialog = function(parent) { }

	this.Show = function() { }

	this.Hide = function() { }

	this.Minimum = function() { }

	this.Close = function() { }

	this.Move = function() { }

	this.MoveEx = function() { }

	this.Resize = function() { }

	this.GetTag = function() { }

	this.SetTag = function() { }

	this.GetTitle = function() { }

	this.SetTitle = function() { }

	this.IsTop = function() { }

	this.IsVisible = function() { }

	this.BringToTop = function() { }

	this.Load = function(url, callback) { }

	this.GetHtmlWindow = function() { }

	this.OnLoad = new Core.Delegate();

	this.OnResize = new Core.Delegate();

	this.OnClosed = new Core.Delegate();

	this.OnHidden = new Core.Delegate();

	this.OnNotify = new Core.Delegate();

	this.OnActivated = new Core.Delegate();

	this.GetClientWidth = function() { };

	this.GetClientHeight = function() { };

	this.GetClientCoord = function() { };

	this.Notify = function() { }

	this.Waiting = function() { }

	this.Completed = function() { }

	this.CompleteAll = function() { }
}

Core.GetUrl = function(url)
{
	return Core.Path.Join(Core.Config.AppPath, Core.Config.ResPath, url);
}

Core.Utility.ShowWarning = function(text)
{
	if (ClientMode) window.external.ShowWarning(text);
	else alert(text);
}

Core.Utility.ShowError = function(text)
{
	if (ClientMode) window.external.ShowError(text);
	else alert(text);
}

Core.Utility.IdleFloatForms = [];

Core.Utility.ShowFloatForm = function(text, type)
{
	var floatForm = null;

	if (Core.Utility.IdleFloatForms.length > 0)
	{
		floatForm = Core.Utility.IdleFloatForms[0];
		Core.Utility.IdleFloatForms.splice(0, 1);
		floatForm.GetHtmlWindow().ShowMessage(text, type);
		floatForm.MoveEx("RIGHT|BOTTOM", 0, 0, true);
		floatForm.Show();
		Core.Session.GetGlobal("WindowManagement").Add(floatForm);
	}
	else
	{
		floatForm = Core.CreateWindow(
			{
				Left: 0, Top: 0, Width: 262, Height: 230,
				Title: { InnerHTML: "\u6D88\u606F" },
				HasMinButton: false, HasMaxButton: false,
				Resizable: false,
				MinHeight: 80,
				ShowInTaskbar: false/*,
				OnClose: function(f)
				{
					if (Core.Utility.IdleFloatForms.length < 10)
					{
						Core.Session.GetGlobal("WindowManagement").Remove(f);
						Core.Utility.IdleFloatForms.push(f);
						f.Hide();
					}
					else
					{
						f.Close();
					}
				}*/
			}
		);

		floatForm.MoveEx("RIGHT|BOTTOM", 10000, 10000, true);
		floatForm.Show();
		
		/*floatForm.OnClosed.Attach(
			function()
			{
				var i = 0;
				for(var i = 0;i < Core.Utility.IdleFloatForms.length; i++)
				{
					if(Core.Utility.IdleFloatForms[i] == floatForm) break;
				}
				if(i < Core.Utility.IdleFloatForms.length)
				{
					Core.Utility.IdleFloatForms.splice(i, 1);
				}
			}
		);*/

		floatForm.Load(
			Core.GetUrl("FloatForm.htm"),
			function()
			{
				floatForm.GetHtmlWindow().ShowMessage(text, type);
				floatForm.MoveEx("RIGHT|BOTTOM", 0, 0, true);
			}
		);
	}
}

Core.GetSrcFromImgTag = function (img)
{
    var src_reg = /src\x3D(\x22|\x27)([^\x22\x27]+)(\x22|\x27)/ig;
    src_reg.lastIndex = 0;
    var src_ret = src_reg.exec(img);
    if (src_ret != 0 && src_ret.length >= 4)
    {
        return src_ret[2];
    }
    return "";
}

Core.GetFileNameFromImgTag = function(img)
{
	var src_reg = /src\x3D(\x22|\x27)([^\x22\x27]+)(\x22|\x27)/ig;
	src_reg.lastIndex = 0;
	var src_ret = src_reg.exec(img);
	if (src_ret != 0 && src_ret.length >= 4)
	{
		var filename_regex = /\x2FDownload.ashx\x3FFileName\=([^\s\x28\x29\x26]+)/ig;
		filename_regex.lastIndex = 0;
		var fnret = filename_regex.exec(src_ret[2]);
		if(fnret != null && fnret.length >= 2)
		{
			return Core.Utility.Unescape(fnret[1]);
		}
	}
	return "";
}

Core.CreateDownloadUrl = function(filename)
{
	if (filename.substr(0, 1) != "/") filename = String.format("/{0}/{1}", Core.Session.GetUserID(), filename);
	return String.format("{0}?FileName={1}", Core.GetUrl("Download.ashx"), Core.Utility.Escape(filename));
}

Core.IsOwnResource = function(url)
{
	var url_lc = url.toLowerCase();
	var host_regex = /([^\x3A]+)\x3A\x2F\x2F([^\x2F]+)\x2F/ig;
	host_regex.lastIndex = 0;
	var ret = host_regex.exec(url_lc);
	if(ret != null && ret.length >= 3)
	{
		var host = window.location.host.toLowerCase();
		if (ret.index == 0 && ret[2] == host) return true;
	}
	return false;
}

Core.CreateFileTag = function(path)
{
	return String.format("[FILE:{0}]", Core.Utility.Escape(path));
}

Core.CreateFileHtml = function (paths)
{
	var fs = [];
	for (var i in paths)
	{
		var html = "<div contenteditable='false' class='message_file'>" + Core.CreateFileTag(paths[i]) + "</div>";
		fs.push(html);
	}
	return "<br/>" + fs.join("<br/>") + "<br/>";
}

Core.CreateHeadImgUrl = function(id, size, gred, headimg)
{
	if (headimg == "" || size == 16)
	{
		var url = Core.GetUrl("Themes/Default/HeadIMG/user");
		if (gred) url += ".gred";
		if (size > 0) url += "." + size;
		url += ".png";
		return url;
	}
	else
	{
		return Core.GetUrl(String.format("HeadImg.ashx?user={0}&size={1}&gred={2}&headimg={3}", id, size, gred, headimg));
	}
}

Core.CreateGroupImgUrl = function(id, isTemp)
{
	var url = Core.GetUrl("Themes/Default/HeadIMG")
	if (isTemp) url += "/temp_group.png"; else url += "/group.png";
	return url;
}

Core.GetMultiUsersInfo = function(callback, byName, ids)
{
	var data = {
		Action: "GetMultiUsersInfo",
		ByName: byName,
		Data: ids
	};
	Core.SendCommand(
		function(ret)
		{
			callback(ret.Infos);
		},
		function(ex)
		{
			callback(null, ex);
		},
		Core.Utility.RenderJson(data), "Core.Web Common_CH", false
	);
}

Core.TranslateMessage = function(msg, data)
{
	msg = msg.replace(
		/<img [^\t\n\r\f\v<>]+>/ig,
		function(img)
		{
		    var filename = Core.GetFileNameFromImgTag(img);
		    if (filename != "")
		    {
		        var url = String.format("{0}?FileName={{Accessory type=\"image\" src=\"{1}\"}", Core.GetUrl("Download.ashx"), Core.Utility.Escape(filename));
		        return String.format("<img src=\"{0}\">", url);
		    }
		    else
		    {
		        return img;
		    }
		}
	);

	msg = msg.replace(
		/<img [^<>]+>/ig,
		function(img)
		{
			// 替换PC客户端本地图片，图片文件内容以base64字符串方式和文字消息一起发送
			var s = img.replace(
				/src[^<>]*=[^<>]*\x22([^<>]+\/|)file\x3A\/\/\/([^\t\n\r\f\v\x22]+)\x22/ig,
				function(text, v1, src)
				{
					var data_id = Core.GenerateUniqueId();
					var base64 = window.external.ToBase64String(src);
					data[data_id] = base64;
					var filename = Core.Utility.Escape(Core.Utility.Unescape(Core.Path.GetFileName(src)));
					url = String.format(Core.GetUrl("Download.ashx") + "?FileName={{Accessory type=\"image\" src=\"{0}\" data=\"{1}\"}", filename, data_id);
					return String.format("src=\"{0}\"", url);
				}
			);			
			return s;
		}
	);

	msg = msg.replace(
		/<a [^<>]+[^\x2F]>/ig,
		function(a)
		{
			var hrefReg = /href=\x22[^\t\n\r\f\v\x22]+\x22/ig
			hrefReg.lastIndex = 0;
			var href = hrefReg.exec(a);
			return String.format("[A:{0}]", href != null && href.length > 0 ? escape(href[0].substr(6, href[0].length - 7)) : "");
		}
	);

	msg = msg.replace(
		/<\x2Fa>/ig,
		function(a)
		{
			return "[/A]";
		}
	);

	msg = msg.replace(
		/\x5BFILE\x3A([^\n\f\r\t\v\x5B\x5D]+)\x5D/g,
		function(file, path)
		{
			return String.format("[FILE:{{Accessory type=\"file\" src=\"{0}\"}]", path);
		}
	);

	msg = msg.replace(
		/<!--[^\n\f\r\t\v<>]*-->/g,
		function()
		{
			return "";
		}
	);
	
	return msg;
}

Core.SendCommand = function(callback, errorCallback, data, handler, isAysn)
{
	if (isAysn == undefined) isAysn = false;

	if (isAysn)
	{
		Core.Session.ResponsesHandler.Start();
	}

	var postData = '<?xml version="1.0" encoding="utf-8" ?>\r\n';
	var id = Core.GenerateUniqueId() + "-" + Math.round(1000000000 + Math.random() * 100000000);
	postData += String.format(
		'<Command ID="{0}" SessionID=\"{1}" Handler=\"{3}\" IsAsyn=\"{4}\">{2}</Command>\r\n',
		id, Core.Session.GetSessionID(), Core.Utility.TransferCharForXML(data), handler, isAysn
	);

	if (isAysn)
	{
		Core.Session.ResponsesHandler.NewCommandHandler(id, callback, errorCallback);
	}

	var post_handler = {
		onsuccess: function(status, responseText)
		{
			try
			{
				var ret = Core.Utility.ParseJson(responseText);
				if (ret.IsSucceed)
				{
					if (!isAysn) callback(ret.Data);
				}
				else
				{
					if (isAysn) Core.Session.ResponsesHandler.InvokeErrorCallback(id);
					else errorCallback(ret.Exception);
				}
			}
			catch (ex)
			{
			}
		},
		onerror: function(status, msg)
		{
			try
			{
				if (isAysn) Core.Session.ResponsesHandler.InvokeErrorCallback(id, new Core.Exception("Server Error", msg == "" ? "\u670D\u52A1\u5668\u9519\u8BEF!" : msg));
				else errorCallback(new Core.Exception("Server Error", msg == "" ? "\u670D\u52A1\u5668\u9519\u8BEF!" : msg));
			}
			catch (ex)
			{
			}
		},
		onabort: function()
		{
		}
	}

	Core.Post(Core.GetUrl("Command.ashx"), postData, 'text/xml', -1, post_handler);
}

function CommandHandler(id, callback, errorCallback)
{
	this.Callback = function(data, type)
	{
		if (type == "json") data = Core.Utility.ParseJson(data);
		callback(data);
	}

	this.ErrorCallback = function(data, type)
	{
		if (type == "json") data = Core.Utility.ParseJson(data);
		errorCallback(data);
	}
}

function SessionConstructor()
{
	var obj = this;

	var m_UserName = null;
	var m_UserInfo = null;
	var m_SessionID = null;
	var m_Cookie = null;
	var GlobalHandler = {};

	obj.AfterInitService = new Core.Delegate();

	obj.InitService = function(username, userinfo, cookie, sessionId)
	{
		m_UserName = username;
		m_UserInfo = userinfo;
		m_SessionID = sessionId;
		m_Cookie = cookie;

		Core.Session.ResponsesHandler.Start();

		obj.AfterInitService.Call();
	}

	obj.GetUserInfo = function()
	{
		return m_UserInfo;
	}

	obj.GetUserID = function()
	{
		if(m_UserInfo == undefined || m_UserInfo == null) return 0;
		return m_UserInfo.ID;
	}

	obj.GetSessionID = function()
	{
		return m_SessionID;
	}

	obj.GetCookie = function()
	{
		return m_Cookie;
	}

	obj.ResetUserInfo = function(info)
	{
		m_UserInfo = info;
	}

	obj.Reset = function()
	{
		m_UserName = null;
		m_UserInfo = null;
		m_SessionID = null;
		m_Cookie = null;
	}

	obj.WriteLog = function(log)
	{
		try
		{
			if (Core.OutputPanel != null) Core.OutputPanel.GetHtmlWindow().Write(log);
		}
		catch (ex)
		{
		}
	}

	var m_GlobalObject = {};

	obj.RegisterGlobal = function(key, value)
	{
		m_GlobalObject[key.toUpperCase()] = value;
	}

	obj.RemoveGlobal = function(key)
	{
		delete m_GlobalObject[key.toUpperCase()];
	}

	obj.GetGlobal = function(key)
	{
		return m_GlobalObject[key.toUpperCase()] == undefined ? null : m_GlobalObject[key.toUpperCase()];
	}
	
	var is_online_ = false;
	var err_count_ = 1;
	
	obj.IsOnline = function()
	{
		return is_online_;
	}
	
	obj.Logout = function()
	{
		var request = null;

		if (window.XMLHttpRequest)
		{
			request = new XMLHttpRequest();
		}
		else if (window.ActiveXObject)
		{
			request = new ActiveXObject("Microsoft.XMLHttp");
		}
		request.onreadystatechange = function()
		{
			if (request.readyState == 4)
			{
			}
		}

		var url = Core.GetUrl("Logout.ashx");
		request.open("POST", url, false);
		request.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		request.send(String.format('SessionID={0}', Core.Session.GetSessionID()));
	}

	obj.ResponsesHandler = (function()
	{
		var CommandCallbackCache = {};

		var obj = {};

		var baseTime = new Date(2009, 0, 1);

		var m_Controler = null;
		var m_Stop = false;
		var m_IsRunning = false;

		obj.NewCommandHandler = function(id, callback, errorCallback)
		{
			var handler = new CommandHandler(id, callback, errorCallback)
			CommandCallbackCache[id] = handler;
		}

		obj.InvokeCallback = function(cmdid, data)
		{
			if (CommandCallbackCache[cmdid] != undefined)
			{
				CommandCallbackCache[cmdid].Callback(data);
				delete CommandCallbackCache[cmdid];
			}
		}

		obj.InvokeErrorCallback = function(cmdid, data)
		{
			if (cmdid == "all")
			{
				var callbacks = CommandCallbackCache;

				CommandCallbackCache = {};

				for (var key in callbacks)
				{
					try
					{
						callbacks[key].ErrorCallback(data);
					}
					catch (ex)
					{
					}
				}
			}
			else
			{
				if (CommandCallbackCache[cmdid] != undefined)
				{
					CommandCallbackCache[cmdid].ErrorCallback(data);
					delete CommandCallbackCache[cmdid];
				}
			}
		}

		obj.IsRunning = function()
		{
			return m_IsRunning;
		}

		obj.Start = function()
		{
			if (!m_IsRunning)
			{
				m_IsRunning = true;
				m_Stop = false;
				Send();
			}
		}

		obj.Stop = function()
		{
			m_Stop = true;
			if (m_Controler != null) m_Controler.Abort();
		}

		function Send()
		{
			if (m_Stop)
			{
				m_IsRunning = false;
				return;
			}

			var RequestID = Core.GenerateUniqueId();

			var data = String.format('RequestID={0}&SessionID={1}&ClientMode=false&ServerVersion={2}', RequestID, Core.Session.GetSessionID(), Core.Config.Version);

			var post_handler = {
				onsuccess: function(status, responseText)
				{
					if(err_count_ > 0)
					{
						err_count_ = 0;
						is_online_ = true;
					}
					try
					{
						Core.Session.GetGlobal("ReponsesProcess").Process(responseText);
					}
					catch (ex)
					{
					}
					setTimeout(Send, 10);
				},
				onerror: function(status, msg)
				{
					err_count_ ++;
					if(err_count_ == 3)
					{
						is_online_ = false;
						Core.Utility.ShowFloatForm(ex.toString(), "text");
					}
					try
					{
						var ex = new Core.Exception("Server Error", msg == "" ? "\u670D\u52A1\u5668\u9519\u8BEF!" : msg);
						Core.Session.ResponsesHandler.InvokeErrorCallback("all", ex);
					}
					catch (ex)
					{
					}
					setTimeout(Send, 5000);
				},
				onabort: function()
				{
					Core.Session.WriteLog("Abort");
					setTimeout(Send, 10);
				}
			};

			m_Controler = Core.Post(
				Core.GetUrl("Responses.ashx") + "?ID=" + RequestID,
				data, 'application/x-www-form-urlencoded', 2 * 60 * 1000,
				post_handler
			);
		}

		return obj;

	})();

	return obj;
}

Core.AllPlugins = {
	"EmbedCS": {
		"Name": "EmbedCS",
		"InjectModules": {
			"MainForm": {
				"DependentModules": [],
				"MainMenuExtent": [
					{
						Command: "EmbedCodeManagement",
						Text: "在线客服嵌入代码",
						AdminPermission: true,
						OnCommand: function()
						{
							Core.PluginsNS.EmbedCS.OnEmbedCodeManagement();
						}
					}
				],
				"UserFormToolbarExtent": [
					{
						Command: "ViewComment",
						Text: "查看留言",
						Css: "btnviewcomment",
						AdminPermission: false,
						AllowTempUser: false,
						OnCommand: function()
						{
							Core.PluginsNS.EmbedCS.OnViewComment();
						}
					}
				]
			}
		},
		"Global": "EmbedCS.js",
		"AfterInit": function()
		{
			Core.PluginsNS.EmbedCS.AfterInit();
		},
		"AfterInitSession": function()
		{
			Core.PluginsNS.EmbedCS.AfterInitSession();
		}
	},
	"EIM": {
		"Name": "EIM",
		"InjectModules": {
			"MainForm": {
				"DependentModules": [],
				"MainMenuExtent": [
					{
						Command: "Management",
						Text: "公司部门管理",
						AdminPermission: true,
						OnCommand: function()
						{
							Core.Session.GetGlobal("SingletonForm").ShowOrgForm();
						}
					},
					{
						Command: "Management",
						Text: "公司人员管理",
						AdminPermission: true,
						OnCommand: function()
						{
							Core.Session.GetGlobal("SingletonForm").ShowOrgForm();
						}
					},
					{
						Command: "Management",
						Text: "公司群组管理",
						AdminPermission: true,
						OnCommand: function()
						{
							Core.Session.GetGlobal("SingletonForm").ShowOrgForm();
						}
					}
				],
				"UserFormToolbarExtent": []
			}
		},
		"Global": null,
		"AfterInit": function()
		{
		},
		"AfterInitSession": function()
		{
		}
	},
	"Comm":{
		"Name": "Comm",
		"InjectModules": {
			"MainForm": {
				"DependentModules": [],
				"MainMenuExtent": [
					{
						Command: "CommMan",
						Text: "用户/群组管理",
						AdminPermission: true,
						OnCommand: function()
						{
							Core.PluginsNS.CommPlugin.ShowCommManForm();
						}
					},
					{
						Command: "CommMan",
						Text: "好友/群组管理",
						AdminPermission: false,
						AllowTempUser: false,
						OnCommand: function()
						{
							Core.PluginsNS.CommPlugin.ShowCommManForm();
						}
					},
					{
						Command: "AddFriend",
						Text: "添加好友/群",
						AdminPermission: false,
						AllowTempUser: false,
						OnCommand: function()
						{
							Core.Session.GetGlobal("SingletonForm").ShowAddFriendForm();
						}
					}
				]
			}
		},
		"Global": "CommPlugin.js",
		"AfterInit": function()
		{
		},
		"AfterInitSession": function()
		{
		}
	}
};
