(function() {

var MsgUserTreeDS = (function()
{
	var _users = null;

	function GetMessageRecordUsers(callback)
	{
		if (_users != null)
		{
			callback(_users);
			return;
		}

		var data = {
			Action: "GetMessageRecordUsers"
		};
		Core.SendCommand(
			function(ret)
			{
				_users = ret.Users;
				callback(ret.Users);
			},
			function(ex)
			{
				callback(null, ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Common_CH", false
		);
	}

	function HasVisitor(users)
	{
		for (var k in users)
		{
			if (users[k].IsTemp == 1 && users[k].Type == 0) return true;
		}
		return false;
	}

	var obj = {};

	obj.GetSubNodes = function(callback, node)
	{
		GetMessageRecordUsers(
			function(users)
			{
				if (node == null)
				{
					var nodes = [
						{
							Name: "Users",
							Text: "联系人",
							Tag: null,
							ImageCss: "Image16_Folder"
						},
						{
							Name: "Groups",
							Text: "群组",
							Tag: null,
							ImageCss: "Image16_Folder"
						},
						{
							Name: "TempGroups",
							Text: "多人会话",
							Tag: null,
							ImageCss: "Image16_Folder"
						}
					];
					if (HasVisitor(users))
					{
						nodes.push(
							{
								Name: "Visitors",
								Text: "访客",
								Tag: null,
								ImageCss: "Image16_Folder"
							}
						);
					}
					callback(nodes);
				}
				else
				{
					var nodes = [];
					var fullPath = node.GetFullPath();
					if (fullPath == "/Users")
					{
						for (var k in users)
						{
							var user = users[k];
							if (user.Type == 0 && user.IsTemp == 0)
							{
								var n = {
									Name: user.ID.toString(),
									Text: String.format("{0}({1})", user.Nickname, user.IsDeleted == 0 ? user.Name: "已删除"),
									Tag: user,
									HasChildren: false,
									ImageSrc: Core.CreateHeadImgUrl(user.ID, 16, false, user.HeadImg)
								};
								nodes.push(n);
							}
						}
					}
					else if (fullPath == "/Groups")
					{
						for (var k in users)
						{
							var user = users[k];
							if (user.Type == 1 && user.IsTemp == 0)
							{
								var n = {
									Name: user.ID.toString(),
									Text: user.Nickname,
									Tag: user,
									HasChildren: false,
									ImageCss: "Image16_Group"
								};
								nodes.push(n);
							}
						}
					}
					else if (fullPath == "/TempGroups")
					{
						for (var k in users)
						{
							var user = users[k];
							if (user.Type == 1 && user.IsTemp == 1)
							{
								var n = {
									Name: user.ID.toString(),
									Text: user.Nickname,
									Tag: user,
									HasChildren: false,
									ImageCss: "Image16_Group"
								};
								nodes.push(n);
							}
						}
					}
					else if (fullPath == "/Visitors")
					{
						for (var k in users)
						{
							var user = users[k];
							if (user.Type == 0 && user.IsTemp == 1)
							{
								var n = {
									Name: user.ID.toString(),
									Text: user.Nickname,
									Tag: user,
									HasChildren: false,
									ImageSrc: Core.CreateHeadImgUrl(user.ID, 16, false, user.HeadImg)
								};
								nodes.push(n);
							}
						}
					}
					callback(nodes);
				}
			}
		);
	}

	obj.Load = function(callback, cache)
	{
		if (cache != undefined && !cache) _users = null;
		GetMessageRecordUsers(callback);
	}

	return obj;
})();

Core.UI.MsgUserTree = function(container, config)
{
	var this_ = this;
	
	config.DataSource = MsgUserTreeDS;
	Core.UI.TreeView.call(this_, container, config);
	
	this_.Load = function(callback, cache)
	{
		MsgUserTreeDS.Load(callback, cache);
	}
}

var MessageFormat = "<div class='messageTitle'>" +
"<div class='sender'>{0}</div>" +
"<div class='time'>{1}</div>" +
"<div class='operationContainer'></div>" +
"</div>" +
"<div class='messageContent'>{2}</div>";

var FileHtmlFormat = "";
var R_FileHtmlFormat = "";

if (!ClientMode)
{
	FileHtmlFormat = "<div class='div_filename'>文件名:{0}</div>" +
	"<div class='operationContainer'>" +
	"<div class='link_download'><a target='_blank' href='" + Core.GetUrl("Download.ashx") + "?FileName={1}'>下载</a></div>" +
	"</div>";
	R_FileHtmlFormat = "<div class='div_filename'>接收文件请求:{0}</div>" +
	"<div class='operationContainer'>" +
	"<div class='link_download'><a>下载</a></div>" +
	"<div class='link_download'><a>拒绝</a></div>" +
	"</div>" +
	"<iframe style='width:100px;height:0px;'></iframe>";
}
else
{
	FileHtmlFormat = "<div class='div_filename'>文件名:{0}</div>" +
	"<div class='operationContainer'>" +
	"<div class='link_saveas'><span onclick='javascript:window.OpenFile(unescape(\"{1}\"))'>打开</span></div>" +
	"<div class='link_saveas'><span onclick='javascript:window.SaveFile(unescape(\"{1}\"))'>另存为</span></div>" +
	"</div>";
	R_FileHtmlFormat = "<div class='div_filename'>接收文件请求:{0}</div>" +
	"<div class='operationContainer'>" +
	"<div class='link_saveas'><span>打开</span></div>" +
	"<div class='link_saveas'><span>另存为</span></div>" +
	"<div class='link_saveas'><span>拒绝</span></div>" +
	"</div>" +
	"<iframe style='width:100px;height:0px;opacity: 0;'></iframe>";
}

//格式化数字
function FormatNumber(num, length)
{
	var s = num.toString();
	var zero = "";
	for (var i = 0; i < length - s.length; i++) zero += "0";
	return zero + s;
}

//时间转字符串
function DateToString(date)
{
	return String.format("{0}-{1}-{2} {3}:{4}:{5}", FormatNumber(date.getFullYear(), 4), FormatNumber(date.getMonth() + 1, 2), FormatNumber(date.getDate(), 2), FormatNumber(date.getHours(), 2), FormatNumber(date.getMinutes(), 2), FormatNumber(date.getSeconds(), 2));
}

Core.UI.MsgPanel = function(container, config)
{
	var this_ = this;
	
	var div_ = document.createElement("DIV");
	div_.className = "msgpanel_body";
	container.appendChild(div_);
	
	var frame_ = document.createElement("IFRAME");
	frame_.frameBorder = 0;
	frame_.width = "100%";
	frame_.height = "100%";
	frame_.style.position = "absolute";
	frame_.style.width = "100%";
	frame_.style.height = "100%";
	div_.appendChild(frame_);
	
	frame_.contentWindow.document.write("<html><head></head><body></body></html>");
	var frame_doc_ = frame_.contentWindow.document;
	var frame_window_ = frame_.contentWindow;
	
	if(window.ClientMode != true)
	{
		Core.Utility.AttachEvent(
			frame_doc_, "mousedown",
			function()
			{
				window.CurrentWindow.BringToTop();
			}
		);
	}

	var css = Core.GetUrl("Themes/Default/EditorCss.css");
	Core.Utility.LoadCss(css, frame_doc_);

	var msg_css = Core.Utility.IsNull(config.MessageCss, "message");
	
	this_.OnCommand = new Core.Delegate();    
	this_.AfterAddMessage = new Core.Delegate();
	
	frame_doc_.oncontextmenu = function()
	{
		if (Core.GetBrowser() == "IE")
		{
			var r = frame_doc_.selection.createRange();
			return r.htmlText != undefined && r.htmlText != "";
		}
		else if (Core.GetBrowser() == "Firefox" || Core.GetBrowser() == "Chrome")
		{
			var sel = this_.GetWindow().getSelection();
			return sel.rangeCount > 0;
		}
		else
		{
			return false;
		}
	}	
	
	this_.Clear = function()
	{
		frame_doc_.body.innerHTML = "";
	}
	
	this_.CreateMessage = function(content)
	{
		content = content.replace(/<img\s[^<>]*>/ig,
			function(val)
			{
				return String.format("<img src='{0}'/>", Core.GetUrl("Themes/Default/Images/loading_img.gif"));
			}
		);
	
		content = content.replace(/\x5BFILE\x3A\s*\{Accessory\s+type=\"file\"\s+src=\"([^\t\n\r\f\v\x5B\x5D]+)\"\}\x5D/ig,
			function(val, file)
			{
				return String.format("<div class='div_filename'><img src='{0}'/></div>", Core.GetUrl("Themes/Default/Images/loading_file.gif"), file);
			}
		);
	
		var msgDiv = frame_doc_.createElement("DIV");
		msgDiv.className = msg_css;
		msgDiv.innerHTML = String.format(
			"<div class='messageTitle'>" + 
				"<div class='sender'>{0}</div>" + 
				"<div class='time'>{1}</div>" + 
				"<div class='operationContainer' style='display:none;'>" + 
				"</div>" + 
			"</div>" + 
			"<div class='messageContent'>{2}</div>", 
			config.User.Nickname, DateToString(new Date()), content
		);
		frame_doc_.body.appendChild(msgDiv);
		frame_doc_.body.scrollTop = frame_doc_.body.scrollHeight;
		
		return msgDiv;
	}
	
	this_.TranslateMessage = function(msg)
	{
		msg.Content = msg.Content.replace(
			/\x5BFILE\x3A([^\t\n\r\f\v\x5B\x5D]+)\x5D/ig,
			function(val, file)
			{
				return String.format(FileHtmlFormat, Core.Path.GetFileName(unescape(file)), file);
			}
		);
	
		msg.Content = msg.Content.replace(
			/\x5BRFILE\x3A([^\t\n\r\f\v\x5B\x5D]+)\x5D/ig,
			function(val, file)
			{
				return String.format(R_FileHtmlFormat, Core.Path.GetFileName(unescape(file)), file);
			}
		);
	
		if (ClientMode)
		{
			msg.Content = msg.Content.replace(
				/<img [^\t\n\r\f\v<>]+>/ig,
				function(img)
				{
					return img.replace(
						/src[^<>]*=[^<>]*(\x22|\x27)([^<>]+\/|)Download.ashx\x3FFileName=([^\t\n\r\f\v\x22]+)(\x22|\x27)/ig,
						function(text, v1, v2, src)
						{
							var url = Core.CreateDownloadUrl(src);
							return String.format("src=\"{0}\"", url);
						}
					);
				}
			);
		}
		msg.Content = msg.Content.replace(
			/<img [^\t\n\r\f\v<>]+>/ig,
			function(img)
			{
				return img.replace(
					/src[^<>]*=[^<>]*(\x22|\x27)([^<>]+\/|)Download.aspx\x3FFileName=([^\t\n\r\f\v\x22]+)(\x22|\x27)/ig,
					function(text, v1, v2, src)
					{
						var url = Core.CreateDownloadUrl(src);
						return String.format("src=\"{0}\"", url);
					}
				);
			}
		);
	
		msg.Content = msg.Content.replace(/\x5BA\x3A[^\t\n\r\f\v\x5B\x5D]+\x5D/ig,
			function(val)
			{
				var file = val.substr(3, val.length - 4);
				return String.format("<a target='_blank' href='{0}' onclick='return window.OpenLink(this.href);'>", unescape(file));
			}
		);
	
		msg.Content = msg.Content.replace(
			/\x5B\x2FA\x5D/ig,
			function(val)
			{
				return "</a>";
			}
		);
	
		return msg;
	}
	
	this_.AddMessage = function(msg, temp, custom)
	{
		if (msg.IsTemp != undefined && msg.IsTemp)
		{
			var data = Core.Utility.ParseJson(msg.Content);
			if (data.Action == "RecvFiles")
			{
				for (var i in data.URLS)
				{   
					(function(url)
					{
						var msgDiv = frame_doc_.createElement("DIV");
						msgDiv.className = msg_css;
						msgDiv.innerHTML = "<div class='messageContent'><div contenteditable='false' class='message_file'></div></div>";
	
						var dom = msgDiv.firstChild.firstChild;
						dom.innerHTML = String.format(R_FileHtmlFormat, Core.Path.GetFileName(url), url);
	
						if (ClientMode)
						{
							dom.childNodes[1].childNodes[0].firstChild.onclick = function()
							{
								setTimeout(
									function()
									{
										window.external.Open(document.cookie, url, Core.Path.GetFileName(url), new DownloadHandler(Core.Path.GetFileName(url)));
									}, 10
								);
								msgDiv.style.display = "none";
							}
							dom.childNodes[1].childNodes[1].firstChild.onclick = function()
							{
								setTimeout(
									function()
									{
										window.external.Save(document.cookie, url, Core.Path.GetFileName(url), new DownloadHandler(Core.Path.GetFileName(url)));
									}, 10
								);
								msgDiv.style.display = "none";
							}
							dom.childNodes[1].childNodes[2].firstChild.onclick = function()
							{
								dom.childNodes[2].src = url + "/cancel";
								msgDiv.style.display = "none";
							}
						}
						else
						{
							dom.childNodes[1].childNodes[0].firstChild.onclick = function()
							{
								dom.childNodes[2].src = url;
								msgDiv.style.display = "none";
							}
							dom.childNodes[1].childNodes[1].firstChild.onclick = function()
							{
								dom.childNodes[2].src = url + "/cancel";
								msgDiv.style.display = "none";
							}
						}
						this_.AppendChild(msgDiv);
					})(data.URLS[i]);
				}
			}
			return null;
		}
		else
		{
			msg = this_.TranslateMessage(msg);
	
			var msgDiv = frame_doc_.createElement("DIV");
			msgDiv.className = msg_css;
			msgDiv.innerHTML = String.format(
				MessageFormat, Core.Utility.IsNull(msg.Sender.Nickname, msg.Sender.Name), 
				DateToString(msg.CreatedTime == undefined ? new Date() : msg.CreatedTime), 
				msg.Content, Core.GetUrl("Themes/Default/Images/user.png")
			);
	
			var operations = null;
	
			if (config.Operations != undefined) operations = config.Operations;
			if (config.GetOperations != undefined) operations = config.GetOperations(msg);
	
			if (operations != null)
			{
				var opeContainer = msgDiv.firstChild.childNodes[2];
				opeContainer.style.display = "none";
	
				msgDiv.onmousemove = function()
				{
					opeContainer.style.display = "inline";
				}
	
				msgDiv.onmouseout = function()
				{
					opeContainer.style.display = "none";
				}
	
				var opeHtml = "";
				for (var i in operations)
				{
					var opeA = frame_doc_.createElement("A");
					opeA.innerHTML = operations[i].Text;
					opeContainer.appendChild(opeA);
	
					(function(command, msg, opeA, div)
					{
						opeA.onclick = function()
						{
							this_.OnCommand.Call(command, msg, div);
						}
					})(operations[i].Command, msg, opeA, msgDiv);
				}
	
				if (custom != undefined)
				{
					msgDiv.appendChild(custom);
				}
			}
			try
			{
				msgDiv.Message = msg;
			}
			catch(ex)
			{
				//
			}
			try
			{
				if (temp == undefined) frame_doc_.body.appendChild(msgDiv);
				else frame_doc_.body.replaceChild(msgDiv, temp);
			}
			catch(ex)
			{
				//
			}
	
			//msgDiv.scrollIntoView();
			if (temp == undefined || temp == null)
			{
				frame_doc_.body.scrollTop = frame_doc_.body.scrollHeight;
			}
	
			this_.AfterAddMessage.Call(msg, msgDiv);
	
			return msgDiv;
		}
   }
	
	this_.GetAllMessages = function(callback)
	{
		for (var i = 0; i < frame_doc_.body.childNodes.length; i++)
		{
			var n = frame_doc_.body.childNodes[i];
			if (Core.Utility.ContainsCss(n, "message"))
			{
				callback(n);
			}
		}
	}
	
	this_.AppendChild = function(elem)
	{
		frame_doc_.body.appendChild(elem);	    
		frame_doc_.body.scrollTop = frame_doc_.body.scrollHeight;	    
		msgDiv = null;
	}

	this_.GetBody = function()
	{
		return frame_doc_ != null ? frame_doc_.body: null;
	}

	frame_doc_.onkeydown = function(e)
	{
		var evt = new Core.Event(e, frame_window_);
		if (evt.GetEvent().keyCode == 116 || (evt.GetEvent().ctrlKey && evt.GetEvent().keyCode == 82))
		{
			evt.GetEvent().keyCode = 0;
			evt.GetEvent().returnValue = false;
			return false;
		}
		if (evt.GetEvent().keyCode == 70 && evt.GetEvent().ctrlKey && !evt.GetEvent().altKey && !evt.GetEvent().shiftKey)
		{
			evt.GetEvent().keyCode = 0;
			evt.GetEvent().returnValue = false;
			return false;
		}
	}

	function UploadHandler(file, afterUpload)
	{
		var _ctr_id = Core.GenerateUniqueId();
		var _ctr = null;
	
		var html_format = "<div class='dl_image_dl'></div>" +	    
		"<div class='dl_text'>" +	    
		"<span class='span_normal'>正在上传 \"{0}\"...</span>" +	    
		"<span class='processing'></span>" +	    
		"<a>取消</a>" +	    
		"</div>";
	
		var html = String.format(html_format, file, Core.GetUrl("Themes/Default/Images/dl_file.gif"));
	
		var dom = frame_doc_.createElement("DIV");
		dom.className = 'message_file_dl';
		dom.innerHTML = html;
	
		var info_dom = dom.childNodes[1].childNodes[0];
		var pdom = dom.childNodes[1].childNodes[1];
		var a_dom = dom.childNodes[1].childNodes[2];
		var img_dom = dom.childNodes[0];
	
		var _isCanceled = false;
		a_dom.onclick = function()
		{
			_isCanceled = true;
		}
	
		this.Cancel = function()
		{
			_isCanceled = true;
			if (_ctr != null) _ctr.Cancel();
		}
	
		var _recv = 0,
		_total = 0;
	
		this.BeforeUpload = function(ctr)
		{
			_ctr = ctr;
			_dl_handlers[_ctr_id] = this;
			this_.AppendChild(dom);
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
			pdom.innerHTML = String.format("上传速度:{2}KB/s 已完成:{0}%, 共 {1}", (Math.round(length / size * 1000) / 10), temp, Math.round(speed / 10) / 100);
			return (_dispose || _isCanceled) ? 0 : 1;
		}
	
		this.AfterUpload = function(path)
		{
			info_dom.innerHTML = String.format(_recv == _total ? "\"{0}\" 上传完毕": "\"{0}\" 上传已取消！", file);
			info_dom.className = (_recv == _total ? "span_normal": "span_red");
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			img_dom.className = (_recv == _total ? "dl_image_file": "dl_image_cancel");
			delete _dl_handlers[_ctr_id];
			if (afterUpload != undefined) afterUpload(path);
		}
	
		this.HandleError = function(msg)
		{
			info_dom.className = "span_red";
			info_dom.innerHTML = String.format("上传 \"{0}\" 时发生错误: {1}", file, msg);
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			img_dom.className = "dl_image_error";
			delete _dl_handlers[_ctr_id];
		}
	}

	this_.UploadFile = function(file, afterUpload)
	{
		if (ClientMode)
		{
			var handler = new UploadHandler(Core.Path.GetFileName(file), afterUpload);
			window.external.Upload(document.cookie, file, handler);
		}
	}

	var _dl_handlers = {};

	function DownloadHandler(file)
	{
		var _ctr_id = Core.GenerateUniqueId();
	
		var _ctr = null;
	
		var html = String.format(
			"<div class='dl_image_dl'></div><div class='dl_text'><span class='span_normal'>正在下载 \"{0}\"...</span><span class='processing'></span><a>取消</a></div>", 
			file, Core.GetUrl("Themes/Default/Images/dl_file.gif")
		);
		var dom = frame_doc_.createElement("DIV");
		dom.className = 'message_file_dl';
		dom.innerHTML = html;
	
		var info_dom = dom.childNodes[1].childNodes[0];
		var pdom = dom.childNodes[1].childNodes[1];
		var a_dom = dom.childNodes[1].childNodes[2];
		var img_dom = dom.childNodes[0];
	
		var _isCanceled = false;
		a_dom.onclick = function()
		{
			_isCanceled = true;
			if (_ctr != null)
			{
				_ctr.Cancel();
				_ctr = null;
			}
		}
	
		var _recv = 0,
		_total = 0;
	
		this.Cancel = function()
		{
			_isCanceled = true;
			if (_ctr != null) _ctr.Cancel();
		}
	
		this.BeforeDownload = function(ctr)
		{
			_ctr = ctr;
			_dl_handlers[_ctr_id] = this;
			this_.AppendChild(dom);
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
			pdom.innerHTML = String.format("下载速度:{2}KB/s 已完成:{0}%, 共 {1}", (Math.round(length / size * 1000) / 10), temp, Math.round(speed / 10) / 100);
			return (_dispose || _isCanceled) ? 0 : 1;
		}
	
		this.AfterDownload = function()
		{
			info_dom.innerHTML = String.format(_recv == _total ? "\"{0}\" 下载完毕": "\"{0}\" 下载已取消！", file);
			info_dom.className = (_recv == _total ? "span_normal": "span_red");
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			img_dom.className = (_recv == _total ? "dl_image_file": "dl_image_cancel");
			delete _dl_handlers[_ctr_id];
		}
	
		this.HandleError = function(msg)
		{
			info_dom.className = "span_red";
			info_dom.innerHTML = String.format("下载 \"{0}\" 时发生错误: {1}", file, msg);
			pdom.innerHTML = "";
			a_dom.style.display = 'none';
			img_dom.className = "dl_image_error";
			delete _dl_handlers[_ctr_id];
		}
	}

	var _dispose = false;

	CurrentWindow.OnClosed.Attach(
		function()
		{
			if (ClientMode)
			{
				for (var k in _dl_handlers)
				{
					try
					{
						_dl_handlers[k].Cancel();
						delete _dl_handlers[k];
					}
					catch(ex)
					{
						//
					}
				}
				_dispose = true;
			}
		}
	);

	this_.GetMessageHtmlFormat = function()
	{
		return MessageFormat;
	}
	
	this_.CreateElement = function(tagName)
	{
		return frame_doc_.createElement(tagName);
	}
	
	this_.ScrollToBottom = function()
	{
		frame_doc_.body.scrollTop = frame_doc_.body.scrollHeight;
	}
	
	frame_window_.OpenLink = function(filePath)
	{
		return confirm(String.format("您确定要打开\"{0}\"？", filePath));
	}
	
	frame_window_.SaveUrl = function(url)
	{
		if (ClientMode)
		{
			window.setTimeout(
				function()
				{
					window.external.Save(document.cookie, url, Core.Path.GetFileName(url), new DownloadHandler(Core.Path.GetFileName(url)));
				}, 100
			);
		}
	}
	
	frame_window_.OpenUrl = function(url)
	{
		if (ClientMode)
		{
			window.setTimeout(
				function()
				{
					window.external.Open(document.cookie, url, Core.Path.GetFileName(url), new DownloadHandler(Core.Path.GetFileName(url)));
				}, 100
			);
		}
	}
	
	frame_window_.SaveFile = function(filePath)
	{
		if (ClientMode)
		{
			window.setTimeout(
				function()
				{
					var url = Core.GetUrl("Download.ashx") + "?FileName=" + escape(filePath).replace(/\x2B/ig, "%2B");
					window.external.Save(document.cookie, url, Core.Path.GetFileName(filePath), new DownloadHandler(Core.Path.GetFileName(filePath)));
				}, 100
			);
		}
	}
	frame_window_.OpenFile = function(filePath)
	{    
		if (ClientMode)
		{
			window.setTimeout(
				function()
				{
					var url = Core.GetUrl("Download.ashx") + "?FileName=" + escape(filePath).replace(/\x2B/ig, "%2B");
					window.external.Open(document.cookie, url, Core.Path.GetFileName(filePath), new DownloadHandler(Core.Path.GetFileName(filePath)));
				}, 100
			);
		}
	}
}

function MsgSort(m1, m2)
{
	if (m1.CreatedTime > m2.CreatedTime) return - 1;
	if (m1.CreatedTime < m2.CreatedTime) return 1;
	return 0;
}

function ParseDate(str)
{
	var reg = /(\d\d\d\d)(\d\d)(\d\d)/ig;
	var ret = reg.exec(str);
	if(ret == null || ret[0] != str || ret[1] == undefined || ret[2] == undefined || ret[3] == undefined) return null;
	if(!Date.check(parseInt(ret[1]), parseInt(ret[2]), parseInt(ret[3]))) return null;
	return new Date(parseInt(ret[1]), parseInt(ret[2]) - 1, parseInt(ret[3]));
}
	
Core.UI.SearchPanel = function(msgmanager_, searchbar_, searchpanel_, config)
{
	var this_ = this;
	
	if(config == undefined) config = {};
	if(config.ShowUserCombox == undefined) config.ShowUserCombox = false;
	
	this_.BeforeSearch = new Core.Delegate();
	this_.OnBtnHiddenClick = new Core.Delegate();
	
	searchbar_.innerHTML = "<div class='text'>查找范围：</div>" +
	"<div class='search_user'></div>" +
	"<div class='search_time ct_toolbar_combox'></div>" +
	"<input class='search_time_from' type='text' />" +
	"<div class='text'>至</div>" +
	"<input class='search_time_to' type='text' />" +
	"<div class='text'>查找内容：</div>" +
	"<input class='search_content' type='text' />" +
	"<div class='search_btn ct_toolbar_button image_text_button'></div>";
	var search_ctrl_ = null;
	if(config.Peer == undefined)
	{
		if(config.ShowUserCombox)
		{
			Core.Utility.ModifyCss(searchbar_.childNodes[1], "ct_toolbar_combox");
			search_ctrl_ = new Core.UI.Combox(searchbar_.childNodes[1]);
			search_ctrl_.AddItem("全部", 1);
			search_ctrl_.AddItem("当前联系人", 2);
			search_ctrl_.SetValue(1);
		}
		else
		{
			Core.Utility.ModifyCss(searchbar_.childNodes[1], "search_user_block");
			search_ctrl_ = {
				Elem: searchbar_.childNodes[1],
				ID: 0
			};
			search_ctrl_.Elem.innerHTML = "全部";
		}
	}

	var combox_serach_time_ = new Core.UI.Combox(searchbar_.childNodes[2]);
	combox_serach_time_.AddItem("最近一周", 1);
	combox_serach_time_.AddItem("最近三个月", 2);
	combox_serach_time_.AddItem("最近一年", 3);
	combox_serach_time_.AddItem("自定义时间段", 5);
	combox_serach_time_.AddItem("全部", 4);
	combox_serach_time_.SetValue(1);
	
	var search_content_ = searchbar_.childNodes[7];
	var search_btn_ = new Core.UI.Button(searchbar_.childNodes[8], { Text: "立即查找" });
	
	var date_from = searchbar_.childNodes[3], date_to = searchbar_.childNodes[5];
	date_from.title = "日期：例如2013年1月1日，请输入20130101";
	date_to.title = "日期：例如2013年1月1日，请输入20130101";
	
	combox_serach_time_.OnChanged.Attach(
		function()
		{
			if(combox_serach_time_.GetValue() == 5)
			{    
				var now = new Date();
				now = new Date(now.getFullYear(), now.getMonth(), now.getDate());
				var now_7 = new Date();
				now_7.setTime(now.getTime() - 7 * 24 * 60 * 60 * 1000);
				date_from.value = now_7.format("yyyyMMdd");
				date_to.value = now.format("yyyyMMdd");
				
				date_from.style.display = "";
				searchbar_.childNodes[4].style.display = "";
				date_to.style.display = "";
			}
			else
			{
				date_from.style.display = "none";
				searchbar_.childNodes[4].style.display = "none";
				date_to.style.display = "none";
			}
		}
	);
	
	combox_serach_time_.SetValue(1);
	
	var title_ = searchpanel_.GetContainer("title");
	if(title_ != null)
	{
		title_.innerHTML = "<div><span>搜索结果</span><a class='close_link'>关闭</a></div>";
		title_.firstChild.childNodes[1].onclick = function()
		{
			this_.OnBtnHiddenClick.Call();
		}
	}
	
	var pagebar_ctrl_ = searchpanel_.FindControl("pagebar");
	var pagebar_ = searchpanel_.GetContainer("pagebar");
	pagebar_.innerHTML = "<a href='javascript:void(0)'>尾页</a>" +
	"<a href='javascript:void(0)'>下一页</a>" +
	"<span>/1</span>" +
	"<input type='text' class='curpage' value='1' />" +
	"<a href='javascript:void(0)'>上一页</a>" +
	"<a href='javascript:void(0)'>首页</a>";
	
	var pagecount_obj_ = pagebar_.childNodes[2];
	var curpage_obj_ = pagebar_.childNodes[3];
	
	var page_ = -1; 
	var messages_ = [];    
	
	var msgpanel_opes_1 = [
		{
			Text: "查看前后消息",
			Command: "View"
		}
	];  
	
	var msgpanel_opes_2 = [
		{
			Text: "回复",
			Command: "Chat"
		},
		{
			Text: "查看前后消息",
			Command: "View"
		}
	];
	
	var msgpanel_config = {
		GetOperations : function(msg)
		{
			if(msg.Sender.ID != Core.Session.GetUserID())
			{
				return msgpanel_opes_2;
			}
			else
			{
				return msgpanel_opes_1;
			}
		}
	};
	var msgpanel_ = new Core.UI.MsgPanel(searchpanel_.GetContainer("msgpanel"), msgpanel_config);
	
	function GetMessages(msg, refreshTree)
	{    
		CurrentWindow.CompleteAll();    
		CurrentWindow.Waiting("正在载入消息记录...");    
		msgpanel_.Clear();   
		var data = {
			Action: "GetMessages",
			Time: combox_serach_time_.GetValue(),
			Content: String.format("%{0}%", search_content_.value),
			Page: page_,
			PageSize: 20,
			MsgID: 0
		};
		var user = null;
		if(config.Peer == undefined)
		{
			user = msgmanager_.GetCurrentUser != undefined ? msgmanager_.GetCurrentUser() : null;
			if(config.ShowUserCombox)
			{
				data.Peer = (search_ctrl_.GetValue() == 2 && user != null ? user.ID : 0);
			}
			else
			{
				data.Peer = search_ctrl_.ID;
				user = search_ctrl_.User;
			}
		}
		else
		{
			user = config.Peer;
			data.Peer = user.ID;
		}
		if(combox_serach_time_.GetValue() == 5)
		{
			data.From = ParseDate(date_from.value);
			if(data.From == null)
			{
				Core.Utility.ShowError("日期格式错误(例如2013年1月1日，请输入20130101)！");
				date_from.focus();
				CurrentWindow.Completed();
				return;
			}
			data.To = ParseDate(date_to.value);
			if(data.To == null)
			{
				Core.Utility.ShowError("日期格式错误(例如2013年1月1日，请输入20130101)！");
				date_to.focus();
				CurrentWindow.Completed();
				return;
			}
			data.To.setTime(data.To.getTime() + 24 * 60 * 60 * 1000 - 1);
		}
		
		if(title_ != null)
		{
			var search_user = data.Peer == 0 ? "全部联系人" : String.format("{0}({1})", user.Nickname, user.ID);
			title_.firstChild.childNodes[0].innerHTML = String.format("搜索结果 [{0}]", search_user);
		}
		if (msg != undefined && msg != null) data.MsgCreatedTime = msg.CreatedTime;

		Core.SendCommand(
			function(ret)
			{
				msgpanel_.Clear();
				ret.Messages.sort(MsgSort);
				for (var i = ret.Messages.length - 1; i >= 0; i--)
				{
					msgpanel_.AddMessage(ret.Messages[i]);
				}
				messages_ = ret.Messages;
				page_ = ret.Page;
				pagecount_obj_.innerHTML = String.format("/{0}", ret.PageCount < 1 ? 1 : ret.PageCount);
				curpage_obj_.value = ret.Page.toString();
				pagebar_ctrl_.SetVisible(ret.PageCount > 1);
				CurrentWindow.Completed();
			},
			function(ex)
			{
				CurrentWindow.Completed();
				Core.Utility.ShowError(ex.toString());
			},
			Core.Utility.RenderJson(data), "Core.Web Common_CH", false
		);
	}
	
	pagebar_.childNodes[0].onclick = function()
	{
		page_ = -1;
		msgid_ = 0;
		GetMessages();
	}
	
	pagebar_.childNodes[1].onclick = function()
	{
		page_++;
		msgid_ = 0;
		GetMessages();
	}
	
	pagebar_.childNodes[4].onclick = function()
	{
		if(page_ > 1)
		{
			page_--;
			msgid_ = 0;
			GetMessages();
		}
	}
	
	pagebar_.childNodes[5].onclick = function()
	{
		page_ = 1;
		msgid_ = 0;
		GetMessages();
	}
	
	curpage_obj_.onkeydown = function(evt)
	{
		if(evt == undefined) evt = window.event;
		if(evt.keyCode == 13)
		{
			var page = parseInt(curpage_obj_.value);
			if(isNaN(page))
			{
				curpage_obj_.value = (page_ < 1 ? 1 : page_).toString();
			}
			else
			{
				page_ = page;
				msgid_ = 0;
				GetMessages();
			}
		}
	}
	
	search_btn_.OnClick.Attach(
		function()
		{
			if(config.ShowUserCombox && search_ctrl_.GetValue() == 2 && 
				msgmanager_.GetCurrentUser != undefined && msgmanager_.GetCurrentUser() == 0)
			{
				Core.Utility.ShowWarning("请选择一个联系人！");
				return;
			}
			page_ = -1;
			this_.BeforeSearch.Call();
			GetMessages();
		}
	);
	  
	msgpanel_.OnCommand.Attach(
		function(cmd, msg)
		{
			if (cmd == "View")
			{
				msgmanager_.View(msg);
			}
			else if (cmd == "Chat")
			{
				if (msg.Sender.ID != Core.Session.GetUserInfo().ID)
				{
					Core.Session.GetGlobal("ChatService").Open(msg.Sender.ID);
				}
			}
		}
	);
	
	this_.SetVisible = function(visible)
	{
		searchpanel_.SetVisible(visible);
	}
}

Core.UI.MsgManager = function(parent_, config_)
{
	var this_ = this;

	Core.UI.Control.call(this_, parent_, config_);

	var fillpanel_ = new Core.UI.Control(
		this_,
		{
			ID: "fillpanel",
			Css: "fillpanel",
			BorderWidth: 0,
			DockStyle: Core.UI.DockStyle.Fill,
			Controls: [
				{
					ID: "searchbar",
					Css: "searchbar",
					BorderWidth: 0,
					Height: 28,
					DockStyle: Core.UI.DockStyle.Top
				},
				{
					ID: "usertree",
					Css: "ct_treeview",
					BorderWidth: 0,
					Width: 200,
					Resizable: true,
					SplitterSize: 5,
					Visible: config_.Peer == undefined,
					DockStyle: Core.UI.DockStyle.Left
				},
				{
					ID: "rightpanel",
					Css: "rightpanel",
					BorderWidth: 1,
					DockStyle: Core.UI.DockStyle.Fill,
					Controls: [
						{
							ID: "pagebar",
							Css: "pagebar",
							BorderWidth: 0,
							Height: 20,
							Visible: false,
							DockStyle: Core.UI.DockStyle.Bottom
						},
						{
							ID: "msgpanel",
							Css: "msgpanel",
							BorderWidth: 0,
							Height: 20,
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				},
				{
					ID: "searchpanel",
					Css: "searchpanel",
					BorderWidth: 1,
					DockStyle: Core.UI.DockStyle.Fill,
					Visible: false,
					Controls: [
						{
							ID: "title",
							Css: "title",
							BorderWidth: 0,
							Height: 21,
							DockStyle: Core.UI.DockStyle.Top
						},
						{
							ID: "pagebar",
							Css: "pagebar",
							BorderWidth: 0,
							Height: 20,
							Visible: false,
							DockStyle: Core.UI.DockStyle.Bottom
						},
						{
							ID: "msgpanel",
							Css: "msgpanel",
							BorderWidth: 0,
							Height: 20,
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				}
			]
		}
	);

	var peer_ = 0;    
	var to_ = new Date(2100, 0, 1);
	var from_ = new Date(2000, 0, 1);
	var page_ = -1; 
	var msgid_ = 0;
	var messages_ = [];
	
	var msgpanel_opes_1 = [
		{
			Text: "回复",
			Command: "Chat"
		}
	];
	
	var msgpanel_config = {
		GetOperations : function(msg)
		{
			if(msg.Sender.ID != Core.Session.GetUserID())
			{
				return msgpanel_opes_1;
			}
			else
			{
				return [];
			}
		}
	};

	var rightpanel_ = fillpanel_.FindControl("rightpanel");
	var msgpanel_ = new Core.UI.MsgPanel(fillpanel_.GetContainer("rightpanel/msgpanel"), msgpanel_config);	
	var usertree_ = new Core.UI.MsgUserTree(fillpanel_.GetContainer("usertree"), {});
	
	var pagebar_ctrl_ = fillpanel_.FindControl("rightpanel/pagebar");
	var pagebar_ = fillpanel_.GetContainer("rightpanel/pagebar");
	pagebar_.innerHTML = "<a href='javascript:void(0)'>尾页</a>" +
	"<a href='javascript:void(0)'>下一页</a>" +
	"<span>/1</span>" +
	"<input type='text' class='curpage' value='1' />" +
	"<a href='javascript:void(0)'>上一页</a>" +
	"<a href='javascript:void(0)'>首页</a>";
	
	var search_panel_conf = {
		ShowUserCombox: config_.Peer == undefined, 
		Peer: config_.Peer
	};
	var search_panel_ = new Core.UI.SearchPanel(this_, fillpanel_.GetContainer("searchbar"), fillpanel_.FindControl("searchpanel"), search_panel_conf);
	
	var pagecount_obj_ = pagebar_.childNodes[2];
	var curpage_obj_ = pagebar_.childNodes[3];
	
	function FindNode(peerid)
	{
		var targetNode = null;
		usertree_.GetAllNodes(
			function(node)
			{
				if (node.GetTag() != null && node.GetTag().ID == peerid)
				{
					targetNode = node;
				}
			}
		);
		return targetNode;
	}
	
	function RefreshMessages(msg, refreshTree)
	{
		if (refreshTree == undefined) refreshTree = false;
		if (peer_ == 0) return;
	
		CurrentWindow.CompleteAll();
	
		CurrentWindow.Waiting("正在载入消息记录...");
	
		msgpanel_.Clear();
	
		usertree_.Load(
			function(users, ex)
			{
				if (users != null)
				{
					usertree_.Refresh(
						function()
						{
							usertree_.Expand(Core.EmptyCallback, "/Users");
							usertree_.Expand(Core.EmptyCallback, "/Groups");
							usertree_.Expand(Core.EmptyCallback, "/TempGroups");
							usertree_.Expand(Core.EmptyCallback, "/Visitors");
						}
					);
		
					var node = FindNode(peer_);
					if (node == null)
					{
						CurrentWindow.Completed();
						return;
					}
		
					var data = {
						Action: "GetMessages",
						Peer: node.GetTag().ID,
						From: from_,
						To: to_,
						Page: page_,
						PageSize: 20,
						MsgID: msgid_
					};
		
					if (msg != undefined && msg != null) data.MsgCreatedTime = msg.CreatedTime;
		
					Core.SendCommand(
						function(ret)
						{
							var info = node.GetTag();
							usertree_.Select(Core.EmptyCallback, node.GetFullPath());
			
							msgpanel_.Clear();
							ret.Messages.sort(MsgSort);
							for (var i = ret.Messages.length - 1; i >= 0; i--)
							{
								msgpanel_.AddMessage(ret.Messages[i]);
							}
							messages_ = ret.Messages;
							page_ = ret.Page;
							pagecount_obj_.innerHTML = String.format("/{0}", ret.PageCount < 1 ? 1 : ret.PageCount);
							curpage_obj_.value = ret.Page.toString();
							pagebar_ctrl_.SetVisible(ret.PageCount > 1);
							CurrentWindow.Completed();
						},
						function(ex)
						{
							CurrentWindow.Completed();
							Core.Utility.ShowError(ex.toString());
						},
						Core.Utility.RenderJson(data), "Core.Web Common_CH", false
					);
		
				}
				else
				{
					CurrentWindow.Completed();
					Core.Utility.ShowError(ex.toString());
				}
			}, 
			!refreshTree
		);
	}
	
	this_.GetCurrentUser = function()
	{
		if(config_.Peer != undefined) return config_.Peer;

		var node = usertree_.GetSelectedNode();
		return node != null ? node.GetTag() : 0;
	}
	
	this_.View = function(msg)
	{
		search_panel_.SetVisible(false);
		rightpanel_.SetVisible(true);
		if (msg.Receiver.Type == 1)
		{
			peer_ = msg.Receiver.ID;
		}
		else
		{
			peer_ = msg.Receiver.ID == Core.Session.GetUserID() ? msg.Sender.ID: msg.Receiver.ID;
		}
		msgid_ = msg.ID;
		page_ = 1;
		RefreshMessages(msg, true);
	}
	
	this_.Select = function(peerid)
	{
		peer_ = peerid;
		page_ = -1;
		msgid_ = 0;
		search_panel_.SetVisible(false);
		rightpanel_.SetVisible(true);
		RefreshMessages(null, true);
	}
	
	this_.Load = function()
	{
		search_panel_.SetVisible(false);
		rightpanel_.SetVisible(true);
		CurrentWindow.Waiting("正在载入消息记录...");    
		usertree_.Load(
			function(users, ex)
			{
				if (users != null)
				{
					usertree_.Refresh(
						function()
						{
							usertree_.Expand(Core.EmptyCallback, "/Users");
							usertree_.Expand(Core.EmptyCallback, "/Groups");
							usertree_.Expand(Core.EmptyCallback, "/TempGroups");
							usertree_.Expand(Core.EmptyCallback, "/Visitors");
						}
					);
					CurrentWindow.Completed();
				}
			}, false
		);
	} 
	
	pagebar_.childNodes[0].onclick = function()
	{
		page_ = -1;
		msgid_ = 0;
		RefreshMessages();
	}
	
	pagebar_.childNodes[1].onclick = function()
	{
		page_++;
		msgid_ = 0;
		RefreshMessages();
	}
	
	pagebar_.childNodes[4].onclick = function()
	{
		if(page_ > 1)
		{
			page_--;
			msgid_ = 0;
			RefreshMessages();
		}
	}
	
	pagebar_.childNodes[5].onclick = function()
	{
		page_ = 1;
		msgid_ = 0;
		RefreshMessages();
	}
	
	curpage_obj_.onkeydown = function(evt)
	{
		if(evt == undefined) evt = window.event;
		if(evt.keyCode == 13)
		{
			var page = parseInt(curpage_obj_.value);
			if(isNaN(page))
			{
				curpage_obj_.value = (page_ < 1 ? 1 : page_).toString();
			}
			else
			{
				page_ = page;
				msgid_ = 0;
				RefreshMessages();
			}
		}
	}
	  
	msgpanel_.OnCommand.Attach(
		function(cmd, msg)
		{
			if (cmd == "Chat")
			{
				if (msg.Sender.ID != Core.Session.GetUserInfo().ID)
				{
					Core.Session.GetGlobal("ChatService").Open(msg.Sender.ID);
				}
			}
		}
	);
	
	usertree_.OnClick.Attach(
		function()
		{
			search_panel_.SetVisible(false);
			rightpanel_.SetVisible(true);
			page_ = -1;
			msgid_ = 0;
			var node = usertree_.GetSelectedNode();
			if (node != null && node.GetTag() != null)
			{
				peer_ = node.GetTag().ID;
				RefreshMessages();
			}
		}
	);
	
	search_panel_.BeforeSearch.Attach(
		function()
		{
			rightpanel_.SetVisible(false);
			search_panel_.SetVisible(true);
		}
	);
	
	search_panel_.OnBtnHiddenClick.Attach(
		function()
		{
			search_panel_.SetVisible(false);
			rightpanel_.SetVisible(true);
		}
	);
}

})();