if(window.Core == undefined) window.Core = {};
if(window.Core.UI == undefined) window.Core.UI = {};

Core.UI.CalcOffsetWidth = function(obj, style_width)
{
	var current_style = Core.Utility.GetCurrentStyle(obj);
	var diffx = Core.Utility.ParseInt(current_style.borderLeftWidth) + 
		Core.Utility.ParseInt(current_style.borderRightWidth) + 
		Core.Utility.ParseInt(current_style.paddingLeft) + 
		Core.Utility.ParseInt(current_style.paddingRight);
	return style_width + diffx;
}

Core.UI.CalcOffsetHeight = function(obj, style_height)
{
	var current_style = Core.Utility.GetCurrentStyle(obj);
	var diffy = Core.Utility.ParseInt(current_style.borderTopWidth) + 
		Core.Utility.ParseInt(current_style.borderBottomWidth) + 
		Core.Utility.ParseInt(current_style.paddingTop) + 
		Core.Utility.ParseInt(current_style.paddingBottom);
	return style_height + diffy;
}

Core.UI.CalcStyleWidth = function(obj, offset_width)
{
	var current_style = Core.Utility.GetCurrentStyle(obj);
	var diffx = Core.Utility.ParseInt(current_style.borderLeftWidth) + 
		Core.Utility.ParseInt(current_style.borderRightWidth) + 
		Core.Utility.ParseInt(current_style.paddingLeft) + 
		Core.Utility.ParseInt(current_style.paddingRight);
	return offset_width - diffx;
}

Core.UI.CalcStyleHeight = function(obj, offset_height)
{
	var current_style = Core.Utility.GetCurrentStyle(obj);
	var diffy = Core.Utility.ParseInt(current_style.borderTopWidth) + 
		Core.Utility.ParseInt(current_style.borderBottomWidth) + 
		Core.Utility.ParseInt(current_style.paddingTop) + 
		Core.Utility.ParseInt(current_style.paddingBottom);
	return offset_height - diffy;
}

Core.UI.ReplaceTag = function(str, ids)
{
	return str.replace(
		/\x24\x28([a-zA-Z\d]+)\x29/ig,
		function(s, id_key)
		{
			return ids[id_key];
		}
	)
}

