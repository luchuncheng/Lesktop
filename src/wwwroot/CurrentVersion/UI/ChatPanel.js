(function () {

var DefaultFormat = {
	Bold: false,
	Italic: false,
	Underline: false,
	FontSize: "small",
	FontName: "宋体",
	ForeColor: "#000000"
};

function LoadDefaultFormat()
{
	if (ClientMode)
	{
		var val = window.external.LocalSetting.GetValue("DefaultFormat");
		if (val != "")
		{
			try { DefaultFormat = Core.Utility.ParseJson(val); } catch (ex) { }
		}
	}
}

function SaveDefaultFormat()
{
	if (ClientMode) window.external.LocalSetting.SetValue("DefaultFormat", Core.Utility.RenderJson(DefaultFormat));
}

function CreateFileHtml(paths)
{
	var fs = [];
	for (var i in paths)
	{
		var html = "<div contenteditable='false' class='message_file'>" + String.format("[FILE:{0}]", paths[i]) + "</div>";
		fs.push(html);
	}
	return "<br/>" + fs.join("<br/>") + "<br/>";
}

function IsEmpty(str)
{
	for (var i = 0; i < str.length;)
	{
		var c = str.substr(i, 1);
		if (str.substr(i, 6).toLowerCase() == "&nbsp;") i += 6;
		else if (c == '\n' || c == '\r' || c == '\f' || c == '\t' || c == '\v' || c == ' ') i++;
		else return false;
	}
	return true;
}

Core.UI.ChatPanel = function(parent, config)
{
	var this_ = this;
	var msgpanel_ = null;
	var toolbar_ = null;
	var editor_ = null;
	var btnok_ = null;
	var user_ = config.User;
	var peer_ = config.Peer;
	var dispose_ = false;
	var leavetip_ = null;
	var leavetip_container_ = null;
	var btnsendfile_ = null;
	var btnsendimage_ = null;
	
	Core.UI.Control.call(this, parent, config);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			dispose_ = true;
		}
	);
	
	var fillpanel_ = new Core.UI.Control(
		this_,
		{
			ID: "fillpanel",
			DockStyle: Core.UI.DockStyle.Fill,
			Controls: [
				{
					ID: "toolbar",
					Css: "ct_toolbar",
					Height: 24,
					Margin: [0, 0, 2, 0],
					DockStyle: Core.UI.DockStyle.Top
				},
				{
					ID: "bottompanel",
					Css: "bottompanel",
					Height: 30,
					Margin: [4, 0, 0, 0],
					DockStyle: Core.UI.DockStyle.Bottom,
					Controls: [
						{ ID: "btnok_container", Css: "ct_custom_default_button", Width: 60, DockStyle: Core.UI.DockStyle.Right }
					]
				},
				{
					ID: "msgeditor",
					Css: "msgeditor",
					Height: 100,
					Resizable: true,
					SplitterSize: 4,
					DockStyle: Core.UI.DockStyle.Bottom
				},
				{
					ID: "msgarea",
					Css: "msgarea",
					BorderWidth: 1,
					DockStyle: Core.UI.DockStyle.Fill,
					Controls: [
						{
							ID: "leavetip",
							Css: "leavetip",
							Height: 21,
							Visible: false,
							DockStyle: Core.UI.DockStyle.Top
						},
						{
							ID: "msgpanel",
							Css: "msgpanel",
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				}
			]
		}
	);

	function SendInstantMessage(content, showMsg, callback)
	{
		if (showMsg == undefined) showMsg = true;
		var data = {
			Action: "NewMessage",
			Sender: user_.ID,
			Receiver: peer_.ID
		};

		data.Content = Core.TranslateMessage(content, data);

		var temp = null;
		if (showMsg)
		{
			temp = msgpanel_.CreateMessage(data.Content);
		}
		editor_.SetText(Core.GetBrowser() == "IE" ? "" : "&nbsp;");

		Core.SendCommand(
			function(data)
			{
				var message = data;
				if (showMsg)
				{
					msgpanel_.AddMessage(message, temp);
				}
				if(callback != undefined) callback();
			}, 
			function(ex)
			{
				if (showMsg)
				{
					var text = Core.Utility.GetInnerText(temp.childNodes[1]);
					if (text.length > 20) text = text.substr(0, 20) + "...";
					var html = String.format("<div class='error'>由于网络原因，消息\"{0}\"发送失败，错误信息:<br/>{1}</div>", text, ex.Message);
					var dom = msgpanel_.CreateElement("DIV");
					dom.className = "message";
					dom.innerHTML = html;
					msgpanel_.ScrollToBottom();
					editor_.Focus();
					temp.childNodes[1].innerHTML += String.format("<div class='error'>{0}</div>", ex.Message);
				}
				if(callback != undefined) callback();
			}, 
			Core.Utility.RenderJson(data), "Core.Web WebIM", false
		);
	}
	
	function SendFiles(files, index, dsts)
	{
		if (index >= files.length)
		{
			var html = CreateFileHtml(dsts);
			SendInstantMessage(html);
			return;
		}
		msgpanel_.UploadFile(
			files[index],
			function(path)
			{
				dsts.push(path);
				SendFiles(files, index + 1, dsts);
			}
		);
	}
	
	//局域网内发送文件回调处理，用于显示进度等
	function SendFileHandler(file)
	{
		var html_fmt = "<div class='dl_image_dl'></div>" +    
		"<div class='dl_text'>" +    
		"<span class='span_normal'>发送文件 \"{0}\" </span>" +    
		"<span class='processing'></span>" +    
		"<a>取消</a>" +
		"<a>发送离线文件</a>" +    
		"</div>";
	
		var html = String.format(html_fmt, file);
		var dom = msgpanel_.CreateElement("DIV");
		dom.className = 'message_file_dl';
		dom.innerHTML = html;
	
		var info_dom = dom.childNodes[1].childNodes[0];
		var pdom = dom.childNodes[1].childNodes[1];
		var a_dom = dom.childNodes[1].childNodes[2];
		var upload_dom = dom.childNodes[1].childNodes[3];
		var img_dom = dom.childNodes[0];
	
		var _isCanceled = false;
	
		a_dom.onclick = function()
		{
			_isCanceled = true;
		}
	
		upload_dom.onclick = function()
		{
			_isCanceled = true;
			setTimeout(function()
			{
				SendFiles([file], 0, []);
			},
			10);
		}
	
		var _recv = 0,
		_total = 0;
	
		this.BeforeUpload = function(total)
		{
			_total = total;
			pdom.innerHTML = "正在等待对方接受文件";
			msgpanel_.AppendChild(dom);
		}
	
		this.QueryContinue = function()
		{
			return (dispose_ || _isCanceled) ? 0 : 1;
		}
	
		this.Processing = function(length, size, speed)
		{
			_recv = length;
			_total = size;
			var temp;
			if (size > 1024 * 1024 * 1024) temp = Math.round(size / (1024 * 1024 * 1024) * 100) / 100 + "GB";
			else if (size > 1024 * 1024) temp = Math.round(size / (1024 * 1024) * 100) / 100 + "MB";
			else if (size > 1024) temp = Math.round(size / 1024 * 100) / 100 + "KB";
			else temp = size + "B";
			pdom.innerHTML = String.format("传送速度:{2}KB/s 已完成:{0}%, 共 {1}", (Math.round(length / size * 1000) / 10), temp, Math.round(speed / 10) / 100);
			return (dispose_ || _isCanceled) ? 0 : 1;
		}
	
		this.AfterUpload = function(path)
		{
			info_dom.innerHTML = String.format(_recv == _total ? "\"{0}\" 发送完毕": "\"{0}\" 发送已取消！", file);
			info_dom.className = (_recv == _total ? "span_normal": "span_red");
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			upload_dom.style.display = 'none';
			img_dom.className = (_recv == _total ? "dl_image_file": "dl_image_cancel");
		}
	
		this.HandleError = function(msg)
		{
			info_dom.className = "span_red";
			info_dom.innerHTML = String.format("发送 \"{0}\" 时发生错误: {1}", file, msg);
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			upload_dom.style.display = 'none';
			img_dom.className = "dl_image_error";
		}
	}
	
	//局域网内发送文件，不上传到服务器
	function SendLocalFiles(files)
	{
		var urls = [];
		for (var i in files)
		{
			var file = files[i];
			var url = window.external.SendFile(file, new SendFileHandler(file));
			urls.push(url);
		}
	
		var data = {
			Action: "NewMessage",
			Sender: user_.ID,
			Receiver: peer_.ID,
			IsTemp: true,
			Content: Core.Utility.RenderJson(
				{
					Action: "RecvFiles",
					URLS: urls
				}
			)
		};
		
		Core.SendCommand(
			function(data)
			{
				//
			}, 
			function(ex)
			{
				Core.Utility.ShowError(ex);
			},  
			Core.Utility.RenderJson(data), "Core.Web WebIM", false
		);
	}

	// 私聊和新闻聊天室消息快捷操作
	var opes1 = [
		{
			Text: "引用",
			Command: "Quote"
		}
	];
	
	// 群组消息快捷操作
	var opes2 = [
		{
			Text: "私聊",
			Command: "Chat"
		},
		{
			Text: "引用",
			Command: "Quote"
		}
	];
	
	var leavetip_ = fillpanel_.FindControl("msgarea/leavetip");
	leavetip_container_ = leavetip_.GetContainer();
	leavetip_container_.innerHTML = "<div></div>";
	
	var msgpanel_config_ = {
		User: user_,
		Peer: peer_,
		GetOperations : function(msg)
		{
			if (msg.Receiver.Type == 0 || (msg.Receiver.Type == 1 && msg.Receiver.SubType == 3) || 
				(msg.Receiver.Type == 1 && msg.Sender.ID == user_.ID))
			{
				return opes1;
			}
			else
			{
				return opes2;
			}
		}
	};	
	msgpanel_ = new Core.UI.MsgPanel(fillpanel_.GetContainer("msgarea/msgpanel"), msgpanel_config_);    
	msgpanel_.OnCommand.Attach(
		function(cmd, msg)
		{
			if (cmd == "AddFriend")
			{
				if (msg.Sender.ID != Core.Session.GetUserID())
				{
					Core.Session.GetGlobal("SingletonForm").ShowAddFriendForm(msg.Sender.Name);
				}
			}
			else if (cmd == "Chat")
			{
				if (msg.Sender.ID != Core.Session.GetUserID())
				{
					Core.Session.GetGlobal("ChatService").Open(msg.Sender.ID);
				}
			}
			else if (cmd == "Quote")
			{
				msg = msgpanel_.TranslateMessage(msg);
		
				var html = String.format(
					msgpanel_.GetMessageHtmlFormat(), 
					Core.Utility.IsNull(msg.Sender.Nickname, msg.Sender.Name), 
					(msg.CreatedTime == undefined ? new Date() : msg.CreatedTime).format("yyyy-MM-dd hh:mm:ss"), 
					msg.Content
				);
				html = String.format("<div class='message_quote'><div class='message'>{0}</div></div><br/>", html);
				editor_.AppendHtml(html);
			}
		}
	);

	btnok_ = new Core.UI.Button(fillpanel_.GetContainer("bottompanel/btnok_container"), { Text: "发&nbsp;送", TabIndex: 1 });
	function btnok_click()
	{
		var t = fillpanel_.GetContainer("msgarea");
		if (IsEmpty(editor_.GetText()))
		{
			Core.Utility.ShowError("消息不能为空！");
			return;
		}
		SendInstantMessage(
			editor_.GetText(), true, 
			function()
			{
				editor_.Focus();
			}
		);
	}
	btnok_.OnClick.Attach(btnok_click);

	function ToolbarUploadFileHandler(id_, file_)
	{
		var name = Core.Path.GetFileName(file_.replace(/\x5C/ig, '/'));

		var html_fmt = "<div class='dl_image_dl'></div>" +
		"<div class='dl_text'>" +
			"<span class='span_normal'>正在上传文件 \"{0}\" </span>" +
			"<a>取消</a>" +
		"</div>";

		var html = String.format(html_fmt, name);
		var dom = msgpanel_.CreateElement("DIV");
		dom.className = 'message_file_dl';
		dom.innerHTML = html;

		var info_dom = dom.childNodes[1].childNodes[0];
		var img_dom = dom.childNodes[0];
		var btncancel = dom.childNodes[1].childNodes[1];

		msgpanel_.AppendChild(dom);

		btncancel.onclick = function()
		{
			btnsendfile_.Cancel(id_);
		}

		this.OnCancel = function(path)
		{
			info_dom.innerHTML = String.format("已取消上传文件 \"{0}\"", name);
			img_dom.className = "dl_image_error";
			btncancel.style.display = 'none';
		}

		this.OnCompleted = function(path)
		{
			info_dom.innerHTML = String.format("文件 \"{0}\" 上传完毕", name);
			img_dom.className = "dl_image_file";
			btncancel.style.display = 'none';

			var html = CreateFileHtml([path]);
			SendInstantMessage(html);
		}

		this.OnError = function(ex)
		{
			info_dom.innerHTML = String.format("上传文件\"{0}\"出错：{1}", name, ex.Message);
			img_dom.className = "dl_image_error";
			btncancel.style.display = 'none';
		}
	}

	function ToolbarUploadImageHandler(id_, file_)
	{
		var name = Core.Path.GetFileName(file_.replace(/\x5C/ig, '/'));

		var html_fmt = "<div class='dl_image_dl'></div>" +
		"<div class='dl_text'>" +
			"<span class='span_normal'>正在上传图片 \"{0}\" </span>" +
			"<a>取消</a>" +
		"</div>";

		var html = String.format(html_fmt, name);
		var dom = msgpanel_.CreateElement("DIV");
		dom.className = 'message_file_dl';
		dom.innerHTML = html;

		var info_dom = dom.childNodes[1].childNodes[0];
		var img_dom = dom.childNodes[0];
		var btncancel = dom.childNodes[1].childNodes[1];

		msgpanel_.AppendChild(dom);

		btncancel.onclick = function()
		{
			btnsendimage_.Cancel(id_);
		}

		this.OnCancel = function(path)
		{
			info_dom.innerHTML = String.format("已取消上传图片 \"{0}\"", name);
			img_dom.className = "dl_image_error";
			btncancel.style.display = 'none';
		}

		this.OnCompleted = function(path)
		{
			info_dom.innerHTML = String.format("图片\"{0}\"上传完毕", name);
			img_dom.className = "dl_image_file";
			btncancel.style.display = 'none';
			
			editor_.AppendHtml(String.format("<img src=\"{0}\"/>", Core.CreateDownloadUrl(path)));
		}

		this.OnError = function(ex)
		{
			info_dom.innerHTML = String.format("上传图片\"{0}\"出错：{1}", name, ex.Message);
			img_dom.className = "dl_image_error";
			btncancel.style.display = 'none';
		}
	}
		
	var toolbar_config = {
		Items: [
			{
				ID: "SendFile",
				Type: ClientMode ? "Button" : "UploadButton",
				Css: ClientMode ? "image_text_button btnsendfile" : "fbtnsendfile",
				Config: { 
					Text: "发送文件",
					OnRequestUpload: function(id, file)
					{
						return new ToolbarUploadFileHandler(id, file);
					}
				}
			},
			{
				ID: "SendImage",
				Type: ClientMode ? "Button" : "UploadButton",
				Css: ClientMode ? "image_text_button btnsendimage" : "fbtnsendimage",
				Config: { 
					Text: "发送图片",
					OnRequestUpload: function(id, file)
					{
						return new ToolbarUploadImageHandler(id, file);
					}
				}
			},
			{
				ID: "SendWebImage",
				Type: "Button",
				Css: "image_text_button btnsendwebimage",
				Config: { 
					Text: "发送网络图片"
				}
			},
			{
				ID: "GradScreen",
				Type: "Button",
				Css: "image_text_button btngradscreen",
				Config: { 
					Text: "截图"
				}
			},
			{
				ID: "MsgRecord",
				Type: "Button",
				Css: "image_text_button btnmsgrecord",
				Config: { 
					Text: "消息历史"
				}
			},
			{
				ID: "AddFriend",
				Type: "Button",
				Css: "image_text_button btnaddfriend",
				Config: { 
					Text: "加为好友"
				}
			},
			{
				ID: "ExitTempGroup",
				Type: "Button",
				Css: "image_text_button btnexittempgroup",
				Config: { 
					Text: "退出"
				}
			},
			{
				ID: "AddToTempGroup",
				Type: "Button",
				Css: "image_text_button btnaddtotempgroup",
				Config: { 
					Text: "添加"
				}
			}
		]
	};
	toolbar_ = new Core.UI.Toolbar(fillpanel_.GetContainer("toolbar"), toolbar_config);
	toolbar_.SetItemVisible("SendWebImage", false);
	toolbar_.SetItemVisible("MsgRecord", peer_.Type != 1 || peer_.SubType != 3);
	toolbar_.SetItemVisible("AddFriend", false);
	toolbar_.SetItemVisible("ExitTempGroup", peer_.Type == 1 && peer_.IsTemp == 1);
	toolbar_.SetItemVisible("AddToTempGroup", peer_.Type == 1 && peer_.IsTemp == 1);
	toolbar_.SetItemVisible("GradScreen", ClientMode);

	btnsendfile_ = toolbar_.GetItem("SendFile");
	btnsendimage_ = toolbar_.GetItem("SendImage");
	
	var editor_conf_ = {
		EmotUrlFmt: Core.GetUrl("Themes/Default/emot/e{0}.gif"),
		BeforeSetFormat: function(cmd, data)
		{
			if(editor_.GetSelection() == "")
			{
				if(cmd == "Bold" || cmd == "Italic" || cmd == "Underline")
				{
					DefaultFormat[cmd] = !DefaultFormat[cmd];
				}
				else
				{
					DefaultFormat[cmd] = data;
				}
				editor_.SetDefaultFormat(DefaultFormat);
				SaveDefaultFormat();
				return true;
			}
			else
			{
				return false;
			}
		}
	};
	editor_ = new Core.UI.RichEditor(fillpanel_.GetContainer("msgeditor"), editor_conf_);	
	editor_.LoadCss(Core.GetUrl("Themes/Default/EditorCss.css"));	

	LoadDefaultFormat();
	editor_.SetDefaultFormat(DefaultFormat);
	
	toolbar_.OnCommand.Attach(
		function(cmd)
		{
			if(config.ToolbarCommandHandlers != undefined && config.ToolbarCommandHandlers[cmd] != undefined)
			{
				config.ToolbarCommandHandlers[cmd]();
				return;
			}
			if(cmd == "SendImage")
			{
				if (ClientMode)
				{
					var file = window.external.OpenFile("图片文件|*.png;*.gif;*.jpg;*.jpeg;*.bmp;*.jpe");
					if (file != "")
					{
						var html = String.format("<img src=\"file:///{0}\"/>", file);
						editor_.AppendHtml(html);
					}
				}
			}
			else if (cmd == "SendFile")
			{
				if (ClientMode)
				{
					var file = window.external.OpenFile("");
					if (file != "")
					{
						if (peer_.Type == 0 && peer_.IsTemp == 0 && window.external.GetTotalSize(file) > window.external.LocalSetting.AttachSize * 1024 * 1024)
						{
							SendLocalFiles([file]);
						}
						else
						{
							msgpanel_.UploadFile(file,
								function(path)
								{
									if (path == "") return;
									var html = CreateFileHtml([path]);
									SendInstantMessage(html);
								}
							);
						}
					}
				}
			}
			else if (cmd == "GradScreen")
			{
				if (ClientMode)
				{
					var file = window.external.GradScreen();
					if (file != "")
					{
						var html = String.format("<img src=\"file:///{0}\"/>", file);
						editor_.AppendHtml(html);
					}
				}
			}
			else if (cmd == "MsgRecord")
			{
				Core.Session.GetGlobal("SingletonForm").ShowMsgManagerForm(peer_);
			}
			else if (cmd == "ExitTempGroup")
			{
				if (!confirm("您确定要退出多人会话？")) return;

				CurrentWindow.Waiting("正在退出多人会话...");
				var data = {
					Action: "ExitTempGroup",
					GroupID: peer_.ID
				};
				Core.SendCommand(
					function(ret)
					{
						CurrentWindow.Completed();
						setTimeout(function() { CurrentWindow.Close(); }, 10);
					},
					function(ex)
					{
						CurrentWindow.Completed();
						Core.Utility.ShowError(ex);
					},
					Core.Utility.RenderJson(data), "Core.Web Common_CH", false
				);
			}
			else if (cmd == "AddToTempGroup")
			{
				var wndconf = {
					Left: 0,
					Top: 0,
					Width: 450,
					Height: 450,
					MinWidth: 450,
					MinHeight: 450,
					Title: {
						InnerHTML: "请选择要添加到当前多人会话的人员..."
					},
					Resizable: false,
					HasMaxButton: false,
					HasMinButton: false,
					AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
				};

				var form = Core.CreateWindow(wndconf);
				form.SetTag({ IDS: "" });
				form.MoveEx('CENTER', 0, 0, true);
				form.ShowDialog(
					CurrentWindow, 'CENTER', 0, 0, true,
					function()
					{
						if (form.GetTag().IDS != "")
						{
							var data = {
								Action: "AddToTempGroup",
								IDS: form.GetTag().IDS,
								GroupID: peer_.ID
							};
							CurrentWindow.Waiting("正在加入临时会话...");
							Core.SendCommand(
							function(ret)
							{
								CurrentWindow.Completed();
							},
							function(ex)
							{
								CurrentWindow.Completed();
								Core.Utility.ShowError(ex);
							},
							Core.Utility.RenderJson(data), "Core.Web Common_CH", false);
						}
					}
				);
				var url = Core.GetUrl("SelectUsersForm.htm");
				form.Load(url, null);
			}
		}
	);
	
	editor_.OnKeyDown.Attach(
		function(evt)
		{
			if (evt == undefined) evt = editor_window.event;
			if (evt.keyCode == 13 && !evt.ctrlKey && !evt.altKey && !evt.shiftKey)
			{
				btnok_click();
				Core.Utility.CancelBubble(evt);
				Core.Utility.PreventDefault(evt);
			}
		}
	);
	
	editor_.OnKeyUp.Attach(
		function(evt)
		{
			if (evt == undefined) evt = editor_window.event;
			if (evt.keyCode == 13 && !evt.ctrlKey && !evt.altKey && !evt.shiftKey)
			{
				Core.Utility.CancelBubble(evt);
				Core.Utility.PreventDefault(evt);
			}
		}
	);
		 
	if (ClientMode)  
	{
		editor_.OnKeyDown.Attach(
			function(evt)
			{
				if (evt == undefined) evt = editor_window.event;
				if (evt.keyCode == 86 && evt.ctrlKey && !evt.altKey && !evt.shiftKey)
				{
					var image = window.external.GetClipboardImage();
					if (image != "")
					{
						var src = String.format("file:///{0}", image);
						editor_.InsertImage(src);
						Core.Utility.CancelBubble(evt);
						Core.Utility.PreventDefault(evt);
					}
				}
			}
		);
	}
	
	this_.Focus = function()
	{
		editor_.Focus();
	}
	
	this_.AddMessage = function(msg)
	{
		if(msgpanel_ != null) msgpanel_.AddMessage(msg);
		CurrentWindow.Notify();
		if (!ClientMode)
		{
			Core.main.Core.Utility.ScrollTitle(
				String.format("\"{0}\" 来消息了", 
				msg.Receiver.Type == 0 ? msg.Sender.Nickname: msg.Receiver.Nickname)
			);
		}
	}
	
	this_.ShowLeaveTip = function(visible)
	{
		if(visible)
		{
			var tip = String.format("\"{0}\"已离开，可能无法立刻回复消息！", peer_.Nickname);
			leavetip_container_.firstChild.innerHTML = tip;
		}
		leavetip_.SetVisible(visible);
	}
	
	this_.AppendElement = function(tagName)
	{
		var dom = msgpanel_.CreateElement(tagName);
		msgpanel_.AppendChild(dom);
		return dom;
	}

	if (ClientMode)
	{
		CurrentWindow.OnDropFiles.Attach(
			function(files)
			{
				config.Owner.ShowChatTab();
				if (peer_.Type == 0 && peer_.IsTemp == 0 && window.external.GetTotalSize(files) > window.external.LocalSetting.AttachSize * 1024 * 1024)
				{
					SendLocalFiles(files.split('\x00'));
				}
				else
				{
					SendFiles(files.split('\x00'), 0, []);
				}
			}
		);
		if (peer_.Type == 0)
		{
			CurrentWindow.OnDrop.Attach(
				function(type, data)
				{
					var userInfo = Core.Utility.ParseJson(data);
					if (userInfo.ID != peer_.ID && userInfo.ID != user_.ID)
					{
						var ids = [];
						ids.push(user_.ID);
						ids.push(peer_.ID);
						ids.push(userInfo.ID);
		
						var data = {
							Action: "CreateTempGroup",
							Members: ids.join(",")
						};
						CurrentWindow.Waiting("正在创建临时会话...");
						Core.SendCommand(
						function(ret)
						{
							CurrentWindow.Completed();
							Core.Session.GetGlobal("ChatService").Open(ret.GroupInfo.ID, false);
						},
						function(ex)
						{
							CurrentWindow.Completed();
							Core.Utility.ShowError(ex);
						},
						Core.Utility.RenderJson(data), "Core.Web Common_CH", false);
					}
				}
			);
		}
		else if (peer_.Type == 1 && peer_.IsTemp)
		{
			CurrentWindow.OnDrop.Attach(
				function(type, data)
				{
					var userInfo = Core.Utility.ParseJson(data);
					if (userInfo.ID != user_.ID)
					{
						var data = {
							Action: "AddToTempGroup",
							IDS: userInfo.ID,
							GroupID: peer_.ID
						};
						CurrentWindow.Waiting("正在加入临时会话...");
						Core.SendCommand(
						function(ret)
						{
							CurrentWindow.Completed();
						},
						function(ex)
						{
							CurrentWindow.Completed();
							Core.Utility.ShowError(ex);
						},
						Core.Utility.RenderJson(data), "Core.Web Common_CH", false);
					}
				}
			);
		}
	}
}

})();