Core.UI.PagePanel = (function()
{
	var this_ = {};

	var append_to_document_ = false;
	var scroll_dom_ = document.createElement("DIV");
	scroll_dom_.style.left = "0px";
	scroll_dom_.style.top = "0px";
	scroll_dom_.style.width = "0px";
	scroll_dom_.style.height = "0px";
	scroll_dom_.style.zIndex = "1000000";
	scroll_dom_.style.overflow = "visible";
	scroll_dom_.style.position = "absolute";

	var move_div_ = document.createElement("DIV");
	move_div_.style.display = 'none';
	move_div_.className = 'move_div';
	move_div_.setAttribute("unselectable", "on");
	move_div_.innerHTML = "<div class='move_div_bk'></div>";
	scroll_dom_.appendChild(move_div_);
	
	var dom_ = null;
	
	this_.OnResize = new Core.Delegate();

	this_.Create = function(html)
	{
		if(window.CurrentWindow == undefined)
		{
			Core.Utility.AttachEvent(
				window, "resize",
				function()
				{
					this_.Resize();
				}
			);
		}
		else
		{
			window.CurrentWindow.OnResize.Attach(
				function()
				{
					this_.Resize();
				}
			);
		}
		
		dom_ = document.createElement("DIV");
		dom_.className = "page_panel";
		dom_.setAttribute("unselectable", "on");
		scroll_dom_.appendChild(dom_);
		if(!append_to_document_)
		{
			append_to_document_ = true;
			document.body.appendChild(scroll_dom_);
		}

		var enableSelTag = {
			"TEXTAREA": "",
			"INPUT": ""
		};

		dom_.onselectstart = function(evt)
		{
			var e = new Core.Event(evt, window);
			return (e.GetTarget().tagName != undefined && enableSelTag[e.GetTarget().tagName.toUpperCase()] != undefined)
		}

		move_div_.onselectstart = function(evt)
		{
			var e = new Core.Event(evt, window);
			return (e.GetTarget().tagName != undefined && enableSelTag[e.GetTarget().tagName.toUpperCase()] != undefined)
		}
		
		this_.Resize();
		if(html != undefined) dom_.innerHTML = html;
	}	
	
	this_.Resize = function()
	{
		if(dom_ != null)
		{
			dom_.style.width = this_.GetWidth() + 'px';
			dom_.style.height = this_.GetHeight() + 'px';
		}
		if(move_div_ != null)
		{
			move_div_.style.width = this_.GetWidth() + 'px';
			move_div_.style.height = this_.GetHeight() + 'px';
		}
		this_.OnResize.Call();
	}

	if (document.body == null)
	{
		Core.Utility.AttachEvent(
			window, "load",
			function()
			{
				if(!append_to_document_)
				{
					append_to_document_ = true;
					document.body.appendChild(scroll_dom_);
					this_.Resize();
				}
			}
		);
	}
	else
	{
		document.body.appendChild(scroll_dom_);
		this_.Resize();
	}

	Core.Utility.AttachEvent(
		window, "scroll",
		function()
		{
			scroll_dom_.style.left = (document.documentElement.scrollLeft) + "px"
			scroll_dom_.style.top = (document.documentElement.scrollTop) + "px"
		}
	);
	
	var capture_mousemove_callback_ = null, capture_mouseup_callback_ = null;
	var capture_mouse_event_ = false;
	
	function capture_onmousemove(e)
	{
		if(capture_mouse_event_)
		{
			if(e == undefined) e = window.event;
			if(capture_mousemove_callback_ != null) capture_mousemove_callback_(e);
		}
	}
	
	function capture_onmouseup(e)
	{
		if(capture_mouse_event_)
		{
			if(e == undefined) e = window.event;
			capture_mouse_event_ = false;
			if(scroll_dom_.releaseCapture != undefined) 
			{
				scroll_dom_.releaseCapture();
			}
			if(capture_mouseup_callback_ != null) capture_mouseup_callback_(e);
			capture_mousemove_callback_ = null;
			capture_mouseup_callback_ = null;
		}
	}

	if(scroll_dom_.setCapture == undefined)
	{
		Core.Utility.AttachEvent(window, "mousemove", capture_onmousemove);
		Core.Utility.AttachEvent(window, "mouseup", capture_onmouseup);
	}
	else
	{
		Core.Utility.AttachEvent(scroll_dom_, "mousemove", capture_onmousemove);
		Core.Utility.AttachEvent(scroll_dom_, "mouseup", capture_onmouseup);
	}
	
	this_.CaptureMouseEvent = function(mousemove_cb, mouseup_cb)
	{
		capture_mousemove_callback_ = mousemove_cb;
		capture_mouseup_callback_ = mouseup_cb;
		
		capture_mouse_event_ = true;
		if(scroll_dom_.setCapture != undefined) 
		{
			scroll_dom_.setCapture();
		}
	}
	
	var popup_mousedown_callback_ = null;
	var popup_obj_ = null;
	
	this_.ClosePopupCtrl = function()
	{
		move_div_.style.display = 'none';
		if(popup_obj_ != null) move_div_.removeChild(popup_obj_);
		popup_mousedown_callback_ = null;
		popup_obj_ = null;
	}
	
	Core.Utility.AttachEvent(
		document, "mousedown", 
		function(e)
		{
			if(e == undefined) e = window.event;
			move_div_.style.display = 'none';
			if(popup_obj_ != null) move_div_.removeChild(popup_obj_);
			try
			{
				if(popup_mousedown_callback_ != null) popup_mousedown_callback_(e);
			}
			catch(ex)
			{
			}
			popup_mousedown_callback_ = null;
			popup_obj_ = null;
		}
	);
	
	this_.Popup = function(obj, x, y, mousedown_cb)
	{	
		move_div_.style.display = 'block';
		Core.Utility.DisableSelect(move_div_, true);
		
		var move_div_coord = Core.Utility.GetClientCoord(move_div_);
		x -= move_div_coord.X;
		y -= move_div_coord.Y;
		
		obj.style.left = x + "px";
		obj.style.top = y + "px";
		obj.style.display = "block";
		popup_obj_ = obj;
		
		if(mousedown_cb != undefined) popup_mousedown_callback_ = mousedown_cb;
		
		move_div_.appendChild(popup_obj_);
	}
	
	this_.SetCursor = function(cursor)
	{
		move_div_.style.cursor = cursor;
		scroll_dom_.style.cursor = cursor;
	}

	this_.GetDom = function()
	{
		return dom_;
	}

	this_.AppendChildNode = function(elem)
	{
		dom_.appendChild(elem);
	}
	
	this_.GetHeight = function()
	{
		return window.CurrentWindow != undefined ? window.CurrentWindow.GetClientHeight() : document.documentElement.clientHeight;
	}
	
	this_.GetWidth = function()
	{
		return window.CurrentWindow != undefined ? window.CurrentWindow.GetClientWidth() : document.documentElement.clientWidth;
	}

	return this_;
})();

Core.UI.InputUnit = function(label_text, input_type, input_name, textbox_maxlength, format_tip, empty_tip, rules_data)
{
	var rules_ = [];
	if (rules_data != undefined)
	{
		for (var i = 0; i + 1 < rules_data.length; i += 2)
		{
			rules_.push({ Reg: rules_data[i], Tip: rules_data[i + 1] });
		}
	}

	var this_ = this;
	var id_ = "INPUTUNIT_" + Core.GenerateUniqueId();
	var html =
	"<div class='input_area'>" +
		"<label class='ct_single_line_label input_unit_label' for='$(ID)'></label>" +
		"<input class='ct_textbox input_unit_textbox' name='$(TBNAME)' id='$(ID)' type='$(TYPE)' />" +
	"</div>" + 
	"<div class='ct_single_line_label input_unit_tip'></div>";
	var dom_ = document.createElement("DIV");
	dom_.className = "input_unit";
	dom_.innerHTML = Core.UI.ReplaceTag(html, { ID: id_, TBNAME: input_name, TYPE: input_type });
	var label_ = dom_.firstChild.childNodes[0];
	var textbox_ = dom_.firstChild.childNodes[1];
	var div_tip = dom_.childNodes[1];
	label_.innerHTML = label_text;
	textbox_.name = input_name;
	textbox_.maxLength = textbox_maxlength;
	textbox_.onblur = function()
	{
		div_tip.innerHTML = "";
	}
	textbox_.onfocus = function()
	{
		div_tip.innerHTML = format_tip;
		Core.Utility.ModifyCss(dom_, "-input_unit_error");
	}

	function Match(reg, str)
	{
		reg.lastIndex = 0;
		var ft = reg.exec(str);
		return (ft != null && ft.length == 1 && ft[0] == str)
	}

	this_.GetDom = function(elem)
	{
		return dom_;
	}

	this_.Check = function()
	{
		if (empty_tip != null && textbox_.value == "")
		{
			div_tip.innerHTML = empty_tip;
			Core.Utility.ModifyCss(dom_, "input_unit_error");
			return false;
		}
		for (var i in rules_)
		{
			if (rules_[i].Reg.call != undefined && !rules_[i].Reg(textbox_.value))
			{
				div_tip.innerHTML = rules_[i].Tip;
				Core.Utility.ModifyCss(dom_, "input_unit_error");
				return false;
			}
			else if (rules_[i].Reg.exec != undefined && !Match(rules_[i].Reg, textbox_.value))
			{
				div_tip.innerHTML = rules_[i].Tip;
				Core.Utility.ModifyCss(dom_, "input_unit_error");
				return false;
			}
		}
		return true;
	}

	this_.SetReadOnly = function(readonly)
	{
		textbox_.readOnly = readonly;
	}

	this_.GetValue = function()
	{
		return textbox_.value;
	}

	this_.SetValue = function(value)
	{
		textbox_.value = value;
	}

	return this_;
}

Core.UI.InputUnit.MultiCheck = function(units)
{
	var result = true;
	for (var i in units)
	{
		if (!units[i].Check()) result = false;
	}
	return result;
}

Core.UI.AttachButtonEvent = function(elem, src_elem)
{
	if(src_elem == undefined) src_elem = elem;

	Core.Utility.ModifyCss(elem, "ct_normal");

	Core.Utility.AttachEvent(
		src_elem, "mousedown",
		function(evt)
		{
			if (Core.Utility.ContainsCss(elem, "ct_checked")) 
			{
				Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_press", "-ct_normal")
				return;			
			}
			if (evt == undefined) evt = window.event;

			if (Core.Utility.GetButton(evt) == "Left")
			{
				Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_normal", "ct_press");
			}
		}
	);

	Core.Utility.AttachEvent(
		src_elem, "mouseup",
		function(evt)
		{
			if (Core.Utility.ContainsCss(elem, "ct_checked"))
			{
				Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_press", "-ct_normal")
				return;			
			}
			if (evt == undefined) evt = window.event;
			Core.Utility.ModifyCss(elem, "-ct_press", "-ct_normal", "ct_hover");
		}
	);

	Core.Utility.AttachEvent(
		src_elem, "mouseover",
		function(evt)
		{
			if (Core.Utility.ContainsCss(elem, "ct_checked"))
			{
				Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_press", "-ct_normal")
				return;			
			}
			if (evt == undefined) evt = window.event;
			if (!Core.Utility.ContainsCss(elem, "ct_hover"))
			{
				Core.Utility.ModifyCss(elem, "-ct_press", "-ct_normal", "ct_hover");
			}
		}
	);

	Core.Utility.AttachEvent(
		src_elem, "mouseout",
		function(evt)
		{
			if (Core.Utility.ContainsCss(elem, "ct_checked"))
			{
				Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_press", "-ct_normal")
				return;			
			}
			Core.Utility.ModifyCss(elem, "-ct_hover", "-ct_press", "ct_normal");
		}
	);
}

Core.UI.UploadButton = function(container, config)
{
	var this_ = this;

	this_.OnClick = new Core.Delegate();
	
	var dom_ = document.createElement("DIV");
	container.appendChild(dom_);

	var uploadings_ = {};

	function CreateUploadFrame()
	{
		var frame = document.createElement("iframe");
		frame.frameBorder = 0;
		frame.allowTransparency = true;
		frame.width = container.offsetWidth;
		frame.height = container.offsetHeight;
		frame.style.backgroundColor = "transparent";
		frame.style.width = container.offsetWidth + "px";
		frame.style.height = container.offsetHeight + "px";
		frame.style.overflow = "hidden";
		frame.src = Core.GetUrl("UploadFile.aspx");

		var handler = {
			BeforeUpload: function(id, name)
			{
				if(config.OnRequestUpload == undefined) return false;
				var custom_handler = config.OnRequestUpload(id, name);
				if(custom_handler == null) return false;
				uploadings_[id] = {
					CustomHandler: custom_handler,
					Frame: frame
				};

				frame.width = 0;
				frame.height = 0;
				frame.style.width = "0px";
				frame.style.height = "0px";

				CreateUploadFrame();

				return true;
			},
			OnCompleted: function(id, path)
			{
				dom_.removeChild(frame);

				if(uploadings_[id] != undefined)
				{
					var uf = uploadings_[id];
					if(uf.CustomHandler.OnCompleted != undefined) 
					{
						uf.CustomHandler.OnCompleted(path);
					}
					delete uploadings_[id];
				}
			},
			OnError: function(id, ex)
			{
				dom_.removeChild(frame);

				if(uploadings_[id] != undefined)
				{
					var uf = uploadings_[id];
					if(uf.CustomHandler.OnError != undefined) 
					{
						uf.CustomHandler.OnError(ex);
					}
					delete uploadings_[id];
				}
			}
		};

		if(container.childNodes.length > 0 && container.firstChild != null)
		{
			dom_.insertBefore(frame, dom_.firstChild);
		}
		else
		{
			dom_.appendChild(frame);
		}
	
		try
		{
			Core.Utility.AttachEvent(
				frame, "load",
				function()
				{
					frame.contentWindow.UploadHandler = handler;
					Core.UI.AttachButtonEvent(dom_, frame.contentWindow.document);
					if(frame.contentWindow.Load != undefined)
					{
						frame.contentWindow.Load();
					}
				}
			);
		}
		catch(ex)
		{
		}
	}

	CreateUploadFrame();

	this_.Cancel = function(id)
	{
		if(uploadings_[id] != undefined)
		{
			var uf = uploadings_[id];
			dom_.removeChild(uf.Frame);
			if(uf.CustomHandler.OnCancel != undefined) 
			{
				uf.CustomHandler.OnCancel();
			}
			delete uploadings_[id];
		}
	}
}

Core.UI.Button = function(container, config)
{
	var this_ = this;

	this_.OnClick = new Core.Delegate();

	var dom_ = document.createElement("DIV");
	dom_.className = "ct button_body";
	dom_.tabIndex = Core.Utility.IsNull(config.TabIndex, -1);
	dom_.innerHTML =
	'<div class="btn_left">' +
	'</div>' +
	'<div class="btn_center">' +
	'<div class="btn_text"></div>' +
	'</div>' +
	'<div class="btn_right">' +
	'</div>';
	
	Core.UI.AttachButtonEvent(dom_);
	
	var menu_ = null;
	if(config.Menu != undefined)
	{
		menu_ = config.Menu;
	}
	if(config.MenuConfig != undefined && Core.CreateMenu != undefined)
	{
		menu_ = Core.CreateMenu(config.MenuConfig);
	}
	if(menu_ != null)
	{
		if(window.ClientMode && window.CurrentWindow)
		{
			window.CurrentWindow.OnClosed.Attach(function() { menu_ = null; });
		}
		menu_.OnCommand.Attach(
			function(command)
			{
				this_.SetStatus("ct_normal");
				this_.OnCommand.Call(command);
			}
		);
	}
	dom_.onkeydown = function(evt)
	{
		if (evt == undefined) evt = event;
		if (evt.keyCode == 13)
		{
			this_.Click();
			evt.keyCode = 0;
			Core.Utility.PreventDefault(evt);
		}
		Core.Utility.CancelBubble(evt);
	}

	dom_.onmousedown = function()
	{
		if(menu_ != null)
		{
			this_.SetStatus("ct_checked");
		}
	}
	
	dom_.onclick = function(evt)
	{
		this_.Click();
		if(menu_ != null)
		{
			var coord = Core.Utility.GetClientCoord(dom_);
			var clientCoord = CurrentWindow.GetClientCoord(coord.X, coord.Y + dom_.offsetHeight + 2);
			menu_.Popup(clientCoord.X, clientCoord.Y);
		}
	}
	
	this_.OnCommand = new Core.Delegate();
	
	this_.IsChecked = function()
	{
		return Core.Utility.ContainsCss(dom_, "ct_checked")
	}
	
	this_.SetStatus = function(status)
	{
		Core.Utility.ModifyCss(dom_, "-ct_checked", "-ct_normal", "-ct_hover", "-ct_press", status);
	}

	this_.Click = function()
	{
		if(config.CheckMode)
		{
			if(Core.Utility.ContainsCss(dom_, "ct_checked"))
			{
				Core.Utility.ModifyCss(dom_, "-ct_checked", "ct_normal");
			}
			else
			{
				Core.Utility.ModifyCss(dom_, "ct_checked");
			}
		}
		dom_.focus();
		this_.OnClick.Call(this_);
	}

	this_.SetText = function(value)
	{
		dom_.childNodes[1].firstChild.innerHTML = Core.Utility.FilterHtml(value);
	}

	this_.Focus = function()
	{
		dom_.focus();
	}

	this_.GetType = function()
	{
		return "Core.UI.Button";
	}
	
	this_.GetDom = function()
	{
		return dom_;
	}

	Core.Utility.DisableSelect(dom_, true);

	container.appendChild(dom_);

	this_.SetText(Core.Utility.IsNull(config.Text, ""));
}

Core.UI.Toolbar = function(container, config)
{
	var this_ = this;

	var dom_ = document.createElement("DIV");
	dom_.className = "toolbar_body";
	container.appendChild(dom_);

	var items_ = [];
	var items_table_ = {};

	for (var i in config.Items)
	{
		var item_config = config.Items[i];

		var item_dom = document.createElement("DIV");
		dom_.appendChild(item_dom);

		var item = {};
		if(item_config.Type == "Button")
		{
			item_dom.className = "tb_item ct_toolbar_button" + (item_config.Css == undefined ? "" : " " + item_config.Css)
			item = new Core.UI.Button(item_dom, item_config.Config);;
			item_dom.title = (item_config.Tooltip == undefined ? "" : item_config.Tooltip);
			item.OnCommand.Attach(
				function(cmd)
				{
					this_.OnCommand.Call(cmd);
				}
			);
			
			(function(btnconfig) {
				item.OnClick.Attach(
					function(btn)
					{
						this_.OnCommand.Call(btnconfig.ID);
					}
				);
			})(item_config);
		}
		else if(item_config.Type == "UploadButton")
		{
			item_dom.className = "tb_item" + (item_config.Css == undefined ? "" : " " + item_config.Css);
			item = new Core.UI.UploadButton(item_dom, item_config.Config);
			item_dom.title = (item_config.Tooltip == undefined ? "" : item_config.Tooltip);
		}
		else if(item_config.Type == "Combox")
		{
			item_dom.className = "tb_item ct_toolbar_combox" + (item_config.Css == undefined ? "" : " " + item_config.Css);
			item = new Core.UI.Combox(item_dom, item_config.Config);
			Core.Utility.DisableSelect(item.GetDom(), true);
		}

		items_.push(item);
		items_table_[item_config.ID] = item;
	}

	this_.GetDom = function()
	{
		return dom_;
	}

	this_.GetItem = function(id)
	{
		return items_table_[id];
	}

	this_.SetItemVisible = function(id, visible)
	{
		var item = items_table_[id];
		if(item != undefined)
		{
			item.GetDom().style.display = visible ? "block" : "none";
		}
	}
	
	this_.GetType = function()
	{
		return "Core.UI.Toolbar";
	}
	
	this_.OnCommand = new Core.Delegate();
}

Core.UI.ListBox = function(container, config)
{
	var this_ = this;
	if(config == undefined) config = {};

	var items = [];
	var select_index_ = -1;

	var dom_ = document.createElement("DIV");
	dom_.className = "ct listbox_body";
	container.appendChild(dom_);
	
	var content_height_ = 0;
	var content_width_ = 0;

	Core.Utility.AttachEvent(
		dom_, "mousedown",
		function(evt)
		{
			if (evt == undefined) evt = window.event;
			Core.Utility.CancelBubble(evt);
		}
	);

	this_.OnChanged = new Core.Delegate();

	this_.CalcSize = function()
	{
		content_height_ = 0;
		content_width_ = 0;
		for (var i in items)
		{
			var item_dom = items[i].Dom;
			if (content_width_ < item_dom.firstChild.offsetWidth) content_width_ = item_dom.firstChild.offsetWidth;
			content_height_ += item_dom.offsetHeight;
		}
		content_width_ += 8;
		return { Width: Core.UI.CalcOffsetWidth(dom_, content_width_), Height: Core.UI.CalcOffsetWidth(dom_, content_height_) };
	}
	
	function SelectItem(index)
	{
		if(index >= items.length) return;
		if(!config.PopupMode)
		{
			this_.ClearSelectedCss();
			var item_dom = items[index].Dom;
			Core.Utility.ModifyCss(item_dom, "ct_checked");
			select_index_ = index;
		}
		else
		{
			select_index_ = index;
		}
	}

	this_.AddItem = function(text, value)
	{
		if (value == undefined) value = text;
		var item_dom = document.createElement("DIV");
		item_dom.innerHTML = String.format("<span style='padding: 0px; margin: 0px 4px'>{0}</span>", Core.Utility.FilterHtml(text));
		item_dom.title = Core.Utility.FilterHtml(text);
		item_dom.className = "listbox_item";
		item_dom.nowrap = true;
		dom_.appendChild(item_dom);
		Core.UI.AttachButtonEvent(item_dom);

		items.push({ Text: text, Value: value, Dom: item_dom });

		(function(index)
		{
			item_dom.onmousedown = function(evt)
			{
				if (evt == undefined) evt = window.event;
				SelectItem(index);
				Core.Utility.CancelBubble(evt);
				this_.OnChanged.Call();
			}
		})(items.length - 1);

		
		Core.Utility.DisableSelect(dom_, true);
	}

	this_.Clear = function()
	{
		items = [];
		select_index_ = -1;
		dom_.innerHTML = "";
	}

	this_.GetText = function()
	{
		return select_index_ == -1 ? null : items[select_index_].Text;
	}

	this_.GetValue = function()
	{
		return select_index_ == -1 ? null : items[select_index_].Value;
	}
	
	this_.SetValue = function(value, fire_event)
	{
		for(var i in items)
		{
			if(items[i].Value == value) SelectItem(i, fire_event);
		}
	}

	this_.GetItems = function()
	{
		return items;
	}
	
	this_.GetDom = function()
	{
		return dom_;
	}
	
	this_.ClearSelectedCss = function()
	{
		if(select_index_ != -1)
		{
			var item_dom = items[select_index_].Dom;
			Core.Utility.ModifyCss(item_dom, "-ct_hover", "-ct_checked", "ct_normal");
		}
	}
}

Core.UI.Combox = function(container, config)
{
	var this_ = this;
	if(config == undefined) config = {};
	
	var dom_ = document.createElement("DIV");
	dom_.className = "combox_body ct ct_normal" +(config.CustomCss == undefined ? "" : " " + config.CustomCss);
	if(config.TabIndex != undefined)
	{
		dom_.tabIndex = config.TabIndex;
	}
	container.appendChild(dom_);
	dom_.innerHTML =
	"<div class='combox_text'><div></div></div>" +
	"<div class='combox_button'></div>";
	
	Core.UI.AttachButtonEvent(dom_);

	var list_visible_ = false;

	var listbox_container = document.createElement("DIV");

	listbox_container.style.position = "absolute";
	listbox_container.style.zIndex = "1";
	listbox_container.style.width = container.offsetWidth + "px";
	listbox_container.style.height = "60px";
	listbox_container.className = "ct_listbox";
	
	var listbox = new Core.UI.ListBox(listbox_container, {PopupMode: true});
	
	function ResizeListBox()
	{
		var size = listbox.CalcSize();
		if(size.Width < container.offsetWidth) size.Width = container.offsetWidth;
		if(size.Width > 400) size.Width = 400;
		if(size.Height > 400) size.Height = 400;
		var current_style = Core.Utility.GetCurrentStyle(dom_);
		size.Width -= Core.Utility.ParseInt(current_style.marginLeft) + Core.Utility.ParseInt(current_style.marginRight);
		listbox_container.style.width = size.Width + "px";
		listbox_container.style.height = size.Height + "px";
	}
	
	Core.Utility.AttachEvent(
		dom_, "mousedown",
		function(evt)
		{
			if (evt == undefined) evt = window.event;
			Core.Utility.ModifyCss(dom_, "ct_checked");
			this_.ShowList();
			Core.Utility.CancelBubble(evt);
		}
	);

	this_.OnChanged = new Core.Delegate();

	this_.GetDom = function()
	{
		return dom_;
	}

	this_.GetText = function()
	{
		return dom_.firstChild.firstChild.innerHTML;
	}

	this_.SetText = function(text, fire_event)
	{
		if(fire_event == undefined) fire_event = true;
		var value = null;
		var items = listbox.GetItems();
		for (var i in items)
		{
			if (items[i].Text == text) value = items[i].Value;
		}
		if(value != null) this_.SetValue(value, fire_event);
	}

	this_.GetValue = function()
	{		
		return listbox.GetValue();
	}

	this_.SetValue = function(value, fire_event)
	{
		if(fire_event == undefined) fire_event = true;
		var items = listbox.GetItems();
		for (var i in items)
		{
			if (items[i].Value == value)
			{
				dom_.firstChild.firstChild.innerHTML = Core.Utility.FilterHtml(items[i].Text);
				if(fire_event) this_.OnChanged.Call();
			}
		}
		listbox.SetValue(value, fire_event);
	}

	this_.AddItem = function(text, value)
	{
		listbox.AddItem(text, value);
	}

	this_.Clear = function()
	{
		listbox.Clear();
	}

	listbox.OnChanged.Attach(
		function()
		{
			if (list_visible_)
			{
				Core.UI.PagePanel.ClosePopupCtrl();
				this_.HideList();
				this_.SetText(listbox.GetText());
			}
		}
	);

	this_.ShowList = function()
	{
		if (!list_visible_)
		{
			var coord = Core.Utility.GetClientCoord(dom_);		
			var height = dom_.offsetHeight;
			var top = coord.Y + height;			
			Core.UI.PagePanel.Popup(
				listbox_container, coord.X, top, 
				function()
				{
					this_.HideList();
				}
			);			
			ResizeListBox();					
			if (top + listbox_container.offsetHeight > Core.UI.PagePanel.GetHeight())
			{
				top -= (listbox_container.offsetHeight + height + 1);
				top += 1;
				listbox_container.style.top = top + "px";
			}
			
			list_visible_ = true;
		}
	}

	this_.HideList = function()
	{
		if (list_visible_)
		{
			list_visible_ = false;
			listbox.ClearSelectedCss();
			Core.Utility.ModifyCss(dom_, "-ct_checked", "ct_normal");
		}
	}
	
	this_.AddItem = function(text, value)
	{
		listbox.AddItem(text, value);
	}
	
	if(config.Items != undefined)
	{
		for(var i in config.Items)
		{
			var item = config.Items[i];
			this_.AddItem(item.Text, item.Value);
		}
	}
}

Core.UI.CreateFrame = function(parent)
{
	var frame = document.createElement("iframe");
	frame.frameBorder = 0;
	frame.width = "100%";
	frame.height = "100%";
	frame.style.position = "absolute";
	frame.style.width = "100%";
	frame.style.height = "100%";
	
	if(parent != undefined) parent.appendChild(frame);
	
	if(window.CurrentWindow != undefined && window.ClientMode != true)
	{
		try
		{
			Core.Utility.AttachEvent(
				frame, "load",
				function()
				{
					Core.Utility.AttachEvent(
						frame.contentWindow.document, "mousedown",
						function()
						{
							if(window.CurrentWindow != undefined) window.CurrentWindow.BringToTop();
						}
					);
				}
			);
		}
		catch(ex)
		{
		}
	}
	
	return frame;
}